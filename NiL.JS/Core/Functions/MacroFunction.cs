using NiL.JS.Core.BaseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JS.Core.Functions
{
    /// <summary>
    /// Ограничения для макрофункций:
    /// * нет инструкции debugger
    /// * нет вложенных вызовов функции
    /// * нет eval, arguments и with
    /// * не используется this
    /// * все используемые переменные и константы либо объявлены внутри функции, либо являются её аргументами
    /// * нет получения и записи значений в поля объектов (по причине возможного вызова getter или setter)
    /// * нет создания других функций
    /// </summary>
    public sealed class MacroFunction : Function
    {
        public override NiL.JS.Core.JSObject Invoke(NiL.JS.Core.JSObject thisBind, NiL.JS.Core.Arguments args)
        {
            return null;
        }
    }
}
