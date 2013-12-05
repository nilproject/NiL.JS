
namespace NiL.JS.Core
{
    internal interface IOptimizable
    {
        /// <summary>
        /// Заставляет объект перестроить своё содержимое для ускорения работы
        /// </summary>
        /// <param name="_this">Ссылка на экземпляр, для которого происходит вызов функции</param>
        /// <param name="depth">Глубина рекурсивного погружения, отсчитываемая от нуля</param>
        /// <returns>true если были внесены изменения</returns>
        bool Optimize(ref Statement _this, int depth, System.Collections.Generic.HashSet<string> varibles);
    }
}
