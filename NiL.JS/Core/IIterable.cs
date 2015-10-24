using System;
using NiL.JS.Core.Interop;

namespace NiL.JS.Core
{
    public interface IIterable
    {
        IIterator @iterator();
    }
}
