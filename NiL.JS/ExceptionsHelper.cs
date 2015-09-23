using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JS
{
    internal static class ExceptionsHelper
    {
        internal static void Throw(BaseLibrary.Error error)
        {
            throw error.Wrap();
        }
    }
}
