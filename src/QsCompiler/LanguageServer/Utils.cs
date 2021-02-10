﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Quantum.QsCompiler;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Newtonsoft.Json.Linq;

namespace Microsoft.Quantum.QsLanguageServer
{
    public static class Utils
    {
        // language server tools -
        // wrapping these into a try .. catch .. to make sure errors don't go unnoticed as they otherwise would

        public static T TryJTokenAs<T>(JToken arg)
            where T : class =>
            QsCompilerError.RaiseOnFailure(() => arg.ToObject<T>(), "could not cast given JToken");

        private static ShowMessageParams? AsMessageParams(string text, MessageType severity) =>
            text == null ? null : new ShowMessageParams { Message = text, MessageType = severity };

        /// <summary>
        /// Shows the given text in the editor.
        /// </summary>
        internal static void ShowInWindow(this QsLanguageServer server, string text, MessageType severity)
        {
            var message = AsMessageParams(text, severity);
            QsCompilerError.Verify(server != null && message != null, "cannot show message - given server or text was null");
            _ = server.NotifyClientAsync(Methods.WindowShowMessageName, message);
        }

        /// <summary>
        /// Shows a dialog window with options (actions) to the user, and returns the selected option (action).
        /// </summary>
        internal static async Task<MessageActionItem> ShowDialogInWindowAsync(this QsLanguageServer server, string text, MessageType severity, MessageActionItem[] actionItems)
        {
            var message =
                new ShowMessageRequestParams()
                {
                    Message = text,
                    MessageType = severity,
                    Actions = actionItems
                };
            return await server.InvokeAsync<MessageActionItem>(Methods.WindowShowMessageRequestName, message);
        }

        /// <summary>
        /// Logs the given text in the editor.
        /// </summary>
        internal static void LogToWindow(this QsLanguageServer server, string text, MessageType severity)
        {
            var message = AsMessageParams(text, severity);
            QsCompilerError.Verify(server != null && message != null, "cannot log message - given server or text was null");
            _ = server.NotifyClientAsync(Methods.WindowLogMessageName, message);
        }

        // tools related to project loading and file watching

        /// <summary>
        /// Attempts to apply the given mapper to each element in the given sequence.
        /// Returns a new sequence consisting of all mapped elements for which the mapping succeeded as out parameter,
        /// as well as a bool indicating whether the mapping succeeded for all elements.
        /// The returned out parameter is non-null even if the mapping failed on some elements.
        /// </summary>
        internal static bool TryEnumerate<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TResult> mapper,
            out ImmutableArray<TResult> mapped)
        {
            var succeeded = true;
            var enumerator = source.GetEnumerator();

            T Try<T>(Func<T> getRes, T fallback)
            {
                try
                {
                    return getRes();
                }
                catch
                {
                    succeeded = false;
                    return fallback;
                }
            }

            bool TryMoveNext() => Try(enumerator.MoveNext, false);
            (bool, TResult) ApplyToCurrent() => Try(() => (true, mapper(enumerator.Current)), (false, default!));

            var values = ImmutableArray.CreateBuilder<TResult>();
            while (TryMoveNext())
            {
                var evaluated = ApplyToCurrent();
                if (evaluated.Item1)
                {
                    values.Add(evaluated.Item2);
                }
            }

            mapped = values.ToImmutable();
            return succeeded;
        }

        /// <summary>
        /// Attempts to enumerate the given sequence.
        /// Returns a new sequence consisting of all elements which could be accessed,
        /// as well as a bool indicating whether the enumeration succeeded for all elements.
        /// The returned out parameter is non-null even if access failed on some elements.
        /// </summary>
        internal static bool TryEnumerate<TSource>(this IEnumerable<TSource> source, out ImmutableArray<TSource> enumerated) =>
            source.TryEnumerate(element => element, out enumerated);

        /// <summary>
        /// The given log function is applied to all errors and warning
        /// raised by the ms build routine an instance of this class is given to.
        /// </summary>
        internal class MSBuildLogger : Logger
        {
            private readonly Action<string, MessageType> logToWindow;

            internal MSBuildLogger(Action<string, MessageType> logToWindow) =>
                this.logToWindow = logToWindow;

            public override void Initialize(IEventSource eventSource)
            {
                eventSource.ErrorRaised += (sender, args) =>
                    this.logToWindow?.Invoke(
                        $"MSBuild error in {args.File}({args.LineNumber},{args.ColumnNumber}): {args.Message}",
                        MessageType.Error);

                eventSource.WarningRaised += (sender, args) =>
                    this.logToWindow?.Invoke(
                        $"MSBuild warning in {args.File}({args.LineNumber},{args.ColumnNumber}): {args.Message}",
                        MessageType.Warning);
            }
        }
    }

    internal static class DotNetSdkHelper
    {
        private static readonly Regex DotNet31Regex = new Regex(@"^3\.1\.\d+", RegexOptions.Multiline | RegexOptions.Compiled);

        public static bool? IsDotNet31Installed()
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "--list-sdks",
                RedirectStandardOutput = true,
            });
            if (process?.WaitForExit(3000) != true || process.ExitCode != 0)
            {
                return null;
            }

            var sdks = process.StandardOutput.ReadToEnd();
            return DotNet31Regex.IsMatch(sdks);
        }
    }
}
