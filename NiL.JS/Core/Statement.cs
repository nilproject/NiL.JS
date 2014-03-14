using System;

namespace NiL.JS.Core
{
    [Serializable]
    public abstract class Statement
    {
        internal virtual JSObject InvokeForAssing(Context context)
        {
            return Invoke(context);
        }

        internal abstract JSObject Invoke(Context context);

        /// <summary>
        /// Заставляет объект перестроить своё содержимое для ускорения работы
        /// </summary>
        /// <param name="_this">Ссылка на экземпляр, для которого происходит вызов функции</param>
        /// <param name="depth">Глубина рекурсивного погружения, отсчитываемая от нуля</param>
        /// <returns>true если были внесены изменения</returns>
        internal virtual bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> varibles)
        {
            return false;
        }
    }
}
