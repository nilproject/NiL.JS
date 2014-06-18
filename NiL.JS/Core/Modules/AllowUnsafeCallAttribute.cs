using System;

namespace NiL.JS.Core.Modules
{
    /// <summary>
    /// Внимание! Будте осторожны при использовании данного аттрибута!
    /// 
    /// Указывает на то, что нестатический метод, 
    /// помеченный данным аттрибутом, способен корректно выполнится, 
    /// будучи вызванным с параметром this указанного типа или производного от него,
    /// включая те случаи, когда указанный тип и тип, объявивший помеченный метод,
    /// не находятся в одной иерархии наследования.
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class AllowUnsafeCallAttribute : Attribute
    {
        internal readonly Type baseType;

        /// <summary>
        /// Альтернативный тип для параметра this.
        /// </summary>
        public Type BaseType { get { return baseType; } }

        /// <summary>
        /// Создаёт экземпляр с указанием типа
        /// </summary>
        /// <param name="type">Тип, который следует включит в список допустимых для параметра this.</param>
        public AllowUnsafeCallAttribute(Type type)
        {
            baseType = type;
        }

        /// <summary>
        /// Метод, вызываемый перед вызовом помеченного метода и возвращающий преобразованный объект, если это требуется.
        /// </summary>
        /// <param name="arg">Объект, который следует преобразовать.</param>
        /// <returns>Результат преобразования.</returns>
        protected internal virtual object Convert(object arg)
        {
            return arg;
        }
    }
}
