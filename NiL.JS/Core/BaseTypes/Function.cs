using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using NiL.JS.Core.Modules;
using NiL.JS.Statements;

namespace NiL.JS.Core.BaseTypes
{
    /// <summary>
    /// Возможные типы функции в контексте использования.
    /// </summary>
    [Serializable]
    public enum FunctionType
    {
        Function = 0,
        Get = 1,
        Set = 2,
        AnonymousFunction = 4
    }

    [Serializable]
    public class Function : JSObject
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
                var eargs = new Arguments();
                eargs.length = 0;
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1>(T1 a1)
            {
                var eargs = new Arguments();
                eargs.length = 2;
                eargs[0] = TypeProxy.Proxy(a1);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2>(T1 a1, T2 a2)
            {
                var eargs = new Arguments();
                eargs.length = 2;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3>(T1 a1, T2 a2, T3 a3)
            {
                var eargs = new Arguments();
                eargs.length = 3;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4>(T1 a1, T2 a2, T3 a3, T4 a4)
            {
                var eargs = new Arguments();
                eargs.length = 4;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5)
            {
                var eargs = new Arguments();
                eargs.length = 5;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6)
            {
                var eargs = new Arguments();
                eargs.length = 6;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7)
            {
                var eargs = new Arguments();
                eargs.length = 7;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8)
            {
                var eargs = new Arguments();
                eargs.length = 8;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9)
            {
                var eargs = new Arguments();
                eargs.length = 9;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10)
            {
                var eargs = new Arguments();
                eargs.length = 10;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                eargs[9] = TypeProxy.Proxy(a10);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11)
            {
                var eargs = new Arguments();
                eargs.length = 11;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                eargs[9] = TypeProxy.Proxy(a10);
                eargs[10] = TypeProxy.Proxy(a11);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12)
            {
                var eargs = new Arguments();
                eargs.length = 12;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                eargs[9] = TypeProxy.Proxy(a10);
                eargs[10] = TypeProxy.Proxy(a11);
                eargs[11] = TypeProxy.Proxy(a12);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13)
            {
                var eargs = new Arguments();
                eargs.length = 13;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                eargs[9] = TypeProxy.Proxy(a10);
                eargs[10] = TypeProxy.Proxy(a11);
                eargs[11] = TypeProxy.Proxy(a12);
                eargs[12] = TypeProxy.Proxy(a13);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14)
            {
                var eargs = new Arguments();
                eargs.length = 14;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                eargs[9] = TypeProxy.Proxy(a10);
                eargs[10] = TypeProxy.Proxy(a11);
                eargs[11] = TypeProxy.Proxy(a12);
                eargs[12] = TypeProxy.Proxy(a13);
                eargs[13] = TypeProxy.Proxy(a14);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15)
            {
                var eargs = new Arguments();
                eargs.length = 15;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                eargs[9] = TypeProxy.Proxy(a10);
                eargs[10] = TypeProxy.Proxy(a11);
                eargs[11] = TypeProxy.Proxy(a12);
                eargs[12] = TypeProxy.Proxy(a13);
                eargs[13] = TypeProxy.Proxy(a14);
                eargs[14] = TypeProxy.Proxy(a15);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15, T16 a16)
            {
                var eargs = new Arguments();
                eargs.length = 16;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                eargs[9] = TypeProxy.Proxy(a10);
                eargs[10] = TypeProxy.Proxy(a11);
                eargs[11] = TypeProxy.Proxy(a12);
                eargs[12] = TypeProxy.Proxy(a13);
                eargs[13] = TypeProxy.Proxy(a14);
                eargs[14] = TypeProxy.Proxy(a15);
                eargs[15] = TypeProxy.Proxy(a16);
                return (RT)function.Invoke(eargs).Value;
            }

            public void Invoke()
            {
                var eargs = new Arguments();
                eargs.length = 0;
                function.Invoke(eargs);
            }

            public void Invoke<T1>(T1 a1)
            {
                var eargs = new Arguments();


                eargs.length = 2;
                eargs[0] = TypeProxy.Proxy(a1);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2>(T1 a1, T2 a2)
            {
                var eargs = new Arguments();
                eargs.length = 2;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3>(T1 a1, T2 a2, T3 a3)
            {
                var eargs = new Arguments();
                eargs.length = 3;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4>(T1 a1, T2 a2, T3 a3, T4 a4)
            {
                var eargs = new Arguments();
                eargs.length = 4;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5)
            {
                var eargs = new Arguments();
                eargs.length = 5;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6)
            {
                var eargs = new Arguments();
                eargs.length = 6;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7)
            {
                var eargs = new Arguments();
                eargs.length = 7;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8)
            {
                var eargs = new Arguments();
                eargs.length = 8;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9)
            {
                var eargs = new Arguments();
                eargs.length = 9;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10)
            {
                var eargs = new Arguments();
                eargs.length = 10;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                eargs[9] = TypeProxy.Proxy(a10);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11)
            {
                var eargs = new Arguments();
                eargs.length = 11;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                eargs[9] = TypeProxy.Proxy(a10);
                eargs[10] = TypeProxy.Proxy(a11);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12)
            {
                var eargs = new Arguments();
                eargs.length = 12;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                eargs[9] = TypeProxy.Proxy(a10);
                eargs[10] = TypeProxy.Proxy(a11);
                eargs[11] = TypeProxy.Proxy(a12);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13)
            {
                var eargs = new Arguments();
                eargs.length = 13;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                eargs[9] = TypeProxy.Proxy(a10);
                eargs[10] = TypeProxy.Proxy(a11);
                eargs[11] = TypeProxy.Proxy(a12);
                eargs[12] = TypeProxy.Proxy(a13);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14)
            {
                var eargs = new Arguments();
                eargs.length = 14;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                eargs[9] = TypeProxy.Proxy(a10);
                eargs[10] = TypeProxy.Proxy(a11);
                eargs[11] = TypeProxy.Proxy(a12);
                eargs[12] = TypeProxy.Proxy(a13);
                eargs[13] = TypeProxy.Proxy(a14);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15)
            {
                var eargs = new Arguments();
                eargs.length = 15;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                eargs[9] = TypeProxy.Proxy(a10);
                eargs[10] = TypeProxy.Proxy(a11);
                eargs[11] = TypeProxy.Proxy(a12);
                eargs[12] = TypeProxy.Proxy(a13);
                eargs[13] = TypeProxy.Proxy(a14);
                eargs[14] = TypeProxy.Proxy(a15);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15, T16 a16)
            {
                var eargs = new Arguments();
                eargs.length = 16;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                eargs[9] = TypeProxy.Proxy(a10);
                eargs[10] = TypeProxy.Proxy(a11);
                eargs[11] = TypeProxy.Proxy(a12);
                eargs[12] = TypeProxy.Proxy(a13);
                eargs[13] = TypeProxy.Proxy(a14);
                eargs[14] = TypeProxy.Proxy(a15);
                eargs[15] = TypeProxy.Proxy(a16);
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

        private static readonly FunctionStatement creatorDummy = new FunctionStatement("anonymous");
        internal static readonly Function emptyFunction = new Function();
        private static readonly Function TTEProxy = new MethodProxy(typeof(Function).GetMethod("ThrowTypeError", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)) { attributes = JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.Immutable | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.ReadOnly };
        private static void ThrowTypeError()
        {
            throw new JSException(new TypeError("Properties caller and arguments not allowed in strict mode."));
        }
        private static readonly JSObject propertiesDummySM = new JSObject()
        {
            valueType = JSObjectType.Property,
            oValue = new Function[2] 
            { 
                TTEProxy,
                TTEProxy
            },
            attributes = JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.Immutable | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.NotConfigurable
        };

        private readonly FunctionStatement creator;
        [Hidden]
        [CLSCompliant(false)]
        internal protected readonly Context context;
        [Hidden]
        public Context Context
        {
            [Hidden]
            get { return context; }
        }
        [Field]
        [DoNotDelete]
        [DoNotEnumerate]
        public virtual string name
        {
            [Hidden]
            get { return creator.name; }
        }
        [Hidden]
        public virtual FunctionType Type
        {
            [Hidden]
            get { return creator.type; }
        }

        #region Runtime
        [Hidden]
        [CLSCompliant(false)]
        internal protected JSObject _prototype;
        [Field]
        [DoNotDelete]
        [DoNotEnumerate]
        public JSObject prototype
        {
            [Hidden]
            get { return _prototype; }
            [Hidden]
            set { _prototype.Assign(value["0"]); }
        }
        internal Arguments _arguments;
        /// <summary>
        /// Объект, содержащий параметры вызова функции либо null если в данный момент функция не выполняется.
        /// </summary>
        [DoNotDelete]
        [DoNotEnumerate]
        public JSObject arguments
        {
            [Hidden]
            get { if (creator.body.strict) throw new JSException(new TypeError("Property arguments not allowed in strict mode.")); return _arguments; }
            [Hidden]
            set { if (creator.body.strict) throw new JSException(new TypeError("Property arguments not allowed in strict mode.")); }
        }

        [Hidden]
        internal Number _length = null;
        [Field]
        [ReadOnly]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public JSObject length
        {
            [Hidden]
            get
            {
                if (_length == null)
                {
                    _length = new Number(0) { attributes = JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum };
                    _length.iValue = creator.parameters.Length;
                }
                return _length;
            }
        }

        internal JSObject _caller;
        [DoNotDelete]
        [DoNotEnumerate]
        public JSObject caller
        {
            [Hidden]
            get { if (creator.body.strict) throw new JSException(new TypeError("Property caller not allowed in strict mode.")); return _caller; }
            [Hidden]
            set { if (creator.body.strict) throw new JSException(new TypeError("Property caller not allowed in strict mode.")); }
        }
        #endregion

        [DoNotEnumerate]
        public Function()
        {
            attributes = JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.SystemObject;
            creator = creatorDummy;
            valueType = JSObjectType.Function;
            this.oValue = this;
        }

        [DoNotEnumerate]
        public Function(JSObject[] args)
        {
            attributes = JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.SystemObject;
            context = Context.CurrentContext.Root;
            if (context == Context.globalContext)
                throw new InvalidOperationException("Special Functions constructor can be called only in runtime.");
            var index = 0;
            int len = args.Length - 1;
            var argn = "";
            for (int i = 0; i < len; i++)
                argn += args[i] + (i + 1 < len ? "," : "");
            string code = "function(" + argn + "){" + (len == -1 ? "undefined" : args[len]) + "}";
            var fs = NiL.JS.Statements.FunctionStatement.Parse(new ParsingState(code, code), ref index);
            if (fs.IsParsed)
            {
                Parser.Optimize(ref fs.Statement, 0, 0, new Dictionary<string, VariableDescriptor>(), context.strict);
                var func = fs.Statement.Invoke(context) as Function;
                creator = fs.Statement as FunctionStatement;
            }
            else
                throw new JSException(TypeProxy.Proxy(new SyntaxError()));
            valueType = JSObjectType.Function;
            this.oValue = this;
        }

        internal Function(Context context, FunctionStatement creator)
        {
            attributes = JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.SystemObject;
            this.context = context;
            this.creator = creator;
            valueType = JSObjectType.Function;
            this.oValue = this;
        }

        [Hidden]
        public override void Assign(JSObject value)
        {
            if ((attributes & JSObjectAttributesInternal.ReadOnly) == 0)
            {
#if DEBUG
                System.Diagnostics.Debugger.Break();
#endif
                throw new InvalidOperationException("Try to assign to Function");
            }
        }

        [Hidden]
        public virtual JSObject Invoke(JSObject thisBind, Arguments args)
        {
            var oldargs = _arguments;
            Context internalContext = new Context(context ?? Context.CurrentContext, this);
            var body = creator.body;
            if (body == null || body.Body.Length == 0)
            {
                notExists.valueType = JSObjectType.NotExistInObject;
                return notExists;
            }
            try
            {
                if (!body.strict) // Поправляем this
                {
                    if (thisBind != null)
                    {
                        if (thisBind.valueType > JSObjectType.Undefined && thisBind.valueType < JSObjectType.Object)
                        {
                            thisBind = new JSObject(false)
                            {
                                valueType = JSObjectType.Object,
                                oValue = thisBind,
                                attributes = JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.Immutable,
                                __proto__ = thisBind.__proto__ ?? (thisBind.valueType <= JSObjectType.Undefined ? thisBind.__proto__ : thisBind.GetMember("__proto__"))
                            };
                        }
                        else if (thisBind.valueType <= JSObjectType.Undefined || thisBind.oValue == null)
                            thisBind = internalContext.Root.thisBind;
                    }
                    else
                        thisBind = internalContext.Root.thisBind;
                }

                if (args == null)
                    args = new Arguments();
                _arguments = args;
                int i = 0;
                var ca = internalContext.DefineVariable("arguments");
                ca.Assign(args);
                if (body.strict)
                {
                    ca.attributes |= args.attributes | JSObjectAttributesInternal.ReadOnly;
                    args.callee = propertiesDummySM;
                    args.caller = propertiesDummySM;
                }
                else
                {
                    var callee = this.Clone() as JSObject;
                    callee.attributes = JSObjectAttributesInternal.DoNotEnum;
                    args.callee = callee;
                }
                int min = System.Math.Min(args.length, creator.parameters.Length);
                for (; i < min; i++)
                {
                    JSObject t = args[i];
                    args[i] = t = t.Clone() as JSObject;
                    t.attributes |= JSObjectAttributesInternal.Argument;
                    if (body.strict)
                        t = t.Clone() as JSObject;
                    internalContext.fields[creator.parameters[i].Name] = t;
                }
                for (; i < args.length; i++)
                {
                    var t = args[i];
                    args[i] = t = t.Clone() as JSObject;
                    t.attributes |= JSObjectAttributesInternal.Argument;
                }
                for (; i < creator.parameters.Length; i++)
                    internalContext.fields[creator.parameters[i].Name] = new JSObject() { attributes = JSObjectAttributesInternal.Argument };

                for (i = body.variables.Length; i-- > 0; )
                {
                    if (body.variables[i].Owner == body)
                    {
                        JSObject f = null;
                        if (body.variables[i].name != "arguments" // нельзя переменной перебить аргументы
                            || body.variables[i].Inititalizator != null) // а вот функцией можно
                        {
                            internalContext.fields[body.variables[i].name] = f = new JSObject() { attributes = JSObjectAttributesInternal.DoNotDelete };
                            if (body.variables[i].Inititalizator != null)
                                f.Assign(body.variables[i].Inititalizator.Invoke(internalContext));
                        }
                    }
                }
                internalContext.thisBind = thisBind;
                if (creator.type == FunctionType.Function && creator.name != "" && creator.name != null)
                    internalContext.fields[creator.name] = this;
                internalContext.strict |= body.strict;
                internalContext.variables = body.variables;
                internalContext.Activate();
                body.Invoke(internalContext);
                if (internalContext.abortInfo != null && internalContext.abortInfo.valueType == JSObjectType.NotExist)
                    internalContext.abortInfo.valueType = JSObjectType.NotExistInObject;
#if DEBUG
                if (internalContext.abortInfo == null)
                    System.Diagnostics.Debugger.Break();
#else
                if (internalContext.abortInfo == null)
                    internalContext.abortInfo = notExists;
#endif
                if ((internalContext.abortInfo.attributes & JSObjectAttributesInternal.Temporary) != 0)
                    return internalContext.abortInfo.Clone() as JSObject;
                return internalContext.abortInfo;
            }
            finally
            {
                internalContext.Deactivate();
                _arguments = oldargs;
            }
        }

        [Hidden]
        public JSObject Invoke(Arguments args)
        {
            return Invoke(undefined, args);
        }

        [Hidden]
        internal protected override JSObject GetMember(JSObject nameObj, bool create, bool own)
        {
            string name = nameObj.ToString();
            if (creator.body.strict && (name == "caller" || name == "arguments"))
                return propertiesDummySM;
            if (name == "prototype")
            {
                if (_prototype == null)
                {
                    _prototype = new JSObject(true)
                    {
                        valueType = JSObjectType.Object,
                        __proto__ = JSObject.GlobalPrototype,
                        attributes = JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.DoNotDelete
                    };
                    _prototype.oValue = _prototype;
                    var ctor = _prototype.DefineMember("constructor");
                    ctor.attributes = JSObjectAttributesInternal.DoNotEnum;
                    ctor.Assign(this);
                }
                return _prototype;
            }
            if (__proto__ == null)
                __proto__ = TypeProxy.GetPrototype(this.GetType());
            return DefaultFieldGetter(nameObj, create, own);
        }

        [Hidden]
        public override string ToString()
        {
            var res = ((FunctionType)(creator != null ? (int)creator.type & 3 : 0)).ToString().ToLowerInvariant() + " " + name + "(";
            if (creator != null && creator.parameters != null)
                for (int i = 0; i < creator.parameters.Length; )
                    res += creator.parameters[i].Name + (++i < creator.parameters.Length ? "," : "");
            res += ")" + (creator != creatorDummy ? creator.body as object : "{ [native code] }").ToString();
            return res;
        }

        [DoNotEnumerate]
        public override JSObject valueOf()
        {
            return base.valueOf();
        }

        [DoNotEnumerate]
        public JSObject call(Arguments args)
        {
            var newThis = args[0];
            var prmlen = --args.length;
            if (prmlen >= 0)
            {
                for (var i = 0; i <= prmlen; i++)
                    args[i] = args[i + 1];
                args[prmlen] = null;
            }
            else
                args[0] = null;
            return Invoke(newThis, args);
        }

        [DoNotEnumerate]
        public JSObject apply(Arguments args)
        {
            return Invoke(args[0], args[1] as Arguments);
        }

        [DoNotEnumerate]
        public JSObject bind(JSObject[] args)
        {
            var newThis = args.Length > 0 ? args[0] : null;
            var strict = (creator != null && creator.body != null && creator.body.strict) || Context.CurrentContext.strict;
            if ((newThis != null && newThis.valueType > JSObjectType.Undefined) || strict)
                return new BindedFunction(newThis, this);
            return this;
        }
    }
}