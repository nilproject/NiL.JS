using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.Interop;

namespace NiL.JS
{
    /// <summary>
    /// Предоставляет доступ к указанному при создании пространству имён.
    /// </summary>
#if !PORTABLE
    [Serializable]
#endif
    public class NamespaceProvider : CustomType
    {
        private static BinaryTree<Type> types = new BinaryTree<Type>();

        private static void addTypes(System.Reflection.Assembly assembly)
        {
            try
            {
                if (assembly is System.Reflection.Emit.AssemblyBuilder)
                    return;
                var types = assembly.GetExportedTypes();
                for (var i = 0; i < types.Length; i++)
                {
                    NamespaceProvider.types[types[i].FullName] = types[i];
                }
            }
            catch
            {

            }
        }

        static NamespaceProvider()
        {
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
            var assms = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assms.Length; i++)
                addTypes(assms[i]);
        }

        static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            addTypes(args.LoadedAssembly);
        }

        private Dictionary<string, GenericType> unions;
        private BinaryTree<JSValue> childs;

        /// <summary>
        /// Пространство имён, доступ к которому предоставляет указанный экземпляр.
        /// </summary>
        public string Namespace { get; private set; }

        /// <summary>
        /// Создаёт экземпляр объекта, предоставляющего доступ к указанному пространству имён.
        /// </summary>
        /// <param name="namespace">Пространство имён, доступ к которому требуется предоставить.</param>
        public NamespaceProvider(string @namespace)
        {
            Namespace = @namespace;
        }

        /// <summary>
        /// Создаёт экземпляр объекта, предоставляющего доступ к указанному пространству имён.
        /// </summary>
        /// <param name="namespace">Пространство имён, доступ к которому требуется предоставить.</param>
        /// <param name="union">Если установлено, одноимённые обобщённые типы будут объеденены в псевдотип под общим названием без указания количества обобщённых аргументов,
        /// конструктор которого будет возвращать соответствующую реализацию обобщённого типа.</param>
        public NamespaceProvider(string @namespace, bool union)
            : this(@namespace)
        {
            if (union)
                unions = new Dictionary<string, GenericType>();
        }

        internal protected override JSValue GetMember(JSValue key, bool forWrite, MemberScope memberScope)
        {
            if (key.valueType != JSValueType.Symbol)
            {
                var name = key.ToString();
                JSValue res = null;
                if (childs != null && childs.TryGetValue(name, out res))
                    return res;
                string reqname = Namespace + "." + name;
                var selection = types.StartedWith(reqname).GetEnumerator();
                if (selection.MoveNext())
                {
                    if (unions != null && selection.Current.Key != reqname && selection.Current.Value.FullName[reqname.Length] == '`')
                    {
                        var ut = new GenericType(reqname);
                        ut.Add(selection.Current.Value);
                        while (selection.MoveNext())
                            if (selection.Current.Value.FullName[reqname.Length] == '`')
                            {
                                string fn = selection.Current.Value.FullName;
                                for (var i = fn.Length - 1; i > reqname.Length; i--)
                                    if (!Tools.IsDigit(fn[i]))
                                    {
                                        fn = null;
                                        break;
                                    }
                                if (fn != null)
                                    ut.Add(selection.Current.Value);
                            }
                        res = TypeProxy.GetConstructor(ut);
                        if (childs == null)
                            childs = new BinaryTree<JS.Core.JSValue>();
                        childs[name] = res;
                        return res;
                    }
                    if (selection.Current.Key == reqname)
                        return TypeProxy.GetConstructor(selection.Current.Value);
                    res = new NamespaceProvider(reqname, unions != null);
                    childs.Add(name, res);
                    return res;
                }
            }
            return base.GetMember(key, forWrite, memberScope);
        }

        /// <summary>
        /// Возвращает тип, доступный в текущем домене по его имени.
        /// </summary>
        /// <param name="name">Имя типа, который необходимо вернуть.</param>
        /// <returns>Запрошенный тип или null, если такой тип не загружен в домен.</returns>
        public static Type GetType(string name)
        {
            var selection = types.StartedWith(name).GetEnumerator();
            if (selection.MoveNext() && selection.Current.Key == name)
                return selection.Current.Value;
            return null;
        }

        /// <summary>
        /// Перечисляет типы, полные имена которых начинаются с указанного префикса.
        /// </summary>
        /// <param name="name">Префикс имен типов.</param>
        public static IEnumerable<Type> GetTypesByPrefix(string prefix)
        {
            foreach (KeyValuePair<string, Type> type in types.StartedWith(prefix))
                yield return type.Value;
        }

        public override IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator(bool pdef, EnumerationMode enumerationMode)
        {
            yield break;
        }
    }
}
