﻿/// This file contains redefinitions of types and callables declared in Tests.Compiler\TestCases\AccessModifiers.qs. It
/// is used as an assembly reference to test support for re-using names of inaccessible declarations in references.
namespace Microsoft.Quantum.Testing.AccessModifiers {
    // TODO: Uncomment these definitions when re-using names of inaccessible declarations in references is supported.

    // private newtype T1 = Unit;

    // internal newtype T2 = Unit;

    // private function F1 () : Unit {}

    // internal function F2 () : Unit {}
}

/// This namespace contains additional definitions of types and callables meant to be used by the
/// Microsoft.Quantum.Testing.AccessModifiers namespace.
namespace Microsoft.Quantum.Testing.AccessModifiers.C {
    private function CF1 () : Unit {}

    internal function CF2 () : Unit {}

    private newtype CT1 = Unit;

    internal newtype CT2 = Unit;
}
