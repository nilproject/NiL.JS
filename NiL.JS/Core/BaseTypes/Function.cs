using System;
using System.Collections.Generic;
using System.Text;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
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
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(0);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1>(T1 a1)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(2);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2>(T1 a1, T2 a2)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(2);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3>(T1 a1, T2 a2, T3 a3)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(3);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                eargs.GetField("2", false, true).Assign(TypeProxy.Proxy(a3));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4>(T1 a1, T2 a2, T3 a3, T4 a4)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(4);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                eargs.GetField("2", false, true).Assign(TypeProxy.Proxy(a3));
                eargs.GetField("3", false, true).Assign(TypeProxy.Proxy(a4));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(5);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                eargs.GetField("2", false, true).Assign(TypeProxy.Proxy(a3));
                eargs.GetField("3", false, true).Assign(TypeProxy.Proxy(a4));
                eargs.GetField("4", false, true).Assign(TypeProxy.Proxy(a5));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(6);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                eargs.GetField("2", false, true).Assign(TypeProxy.Proxy(a3));
                eargs.GetField("3", false, true).Assign(TypeProxy.Proxy(a4));
                eargs.GetField("4", false, true).Assign(TypeProxy.Proxy(a5));
                eargs.GetField("5", false, true).Assign(TypeProxy.Proxy(a6));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(7);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                eargs.GetField("2", false, true).Assign(TypeProxy.Proxy(a3));
                eargs.GetField("3", false, true).Assign(TypeProxy.Proxy(a4));
                eargs.GetField("4", false, true).Assign(TypeProxy.Proxy(a5));
                eargs.GetField("5", false, true).Assign(TypeProxy.Proxy(a6));
                eargs.GetField("6", false, true).Assign(TypeProxy.Proxy(a7));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(8);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                eargs.GetField("2", false, true).Assign(TypeProxy.Proxy(a3));
                eargs.GetField("3", false, true).Assign(TypeProxy.Proxy(a4));
                eargs.GetField("4", false, true).Assign(TypeProxy.Proxy(a5));
                eargs.GetField("5", false, true).Assign(TypeProxy.Proxy(a6));
                eargs.GetField("6", false, true).Assign(TypeProxy.Proxy(a7));
                eargs.GetField("7", false, true).Assign(TypeProxy.Proxy(a8));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(9);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                eargs.GetField("2", false, true).Assign(TypeProxy.Proxy(a3));
                eargs.GetField("3", false, true).Assign(TypeProxy.Proxy(a4));
                eargs.GetField("4", false, true).Assign(TypeProxy.Proxy(a5));
                eargs.GetField("5", false, true).Assign(TypeProxy.Proxy(a6));
                eargs.GetField("6", false, true).Assign(TypeProxy.Proxy(a7));
                eargs.GetField("7", false, true).Assign(TypeProxy.Proxy(a8));
                eargs.GetField("8", false, true).Assign(TypeProxy.Proxy(a9));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(10);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                eargs.GetField("2", false, true).Assign(TypeProxy.Proxy(a3));
                eargs.GetField("3", false, true).Assign(TypeProxy.Proxy(a4));
                eargs.GetField("4", false, true).Assign(TypeProxy.Proxy(a5));
                eargs.GetField("5", false, true).Assign(TypeProxy.Proxy(a6));
                eargs.GetField("6", false, true).Assign(TypeProxy.Proxy(a7));
                eargs.GetField("7", false, true).Assign(TypeProxy.Proxy(a8));
                eargs.GetField("8", false, true).Assign(TypeProxy.Proxy(a9));
                eargs.GetField("9", false, true).Assign(TypeProxy.Proxy(a10));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(11);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                eargs.GetField("2", false, true).Assign(TypeProxy.Proxy(a3));
                eargs.GetField("3", false, true).Assign(TypeProxy.Proxy(a4));
                eargs.GetField("4", false, true).Assign(TypeProxy.Proxy(a5));
                eargs.GetField("5", false, true).Assign(TypeProxy.Proxy(a6));
                eargs.GetField("6", false, true).Assign(TypeProxy.Proxy(a7));
                eargs.GetField("7", false, true).Assign(TypeProxy.Proxy(a8));
                eargs.GetField("8", false, true).Assign(TypeProxy.Proxy(a9));
                eargs.GetField("9", false, true).Assign(TypeProxy.Proxy(a10));
                eargs.GetField("10", false, true).Assign(TypeProxy.Proxy(a11));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(12);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                eargs.GetField("2", false, true).Assign(TypeProxy.Proxy(a3));
                eargs.GetField("3", false, true).Assign(TypeProxy.Proxy(a4));
                eargs.GetField("4", false, true).Assign(TypeProxy.Proxy(a5));
                eargs.GetField("5", false, true).Assign(TypeProxy.Proxy(a6));
                eargs.GetField("6", false, true).Assign(TypeProxy.Proxy(a7));
                eargs.GetField("7", false, true).Assign(TypeProxy.Proxy(a8));
                eargs.GetField("8", false, true).Assign(TypeProxy.Proxy(a9));
                eargs.GetField("9", false, true).Assign(TypeProxy.Proxy(a10));
                eargs.GetField("10", false, true).Assign(TypeProxy.Proxy(a11));
                eargs.GetField("11", false, true).Assign(TypeProxy.Proxy(a12));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(13);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                eargs.GetField("2", false, true).Assign(TypeProxy.Proxy(a3));
                eargs.GetField("3", false, true).Assign(TypeProxy.Proxy(a4));
                eargs.GetField("4", false, true).Assign(TypeProxy.Proxy(a5));
                eargs.GetField("5", false, true).Assign(TypeProxy.Proxy(a6));
                eargs.GetField("6", false, true).Assign(TypeProxy.Proxy(a7));
                eargs.GetField("7", false, true).Assign(TypeProxy.Proxy(a8));
                eargs.GetField("8", false, true).Assign(TypeProxy.Proxy(a9));
                eargs.GetField("9", false, true).Assign(TypeProxy.Proxy(a10));
                eargs.GetField("10", false, true).Assign(TypeProxy.Proxy(a11));
                eargs.GetField("11", false, true).Assign(TypeProxy.Proxy(a12));
                eargs.GetField("12", false, true).Assign(TypeProxy.Proxy(a13));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(14);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                eargs.GetField("2", false, true).Assign(TypeProxy.Proxy(a3));
                eargs.GetField("3", false, true).Assign(TypeProxy.Proxy(a4));
                eargs.GetField("4", false, true).Assign(TypeProxy.Proxy(a5));
                eargs.GetField("5", false, true).Assign(TypeProxy.Proxy(a6));
                eargs.GetField("6", false, true).Assign(TypeProxy.Proxy(a7));
                eargs.GetField("7", false, true).Assign(TypeProxy.Proxy(a8));
                eargs.GetField("8", false, true).Assign(TypeProxy.Proxy(a9));
                eargs.GetField("9", false, true).Assign(TypeProxy.Proxy(a10));
                eargs.GetField("10", false, true).Assign(TypeProxy.Proxy(a11));
                eargs.GetField("11", false, true).Assign(TypeProxy.Proxy(a12));
                eargs.GetField("12", false, true).Assign(TypeProxy.Proxy(a13));
                eargs.GetField("13", false, true).Assign(TypeProxy.Proxy(a14));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(15);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                eargs.GetField("2", false, true).Assign(TypeProxy.Proxy(a3));
                eargs.GetField("3", false, true).Assign(TypeProxy.Proxy(a4));
                eargs.GetField("4", false, true).Assign(TypeProxy.Proxy(a5));
                eargs.GetField("5", false, true).Assign(TypeProxy.Proxy(a6));
                eargs.GetField("6", false, true).Assign(TypeProxy.Proxy(a7));
                eargs.GetField("7", false, true).Assign(TypeProxy.Proxy(a8));
                eargs.GetField("8", false, true).Assign(TypeProxy.Proxy(a9));
                eargs.GetField("9", false, true).Assign(TypeProxy.Proxy(a10));
                eargs.GetField("10", false, true).Assign(TypeProxy.Proxy(a11));
                eargs.GetField("11", false, true).Assign(TypeProxy.Proxy(a12));
                eargs.GetField("12", false, true).Assign(TypeProxy.Proxy(a13));
                eargs.GetField("13", false, true).Assign(TypeProxy.Proxy(a14));
                eargs.GetField("14", false, true).Assign(TypeProxy.Proxy(a15));
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15, T16 a16)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(16);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                eargs.GetField("2", false, true).Assign(TypeProxy.Proxy(a3));
                eargs.GetField("3", false, true).Assign(TypeProxy.Proxy(a4));
                eargs.GetField("4", false, true).Assign(TypeProxy.Proxy(a5));
                eargs.GetField("5", false, true).Assign(TypeProxy.Proxy(a6));
                eargs.GetField("6", false, true).Assign(TypeProxy.Proxy(a7));
                eargs.GetField("7", false, true).Assign(TypeProxy.Proxy(a8));
                eargs.GetField("8", false, true).Assign(TypeProxy.Proxy(a9));
                eargs.GetField("9", false, true).Assign(TypeProxy.Proxy(a10));
                eargs.GetField("10", false, true).Assign(TypeProxy.Proxy(a11));
                eargs.GetField("11", false, true).Assign(TypeProxy.Proxy(a12));
                eargs.GetField("12", false, true).Assign(TypeProxy.Proxy(a13));
                eargs.GetField("13", false, true).Assign(TypeProxy.Proxy(a14));
                eargs.GetField("14", false, true).Assign(TypeProxy.Proxy(a15));
                eargs.GetField("15", false, true).Assign(TypeProxy.Proxy(a16));
                return (RT)function.Invoke(eargs).Value;
            }

            public void Invoke()
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(0);
                function.Invoke(eargs);
            }

            public void Invoke<T1>(T1 a1)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(2);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2>(T1 a1, T2 a2)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(2);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3>(T1 a1, T2 a2, T3 a3)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(3);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                eargs.GetField("2", false, true).Assign(TypeProxy.Proxy(a3));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4>(T1 a1, T2 a2, T3 a3, T4 a4)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(4);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                eargs.GetField("2", false, true).Assign(TypeProxy.Proxy(a3));
                eargs.GetField("3", false, true).Assign(TypeProxy.Proxy(a4));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(5);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                eargs.GetField("2", false, true).Assign(TypeProxy.Proxy(a3));
                eargs.GetField("3", false, true).Assign(TypeProxy.Proxy(a4));
                eargs.GetField("4", false, true).Assign(TypeProxy.Proxy(a5));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(6);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                eargs.GetField("2", false, true).Assign(TypeProxy.Proxy(a3));
                eargs.GetField("3", false, true).Assign(TypeProxy.Proxy(a4));
                eargs.GetField("4", false, true).Assign(TypeProxy.Proxy(a5));
                eargs.GetField("5", false, true).Assign(TypeProxy.Proxy(a6));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(7);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                eargs.GetField("2", false, true).Assign(TypeProxy.Proxy(a3));
                eargs.GetField("3", false, true).Assign(TypeProxy.Proxy(a4));
                eargs.GetField("4", false, true).Assign(TypeProxy.Proxy(a5));
                eargs.GetField("5", false, true).Assign(TypeProxy.Proxy(a6));
                eargs.GetField("6", false, true).Assign(TypeProxy.Proxy(a7));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(8);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                eargs.GetField("2", false, true).Assign(TypeProxy.Proxy(a3));
                eargs.GetField("3", false, true).Assign(TypeProxy.Proxy(a4));
                eargs.GetField("4", false, true).Assign(TypeProxy.Proxy(a5));
                eargs.GetField("5", false, true).Assign(TypeProxy.Proxy(a6));
                eargs.GetField("6", false, true).Assign(TypeProxy.Proxy(a7));
                eargs.GetField("7", false, true).Assign(TypeProxy.Proxy(a8));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(9);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                eargs.GetField("2", false, true).Assign(TypeProxy.Proxy(a3));
                eargs.GetField("3", false, true).Assign(TypeProxy.Proxy(a4));
                eargs.GetField("4", false, true).Assign(TypeProxy.Proxy(a5));
                eargs.GetField("5", false, true).Assign(TypeProxy.Proxy(a6));
                eargs.GetField("6", false, true).Assign(TypeProxy.Proxy(a7));
                eargs.GetField("7", false, true).Assign(TypeProxy.Proxy(a8));
                eargs.GetField("8", false, true).Assign(TypeProxy.Proxy(a9));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(10);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                eargs.GetField("2", false, true).Assign(TypeProxy.Proxy(a3));
                eargs.GetField("3", false, true).Assign(TypeProxy.Proxy(a4));
                eargs.GetField("4", false, true).Assign(TypeProxy.Proxy(a5));
                eargs.GetField("5", false, true).Assign(TypeProxy.Proxy(a6));
                eargs.GetField("6", false, true).Assign(TypeProxy.Proxy(a7));
                eargs.GetField("7", false, true).Assign(TypeProxy.Proxy(a8));
                eargs.GetField("8", false, true).Assign(TypeProxy.Proxy(a9));
                eargs.GetField("9", false, true).Assign(TypeProxy.Proxy(a10));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(11);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                eargs.GetField("2", false, true).Assign(TypeProxy.Proxy(a3));
                eargs.GetField("3", false, true).Assign(TypeProxy.Proxy(a4));
                eargs.GetField("4", false, true).Assign(TypeProxy.Proxy(a5));
                eargs.GetField("5", false, true).Assign(TypeProxy.Proxy(a6));
                eargs.GetField("6", false, true).Assign(TypeProxy.Proxy(a7));
                eargs.GetField("7", false, true).Assign(TypeProxy.Proxy(a8));
                eargs.GetField("8", false, true).Assign(TypeProxy.Proxy(a9));
                eargs.GetField("9", false, true).Assign(TypeProxy.Proxy(a10));
                eargs.GetField("10", false, true).Assign(TypeProxy.Proxy(a11));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(12);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                eargs.GetField("2", false, true).Assign(TypeProxy.Proxy(a3));
                eargs.GetField("3", false, true).Assign(TypeProxy.Proxy(a4));
                eargs.GetField("4", false, true).Assign(TypeProxy.Proxy(a5));
                eargs.GetField("5", false, true).Assign(TypeProxy.Proxy(a6));
                eargs.GetField("6", false, true).Assign(TypeProxy.Proxy(a7));
                eargs.GetField("7", false, true).Assign(TypeProxy.Proxy(a8));
                eargs.GetField("8", false, true).Assign(TypeProxy.Proxy(a9));
                eargs.GetField("9", false, true).Assign(TypeProxy.Proxy(a10));
                eargs.GetField("10", false, true).Assign(TypeProxy.Proxy(a11));
                eargs.GetField("11", false, true).Assign(TypeProxy.Proxy(a12));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(13);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                eargs.GetField("2", false, true).Assign(TypeProxy.Proxy(a3));
                eargs.GetField("3", false, true).Assign(TypeProxy.Proxy(a4));
                eargs.GetField("4", false, true).Assign(TypeProxy.Proxy(a5));
                eargs.GetField("5", false, true).Assign(TypeProxy.Proxy(a6));
                eargs.GetField("6", false, true).Assign(TypeProxy.Proxy(a7));
                eargs.GetField("7", false, true).Assign(TypeProxy.Proxy(a8));
                eargs.GetField("8", false, true).Assign(TypeProxy.Proxy(a9));
                eargs.GetField("9", false, true).Assign(TypeProxy.Proxy(a10));
                eargs.GetField("10", false, true).Assign(TypeProxy.Proxy(a11));
                eargs.GetField("11", false, true).Assign(TypeProxy.Proxy(a12));
                eargs.GetField("12", false, true).Assign(TypeProxy.Proxy(a13));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(14);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                eargs.GetField("2", false, true).Assign(TypeProxy.Proxy(a3));
                eargs.GetField("3", false, true).Assign(TypeProxy.Proxy(a4));
                eargs.GetField("4", false, true).Assign(TypeProxy.Proxy(a5));
                eargs.GetField("5", false, true).Assign(TypeProxy.Proxy(a6));
                eargs.GetField("6", false, true).Assign(TypeProxy.Proxy(a7));
                eargs.GetField("7", false, true).Assign(TypeProxy.Proxy(a8));
                eargs.GetField("8", false, true).Assign(TypeProxy.Proxy(a9));
                eargs.GetField("9", false, true).Assign(TypeProxy.Proxy(a10));
                eargs.GetField("10", false, true).Assign(TypeProxy.Proxy(a11));
                eargs.GetField("11", false, true).Assign(TypeProxy.Proxy(a12));
                eargs.GetField("12", false, true).Assign(TypeProxy.Proxy(a13));
                eargs.GetField("13", false, true).Assign(TypeProxy.Proxy(a14));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(15);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                eargs.GetField("2", false, true).Assign(TypeProxy.Proxy(a3));
                eargs.GetField("3", false, true).Assign(TypeProxy.Proxy(a4));
                eargs.GetField("4", false, true).Assign(TypeProxy.Proxy(a5));
                eargs.GetField("5", false, true).Assign(TypeProxy.Proxy(a6));
                eargs.GetField("6", false, true).Assign(TypeProxy.Proxy(a7));
                eargs.GetField("7", false, true).Assign(TypeProxy.Proxy(a8));
                eargs.GetField("8", false, true).Assign(TypeProxy.Proxy(a9));
                eargs.GetField("9", false, true).Assign(TypeProxy.Proxy(a10));
                eargs.GetField("10", false, true).Assign(TypeProxy.Proxy(a11));
                eargs.GetField("11", false, true).Assign(TypeProxy.Proxy(a12));
                eargs.GetField("12", false, true).Assign(TypeProxy.Proxy(a13));
                eargs.GetField("13", false, true).Assign(TypeProxy.Proxy(a14));
                eargs.GetField("14", false, true).Assign(TypeProxy.Proxy(a15));
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15, T16 a16)
            {
                var eargs = new JSObject();
                eargs.oValue = "[object Arguments]".Clone();
                eargs.ValueType = JSObjectType.Object;
                eargs.GetField("length", false, true).Assign(16);
                eargs.GetField("0", false, true).Assign(TypeProxy.Proxy(a1));
                eargs.GetField("1", false, true).Assign(TypeProxy.Proxy(a2));
                eargs.GetField("2", false, true).Assign(TypeProxy.Proxy(a3));
                eargs.GetField("3", false, true).Assign(TypeProxy.Proxy(a4));
                eargs.GetField("4", false, true).Assign(TypeProxy.Proxy(a5));
                eargs.GetField("5", false, true).Assign(TypeProxy.Proxy(a6));
                eargs.GetField("6", false, true).Assign(TypeProxy.Proxy(a7));
                eargs.GetField("7", false, true).Assign(TypeProxy.Proxy(a8));
                eargs.GetField("8", false, true).Assign(TypeProxy.Proxy(a9));
                eargs.GetField("9", false, true).Assign(TypeProxy.Proxy(a10));
                eargs.GetField("10", false, true).Assign(TypeProxy.Proxy(a11));
                eargs.GetField("11", false, true).Assign(TypeProxy.Proxy(a12));
                eargs.GetField("12", false, true).Assign(TypeProxy.Proxy(a13));
                eargs.GetField("13", false, true).Assign(TypeProxy.Proxy(a14));
                eargs.GetField("14", false, true).Assign(TypeProxy.Proxy(a15));
                eargs.GetField("15", false, true).Assign(TypeProxy.Proxy(a16));
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
                var temp = typeof(_DelegateWraper).GetMember("Invoke");
                if (del.ReturnType != typeof(void))
                {
                    Type[] argtypes = new Type[prms.Length + 1];
                    for (int i = 0; i < prms.Length; i++)
                        argtypes[i + 1] = prms[i].ParameterType;
                    argtypes[0] = del.ReturnType;
                    var instance = new _DelegateWraper(this);
                    var method = ((System.Reflection.MethodInfo)typeof(_DelegateWraper).GetMember("Invoke")[prms.Length]).MakeGenericMethod(argtypes);
                    return Delegate.CreateDelegate(delegateType, instance, method);
                }
                else
                {
                    Type[] argtypes = new Type[prms.Length];
                    for (int i = 0; i < prms.Length; i++)
                        argtypes[i] = prms[i].ParameterType;
                    var instance = new _DelegateWraper(this);
                    var method = ((System.Reflection.MethodInfo)typeof(_DelegateWraper).GetMember("Invoke")[17 + prms.Length]).MakeGenericMethod(argtypes);
                    return Delegate.CreateDelegate(delegateType, instance, method);
                }
            }
            else
                throw new ArgumentException("Parameters count must be no more 16.");
        }

        [Hidden]
        internal protected Context context;
        [Hidden]
        internal protected JSObject protorypeField;
        [Hidden]
        public Context Context { get { return context; } }
        [Hidden]
        private string[] argumentsNames;
        [Hidden]
        private Statements.CodeBlock body;
        [Hidden]
        public readonly string Name;

        #region Runtime
        [Hidden]
        private JSObject _arguments;
        [Hidden]
        public JSObject arguments
        {
            get
            {
                return _arguments;
            }
        }

        #endregion

        public Function()
        {
            context = Context.currentRootContext ?? Context.globalContext;
            body = new Statements.CodeBlock(new Statement[0], false);
            argumentsNames = new string[0];
            Name = "";
            ValueType = JSObjectType.Function;
        }

        public Function(JSObject args)
        {
            context = Context.currentRootContext ?? Context.globalContext;
            var index = 0;
            int len = args.GetField("length", true, false).iValue - 1;
            var argn = "";
            for (int i = 0; i < len; i++)
                argn += args.GetField(i.ToString(), true, false) + (i + 1 < len ? "," : "");
            var fs = NiL.JS.Statements.FunctionStatement.Parse(new ParsingState("function(" + argn + "){" + args.GetField(len.ToString(), true, false) + "}"), ref index);
            if (fs.IsParsed)
            {
                Parser.Optimize(ref fs.Statement, new Dictionary<string, Statement>());
                var func = fs.Statement.Invoke(context) as Function;
                body = func.body;
                argumentsNames = func.argumentsNames;
            }
        }

        internal Function(Context context, Statements.CodeBlock body, string[] argumentsNames, string name)
        {
            this.context = context;
            this.argumentsNames = argumentsNames;
            this.body = body;
            Name = name;
            ValueType = JSObjectType.Function;
        }

        internal Number _length = null;

        [Modules.DoNotDelete]
        public virtual JSObject length
        {
            get
            {
                if (_length == null)
                    _length = new Number(0) { attributes = ObjectAttributes.ReadOnly | ObjectAttributes.DontDelete | ObjectAttributes.DontEnum };
                _length.iValue = argumentsNames.Length;
                return _length;
            }
        }

        [Hidden]
        public virtual JSObject Invoke(JSObject args)
        {
            return Invoke(null as JSObject, args);
        }

        [Hidden]
        public virtual JSObject Invoke(Context contextOverride, JSObject args)
        {
            return Invoke(args);
        }
        
        [Hidden]
        public virtual JSObject Invoke(Context contextOverride, JSObject thisOverride, JSObject args)
        {
            return Invoke(thisOverride, args);
        }
        
        [Hidden]
        public virtual JSObject Invoke(JSObject thisOverride, JSObject args)
        {
            context.ValidateThreadID();
            var oldargs = _arguments;
            try
            {
                Context internalContext = new Context(context);
                var @this = thisOverride ?? context.thisBind;
                if (@this != null && @this.ValueType < JSObjectType.Object && !body.strict)
                {
                    @this = new JSObject(false)
                    {
                        ValueType = JSObjectType.Object,
                        oValue = @this,
                        attributes = ObjectAttributes.DontEnum | ObjectAttributes.DontDelete | ObjectAttributes.Immutable
                    };
                }
                internalContext.thisBind = @this;
                if (args == null)
                {
                    args = new JSObject(true) { ValueType = JSObjectType.Object };
                    args.oValue = args;
                    args.GetField("callee", false, true).Assign(this);
                    args.GetField("length", false, true).Assign(0);
                }
                _arguments = args;
                internalContext.InitField("arguments").Assign(_arguments);
                if (!string.IsNullOrEmpty(Name))
                    internalContext.InitField(Name).Assign(this);
                int i = 0;
                int min = System.Math.Min(args.GetField("length", true, false).iValue, argumentsNames.Length);
                for (; i < min; i++)
                    internalContext.InitField(argumentsNames[i]).Assign(args.GetField(i.ToString(), true, false));
                for (; i < argumentsNames.Length; i++)
                    internalContext.InitField(argumentsNames[i]).Assign(null);

                body.Invoke(internalContext);
                return internalContext.abortInfo;
            }
            finally
            {
                _arguments = oldargs;
            }
        }

        [Hidden]
        public override JSObject GetField(string name, bool fast, bool own)
        {
            if (name == "prototype")
            {
                if (protorypeField == null)
                {
                    protorypeField = new JSObject()
                    {
                        ValueType = JSObjectType.Object,
                        prototype = JSObject.GlobalPrototype,
                        attributes = ObjectAttributes.DontDelete | ObjectAttributes.DontEnum
                    };
                    protorypeField.oValue = protorypeField;
                    var ctor = protorypeField.GetField("constructor", false, true);
                    ctor.attributes = ObjectAttributes.DontDelete | ObjectAttributes.DontEnum;
                    ctor.Assign(this);
                }
                return protorypeField;
            }
            if (prototype == null)
                prototype = TypeProxy.GetPrototype(this.GetType());
            return DefaultFieldGetter(name, fast, own);
        }

        [Hidden]
        public override string ToString()
        {
            var res = "function " + Name + "(";
            if (argumentsNames != null)
                for (int i = 0; i < argumentsNames.Length; )
                    res += argumentsNames[i] + (++i < argumentsNames.Length ? "," : "");
            res += ")" + ((object)body ?? "{ [native code] }").ToString();
            return res;
        }

        public virtual JSObject call(JSObject args)
        {
            var newThis = args.GetField("0", true, false);
            var prmlen = --args.GetField("length", true, false).iValue;
            for (int i = 0; i < prmlen; i++)
                args.fields[i.ToString()] = args.GetField((i + 1).ToString(), true, false);
            args.fields.Remove(prmlen.ToString());
            if (newThis.ValueType < JSObjectType.Object || newThis.oValue != null)
                return Invoke(newThis, args);
            else
                return Invoke(Context.currentRootContext.thisBind ?? Context.currentRootContext.GetField("this"), args);
        }
    }
}