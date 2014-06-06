using System;
using System.Collections.Generic;

namespace NiL.JS.Core
{
    [Serializable]
    public abstract class Statement
    {
        private static readonly Statement[] emptyArray = new Statement[0];

        public virtual int Position { get; internal set; }
        public virtual int Length { get; internal set; }
        public virtual int EndPosition { get { return Position + Length; } }

        private Statement[] childs;
        public virtual Statement[] Childs { get { return childs ?? (childs = getChildsImpl() ?? emptyArray); } }

        protected abstract Statement[] getChildsImpl();

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
        internal virtual bool Optimize(ref Statement _this, int depth, Dictionary<string, VaribleDescriptor> varibles)
        {
            return false;
        }
    }
}
