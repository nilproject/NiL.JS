using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using NiL.JS.Core;
using NiL.JS.Core.Interop;

#if !NETCORE
namespace NiL.JS.BaseLibrary
{
    /// <summary>
    /// A console modelled after the Console Living Standard.
    /// https://console.spec.whatwg.org
    /// </summary>
    public class JSConsole
    {

        [Hidden]
        public enum LogLevel
        {
            Log = 0,
            Info = 1,
            Warn = 2,
            Error = 3
        }

        private Dictionary<string, int> _counters = new Dictionary<string, int>();
        private List<string> _groups = new List<string>();
        private Dictionary<string, Stopwatch> _timers = new Dictionary<string, Stopwatch>();



        [Hidden]
        public virtual TextWriter GetLogger(LogLevel ll)
        {
            if (ll == LogLevel.Error)
                return System.Console.Error;
            else
                return System.Console.Out;
        }

        internal void LogArguments(LogLevel level, Arguments args)
        {
            LogArguments(level, args, 0);
        }
        internal void LogArguments(LogLevel level, Arguments args, int argsStart)
        {
            if (args == null || args.length == 0 || args.length <= argsStart)
                return;

            LogMessage(level, Tools.FormatArgs(args.Skip(argsStart)));
        }
        [Hidden]
        public void LogMessage(LogLevel level, string message)
        {
            Print(level, GetLogger(level), message, _groups.Count, "|   ");
        }

        [Hidden]
        public void Print(LogLevel level, TextWriter textWriter, string message)
        {
            Print(level, textWriter, message, 0, "|   ");
        }
        [Hidden]
        public void Print(LogLevel level, TextWriter textWriter, string message, int indent, string indentChar)
        {
            if (message == null || textWriter == null)
                return;

            if (indent > 0)
            {
                string ind = "";
                for (int j = 0; j < indent; j++)
                    ind += indentChar;

                int last = 0;
                for (int i = 0; i < message.Length; i++)
                {
                    if (message[i] == '\n' || message[i] == '\r')
                    {
                        textWriter.WriteLine(ind + message.Substring(last, i - last));
                        if (message[i] == '\r' && i + 1 < message.Length && message[i + 1] == '\n')
                            i++;
                        last = i + 1;
                    }
                }
                textWriter.WriteLine(ind + message.Substring(last));
            }
            else
                textWriter.WriteLine(message);
        }


        public JSValue assert(Arguments args)
        {
            if (!(bool)args[0])
                LogArguments(LogLevel.Log, args, 1);
            return JSValue.undefined;
        }

        public JSValue asserta(Function f, JSValue sample)
        {
            if (sample == null || !sample.Exists)
                sample = Boolean.True;

            if (!JSObject.@is(f.Call(null), sample))
            {
                var message = f.ToString();
                message = message.Substring(message.IndexOf("=>") + 2).Trim();
                LogMessage(LogLevel.Error, message + " not equals " + sample);
            }
            return JSValue.undefined;
        }

        public virtual JSValue clear(Arguments args)
        {
            _groups.Clear();
            //System.Console.Clear();

            return JSValue.undefined;
        }

        public JSValue count(Arguments args)
        {
            string label = "";
            if (args.length > 0)
                label = (args[0] ?? "null").ToString();

            if (!_counters.ContainsKey(label))
                _counters.Add(label, 0);

            string c = Tools.Int32ToString(++_counters[label]);

            if (label != "")
                label += ": ";
            LogMessage(LogLevel.Info, label + c);

            return JSValue.undefined;
        }

        public JSValue debug(Arguments args)
        {
            LogArguments(LogLevel.Log, args);
            return JSValue.undefined;
        }

        public JSValue error(Arguments args)
        {
            LogArguments(LogLevel.Error, args);
            return JSValue.undefined;
        }

        public JSValue info(Arguments args)
        {
            LogArguments(LogLevel.Info, args);
            return JSValue.undefined;
        }

        public JSValue log(Arguments args)
        {
            LogArguments(LogLevel.Log, args);
            return JSValue.undefined;
        }

        //public JSValue table(Arguments args)
        //{
        //    return JSValue.undefined;
        //}

        //public JSValue trace(Arguments args)
        //{
        //    return JSValue.undefined;
        //}

        public JSValue warn(Arguments args)
        {
            LogArguments(LogLevel.Warn, args);
            return JSValue.undefined;
        }

        public virtual JSValue dir(Arguments args)
        {
            LogMessage(LogLevel.Log, Tools.JSValueToObjectString(args[0], 2));
            return JSValue.undefined;
        }

        //public JSValue dirxml(Arguments args)
        //{
        //    LogMessage(LogLevel.Log, Tools.JSValueToObjectString(args[0], 2));
        //    return JSValue.undefined;
        //}


        public JSValue group(Arguments args)
        {
            string label = Tools.FormatArgs(args) ?? "null";
            if (label == "")
                label = "console.group";

            if (_groups.Count > 0)
            {
                string _temp = _groups[_groups.Count - 1];
                _groups.RemoveAt(_groups.Count - 1);
                LogMessage(LogLevel.Info, "|---# " + label);

                _groups.Add(_temp);
            }
            else
                LogMessage(LogLevel.Info, "# " + label);

            _groups.Add(label);

            return JSValue.undefined;
        }

        public JSValue groupCollapsed(Arguments args)
        {
            group(args);
            return JSValue.undefined;
        }

        public JSValue groupEnd(Arguments args)
        {
            if (_groups.Count == 0)
                _groups.RemoveAt(_groups.Count - 1);

            return JSValue.undefined;
        }


        public JSValue time(Arguments args)
        {
            string label = "";
            if (args.length > 0)
                label = (args[0] ?? "null").ToString();

            if (_timers.ContainsKey(label))
                _timers[label].Restart();
            else
                _timers.Add(label, Stopwatch.StartNew());

            return JSValue.undefined;
        }

        public JSValue timeEnd(Arguments args)
        {
            string label = "";
            if (args.length > 0)
                label = (args[0] ?? "null").ToString();

            double elapsed = 0.0;
            if (_timers.ContainsKey(label))
            {
                _timers[label].Stop();
                elapsed = (double)_timers[label].ElapsedTicks / Stopwatch.Frequency * 1000.0;
                _timers.Remove(label);
            }

            if (label != "")
                label += ": ";
            LogMessage(LogLevel.Info, label + Tools.DoubleToString(System.Math.Round(elapsed, 10)) + "ms");

            return JSValue.undefined;
        }


        [Hidden]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [Hidden]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [Hidden]
        public override string ToString()
        {
            return base.ToString();
        }

    }
}
#endif
