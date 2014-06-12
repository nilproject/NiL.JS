using System;
using System.Collections.Generic;
using System.Text;
using NiL.JS.Core.Modules;
using System.Diagnostics;
using System.Globalization;

namespace NiL.JS.Core.BaseTypes
{
    /// <summary>
    /// Возможные типы функции в контексте использования.
    /// </summary>
    [Serializable]
    public enum FunctionType
    {
        Function = 0,
        Get,
        Set
    }

    [Serializable]
    public class Function : EmbeddedType
    {
        private class _DelegateWraper
        {
            private Function function;

            public _DelegateWraper(Function func)
            {
                function = func;
            }

            public RT Invoke<RT>()
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(0);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1>(T1 a1)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(2);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2>(T1 a1, T2 a2)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(2);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3>(T1 a1, T2 a2, T3 a3)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(3);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                eargs.DefineMember("2").Assign(TypeProxy.Proxy(a3));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4>(T1 a1, T2 a2, T3 a3, T4 a4)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(4);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                eargs.DefineMember("2").Assign(TypeProxy.Proxy(a3));
                eargs.DefineMember("3").Assign(TypeProxy.Proxy(a4));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(5);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                eargs.DefineMember("2").Assign(TypeProxy.Proxy(a3));
                eargs.DefineMember("3").Assign(TypeProxy.Proxy(a4));
                eargs.DefineMember("4").Assign(TypeProxy.Proxy(a5));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(6);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                eargs.DefineMember("2").Assign(TypeProxy.Proxy(a3));
                eargs.DefineMember("3").Assign(TypeProxy.Proxy(a4));
                eargs.DefineMember("4").Assign(TypeProxy.Proxy(a5));
                eargs.DefineMember("5").Assign(TypeProxy.Proxy(a6));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(7);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                eargs.DefineMember("2").Assign(TypeProxy.Proxy(a3));
                eargs.DefineMember("3").Assign(TypeProxy.Proxy(a4));
                eargs.DefineMember("4").Assign(TypeProxy.Proxy(a5));
                eargs.DefineMember("5").Assign(TypeProxy.Proxy(a6));
                eargs.DefineMember("6").Assign(TypeProxy.Proxy(a7));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(8);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                eargs.DefineMember("2").Assign(TypeProxy.Proxy(a3));
                eargs.DefineMember("3").Assign(TypeProxy.Proxy(a4));
                eargs.DefineMember("4").Assign(TypeProxy.Proxy(a5));
                eargs.DefineMember("5").Assign(TypeProxy.Proxy(a6));
                eargs.DefineMember("6").Assign(TypeProxy.Proxy(a7));
                eargs.DefineMember("7").Assign(TypeProxy.Proxy(a8));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(9);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                eargs.DefineMember("2").Assign(TypeProxy.Proxy(a3));
                eargs.DefineMember("3").Assign(TypeProxy.Proxy(a4));
                eargs.DefineMember("4").Assign(TypeProxy.Proxy(a5));
                eargs.DefineMember("5").Assign(TypeProxy.Proxy(a6));
                eargs.DefineMember("6").Assign(TypeProxy.Proxy(a7));
                eargs.DefineMember("7").Assign(TypeProxy.Proxy(a8));
                eargs.DefineMember("8").Assign(TypeProxy.Proxy(a9));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(10);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                eargs.DefineMember("2").Assign(TypeProxy.Proxy(a3));
                eargs.DefineMember("3").Assign(TypeProxy.Proxy(a4));
                eargs.DefineMember("4").Assign(TypeProxy.Proxy(a5));
                eargs.DefineMember("5").Assign(TypeProxy.Proxy(a6));
                eargs.DefineMember("6").Assign(TypeProxy.Proxy(a7));
                eargs.DefineMember("7").Assign(TypeProxy.Proxy(a8));
                eargs.DefineMember("8").Assign(TypeProxy.Proxy(a9));
                eargs.DefineMember("9").Assign(TypeProxy.Proxy(a10));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(11);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                eargs.DefineMember("2").Assign(TypeProxy.Proxy(a3));
                eargs.DefineMember("3").Assign(TypeProxy.Proxy(a4));
                eargs.DefineMember("4").Assign(TypeProxy.Proxy(a5));
                eargs.DefineMember("5").Assign(TypeProxy.Proxy(a6));
                eargs.DefineMember("6").Assign(TypeProxy.Proxy(a7));
                eargs.DefineMember("7").Assign(TypeProxy.Proxy(a8));
                eargs.DefineMember("8").Assign(TypeProxy.Proxy(a9));
                eargs.DefineMember("9").Assign(TypeProxy.Proxy(a10));
                eargs.DefineMember("10").Assign(TypeProxy.Proxy(a11));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(12);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                eargs.DefineMember("2").Assign(TypeProxy.Proxy(a3));
                eargs.DefineMember("3").Assign(TypeProxy.Proxy(a4));
                eargs.DefineMember("4").Assign(TypeProxy.Proxy(a5));
                eargs.DefineMember("5").Assign(TypeProxy.Proxy(a6));
                eargs.DefineMember("6").Assign(TypeProxy.Proxy(a7));
                eargs.DefineMember("7").Assign(TypeProxy.Proxy(a8));
                eargs.DefineMember("8").Assign(TypeProxy.Proxy(a9));
                eargs.DefineMember("9").Assign(TypeProxy.Proxy(a10));
                eargs.DefineMember("10").Assign(TypeProxy.Proxy(a11));
                eargs.DefineMember("11").Assign(TypeProxy.Proxy(a12));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(13);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                eargs.DefineMember("2").Assign(TypeProxy.Proxy(a3));
                eargs.DefineMember("3").Assign(TypeProxy.Proxy(a4));
                eargs.DefineMember("4").Assign(TypeProxy.Proxy(a5));
                eargs.DefineMember("5").Assign(TypeProxy.Proxy(a6));
                eargs.DefineMember("6").Assign(TypeProxy.Proxy(a7));
                eargs.DefineMember("7").Assign(TypeProxy.Proxy(a8));
                eargs.DefineMember("8").Assign(TypeProxy.Proxy(a9));
                eargs.DefineMember("9").Assign(TypeProxy.Proxy(a10));
                eargs.DefineMember("10").Assign(TypeProxy.Proxy(a11));
                eargs.DefineMember("11").Assign(TypeProxy.Proxy(a12));
                eargs.DefineMember("12").Assign(TypeProxy.Proxy(a13));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(14);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                eargs.DefineMember("2").Assign(TypeProxy.Proxy(a3));
                eargs.DefineMember("3").Assign(TypeProxy.Proxy(a4));
                eargs.DefineMember("4").Assign(TypeProxy.Proxy(a5));
                eargs.DefineMember("5").Assign(TypeProxy.Proxy(a6));
                eargs.DefineMember("6").Assign(TypeProxy.Proxy(a7));
                eargs.DefineMember("7").Assign(TypeProxy.Proxy(a8));
                eargs.DefineMember("8").Assign(TypeProxy.Proxy(a9));
                eargs.DefineMember("9").Assign(TypeProxy.Proxy(a10));
                eargs.DefineMember("10").Assign(TypeProxy.Proxy(a11));
                eargs.DefineMember("11").Assign(TypeProxy.Proxy(a12));
                eargs.DefineMember("12").Assign(TypeProxy.Proxy(a13));
                eargs.DefineMember("13").Assign(TypeProxy.Proxy(a14));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(15);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                eargs.DefineMember("2").Assign(TypeProxy.Proxy(a3));
                eargs.DefineMember("3").Assign(TypeProxy.Proxy(a4));
                eargs.DefineMember("4").Assign(TypeProxy.Proxy(a5));
                eargs.DefineMember("5").Assign(TypeProxy.Proxy(a6));
                eargs.DefineMember("6").Assign(TypeProxy.Proxy(a7));
                eargs.DefineMember("7").Assign(TypeProxy.Proxy(a8));
                eargs.DefineMember("8").Assign(TypeProxy.Proxy(a9));
                eargs.DefineMember("9").Assign(TypeProxy.Proxy(a10));
                eargs.DefineMember("10").Assign(TypeProxy.Proxy(a11));
                eargs.DefineMember("11").Assign(TypeProxy.Proxy(a12));
                eargs.DefineMember("12").Assign(TypeProxy.Proxy(a13));
                eargs.DefineMember("13").Assign(TypeProxy.Proxy(a14));
                eargs.DefineMember("14").Assign(TypeProxy.Proxy(a15));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15, T16 a16)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(16);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                eargs.DefineMember("2").Assign(TypeProxy.Proxy(a3));
                eargs.DefineMember("3").Assign(TypeProxy.Proxy(a4));
                eargs.DefineMember("4").Assign(TypeProxy.Proxy(a5));
                eargs.DefineMember("5").Assign(TypeProxy.Proxy(a6));
                eargs.DefineMember("6").Assign(TypeProxy.Proxy(a7));
                eargs.DefineMember("7").Assign(TypeProxy.Proxy(a8));
                eargs.DefineMember("8").Assign(TypeProxy.Proxy(a9));
                eargs.DefineMember("9").Assign(TypeProxy.Proxy(a10));
                eargs.DefineMember("10").Assign(TypeProxy.Proxy(a11));
                eargs.DefineMember("11").Assign(TypeProxy.Proxy(a12));
                eargs.DefineMember("12").Assign(TypeProxy.Proxy(a13));
                eargs.DefineMember("13").Assign(TypeProxy.Proxy(a14));
                eargs.DefineMember("14").Assign(TypeProxy.Proxy(a15));
                eargs.DefineMember("15").Assign(TypeProxy.Proxy(a16));
                return (RT)function.Invoke(eargs).Value;
            }

            public void Invoke()
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(0);
                function.Invoke(eargs);
            }

            public void Invoke<T1>(T1 a1)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(2);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2>(T1 a1, T2 a2)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(2);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3>(T1 a1, T2 a2, T3 a3)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(3);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                eargs.DefineMember("2").Assign(TypeProxy.Proxy(a3));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4>(T1 a1, T2 a2, T3 a3, T4 a4)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(4);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                eargs.DefineMember("2").Assign(TypeProxy.Proxy(a3));
                eargs.DefineMember("3").Assign(TypeProxy.Proxy(a4));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(5);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                eargs.DefineMember("2").Assign(TypeProxy.Proxy(a3));
                eargs.DefineMember("3").Assign(TypeProxy.Proxy(a4));
                eargs.DefineMember("4").Assign(TypeProxy.Proxy(a5));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(6);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                eargs.DefineMember("2").Assign(TypeProxy.Proxy(a3));
                eargs.DefineMember("3").Assign(TypeProxy.Proxy(a4));
                eargs.DefineMember("4").Assign(TypeProxy.Proxy(a5));
                eargs.DefineMember("5").Assign(TypeProxy.Proxy(a6));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(7);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                eargs.DefineMember("2").Assign(TypeProxy.Proxy(a3));
                eargs.DefineMember("3").Assign(TypeProxy.Proxy(a4));
                eargs.DefineMember("4").Assign(TypeProxy.Proxy(a5));
                eargs.DefineMember("5").Assign(TypeProxy.Proxy(a6));
                eargs.DefineMember("6").Assign(TypeProxy.Proxy(a7));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(8);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                eargs.DefineMember("2").Assign(TypeProxy.Proxy(a3));
                eargs.DefineMember("3").Assign(TypeProxy.Proxy(a4));
                eargs.DefineMember("4").Assign(TypeProxy.Proxy(a5));
                eargs.DefineMember("5").Assign(TypeProxy.Proxy(a6));
                eargs.DefineMember("6").Assign(TypeProxy.Proxy(a7));
                eargs.DefineMember("7").Assign(TypeProxy.Proxy(a8));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(9);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                eargs.DefineMember("2").Assign(TypeProxy.Proxy(a3));
                eargs.DefineMember("3").Assign(TypeProxy.Proxy(a4));
                eargs.DefineMember("4").Assign(TypeProxy.Proxy(a5));
                eargs.DefineMember("5").Assign(TypeProxy.Proxy(a6));
                eargs.DefineMember("6").Assign(TypeProxy.Proxy(a7));
                eargs.DefineMember("7").Assign(TypeProxy.Proxy(a8));
                eargs.DefineMember("8").Assign(TypeProxy.Proxy(a9));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(10);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                eargs.DefineMember("2").Assign(TypeProxy.Proxy(a3));
                eargs.DefineMember("3").Assign(TypeProxy.Proxy(a4));
                eargs.DefineMember("4").Assign(TypeProxy.Proxy(a5));
                eargs.DefineMember("5").Assign(TypeProxy.Proxy(a6));
                eargs.DefineMember("6").Assign(TypeProxy.Proxy(a7));
                eargs.DefineMember("7").Assign(TypeProxy.Proxy(a8));
                eargs.DefineMember("8").Assign(TypeProxy.Proxy(a9));
                eargs.DefineMember("9").Assign(TypeProxy.Proxy(a10));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(11);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                eargs.DefineMember("2").Assign(TypeProxy.Proxy(a3));
                eargs.DefineMember("3").Assign(TypeProxy.Proxy(a4));
                eargs.DefineMember("4").Assign(TypeProxy.Proxy(a5));
                eargs.DefineMember("5").Assign(TypeProxy.Proxy(a6));
                eargs.DefineMember("6").Assign(TypeProxy.Proxy(a7));
                eargs.DefineMember("7").Assign(TypeProxy.Proxy(a8));
                eargs.DefineMember("8").Assign(TypeProxy.Proxy(a9));
                eargs.DefineMember("9").Assign(TypeProxy.Proxy(a10));
                eargs.DefineMember("10").Assign(TypeProxy.Proxy(a11));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(12);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                eargs.DefineMember("2").Assign(TypeProxy.Proxy(a3));
                eargs.DefineMember("3").Assign(TypeProxy.Proxy(a4));
                eargs.DefineMember("4").Assign(TypeProxy.Proxy(a5));
                eargs.DefineMember("5").Assign(TypeProxy.Proxy(a6));
                eargs.DefineMember("6").Assign(TypeProxy.Proxy(a7));
                eargs.DefineMember("7").Assign(TypeProxy.Proxy(a8));
                eargs.DefineMember("8").Assign(TypeProxy.Proxy(a9));
                eargs.DefineMember("9").Assign(TypeProxy.Proxy(a10));
                eargs.DefineMember("10").Assign(TypeProxy.Proxy(a11));
                eargs.DefineMember("11").Assign(TypeProxy.Proxy(a12));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(13);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                eargs.DefineMember("2").Assign(TypeProxy.Proxy(a3));
                eargs.DefineMember("3").Assign(TypeProxy.Proxy(a4));
                eargs.DefineMember("4").Assign(TypeProxy.Proxy(a5));
                eargs.DefineMember("5").Assign(TypeProxy.Proxy(a6));
                eargs.DefineMember("6").Assign(TypeProxy.Proxy(a7));
                eargs.DefineMember("7").Assign(TypeProxy.Proxy(a8));
                eargs.DefineMember("8").Assign(TypeProxy.Proxy(a9));
                eargs.DefineMember("9").Assign(TypeProxy.Proxy(a10));
                eargs.DefineMember("10").Assign(TypeProxy.Proxy(a11));
                eargs.DefineMember("11").Assign(TypeProxy.Proxy(a12));
                eargs.DefineMember("12").Assign(TypeProxy.Proxy(a13));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(14);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                eargs.DefineMember("2").Assign(TypeProxy.Proxy(a3));
                eargs.DefineMember("3").Assign(TypeProxy.Proxy(a4));
                eargs.DefineMember("4").Assign(TypeProxy.Proxy(a5));
                eargs.DefineMember("5").Assign(TypeProxy.Proxy(a6));
                eargs.DefineMember("6").Assign(TypeProxy.Proxy(a7));
                eargs.DefineMember("7").Assign(TypeProxy.Proxy(a8));
                eargs.DefineMember("8").Assign(TypeProxy.Proxy(a9));
                eargs.DefineMember("9").Assign(TypeProxy.Proxy(a10));
                eargs.DefineMember("10").Assign(TypeProxy.Proxy(a11));
                eargs.DefineMember("11").Assign(TypeProxy.Proxy(a12));
                eargs.DefineMember("12").Assign(TypeProxy.Proxy(a13));
                eargs.DefineMember("13").Assign(TypeProxy.Proxy(a14));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(15);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                eargs.DefineMember("2").Assign(TypeProxy.Proxy(a3));
                eargs.DefineMember("3").Assign(TypeProxy.Proxy(a4));
                eargs.DefineMember("4").Assign(TypeProxy.Proxy(a5));
                eargs.DefineMember("5").Assign(TypeProxy.Proxy(a6));
                eargs.DefineMember("6").Assign(TypeProxy.Proxy(a7));
                eargs.DefineMember("7").Assign(TypeProxy.Proxy(a8));
                eargs.DefineMember("8").Assign(TypeProxy.Proxy(a9));
                eargs.DefineMember("9").Assign(TypeProxy.Proxy(a10));
                eargs.DefineMember("10").Assign(TypeProxy.Proxy(a11));
                eargs.DefineMember("11").Assign(TypeProxy.Proxy(a12));
                eargs.DefineMember("12").Assign(TypeProxy.Proxy(a13));
                eargs.DefineMember("13").Assign(TypeProxy.Proxy(a14));
                eargs.DefineMember("14").Assign(TypeProxy.Proxy(a15));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15, T16 a16)
            {
                var eargs = new JSObject();
                eargs.oValue = Arguments.Instance;
                eargs.valueType = JSObjectType.Object;
                eargs.DefineMember("length").Assign(16);
                eargs.DefineMember("0").Assign(TypeProxy.Proxy(a1));
                eargs.DefineMember("1").Assign(TypeProxy.Proxy(a2));
                eargs.DefineMember("2").Assign(TypeProxy.Proxy(a3));
                eargs.DefineMember("3").Assign(TypeProxy.Proxy(a4));
                eargs.DefineMember("4").Assign(TypeProxy.Proxy(a5));
                eargs.DefineMember("5").Assign(TypeProxy.Proxy(a6));
                eargs.DefineMember("6").Assign(TypeProxy.Proxy(a7));
                eargs.DefineMember("7").Assign(TypeProxy.Proxy(a8));
                eargs.DefineMember("8").Assign(TypeProxy.Proxy(a9));
                eargs.DefineMember("9").Assign(TypeProxy.Proxy(a10));
                eargs.DefineMember("10").Assign(TypeProxy.Proxy(a11));
                eargs.DefineMember("11").Assign(TypeProxy.Proxy(a12));
                eargs.DefineMember("12").Assign(TypeProxy.Proxy(a13));
                eargs.DefineMember("13").Assign(TypeProxy.Proxy(a14));
                eargs.DefineMember("14").Assign(TypeProxy.Proxy(a15));
                eargs.DefineMember("15").Assign(TypeProxy.Proxy(a16));
                function.Invoke(eargs);
            }

        }

        [Hidden]
        public object MakeDelegate(Type delegateType)
        {
            var del = delegateType.GetMethod("Invoke");
            var prms = del.GetParameters();
            if (prms.Length <= 16)
            {
                var invokes = typeof(_DelegateWraper).GetMember("Invoke");
                if (del.ReturnType != typeof(void))
                {
                    Type[] argtypes = new Type[prms.Length + 1];
                    for (int i = 0; i < prms.Length; i++)
                        argtypes[i + 1] = prms[i].ParameterType;
                    argtypes[0] = del.ReturnType;
                    var instance = new _DelegateWraper(this);
                    var method = ((System.Reflection.MethodInfo)invokes[prms.Length]).MakeGenericMethod(argtypes);
                    return Delegate.CreateDelegate(delegateType, instance, method);
                }
                else
                {
                    Type[] argtypes = new Type[prms.Length];
                    for (int i = 0; i < prms.Length; i++)
                        argtypes[i] = prms[i].ParameterType;
                    var instance = new _DelegateWraper(this);
                    var method = ((System.Reflection.MethodInfo)invokes[17 + prms.Length]).MakeGenericMethod(argtypes);
                    return Delegate.CreateDelegate(delegateType, instance, method);
                }
            }
            else
                throw new ArgumentException("Parameters count must be no more 16.");
        }

        [Hidden]
        [CLSCompliant(false)]
        internal protected readonly Context context;
        [Hidden]
        internal protected JSObject prototypeField;
        [Hidden]
        public Context Context { get { return context; } }
        [Hidden]
        private string[] argumentsNames;
        [Hidden]
        private Statements.CodeBlock body;
        private string name;
        [Hidden]
        public virtual string Name
        {
            get { return name; }
        }
        private FunctionType type;
        [Hidden]
        public virtual FunctionType Type
        {
            get { return type; }
        }

        #region Runtime
        [Hidden]
        private JSObject _arguments;
        /// <summary>
        /// Объект, содержащий параметры вызова функции либо null если в данный момент функция не выполняется.
        /// </summary>
        [DoNotEnumerate]
        public JSObject arguments
        {
            [Modules.Hidden]
            get
            {
                return _arguments;
            }
        }
        internal Number _length = null;
        #endregion

        public Function()
        {
            context = Context.CurrentContext ?? Context.globalContext;
            body = new Statements.CodeBlock(new Statement[0], false);
            argumentsNames = new string[0];
            name = "";
            valueType = JSObjectType.Function;
        }

        public Function(JSObject args)
        {
            context = Context.CurrentContext ?? Context.globalContext;
            var index = 0;
            int len = args.GetMember("length").iValue - 1;
            var argn = "";
            for (int i = 0; i < len; i++)
                argn += args.GetMember(i < 16 ? Tools.NumString[i] : i.ToString(CultureInfo.InvariantCulture)) + (i + 1 < len ? "," : "");
            string code = "function(" + argn + "){" + args.GetMember(len < 16 ? Tools.NumString[len] : len.ToString(CultureInfo.InvariantCulture)) + "}";
            var fs = NiL.JS.Statements.FunctionStatement.Parse(new ParsingState(code, code), ref index);
            if (fs.IsParsed)
            {
                Parser.Optimize(ref fs.Statement, new Dictionary<string, VariableDescriptor>());
                var func = fs.Statement.Invoke(context) as Function;
                body = func.body;
                argumentsNames = func.argumentsNames;
            }
        }

        internal Function(Context context, Statements.CodeBlock body, string[] argumentsNames, string name, FunctionType type)
        {
            this.context = context;
            this.argumentsNames = argumentsNames;
            this.body = body;
            this.name = name;
            this.type = type;
            valueType = JSObjectType.Function;
        }

        [Modules.DoNotEnumerate]
        [Modules.DoNotDelete]
        public virtual JSObject length
        {
            [Modules.Hidden]
            get
            {
                if (_length == null)
                    _length = new Number(0) { attributes = JSObjectAttributes.ReadOnly | JSObjectAttributes.DoNotDelete | JSObjectAttributes.DoNotEnum };
                _length.iValue = argumentsNames.Length;
                return _length;
            }
        }

        [Hidden]
        public virtual JSObject Invoke(JSObject thisOverride, JSObject args)
        {
            var oldargs = _arguments;
            Context internalContext = new Context(context);
            try
            {
                var thisBind = thisOverride;
                if (!body.strict)
                {
                    if (thisBind != null)
                    {
                        if (thisBind.valueType > JSObjectType.Undefined && thisBind.valueType < JSObjectType.Object)
                        {
                            thisBind = new JSObject(false)
                            {
                                valueType = JSObjectType.Object,
                                oValue = thisBind,
                                attributes = JSObjectAttributes.DoNotEnum | JSObjectAttributes.DoNotDelete | JSObjectAttributes.Immutable,
                                prototype = thisBind.prototype ?? (thisBind.valueType <= JSObjectType.Undefined ? thisBind.prototype : thisBind.GetMember("__proto__"))
                            };
                        }
                        else if (thisBind.valueType <= JSObjectType.Undefined || thisBind.oValue == null)
                        {
                            var thc = context;
                            while (thc.prototype != Context.globalContext && !(thc.thisBind is ThisObject))
                                thc = thc.prototype;
                            thisBind = thc.thisBind ?? thc.GetVariable("this");
                        }
                        internalContext.thisBind = thisBind;
                    }
                }
                else
                    internalContext.thisBind = thisBind ?? undefined;
                if (args == null)
                {
                    args = new JSObject(true) { valueType = JSObjectType.Object };
                    args.oValue = args;
                    args.DefineMember("length").Assign(0);
                }
                if (body.strict)
                {
                    _arguments = strictModeArgumentsPropertyDammy;
                    args.DefineMember("callee").Assign(strictModeArgumentsPropertyDammy);
                }
                else
                {
                    _arguments = args;
                    var callee = args.DefineMember("callee");
                    callee.Assign(this);
                    callee.attributes |= JSObjectAttributes.DoNotEnum;
                }
                internalContext.fields["arguments"] = args;
                if (type == FunctionType.Function && name != null && Parser.ValidateName(name))
                    internalContext.DefineVariable(name).Assign(this);
                int i = 0;
                JSObject argsLength = args.GetMember("length");
                if (argsLength.valueType == JSObjectType.Property)
                    argsLength = (argsLength.oValue as Function[])[1].Invoke(args, null);
                int min = System.Math.Min(argsLength.iValue, argumentsNames.Length);
                for (; i < min; i++)
                    internalContext.fields[argumentsNames[i]] = args.GetMember(i < 16 ? Tools.NumString[i] : i.ToString(CultureInfo.InvariantCulture));
                for (; i < argumentsNames.Length; i++)
                    internalContext.fields[argumentsNames[i]] = new JSObject();

                internalContext.Run();
                body.Invoke(internalContext);
                return internalContext.abortInfo;
            }
            finally
            {
                internalContext.Stop();
                _arguments = oldargs;
            }
        }

        [Hidden]
        public JSObject Invoke(JSObject args)
        {
            return Invoke(undefined, args);
        }

        [Hidden]
        internal protected override JSObject GetMember(string name, bool create, bool own)
        {
            if (name == "prototype")
            {
                if (prototypeField == null)
                {
                    prototypeField = new JSObject(true)
                    {
                        valueType = JSObjectType.Object,
                        prototype = JSObject.GlobalPrototype,
                        attributes = JSObjectAttributes.DoNotDelete | JSObjectAttributes.DoNotEnum
                    };
                    prototypeField.oValue = prototypeField;
                    var ctor = prototypeField.GetMember("constructor", true, true);
                    ctor.attributes = JSObjectAttributes.DoNotDelete | JSObjectAttributes.DoNotEnum;
                    ctor.Assign(this);
                }
                return prototypeField;
            }
            if (prototype == null)
                prototype = TypeProxy.GetPrototype(this.GetType());
            return DefaultFieldGetter(name, create, own);
        }

        [Hidden]
        public override string ToString()
        {
            var res = type.ToString().ToLowerInvariant() + " " + Name + "(";
            if (argumentsNames != null)
                for (int i = 0; i < argumentsNames.Length; )
                    res += argumentsNames[i] + (++i < argumentsNames.Length ? "," : "");
            res += ")" + ((object)body ?? "{ [native code] }").ToString();
            return res;
        }

        public virtual JSObject call(JSObject args)
        {
            var newThis = args.GetMember("0");
            var prmlen = args.GetMember("length").iValue - 1;
            if (prmlen >= 0)
            {
                for (int i = 0; i < prmlen; i++)
                    args.fields[i < 16 ? Tools.NumString[i] : i.ToString()] = args.GetMember(i < 15 ? Tools.NumString[i + 1] : (i + 1).ToString(CultureInfo.InvariantCulture));
                args.fields.Remove(prmlen < 16 ? Tools.NumString[prmlen] : prmlen.ToString(CultureInfo.InvariantCulture));
            }
            return Invoke(newThis, args);
        }

        public virtual JSObject apply(JSObject args)
        {
            var newThis = args.GetMember("0");
            var iargs = args.GetMember("1");
            var lengthO = args.GetMember("length");
            var callee = args.DefineMember("callee");
            args.fields.Clear();
            var prmsC = 0;
            if (iargs != notExist)
            {
                var prmsCR = iargs.GetMember("length");
                prmsC = Tools.JSObjectToInt(prmsCR.valueType == JSObjectType.Property ? (prmsCR.oValue as Function[])[1].Invoke(iargs, null) : prmsCR);
                for (int i = 0; i < prmsC; i++)
                    args.fields[i < 16 ? Tools.NumString[i] : i.ToString(CultureInfo.InvariantCulture)] = iargs.GetMember(i < 16 ? Tools.NumString[i] : i.ToString(CultureInfo.InvariantCulture));
            }
            if (callee != notExist)
            {
                callee.oValue = this;
                args.fields["callee"] = callee;
            }
#if DEBUG
            Debug.Assert(lengthO.assignCallback == null);
#endif
            if (lengthO.assignCallback != null)
                lengthO.assignCallback = null;
            args.fields["length"] = lengthO;
            lengthO.valueType = JSObjectType.Int;
            lengthO.iValue = prmsC;
            return Invoke(newThis, args);
        }

        public virtual JSObject bind(JSObject args)
        {
            var prmsCR = args.GetMember("length");
            var prmsC = Tools.JSObjectToInt(prmsCR.valueType == JSObjectType.Property ? (prmsCR.oValue as Function[])[1].Invoke(args, null) : prmsCR);
            var strict = this.body.strict || Context.CurrentContext.strict;
            if (prmsC > 0 || strict)
            {
                var newThis = args.GetMember("0");
                if (newThis.valueType > JSObjectType.Undefined || strict)
                    return new ExternalFunction((context, bargs) =>
                    {
                        return Invoke(newThis, bargs);
                    });
            }
            return this;
        }

        public override IEnumerator<string> GetEnumerator()
        {
            return EmptyEnumerator;
        }
    }
}