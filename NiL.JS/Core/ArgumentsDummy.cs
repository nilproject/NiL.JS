using System;

namespace NiL.JS.Core
{
    /// <summary>
    /// Фиктивный класс для заполнения поля oValue объектов, хранящих аргументы вызовов функций
    /// </summary>
    [Serializable]
    internal sealed class ArgumentsDummy
    {
        private class Arguments
        {

        }

        public static object Instance { get { return new Arguments(); } }
    }
}
