using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JS.Core.BaseTypes
{
    internal class JSString : JSObject
    {
        private static JSObject charCodeAt(JSObject _this, IContextStatement[] args)
        {
            if (_this.ValueType == ObjectValueType.Object)
                return (int)_this.oValue.ToString()[args[0].Invoke().iValue];
            return (int)(_this.Value as string)[args[0].Invoke().iValue];
        }

        private JSObject _length;
        private JSObject _charCodeAt;

        public JSString()
        {
            ValueType = ObjectValueType.String;
            fieldGetter = (name, b) =>
            {
                switch (name)
                {
                    case "length":
                        {
                            if (_length == null)
                            {
                                _length = new JSObject() { iValue = (oValue as string).Length, ValueType = ObjectValueType.Int };
                                _length.Protect();
                            }
                            return _length;
                        }
                    case "charCodeAt":
                        {
                            if (_charCodeAt == null)
                            {
                                _charCodeAt = new JSObject() { ValueType = ObjectValueType.Statement, oValue = new Statements.ExternalFunction(charCodeAt) };
                                _charCodeAt.Protect();
                            }
                            return _charCodeAt;
                        }
                    default:
                        return DefaultFieldGetter(name, b);
                }
            };
        }
    }
}
