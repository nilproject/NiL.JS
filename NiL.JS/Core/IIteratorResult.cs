using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JS.Core
{
    public interface IIteratorResult
    {
        JSValue value { get; }
        bool done { get; }
    }
}
