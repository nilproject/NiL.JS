using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.Functions;
using NiL.JS.Core.Interop;
using NiL.JS.Extensions;

namespace NiL.JS
{
    /// <summary>
    /// Provides access to a CLR-namespace
    /// </summary>
#if !NETCORE
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

        private BinaryTree<JSValue> children;

        /// <summary>
        /// Contract NamespacesProvider
        /// </summary>
        public string Namespace { get; private set; }

        /// <summary>
        /// Contract NamespacesProvider
        /// </summary>
        /// <param name="namespace">Namespace</param>
        public NamespaceProvider(string @namespace)
        {
            Namespace = @namespace;
        }

        internal protected override JSValue GetProperty(JSValue key, bool forWrite, PropertyScope memberScope)
        {
            if (memberScope < PropertyScope.Super && key._valueType != JSValueType.Symbol)
            {
                var name = key.ToString();
                JSValue res = null;
                if (children != null && children.TryGetValue(name, out res))
                    return res;
                string reqname = Namespace + "." + name;
                var selection = types.StartsWith(reqname).GetEnumerator();

                Type resultType = null;
                List<Type> ut = null;

                while (selection.MoveNext())
                {
                    if (selection.Current.Value.FullName.Length > reqname.Length 
                        && selection.Current.Value.FullName[reqname.Length] == '`')
                    {
                        string fn = selection.Current.Value.FullName;
                        for (var i = fn.Length - 1; i > reqname.Length; i--)
                        {
                            if (!NumberUtils.IsDigit(fn[i]))
                            {
                                fn = null;
                                break;
                            }
                        }

                        if (fn != null)
                        {
                            if (resultType == null)
                            {
                                resultType = selection.Current.Value;
                            }
                            else
                            {
                                if (ut == null)
                                    ut = new List<Type> { resultType };

                                ut.Add(selection.Current.Value);
                            }
                        }
                    }
                    else if (selection.Current.Value.Name != name)
                        break;
                    else
                        resultType = selection.Current.Value;
                }

                if (ut != null)
                {
                    res = Proxy.GetGenericTypeSelector(ut);

                    if (children == null)
                        children = new BinaryTree<JSValue>();

                    children[name] = res;
                    return res;
                }

                if (resultType != null)
                    return Context.CurrentGlobalContext.GetConstructor(resultType);

                selection = types.StartsWith(reqname).GetEnumerator();
                if (selection.MoveNext() && selection.Current.Key[reqname.Length] == '.')
                {
                    res = new NamespaceProvider(reqname);

                    if (children == null)
                        children = new BinaryTree<JSValue>();

                    children.Add(name, res);
                    return res;
                }
            }

            return undefined;
        }

        public static Type GetType(string name)
        {
            var selection = types.StartsWith(name).GetEnumerator();
            if (selection.MoveNext() && selection.Current.Key == name)
                return selection.Current.Value;
            return null;
        }

        public static IEnumerable<Type> GetTypesByPrefix(string prefix)
        {
            foreach (KeyValuePair<string, Type> type in types.StartsWith(prefix))
                yield return type.Value;
        }

        protected internal override IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator(bool pdef, EnumerationMode enumerationMode, PropertyScope propertyScope = PropertyScope.Common)
        {
            yield break;
        }
    }
#endif
}
