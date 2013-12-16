using System;

namespace NiL.JS.Core
{
    public interface IContextStatement
    {
        JSObject Invoke();
        JSObject Invoke(JSObject _this, IContextStatement[] args);
    }
}
