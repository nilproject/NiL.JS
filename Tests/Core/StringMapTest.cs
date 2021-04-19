using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;

namespace Tests.Core
{
    [TestClass]
    public sealed class StringMapTest
    {
        private static readonly Func<string, int> _ComputeHash;

        static StringMapTest()
        {
            var method = typeof(StringMap<object>).GetMethod("computeHash", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var parameter = Expression.Parameter(typeof(string), "x");
            var lambda = Expression.Lambda<Func<string, int>>(Expression.Call(method, parameter), parameter);
            _ComputeHash = lambda.Compile();
        }

        private unsafe static string createRandomAsciiString(Random random, byte[] buffer)
        {
            const int printableRangeStart = 0x20;
            const int printableRangeLen = 0x7f - printableRangeStart + 1;

            var chars = stackalloc char[buffer.Length + 1];

            random.NextBytes(buffer);

            for (var i = 0; i < buffer.Length; i++)
            {
                chars[i] = (char)((buffer[i] % printableRangeLen) + printableRangeStart);
            }

            return new string(chars);
        }

        private static double variance(int[] values, int start, int end, int iterations)
        {
            Array.Sort(values);

            var sum = 0d;
            var itersQ = iterations * (double)iterations;
            for (var i = start; i <= end; i++)
            {
                var psum = 0d;
                for (var j = i + 1; j <= end; j++)
                {
                    var v = (j - i);
                    psum += (values[j] * (long)values[i] / itersQ) * v * v;
                }

                sum += psum;
            }

            return sum;
        }

        [TestMethod]
        public void ComputeHashProcessesNonNegativeNumbers()
        {
            var hash0 = _ComputeHash("1");
            var hash1 = _ComputeHash("1a");
            var hash2 = _ComputeHash("-1");
            var hash3 = _ComputeHash("-1a");
            var hash4 = _ComputeHash("0");
            var hash5 = _ComputeHash("1");
            var hash6 = _ComputeHash("2");
            var hash7 = _ComputeHash("3");
            var hash8 = _ComputeHash("4");
            var hash9 = _ComputeHash("5");
            var hash10 = _ComputeHash("6");

            Assert.IsTrue(hash0 < 0, "1");
            Assert.IsTrue(hash1 > 0, "1a");
            Assert.IsTrue(hash2 > 0, "-1");
            Assert.IsTrue(hash3 > 0, "-1a");
            Assert.IsTrue(hash4 < 0, "0");
            Assert.IsTrue(hash5 < 0, "1");
            Assert.IsTrue(hash6 < 0, "2");
            Assert.IsTrue(hash7 < 0, "3");
            Assert.IsTrue(hash8 < 0, "4");
            Assert.IsTrue(hash9 < 0, "5");
            Assert.IsTrue(hash10 < 0, "6");
        }

        [TestMethod]
        public void ComputeHashHasGoodVariance()
        {
            var hbyte0 = new int[16];
            var hbyte1 = new int[16];
            var hbyte2 = new int[16];
            var hbyte3 = new int[16];
            var hbyte4 = new int[16];
            var hbyte5 = new int[16];
            var hbyte6 = new int[16];
            var hbyte7 = new int[16];

            var random = new Random(0x777);
            var buffer = new byte[50];

            var iterations = 1_000_000;
            for (var i = 0; i < iterations; i++)
            {
                random = new Random(random.Next()); // make random sequence endless

                var str = createRandomAsciiString(random, buffer);

                var hash = _ComputeHash(str);

                hbyte0[(byte)(0xf & (hash))]++;
                hbyte1[(byte)(0xf & (hash >> 4))]++;
                hbyte2[(byte)(0xf & (hash >> 8))]++;
                hbyte3[(byte)(0xf & (hash >> 12))]++;
                hbyte4[(byte)(0xf & (hash >> 16))]++;
                hbyte5[(byte)(0xf & (hash >> 20))]++;
                hbyte6[(byte)(0xf & (hash >> 24))]++;
                hbyte7[(byte)(0xf & (hash >> 28))]++;
            }

            var d0 = variance(hbyte0, 0, 15, iterations);
            var d1 = variance(hbyte1, 0, 15, iterations);
            var d2 = variance(hbyte2, 0, 15, iterations);
            var d3 = variance(hbyte3, 0, 15, iterations);
            var d4 = variance(hbyte4, 0, 15, iterations);
            var d5 = variance(hbyte5, 0, 15, iterations);
            var d6 = variance(hbyte6, 0, 15, iterations);
            var d7 = variance(hbyte7, 8, 15, iterations);

            Console.WriteLine(Math.Sqrt(d0));
            Console.WriteLine(Math.Sqrt(d1));
            Console.WriteLine(Math.Sqrt(d2));
            Console.WriteLine(Math.Sqrt(d3));
            Console.WriteLine(Math.Sqrt(d4));
            Console.WriteLine(Math.Sqrt(d5));
            Console.WriteLine(Math.Sqrt(d6));
            Console.WriteLine(Math.Sqrt(d7));

            Assert.IsTrue(Math.Sqrt(d0) >= 4.6);
            Assert.IsTrue(Math.Sqrt(d1) >= 4.6);
            Assert.IsTrue(Math.Sqrt(d2) >= 4.6);
            Assert.IsTrue(Math.Sqrt(d3) >= 4.6);
            Assert.IsTrue(Math.Sqrt(d4) >= 4.6);
            Assert.IsTrue(Math.Sqrt(d5) >= 4.6);
            Assert.IsTrue(Math.Sqrt(d6) >= 4.6);
            Assert.IsTrue(Math.Sqrt(d7) >= 2.29);
        }

        [TestMethod]
        public void CheckBugEnum85()
        {
            var map = new StringMap<int>();
            for (var i = 0; i < 85; i++)
                map[i.ToString()] = i;

            var items = new List<int>();
            foreach (var item in map)
                items.Add(item.Value);

            for(var i = 0; i < 85; i++)
            {
                Assert.AreEqual(i, items[i], "items[i] != i");
            }
        }
    }
}
