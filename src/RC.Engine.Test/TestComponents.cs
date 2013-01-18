using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;

namespace RC.Engine.Test
{
    [ComponentInterface]
    interface A0 { }
    [ComponentInterface]
    interface B0 { }
    [ComponentInterface]
    interface C0 { }

    [ComponentInterface]
    interface A1 { }

    [ComponentInterface]
    interface A2 { }
    [ComponentInterface]
    interface B2 { }

    [CallbackInterface]
    interface X0 { }
    [CallbackInterface]
    interface X1 { }
    [CallbackInterface]
    interface X2 { }

    [Component("C0")]
    class Component0 : A0, B0, C0
    {
        [ComponentReference]
        A1 a1;
        [ComponentReference]
        A2 a2;

        [CallbackReference]
        X0 x0;
        [CallbackReference]
        X1 x1;
    }

    [Component("C1")]
    class Component1 : A1
    {
        [ComponentReference]
        B0 b0;
        [ComponentReference]
        C0 c0;
        [ComponentReference]
        B2 b2;

        [CallbackReference]
        X1 x1;
        [CallbackReference]
        X2 x2;
    }

    [Component("C2")]
    class Component2 : A2, B2
    {
        [ComponentReference]
        A0 a0;
        [ComponentReference]
        A1 a1;

        [CallbackReference]
        X2 x2;
    }

    class Callback0 : X0, X1
    {
    }
    class Callback1 : X2
    {
    }
}
