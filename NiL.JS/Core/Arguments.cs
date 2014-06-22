using System;

namespace NiL.JS.Core
{
    /// <summary>
    /// Фиктивный класс для заполнения поля oValue объектов, хранящих аргументы вызовов функций
    /// </summary>
    [Serializable]
    internal sealed class Arguments
    {
        public static Arguments Instance { get { return new Arguments(); } }
    }
}
