using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NiL.JS.Core;
using NiL.JS.Core.Interop;

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

        private static Regex _lineSplitter;

        private Dictionary<string, int> _counters = new Dictionary<string, int>();
        private List<string> _groups = new List<string>();
        private Dictionary<string, Stopwatch> _timers = new Dictionary<string, Stopwatch>();

        private int _tableMaxColWidth = 100;

        /// <summary>
        /// This controls in part the maximum width (in chars) that a column printed by table can have.
        /// The actual maximum width is the maximum of this value and the length of the columns header (the name of the property displayed).
        /// </summary>
        [Hidden]
        public virtual int TableMaximumColumnWidth
        {
            get
            {
                return _tableMaxColWidth;
            }
            set
            {
                if (value <= 0)
                    ExceptionHelper.Throw(new RangeError("TableMaximumColumnWidth needs to be at least 1"));
                _tableMaxColWidth = value;
            }
        }

        [Hidden]
        public virtual TextWriter GetLogger(LogLevel ll)
        {
            if (ll == LogLevel.Error)
                return Console.Error;
            else
                return Console.Out;
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
            // Console.Clear();

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

        public virtual JSValue debug(Arguments args)
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

        public virtual JSValue table(Arguments args)
        {
            if (_lineSplitter == null)
                _lineSplitter = new Regex("\r\n?|\n");

            if (args[0] == null)
                return log(args);

            var a = args[0].Value as BaseLibrary.Array;
            if (a == null)
                return log(args);

            int len = (int)a.length;
            if (len == 0)
                return log(args);


            HashSet<string> filter = null;
            if (args[1] != null)
            {
                var f = args[1].Value as BaseLibrary.Array;
                if (f != null && (int)f.length > 0)
                {
                    filter = new HashSet<string>();
                    int fLen = (int)f.length;
                    for (int i = 0; i < fLen; i++)
                        filter.Add(Tools.JSValueToString(f[i]));
                }
            }


            var cols = new SortedDictionary<string, int>(); // name of col -> its width
            var rows = new List<Dictionary<string, string[]>>(); // name of cols -> its data as lines of string
            string indexName = "(index)";
            int indexWidth = indexName.Length;

            for (int i = 0; i < len; i++)
            {
                var item = a[i];
                if (item == null || item._valueType != JSValueType.Object)
                    continue;

                var d = new Dictionary<string, string[]>();
                foreach (var prop in item)
                {
                    if (filter != null && !filter.Contains(prop.Key))
                        continue;

                    string colName = prop.Key ?? "";

                    if (!cols.ContainsKey(colName))
                        cols.Add(colName, colName.Length);

                    int colWidth = cols[colName];
                    int colMaxWidth = System.Math.Max(cols[colName], _tableMaxColWidth);

                    string[] splits = _lineSplitter.Split(Tools.JSValueToObjectString(prop.Value, 0));
                    var lines = new List<string>(splits.Length);
                    foreach (var line in splits)
                    {
                        if (line.Length <= colMaxWidth)
                        {
                            colWidth = System.Math.Max(colWidth, line.Length);
                            lines.Add(line);
                        }
                        else
                        {
                            colWidth = colMaxWidth;
                            int p = 0;
                            while (p < line.Length)
                            {
                                lines.Add(line.Substring(p, System.Math.Min(colWidth, line.Length - p)));
                                p += lines.Last().Length;
                            }
                        }
                    }

                    cols[colName] = colWidth;
                    d.Add(prop.Key, lines.ToArray());
                }

                indexWidth = System.Math.Max(indexWidth, i.ToString().Length);
                d.Add(indexName, new string[] { i.ToString() });
                rows.Add(d);
            }

            if (rows.Count == 0 || cols.Count == 0)
                return log(args);

            List<string> colsN = new List<string> { indexName };
            if (filter != null)
                colsN.AddRange(filter.Where((x) => cols.ContainsKey(x)));
            else
                colsN.AddRange(cols.Select((x) => x.Key));
            cols.Add(indexName, indexWidth);
            int columns = colsN.Count;

            var s = new StringBuilder();

            // top line
            s.Append("+-");
            for (int i = 0; i < columns; i++)
            {
                if (i > 0)
                    s.Append("-+-");
                int colWidth = cols[colsN[i]];
                s.Append(new string('-', colWidth));
            }
            s.Append("-+\n");

            // header
            s.Append("| ");
            for (int i = 0; i < columns; i++)
            {
                if (i > 0)
                    s.Append(" | ");
                int colWidth = cols[colsN[i]];
                s.Append(colsN[i].PadRight(colWidth));
            }
            s.Append(" |\n");

            // middle line
            s.Append("+-");
            for (int i = 0; i < columns; i++)
            {
                if (i > 0)
                    s.Append("-+-");
                s.Append(new string('-', cols[colsN[i]]));
            }
            s.Append("-+\n");

            // body
            for (int r = 0; r < rows.Count; r++)
            {
                var row = rows[r];
                bool hasNextLine = true;
                int offset = 0;
                while (hasNextLine)
                {
                    hasNextLine = false;

                    s.Append("| ");
                    for (int i = 0; i < columns; i++)
                    {
                        if (i > 0)
                            s.Append(" | ");
                        string colName = colsN[i];
                        int colWidth = cols[colName];
                        string line = "";

                        if (row.ContainsKey(colName))
                        {
                            var lines = row[colName];
                            if (offset < lines.Length)
                                line = lines[offset];
                            if (lines.Length > offset + 1)
                                hasNextLine = true;
                        }

                        s.Append(line.PadRight(colWidth));
                    }
                    s.Append(" |\n");

                    offset++;
                }
            }

            // bottom line
            s.Append("+-");
            for (int i = 0; i < colsN.Count; i++)
            {
                if (i > 0)
                    s.Append("-+-");
                s.Append(new string('-', cols[colsN[i]]));
            }
            s.Append("-+");


            LogMessage(LogLevel.Log, s.ToString());

            return JSValue.undefined;
        }

        public JSValue trace(Arguments args)
        {
            Context c = Context.CurrentContext;

            StringBuilder s = new StringBuilder();

            int i = 0;
            while (c != null)
            {
                if (c._parent == null) // GlobalContext
                    break;

                Function owner = c._owner;
                if (owner == null)
                    break;

                if (i++ > 0)
                    s.AppendLine();

                s.Append(owner.name);
                if (owner._functionDefinition != null)
                {
                    if (owner._functionDefinition.Length > 0)
                        s.Append(" @" + owner._functionDefinition.Position);
                }

                c = c._parent;
            }

            if (args != null && args.length > 0)
            {
                group(args);
                LogMessage(LogLevel.Log, s.ToString());
                groupEnd(args);
            }
            else
            {
                LogMessage(LogLevel.Log, s.ToString());
            }

            return JSValue.undefined;
        }

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

        public virtual JSValue dirxml(Arguments args)
        {
            return dir(args);
        }

        public virtual JSValue group(Arguments args)
        {
            string label = Tools.FormatArgs(args);
            if (string.IsNullOrEmpty(label))
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

        public virtual JSValue groupCollapsed(Arguments args)
        {
            return group(args);
        }

        public virtual JSValue groupEnd(Arguments args)
        {
            if (_groups.Count > 0)
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
