using System;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core;
using System.Collections.Generic;
using System.Collections;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class GetVaribleStatement : Statement
    {
        public sealed class GVSDescriptor : IEnumerable<GetVaribleStatement>
        {
            private string name;
            private Context cacheContext;
            private JSObject cacheRes;
            private HashSet<GetVaribleStatement> items;

            public string Name { get { return name; } }
            public bool Defined { get; internal set; }

            public IEnumerator<GetVaribleStatement> GetEnumerator()
            {
                return items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return items.GetEnumerator();
            }

            internal GVSDescriptor(GetVaribleStatement proto, bool defined)
            {
                this.name = proto.varibleName;
                items = new HashSet<GetVaribleStatement>() { proto };
                Defined = defined;
            }

            internal void Add(GetVaribleStatement item)
            {
                if (item.varibleName != Name)
                    throw new ArgumentException("Try to add " + typeof(GetVaribleStatement).Name + " to union with different name.");
                items.Add(item);
                item.descriptor = this;
            }

            internal JSObject Get(Context context)
            {
                context.objectSource = null;
                if (context == cacheContext)
                    return cacheRes = context.GetField(name);
                lock (this)
                {
                    if (context.GetType() == typeof(WithContext))
                        return context.GetField(name);
                    else
                    {
                        cacheRes = context.GetField(name);
                        cacheContext = context;
                        return cacheRes;
                    }
                }
            }
        }

        private string varibleName;
        private GVSDescriptor descriptor;
        public GVSDescriptor Descriptor { get { return descriptor; } }

        public string VaribleName { get { return varibleName; } }

        internal GetVaribleStatement(string name)
        {
            int i = 0;
            if ((name != "this") && !Parser.ValidateName(name, ref i, false, true, true, false))
                throw new ArgumentException("Invalid varible name");
            this.varibleName = name;
        }

        internal override JSObject InvokeForAssing(Context context)
        {
            if (context.strict)
                return Tools.RaiseIfNotExist(descriptor.Get(context));
            return descriptor.Get(context);
        }

        internal override JSObject Invoke(Context context)
        {
            var res = Tools.RaiseIfNotExist(descriptor.Get(context));
            if (context.GetType() == typeof(WithContext))
            {
                if (res.ValueType == JSObjectType.Property)
                    return (res.oValue as NiL.JS.Core.BaseTypes.Function[])[1].Invoke(context, context.objectSource, null);
                return res;
            }
            return res;
        }

        public override string ToString()
        {
            return varibleName;
        }

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, Statement> varibles)
        {
            Statement desc = null;
            if (!varibles.TryGetValue(varibleName, out desc) || desc == null)
            {
                varibles[varibleName] = desc = this;
                this.descriptor = new GVSDescriptor(this, false);
            }
            else
                (desc as GetVaribleStatement).descriptor.Add(this);
            return false;
        }
    }
}