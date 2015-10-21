using System;

namespace NiL.JS.BaseLibrary
{
    public interface IIterator
    {
        NiL.JS.Core.JSValue next(NiL.JS.Core.Arguments args);
        void @throw();
    }
}
