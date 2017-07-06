using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NiL.JS.Core;
using NiL.JS.Core.Functions;

#if !NETCORE
namespace NiL.JS.BaseLibrary
{
	/// <summary>
	/// A console modelled after the Console Living Standard.
	/// https://console.spec.whatwg.org
	/// </summary>
	public class console
    {

        public enum LogLevel
        {
            Log = 0,
            Info = 1,
            Warn = 2,
            Error = 3
        }

        private string _indentChar = "|   ";
        private string _indentGroupBegin = "|---# {0}";
        private string _indentGroupBegin0 = "# {0}";

        private GlobalContext _globalContext;
        private JSObject _obj;

        private TextWriter[] _textWriters;

        private Dictionary<string, int> _counters = new Dictionary<string, int>();
        private List<string> _groups = new List<string>();
        private Dictionary<string, Stopwatch> _timers = new Dictionary<string, Stopwatch>();


        public GlobalContext GlobalContext
        {
            get
            {
                return _globalContext;
            }
        }

        public JSObject ConsoleObject
        {
            get
            {
                return _obj;
            }
        }

        
        internal console(GlobalContext globalContext)
            :this(globalContext, null, null, null, null)
        {
        }
        internal console(GlobalContext globalContext, TextWriter logTw, TextWriter infoTw, TextWriter warnTw, TextWriter errorTw)
        {
            this._globalContext = globalContext;
            this._textWriters = new TextWriter[4] { logTw, infoTw, warnTw, errorTw };
            CreateConsoleObject();
        }

        internal void CreateConsoleObject()
        {
            _obj = JSObject.CreateObject();

            _obj["assert"] = new ExternalFunction(this.assert);
            _obj["clear"] = new ExternalFunction(this.clear);
            _obj["count"] = new ExternalFunction(this.count);
            _obj["debug"] = new ExternalFunction(this.debug);
            _obj["error"] = new ExternalFunction(this.error);
            _obj["info"] = new ExternalFunction(this.info);
            _obj["log"] = new ExternalFunction(this.log);
            //_obj["table"] = new ExternalFunction(this.table);
            //_obj["trace"] = new ExternalFunction(this.trace);
            _obj["warn"] = new ExternalFunction(this.warn);
            _obj["dir"] = new ExternalFunction(this.dir);
            //_obj["dirxml"] = new ExternalFunction(this.dirxml);
            
            _obj["group"] = new ExternalFunction(this.group);
            _obj["groupCollapsed"] = new ExternalFunction(this.groupCollapsed);
            _obj["groupEnd"] = new ExternalFunction(this.groupEnd);

            _obj["time"] = new ExternalFunction(this.time);
            _obj["timeEnd"] = new ExternalFunction(this.timeEnd);
        }

        
        public TextWriter GetLogger(LogLevel ll)
        {
            int lli = (int)ll;

            TextWriter res = null;
            if (lli >= 0 && lli < _textWriters.Length)
                res = _textWriters[lli];

            if (res == null)
            {
                if (ll == LogLevel.Error)
                    res = System.Console.Error;
                else
                    res = System.Console.Out;
            }

            return res;
        }
        public void SetLogger(LogLevel ll, TextWriter value)
        {
            int lli = (int)ll;
            if (lli >= 0 && lli < _textWriters.Length)
                _textWriters[lli] = value;
        }
        
        public void LogArguments(LogLevel level, Arguments args)
        {
            LogArguments(level, args, 0, args.length, true);
        }
        public void LogArguments(LogLevel level, Arguments args, int argsStart)
        {
            LogArguments(level, args, argsStart, args.length, true);
        }
        public void LogArguments(LogLevel level, Arguments args, int argsStart, int argsStop, bool formatted)
        {
            if (args.length == 0 || args.length <= argsStart)
                return;

            LogMessage(level, Tools.FormatArgs(args, argsStart, argsStop, formatted));
        }
        public void LogMessage(LogLevel level, string message)
        {
            Print(level, GetLogger(level), message, _groups.Count);
        }
        
        public void Print(LogLevel level, TextWriter textWriter, string message)
        {
            Print(level, textWriter, message, 0);
        }
        public void Print(LogLevel level, TextWriter textWriter, string message, int indent)
        {
            if (message == null || textWriter == null)
                return;

            if (indent > 0)
            {
                string ind = "";
                for (int j = 0; j < indent; j++)
                    ind += _indentChar;

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


        public JSValue assert(JSValue thisBind, Arguments args)
        {
            if (!(bool)args[0])
                LogArguments(LogLevel.Log, args, 1);
            return JSValue.undefined;
        }

        public JSValue clear(JSValue thisBind, Arguments args)
        {
            _groups.Clear();
            //System.Console.Clear();

            return JSValue.undefined;
        }

        public JSValue count(JSValue thisBind, Arguments args)
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

        public JSValue debug(JSValue thisBind, Arguments args)
        {
            LogArguments(LogLevel.Log, args);
            return JSValue.undefined;
        }
        
        public JSValue error(JSValue thisBind, Arguments args)
        {
            LogArguments(LogLevel.Error, args);
            return JSValue.undefined;
        }
        
        public JSValue info(JSValue thisBind, Arguments args)
        {
            LogArguments(LogLevel.Info, args);
            return JSValue.undefined;
        }
        
        public JSValue log(JSValue thisBind, Arguments args)
        {
            LogArguments(LogLevel.Log, args);
            return JSValue.undefined;
        }

        //public JSValue table(JSValue thisBind, Arguments args)
        //{
        //    return JSValue.undefined;
        //}

        //public JSValue trace(JSValue thisBind, Arguments args)
        //{
        //    return JSValue.undefined;
        //}

        public JSValue warn(JSValue thisBind, Arguments args)
        {
            LogArguments(LogLevel.Warn, args);
            return JSValue.undefined;
        }
        
        public JSValue dir(JSValue thisBind, Arguments args)
        {
            LogMessage(LogLevel.Log, Tools.JSValueToObjectString(args[0]));
            return JSValue.undefined;
        }

        //public JSValue dirxml(JSValue thisBind, Arguments args)
        //{
        //    return JSValue.undefined;
        //}


        public JSValue group(JSValue thisBind, Arguments args)
        {
            string label = Tools.FormatArgs(args) ?? "null";
            if (label == "")
                label = "console.group";

            if (_groups.Count > 0)
            {
                string _temp = _groups[_groups.Count - 1];
                _groups.RemoveAt(_groups.Count - 1);
                LogMessage(LogLevel.Info, System.String.Format(_indentGroupBegin, label));
                _groups.Add(_temp);
            }
            else
                LogMessage(LogLevel.Info, System.String.Format(_indentGroupBegin0, label));
            
            _groups.Add(label);

            return JSValue.undefined;
        }

        public JSValue groupCollapsed(JSValue thisBind, Arguments args)
        {
            group(thisBind, args);
            return JSValue.undefined;
        }

        public JSValue groupEnd(JSValue thisBind, Arguments args)
        {
            if (_groups.Count == 0)
                _groups.RemoveAt(_groups.Count - 1);
            
            return JSValue.undefined;
        }


        public JSValue time(JSValue thisBind, Arguments args)
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

        public JSValue timeEnd(JSValue thisBind, Arguments args)
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

        

        internal static List<console> _consoles;
        internal static void AddConsole(console console)
        {
            if (_consoles == null)
                _consoles = new List<console>(4);
            _consoles.Add(console);
        }
        internal static void RemoveConsole(console console)
        {
            if (_consoles == null)
                _consoles = new List<console>(4);
            _consoles.Add(console);
        }
        public static console GetConsole(GlobalContext globalContext)
        {
            if (_consoles == null)
                return null;
            for (int i = 0; i < _consoles.Count; i++)
            {
                if (_consoles[i]._globalContext == globalContext)
                    return _consoles[i];
            }
            return null;
        }

        public static JSValue CreateConsoleObject(GlobalContext globalContext)
        {
            return CreateConsoleObject(CreateConsole(globalContext));
        }
        public static JSValue CreateConsoleObject(GlobalContext globalContext, TextWriter logTw, TextWriter infoTw, TextWriter warnTw, TextWriter errorTw)
        {
            return CreateConsoleObject(CreateConsole(globalContext, logTw, infoTw, warnTw, errorTw));
        }
        public static JSValue CreateConsoleObject(console console)
        {
            return console.ConsoleObject;
        }

        public static console CreateConsole(GlobalContext globalContext)
        {
            console c = GetConsole(globalContext);
            if (c != null)
                return c;

            c = new console(globalContext);
            AddConsole(c);

            return c;
        }
        public static console CreateConsole(GlobalContext globalContext, TextWriter logTw, TextWriter infoTw, TextWriter warnTw, TextWriter errorTw)
        {
            console c = GetConsole(globalContext);
            if (c != null)
            {
                c._textWriters = new TextWriter[4] { logTw, infoTw, warnTw, errorTw };
                return c;
            }

            c = new console(globalContext, logTw, infoTw, warnTw, errorTw);
            AddConsole(c);

            return c;
        }

    }
}
#endif
