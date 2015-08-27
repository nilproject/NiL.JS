using System;
using System.Reflection;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using NiL.JS.Core.Modules;
using NiL.JS.Core.TypeProxing;
using NiL.JS.Expressions;
using NiL.JS.Statements;
using NiL.JS.Core.Functions;
using NiL.JS.Core;

namespace NiL.JS.BaseLibrary
{
    /// <summary>
    /// Возможные типы функции в контексте использования.
    /// </summary>
#if !PORTABLE
    [Serializable]
#endif
    public enum FunctionType
    {
        Function = 0,
        Get = 1,
        Set = 2,
        AnonymousFunction = 4,
        Generator = 8
    }

#if !PORTABLE
    [Serializable]
#endif
    public class Function : JSObject
    {
#if !PORTABLE
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
#endif

        private static readonly FunctionExpression creatorDummy = new FunctionExpression("anonymous");
        internal static readonly Function emptyFunction = new Function();
        private static readonly Function TTEProxy = new MethodProxy(typeof(Function)
#if PORTABLE
            .GetTypeInfo().GetDeclaredMethod("ThrowTypeError"))
#else
.GetMethod("ThrowTypeError", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic))
#endif
            {
                attributes = JSObjectAttributesInternal.DoNotDelete
                | JSObjectAttributesInternal.Immutable
                | JSObjectAttributesInternal.DoNotEnum
                | JSObjectAttributesInternal.ReadOnly
            };
        protected static void ThrowTypeError()
        {
            throw new JSException(new TypeError("Properties caller, callee and arguments not allowed in strict mode."));
        }
        internal static readonly JSObject propertiesDummySM = new JSObject()
        {
            valueType = JSObjectType.Property,
            oValue = new PropertyPair() { get = TTEProxy, set = TTEProxy },
            attributes = JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.Immutable | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.NotConfigurable
        };

        internal readonly FunctionExpression creator;
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
        [Hidden]
        public virtual bool Strict
        {
            [Hidden]
            get
            {
                return creator.body.strict;
            }
        }

        #region Runtime
        [Hidden]
        [CLSCompliant(false)]
        internal protected JSObject _prototype;
        [Field]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public virtual JSObject prototype
        {
            [Hidden]
            get
            {
                if (_prototype == null)
                {
                    if ((attributes & JSObjectAttributesInternal.ProxyPrototype) != 0)
                        _prototype = new JSObject();
                    else
                    {
                        _prototype = new JSObject(true)
                        {
                            valueType = JSObjectType.Object,
                            attributes = JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.DoNotDelete
                        };
                        _prototype.oValue = _prototype;
                        (_prototype.fields["constructor"] = this.CloneImpl()).attributes = JSObjectAttributesInternal.DoNotEnum;
                    }
                }
                return _prototype;
            }
            [Hidden]
            set
            {
                if (value.oValue != value
                    && value.oValue is JSObject
                    && (value.oValue as JSObject).valueType >= JSObjectType.Object)
                    value = value.oValue as JSObject;
                _prototype = value;
            }
        }
        internal JSObject _arguments;
        /// <summary>
        /// Объект, содержащий параметры вызова функции либо null если в данный момент функция не выполняется.
        /// </summary>
        [Field]
        [DoNotDelete]
        [DoNotEnumerate]
        public virtual JSObject arguments
        {
            [Hidden]
            get
            {
                if (creator.body.strict)
                    throw new JSException(new TypeError("Property arguments not allowed in strict mode."));
                if (_arguments == null && creator.recursiveDepth > 0)
                    buildArgumentsObject();
                return _arguments;
            }
            [Hidden]
            set
            {
                if (creator.body.strict)
                    throw new JSException(new TypeError("Property arguments not allowed in strict mode."));
                _arguments = value;
            }
        }

        [Hidden]
        internal Number _length = null;
        [Field]
        [ReadOnly]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public virtual JSObject length
        {
            [Hidden]
            get
            {
                if (this == null || !typeof(Function).IsAssignableFrom(this.GetType()))
                    return 0;
                if (_length == null)
                {
                    _length = new Number(0);
                    _length.iValue = creator.arguments.Length;
                }
                _length.attributes = JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum;
                return _length;
            }
        }

        internal JSObject _caller;
        [Field]
        [DoNotDelete]
        [DoNotEnumerate]
        public virtual JSObject caller
        {
            [Hidden]
            get { if (creator.body.strict || _caller == propertiesDummySM) throw new JSException(new TypeError("Property caller not allowed in strict mode.")); return _caller; }
            [Hidden]
            set { if (creator.body.strict || _caller == propertiesDummySM) throw new JSException(new TypeError("Property caller not allowed in strict mode.")); }
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
        public Function(Arguments args)
        {
            attributes = JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.DoNotDelete | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.SystemObject;
            context = (Context.CurrentContext ?? Context.GlobalContext).Root;
            if (context == Context.globalContext)
                throw new InvalidOperationException("Special Functions constructor can be called only in runtime.");
            var index = 0;
            int len = args.Length - 1;
            var argn = "";
            for (int i = 0; i < len; i++)
                argn += args[i] + (i + 1 < len ? "," : "");
            string code = "function (" + argn + "){" + Environment.NewLine + (len == -1 ? "undefined" : args[len]) + Environment.NewLine + "}";
            var fs = FunctionExpression.Parse(new ParsingState(Tools.RemoveComments(code, 0), code, null), ref index);
            if (fs.IsParsed && code.Length == index)
            {
                Parser.Build(ref fs.Statement, 0, new Dictionary<string, VariableDescriptor>(), context.strict ? _BuildState.Strict : _BuildState.None, null, null, Options.Default);
                creator = fs.Statement as FunctionExpression;
            }
            else
                throw new JSException((new SyntaxError("")));
            valueType = JSObjectType.Function;
            this.oValue = this;
        }

        [Hidden]
        internal Function(Context context, FunctionExpression creator)
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

        protected internal virtual JSObject InternalInvoke(JSObject self, Expression[] arguments, Context initiator)
        {
            if (this.GetType() == typeof(Function))
            {
                var body = creator.body;
                var result = notExists;
                notExists.valueType = JSObjectType.NotExistsInObject;
                for (; ; )
                {
                    if (body != null)
                    {
                        if (body.lines.Length == 1)
                        {
                            var ret = body.lines[0] as ReturnStatement;
                            if (ret != null)
                            {
                                if (ret.Body != null)
                                {
                                    if (ret.Body.IsContextIndependent)
                                        result = ret.Body.Evaluate(null);
                                    else
                                        break;
                                }
                            }
                            else
                                break;
                        }
                        else if (body.lines.Length != 0)
                            break;
                    }
                    correctThisBind(self, body.strict);
                    for (int i = 0; i < arguments.Length; i++)
                        arguments[i].Evaluate(initiator);
                    return result;
                }

                // быстро выполнить не получилось. 
                // Попробуем чуточку медленее
                if (creator != null
                    && creator.statistic != null
                    && !creator.statistic.ContainsArguments
                    && !creator.statistic.ContainsEval
                    && !creator.statistic.ContainsWith
                    && creator.arguments.Length == arguments.Length // из-за необходимости иметь возможность построить аргументы, если они потребуются
                    && arguments.Length < 9
                    )
                {
                    return fastInvoke(self, arguments, initiator);
                }
            }

            // Совсем медленно. Плохая функция попалась
            Arguments _arguments = new Core.Arguments()
            {
                caller = initiator.strict && initiator.caller != null && initiator.caller.creator.body.strict ? Function.propertiesDummySM : initiator.caller,
                length = arguments.Length
            };

            for (int i = 0; i < arguments.Length; i++)
                _arguments[i] = Call.prepareArg(initiator, arguments[i], false, arguments.Length > 1);
            initiator.objectSource = null;

            return Invoke(self, _arguments);
        }

        private JSObject fastInvoke(JSObject self, Expression[] arguments, Context initiator)
        {
            var body = creator.body;
            var oldcaller = _caller;
            var oldArgs = this._arguments;
            self = correctThisBind(self, body.strict);
            if (creator.recursiveDepth > creator.parametersStored) // рекурсивный вызов.
            {
                storeParameters();
                creator.parametersStored++;
            }
            var internalContext = new Context(context, false, this);
            internalContext.thisBind = self;
            initParametersFast(arguments, initiator, internalContext);
            this._arguments = null;
            creator.recursiveDepth++;
            if (body.strict || (initiator.strict && initiator.caller != null && initiator.caller.creator.body.strict))
                _caller = propertiesDummySM;
            else
                _caller = initiator.caller;
            if (this.creator.reference.descriptor != null
                && creator.reference.descriptor.cacheRes == null)
            {
                creator.reference.descriptor.cacheContext = internalContext.parent;
                creator.reference.descriptor.cacheRes = this;
            }
            internalContext.strict |= body.strict;
            internalContext.variables = body.variables;
            internalContext.Activate();
            try
            {
                return evaluate(internalContext);
            }
            finally
            {
                exit(internalContext);
                _caller = oldcaller;
                this._arguments = oldArgs;
            }
        }

        [Hidden]
        public virtual JSObject Invoke(JSObject thisBind, Arguments args)
        {
#if DEBUG && !PORTABLE
            if (creator.trace)
                System.Console.WriteLine("DEBUG: Run \"" + creator.Reference.Name + "\"");
#endif
            JSObject res = null;
            var body = creator.body;
            thisBind = correctThisBind(thisBind, body.strict);
            if (body == null || body.lines.Length == 0)
            {
                notExists.valueType = JSObjectType.NotExistsInObject;
                return notExists;
            }
            var oldargs = _arguments;
            var oldcaller = _caller;
            var ceocw = creator.statistic.ContainsEval || creator.statistic.ContainsWith;
            if (creator.recursiveDepth > creator.parametersStored) // рекурсивный вызов.
            {
                if (!ceocw)
                    storeParameters();
                creator.parametersStored++;
            }
            creator.recursiveDepth++;
            var internalContext = new Context(context, ceocw, this);
            internalContext.thisBind = thisBind;
            try
            {
                if (args == null)
                {
                    var cc = Context.CurrentContext;
                    args = new Arguments()
                    {
                        caller = cc == null ? null : cc.strict && cc.caller != null && cc.caller.creator.body.strict ? Function.propertiesDummySM : cc.caller
                    };
                }
                _arguments = args;
                if (ceocw)
                    internalContext.fields["arguments"] = _arguments;
                if (body.strict)
                {
                    args.attributes |= JSObjectAttributesInternal.ReadOnly;
                    args.callee = propertiesDummySM;
                    args.caller = propertiesDummySM;
                    _caller = propertiesDummySM;
                }
                else
                {
                    args.callee = this;
                    _caller = args.caller;
                }
                if (this.creator.reference.descriptor != null
                    && creator.reference.descriptor.cacheRes == null)
                {
                    creator.reference.descriptor.cacheContext = internalContext.parent;
                    creator.reference.descriptor.cacheRes = this;
                }
                internalContext.strict |= body.strict;
                internalContext.variables = body.variables;
                internalContext.Activate();
                initParameters(args, body, internalContext);
                res = evaluate(internalContext);
            }
            finally
            {
#if DEBUG && !PORTABLE
                if (creator.trace)
                    System.Console.WriteLine("DEBUG: Exit \"" + creator.Reference.Name + "\"");
#endif
                exit(internalContext);
                _caller = oldcaller;
                _arguments = oldargs;
            }
            return res;
        }

        private void exit(Core.Context internalContext)
        {
            creator.recursiveDepth--;
            if (creator.parametersStored > creator.recursiveDepth)
                creator.parametersStored--;
            try
            {
                internalContext.abort = AbortType.Return;
                internalContext.Deactivate();
            }
            finally
            {
                if (creator.recursiveDepth == 0)
                {
                    var i = creator.body.localVariables.Length;
                    for (; i-- > 0; )
                    {
                        creator.body.localVariables[i].cacheRes = null;
                        creator.body.localVariables[i].cacheContext = null;
                    }
                    for (i = creator.arguments.Length; i-- > 0; )
                    {
                        creator.arguments[i].cacheRes = null;
                        creator.arguments[i].cacheContext = null;
                    }
                }
            }
        }

        private JSObject evaluate(Core.Context internalContext)
        {
            initVariables(creator.body, internalContext);
            JSObject ai = null;
            for (; ; )
            {
                creator.body.Evaluate(internalContext);
                ai = internalContext.abortInfo;
                if (internalContext.abort != AbortType.TailRecursion)
                    break;
                initParameters(this._arguments as Arguments, creator.body, internalContext);
                if (creator.recursiveDepth == creator.parametersStored)
                    storeParameters();
                internalContext.abort = AbortType.None;
            }
            if (ai == null)
            {
                notExists.valueType = JSObjectType.NotExistsInObject;
                return notExists;
            }
            else
            {
                if (ai.valueType <= JSObjectType.NotExists)
                    return notExists;
                else if (ai.valueType == JSObjectType.Undefined)
                    return undefined;
                else
                    return ai;
            }
        }

        private void initParametersFast(Expression[] arguments, Core.Context initiator, Context internalContext)
        {
            JSObject a0, a1, a2, a3, a4, a5, a6, a7; // Вместо кучи, выделяем память на стеке

            var m = System.Math.Min(creator.arguments.Length, arguments.Length);
            switch (m)
            {
                case 0:
                    break;
                case 1:
                    {
                        a0 = arguments[0].Evaluate(initiator).CloneImpl();

                        for (int i = m; i < arguments.Length; i++)
                            arguments[i].Evaluate(initiator);

                        setPrmFst(0, a0, internalContext);
                        break;
                    }
                case 2:
                    {
                        a0 = arguments[0].Evaluate(initiator).CloneImpl();
                        a1 = arguments[1].Evaluate(initiator).CloneImpl();

                        for (int i = m; i < arguments.Length; i++)
                            arguments[i].Evaluate(initiator);

                        setPrmFst(0, a0, internalContext);
                        setPrmFst(1, a1, internalContext);
                        break;
                    }
                case 3:
                    {
                        a0 = arguments[0].Evaluate(initiator).CloneImpl();
                        a1 = arguments[1].Evaluate(initiator).CloneImpl();
                        a2 = arguments[2].Evaluate(initiator).CloneImpl();

                        for (int i = m; i < arguments.Length; i++)
                            arguments[i].Evaluate(initiator);

                        setPrmFst(0, a0, internalContext);
                        setPrmFst(1, a1, internalContext);
                        setPrmFst(2, a2, internalContext);
                        break;
                    }
                case 4:
                    {
                        a0 = arguments[0].Evaluate(initiator).CloneImpl();
                        a1 = arguments[1].Evaluate(initiator).CloneImpl();
                        a2 = arguments[2].Evaluate(initiator).CloneImpl();
                        a3 = arguments[3].Evaluate(initiator).CloneImpl();

                        for (int i = m; i < arguments.Length; i++)
                            arguments[i].Evaluate(initiator);

                        setPrmFst(0, a0, internalContext);
                        setPrmFst(1, a1, internalContext);
                        setPrmFst(2, a2, internalContext);
                        setPrmFst(3, a3, internalContext);
                        break;
                    }
                case 5:
                    {
                        a0 = arguments[0].Evaluate(initiator).CloneImpl();
                        a1 = arguments[1].Evaluate(initiator).CloneImpl();
                        a2 = arguments[2].Evaluate(initiator).CloneImpl();
                        a3 = arguments[3].Evaluate(initiator).CloneImpl();
                        a4 = arguments[4].Evaluate(initiator).CloneImpl();

                        for (int i = m; i < arguments.Length; i++)
                            arguments[i].Evaluate(initiator);

                        setPrmFst(0, a0, internalContext);
                        setPrmFst(1, a1, internalContext);
                        setPrmFst(2, a2, internalContext);
                        setPrmFst(3, a3, internalContext);
                        setPrmFst(4, a4, internalContext);
                        break;
                    }
                case 6:
                    {
                        a0 = arguments[0].Evaluate(initiator).CloneImpl();
                        a1 = arguments[1].Evaluate(initiator).CloneImpl();
                        a2 = arguments[2].Evaluate(initiator).CloneImpl();
                        a3 = arguments[3].Evaluate(initiator).CloneImpl();
                        a4 = arguments[4].Evaluate(initiator).CloneImpl();
                        a5 = arguments[5].Evaluate(initiator).CloneImpl();

                        for (int i = m; i < arguments.Length; i++)
                            arguments[i].Evaluate(initiator);

                        setPrmFst(0, a0, internalContext);
                        setPrmFst(1, a1, internalContext);
                        setPrmFst(2, a2, internalContext);
                        setPrmFst(3, a3, internalContext);
                        setPrmFst(4, a4, internalContext);
                        setPrmFst(5, a5, internalContext);
                        break;
                    }
                case 7:
                    {
                        a0 = arguments[0].Evaluate(initiator).CloneImpl();
                        a1 = arguments[1].Evaluate(initiator).CloneImpl();
                        a2 = arguments[2].Evaluate(initiator).CloneImpl();
                        a3 = arguments[3].Evaluate(initiator).CloneImpl();
                        a4 = arguments[4].Evaluate(initiator).CloneImpl();
                        a5 = arguments[5].Evaluate(initiator).CloneImpl();
                        a6 = arguments[6].Evaluate(initiator).CloneImpl();

                        for (int i = m; i < arguments.Length; i++)
                            arguments[i].Evaluate(initiator);

                        setPrmFst(0, a0, internalContext);
                        setPrmFst(1, a1, internalContext);
                        setPrmFst(2, a2, internalContext);
                        setPrmFst(3, a3, internalContext);
                        setPrmFst(4, a4, internalContext);
                        setPrmFst(5, a5, internalContext);
                        setPrmFst(6, a6, internalContext);
                        break;
                    }
                case 8:
                    {
                        a0 = arguments[0].Evaluate(initiator).CloneImpl();
                        a1 = arguments[1].Evaluate(initiator).CloneImpl();
                        a2 = arguments[2].Evaluate(initiator).CloneImpl();
                        a3 = arguments[3].Evaluate(initiator).CloneImpl();
                        a4 = arguments[4].Evaluate(initiator).CloneImpl();
                        a5 = arguments[5].Evaluate(initiator).CloneImpl();
                        a6 = arguments[6].Evaluate(initiator).CloneImpl();
                        a7 = arguments[7].Evaluate(initiator).CloneImpl();

                        for (int i = m; i < arguments.Length; i++)
                            arguments[i].Evaluate(initiator);

                        setPrmFst(0, a0, internalContext);
                        setPrmFst(1, a1, internalContext);
                        setPrmFst(2, a2, internalContext);
                        setPrmFst(3, a3, internalContext);
                        setPrmFst(4, a4, internalContext);
                        setPrmFst(5, a5, internalContext);
                        setPrmFst(6, a6, internalContext);
                        setPrmFst(7, a7, internalContext);
                        break;
                    }
                default:
                    throw new ArgumentException("To many arguments");
            }
            for (int i = m; i < creator.arguments.Length; i++)
            {
                if (creator.arguments[i].assignations != null)
                    creator.arguments[i].cacheRes = new JSObject()
                    {
                        valueType = JSObjectType.Undefined,
                        attributes = JSObjectAttributesInternal.Argument
                    };
                else
                    creator.arguments[i].cacheRes = undefined;
                creator.arguments[i].cacheContext = internalContext;
                if (creator.arguments[i].captured)
                {
                    if (internalContext.fields == null)
                        internalContext.fields = createFields();
                    internalContext.fields[creator.arguments[i].Name] = creator.arguments[i].cacheRes;
                }
            }
        }

        private void setPrmFst(int index, JSObject value, Context context)
        {
            value.attributes |= JSObjectAttributesInternal.Argument;
            creator.arguments[index].cacheRes = value;
            creator.arguments[index].cacheContext = context;
            if (creator.arguments[index].captured)
            {
                if (context.fields == null)
                    context.fields = createFields();
                context.fields[creator.arguments[index].name] = value;
            }
        }

        internal void buildArgumentsObject()
        {
            if (this._arguments == null)
            {
                var args = new Arguments()
                {
                    caller = _caller,
                    callee = this
                };
                for (var i = 0; i < creator.arguments.Length; i++)
                {
                    if (creator.body.strict)
                        args.Add(creator.arguments[i].cacheRes.CloneImpl());
                    else
                        args.Add(creator.arguments[i].cacheRes);
                }
                this._arguments = args;
            }
        }

        private void initParameters(Arguments args, CodeBlock body, Context internalContext)
        {
            var cea = creator.statistic.ContainsEval || creator.statistic.ContainsArguments;
            var cew = creator.statistic.ContainsEval || creator.statistic.ContainsWith;
            int min = System.Math.Min(args.length, creator.arguments.Length);
            int i = 0;
            for (; i < min; i++)
            {
                JSObject t = args[i];
                var arg = creator.arguments[i];
                if (body.strict)
                {
                    if (arg.assignations != null)
                    {
                        args[i] = t = t.CloneImpl();
                        t.attributes |= JSObjectAttributesInternal.Cloned;
                    }
                    if (cea)
                    {
                        if ((t.attributes & JSObjectAttributesInternal.Cloned) == 0)
                            args[i] = t.CloneImpl();
                        t = t.CloneImpl();
                    }
                }
                else
                {
                    if (arg.assignations != null
                        || cea
                        || cew
                        || (t.attributes & JSObjectAttributesInternal.Temporary) != 0)
                    {
                        if ((t.attributes & JSObjectAttributesInternal.Cloned) == 0)
                            args[i] = t = t.CloneImpl();
                        t.attributes |= JSObjectAttributesInternal.Argument;
                    }
                }
                t.attributes &= ~JSObjectAttributesInternal.Cloned;
                if (arg.captured || cew)
                    (internalContext.fields ?? (internalContext.fields = createFields(1)))[arg.Name] = t;
                arg.cacheContext = internalContext;
                arg.cacheRes = t;
                if (string.CompareOrdinal(arg.name, "arguments") == 0)
                    _arguments = t;
            }
            for (; i < args.length; i++)
            {
                JSObject t = args[i];
                if ((t.attributes & JSObjectAttributesInternal.Cloned) != 0)
                    t.attributes &= ~JSObjectAttributesInternal.Cloned;
                else if (cew || cea)
                    args[i] = t = t.CloneImpl();
                t.attributes |= JSObjectAttributesInternal.Argument;
            }
            for (; i < creator.arguments.Length; i++)
            {
                var arg = creator.arguments[i];
                if (cew || arg.assignations != null)
                    arg.cacheRes = new JSObject()
                    {
                        attributes = JSObjectAttributesInternal.Argument,
                        valueType = JSObjectType.Undefined
                    };
                else
                    arg.cacheRes = JSObject.undefined;
                arg.cacheContext = internalContext;
                if (arg.captured || cew)
                {
                    if (internalContext.fields == null)
                        internalContext.fields = createFields();
                    internalContext.fields[arg.Name] = arg.cacheRes;
                }
                if (string.CompareOrdinal(arg.name, "arguments") == 0)
                    _arguments = arg.cacheRes;
            }
        }

        private void initVariables(CodeBlock body, Context internalContext)
        {
            if (body.localVariables != null)
            {
                var cew = creator.statistic.ContainsEval || creator.statistic.ContainsWith;
                for (var i = body.localVariables.Length; i-- > 0; )
                {
                    var v = body.localVariables[i];
                    bool isArg = string.CompareOrdinal(v.name, "arguments") == 0;
                    if (isArg && v.Inititalizator == null)
                        continue;
                    JSObject f = new JSObject() { valueType = JSObjectType.Undefined, attributes = JSObjectAttributesInternal.DoNotDelete };
                    if (v.captured || cew)
                        (internalContext.fields ?? (internalContext.fields = createFields()))[v.name] = f;
                    if (v.Inititalizator != null)
                        f.Assign(v.Inititalizator.Evaluate(internalContext));
                    if (v.isReadOnly)
                        f.attributes |= JSObjectAttributesInternal.ReadOnly;
                    v.cacheRes = f;
                    v.cacheContext = internalContext;
                    if (isArg)
                        _arguments = f;
                }
            }
        }

        internal JSObject correctThisBind(JSObject thisBind, bool strict)
        {
            if (thisBind == null)
                return strict ? undefined : context.Root.thisBind;
            else if (thisBind.oValue == typeof(New) as object)
            {
                thisBind.__proto__ = prototype.oValue as JSObject ?? prototype;
                thisBind.oValue = thisBind;
            }
            else if (context != null)
            {
                if (!strict) // Поправляем this
                {
                    if (thisBind.valueType > JSObjectType.Undefined && thisBind.valueType < JSObjectType.Object)
                        return thisBind.ToObject();
                    else if (thisBind.valueType <= JSObjectType.Undefined || thisBind.oValue == null)
                        return context.Root.thisBind;
                }
                else if (thisBind.valueType < JSObjectType.Undefined)
                    return undefined;
            }
            return thisBind;
        }

        private void storeParameters()
        {
            if (creator.arguments.Length != 0)
            {
                var context = creator.arguments[0].cacheContext;
                if (context.fields == null)
                    context.fields = createFields(creator.arguments.Length);
                for (var i = 0; i < creator.arguments.Length; i++)
                    context.fields[creator.arguments[i].Name] = creator.arguments[i].cacheRes;
            }
            if (creator.body.localVariables != null && creator.body.localVariables.Length > 0)
            {
                var context = creator.body.localVariables[0].cacheContext;
                if (context.fields == null)
                    context.fields = createFields(creator.body.localVariables.Length);
                for (var i = 0; i < creator.body.localVariables.Length; i++)
                    context.fields[creator.body.localVariables[i].Name] = creator.body.localVariables[i].cacheRes;
            }
        }

        [Hidden]
        public JSObject Invoke(Arguments args)
        {
            return Invoke(undefined, args);
        }

        [Hidden]
        internal protected override JSObject GetMember(JSObject nameObj, bool forWrite, bool own)
        {
            string name = nameObj.ToString();
            if (creator.body.strict && (name == "caller" || name == "arguments"))
                return propertiesDummySM;
            if ((attributes & JSObjectAttributesInternal.ProxyPrototype) != 0 && name == "prototype")
                return prototype;
            return DefaultFieldGetter(nameObj, forWrite, own);
        }

        [CLSCompliant(false)]
        [DoNotEnumerate]
        [ArgumentsLength(0)]
        public new virtual JSObject toString(Arguments args)
        {
            return ToString();
        }

        [Hidden]
        public override sealed string ToString()
        {
            return ToString(false);
        }

        [Hidden]
        public virtual string ToString(bool headerOnly)
        {
            StringBuilder res = new StringBuilder();
            switch (creator.type)
            {
                case FunctionType.Generator:
                    res.Append("function*");
                    break;
                case FunctionType.Get:
                    res.Append("get");
                    break;
                case FunctionType.Set:
                    res.Append("set");
                    break;
                default:
                    res.Append("function");
                    break;
            }
            res.Append(" ").Append(name).Append("(");
            if (creator != null && creator.arguments != null)
                for (int i = 0; i < creator.arguments.Length; )
                    res.Append(creator.arguments[i].Name).Append(++i < creator.arguments.Length ? "," : "");
            res.Append(")");
            if (!headerOnly)
                res.Append(' ').Append(creator != creatorDummy ? creator.body as object : "{ [native code] }");
            return res.ToString();
        }

        [Hidden]
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
        [ArgumentsLength(2)]
        [AllowNullArguments]
        public JSObject apply(Arguments args)
        {
            var nargs = args ?? new Arguments();
            var argsSource = nargs[1];
            var self = nargs[0];
            if (args != null)
                nargs.Reset();
            if (argsSource.IsDefinded)
            {
                if (argsSource.valueType < JSObjectType.Object)
                    throw new JSException(new TypeError("Argument list has wrong type."));
                var len = argsSource["length"];
                if (len.valueType == JSObjectType.Property)
                    len = (len.oValue as PropertyPair).get.Invoke(argsSource, null);
                nargs.length = Tools.JSObjectToInt32(len);
                if (nargs.length >= 50000)
                    throw new JSException(new RangeError("Too many arguments."));
                for (var i = nargs.length; i-- > 0; )
                    nargs[i] = argsSource[i < 16 ? Tools.NumString[i] : i.ToString(CultureInfo.InvariantCulture)];
            }
            return Invoke(self, nargs);
        }

        [DoNotEnumerate]
        public JSObject bind(Arguments args)
        {
            var newThis = args.Length > 0 ? args[0] : null;
            var strict = (creator.body != null && creator.body.strict) || Context.CurrentContext.strict;
            if ((newThis != null && newThis.valueType > JSObjectType.Undefined) || strict)
                return new BindedFunction(this, args);
            return this;
        }

        [Hidden]
        public virtual object MakeDelegate(Type delegateType)
        {
#if PORTABLE
            throw new NotSupportedException("Do not supported in portable version");
#else
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
                    var method = ((System.Reflection.MethodInfo)invokes[17 + prms.Length]);
                    if (prms.Length > 0)
                        method = method.MakeGenericMethod(argtypes);
                    return Delegate.CreateDelegate(delegateType, instance, method);
                }
            }
            else
                throw new ArgumentException("Parameters count must be less or equal 16.");
#endif
        }
    }
}
