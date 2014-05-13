using System;
using System.Collections.Generic;

namespace NiL.JS
{
    /// <summary>
    /// Предоставляет доступ к указанному при создании пространству имён.
    /// </summary>
    [Serializable]
    public class NamespaceProvider : NiL.JS.Core.EmbeddedType
    {
        private static BinaryTree<Type> types = new BinaryTree<Type>();

        private static void addTypes(System.Reflection.Assembly assembly)
        {
            var types = assembly.GetTypes();
            for (var i = 0; i < types.Length; i++)
                NamespaceProvider.types[types[i].FullName] = types[i];
        }

        static NamespaceProvider()
        {
            AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(CurrentDomain_AssemblyLoad);
            var assms = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assms.Length; i++)
                addTypes(assms[i]);
        }

        static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            addTypes(args.LoadedAssembly);
        }

        public string Namespace { get; private set; }

        /// <summary>
        /// Создаёт экземпляр объекта, предоставляющего доступ к указанному пространству имён.
        /// </summary>
        /// <param name="namespace">Пространство имён, доступ к которому требуется предоставить.</param>
        public NamespaceProvider(string @namespace)
        {
            Namespace = @namespace;
        }

        public override JS.Core.JSObject GetField(string name, bool fast, bool own)
        {
            string reqname = Namespace + "." + name;
            var selection = types.StartedWith(reqname);
            if (selection.MoveNext())
            {
                if (selection.Current.Key == reqname)
                    return NiL.JS.Core.TypeProxy.GetConstructor(selection.Current.Value);
                return NiL.JS.Core.TypeProxy.Proxy(new NamespaceProvider(reqname));
            }
            return new JS.Core.JSObject();
        }
    }
}
