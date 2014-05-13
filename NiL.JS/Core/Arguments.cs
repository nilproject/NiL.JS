using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NiL.JS.Core
{
    /// <summary>
    /// Фиктивный класс для заполнения поля oValue объектов, хранящих аргументы вызовов функций
    /// </summary>
    [Serializable]
    internal sealed class Arguments
    {
        public static readonly Arguments Instance = new Arguments();
    }
}
