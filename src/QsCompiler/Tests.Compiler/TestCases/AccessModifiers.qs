﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

/// This namespace contains test cases for access modifiers.
namespace Microsoft.Quantum.Testing.TypeChecking {
    open Microsoft.Quantum.Testing.TypeChecking.A;
    open Microsoft.Quantum.Testing.TypeChecking.B as B;
    open Microsoft.Quantum.Testing.TypeChecking.C;
    
    // Redefine inaccessible symbols in reference

    private newtype T1 = Unit;

    internal newtype T2 = Unit;

    private function F1 () : Unit {}

    internal function F2 () : Unit {}

    // Callables with access modifiers

    function CallableUseOK () : Unit {
        F1();
        F2();
        AF2();
        B.BF2();
    }

    function CallableUnqualifiedUsePrivateInaccessible () : Unit {
        AF1();
    }

    function CallableQualifiedUsePrivateInaccessible () : Unit {
        B.BF1();
    }

    function CallableReferencePrivateInaccessible () : Unit {
        CF1();
	}

    function CallableReferenceInternalInaccessible () : Unit {
        CF2();
	}

    // Types with access modifiers

    function TypeUseOK () : Unit {
        let t1 = new T1[1];
        let t2 = new T2[1];
        let at2 = new AT2[1];
        let bt2 = new B.BT2[1];
    }

    function TypeUnqualifiedUsePrivateInaccessible () : Unit {
        let at1 = new AT1[1];
    }

    function TypeQualifiedUsePrivateInaccessible () : Unit {
        let bt1 = new B.BT1[1];
    }

    function TypeReferencePrivateInaccessible () : Unit {
        let ct1 = new CT1[1];
	}

    function TypeReferenceInternalInaccessible () : Unit {
        let ct2 = new CT2[1];
	}

    // Callable signatures

    function PublicCallableLeaksPrivateTypeIn1 (x : T1) : Unit {}
    
    function PublicCallableLeaksPrivateTypeIn2 (x : (Int, (T1, Bool))) : Unit {}

    function PublicCallableLeaksPrivateTypeOut1 () : T1 {
        return T1();
    }

    function PublicCallableLeaksPrivateTypeOut2 () : (Double, ((Result, T1), Bool)) {
        return (1.0, ((Zero, T1()), true));
    }

    internal function InternalCallableLeaksPrivateTypeIn (x : T1) : Unit {}

    internal function InternalCallableLeaksPrivateTypeOut () : T1 {
        return T1();
    }

    private function CallablePrivateTypeOK (x : T1) : T1 {
        return T1();
    }

    function CallableLeaksInternalTypeIn (x : T2) : Unit {}

    function CallableLeaksInternalTypeOut () : T2 {
        return T2();
    }

    internal function InternalCallableInternalTypeOK (x : T2) : T2 {
        return T2();
    }

    private function PrivateCallableInternalTypeOK (x : T1) : T1 {
        return T1();
    }

    // Underlying types

    newtype PublicTypeLeaksPrivateType1 = T1;

    newtype PublicTypeLeaksPrivateType2 = (Int, T1);

    newtype PublicTypeLeaksPrivateType3 = (Int, (T1, Bool));

    internal newtype InternalTypeLeaksPrivateType = T1;

    private newtype PrivateTypePrivateTypeOK = T1;

    newtype PublicTypeLeaksInternalType = T2;

    internal newtype InternalTypeInternalTypeOK = T2;

    private newtype PrivateTypeInternalTypeOK = T2;
}

/// This namespace contains additional definitions of types and callables meant to be used by the
/// Microsoft.Quantum.Testing.TypeChecking namespace.
namespace Microsoft.Quantum.Testing.TypeChecking.A {
    private function AF1 () : Unit {}

    internal function AF2 () : Unit {}

    private newtype AT1 = Unit;

    internal newtype AT2 = Unit;
}

/// This namespace contains additional definitions of types and callables meant to be used by the
/// Microsoft.Quantum.Testing.TypeChecking namespace.
namespace Microsoft.Quantum.Testing.TypeChecking.B {
    private function BF1 () : Unit {}

    internal function BF2 () : Unit {}

    private newtype BT1 = Unit;

    internal newtype BT2 = Unit;
}
