using System.Collections.Generic;
using System.Text;
using NiL.JS.BaseLibrary;

namespace NiL.JS.Core
{
    public sealed class RopeString
    {
        private int _length;
        private object _firstPart;
        private object _secondPart;
        private string firstPart
        {
            get
            {
                if (_firstPart == null)
                    return null;

                return _firstPart as string ?? (_firstPart = _firstPart.ToString()) as string;
            }
        }
        private string secondPart
        {
            get
            {
                if (_secondPart == null)
                    return null;

                return _secondPart as string ?? (_secondPart = _secondPart.ToString()) as string;
            }
        }

        public RopeString()
        {
            _firstPart = "";
            _secondPart = "";
        }

        public RopeString(object source)
        {
            _firstPart = source ?? "";
            _secondPart = "";
        }

        public RopeString(object firstSource, object secondSource)
        {
            _firstPart = firstSource ?? "";
            _secondPart = secondSource ?? "";

            _length = calcLength();

            if (_length < 0)
                ExceptionHelper.Throw(new RangeError("String is too large"));
        }

        public int Length => _length;

        private static void _append(StringBuilder sb, object arg)
        {
            var str = arg.ToString();
            var start = sb.Length;
            if (sb.Capacity < start + str.Length)
                sb.EnsureCapacity(System.Math.Max(sb.Capacity << 1, start + str.Length));
            sb.Length += str.Length;
            for (var i = 0; i < str.Length; i++)
            {
                sb[start + i] = str[i];
            }
        }

        public override string ToString()
        {
            if (_secondPart != null)
            {
                if (!(_firstPart is RopeString)
                    && !(_secondPart is RopeString))
                {
                    _firstPart = firstPart + secondPart;
                    _secondPart = null;
                }
                else
                {
                    var stack = new Stack<RopeString>();
                    var step = new Stack<int>();
                    var res = new StringBuilder(Length);
                    stack.Push(this);
                    step.Push(0);
                    while (stack.Count != 0)
                    {
                        if (step.Peek() < 1)
                        {
                            if (stack.Peek()._firstPart is RopeString)
                            {
                                var child = stack.Peek()._firstPart as RopeString;
                                stack.Push(child);
                                step.Pop();
                                step.Push(1);
                                step.Push(0);
                                continue;
                            }
                            else
                            {
                                _append(res, stack.Peek().firstPart ?? "");
                                step.Pop();
                                step.Push(1);
                            }
                        }

                        if (step.Peek() < 2)
                        {
                            if (stack.Peek()._secondPart is RopeString)
                            {
                                var child = stack.Peek()._secondPart as RopeString;
                                stack.Push(child);
                                step.Pop();
                                step.Push(2);
                                step.Push(0);
                                continue;
                            }
                            else
                            {
                                _append(res, stack.Peek().secondPart ?? "");
                                step.Pop();
                                step.Push(2);
                            }
                        }

                        stack.Pop();
                        step.Pop();
                    }

                    _firstPart = res.ToString();
                    _secondPart = null;
                }
            }
            return firstPart;
        }

        private int calcLength()
        {
            //return ToString().Length;

            int res = 0;
            if (_firstPart != null)
            {
                var rs = _firstPart as RopeString;
                if (rs != null)
                {
                    res = rs.Length;
                }
                else
                {
                    var sb = _firstPart as StringBuilder;
                    if (sb != null)
                        res = sb.Length;
                    else
                        res = firstPart.Length;
                }
            }

            if (_secondPart != null)
            {
                var rs = _secondPart as RopeString;
                if (rs != null)
                {
                    res += rs.Length;
                }
                else
                {
                    var sb = _secondPart as StringBuilder;
                    if (sb != null)
                        res += sb.Length;
                    else
                        res += secondPart.Length;
                }
            }

            return res;
        }
    }
}
