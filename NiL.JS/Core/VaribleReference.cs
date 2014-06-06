using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JS.Core
{
    public abstract class VaribleReference : Statement
    {
        public abstract string Name { get; }
        public abstract VaribleDescriptor Descriptor { get; internal set; }

        protected override Statement[] getChildsImpl()
        {
            return null;
        }
    }
}
