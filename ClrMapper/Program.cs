using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClrMapper
{
    class Program
    {
        static void Main(string[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == "-la")
                {
                    var assemblyPath = args[++i];
                    try
                    {
                        try
                        {
#pragma warning disable CS0618 // Тип или член устарел
                            Assembly.LoadWithPartialName(assemblyPath);
#pragma warning restore CS0618 // Тип или член устарел
                        }
                        catch
                        {
                            Assembly.Load(assemblyPath);
                        }
                    }
                    catch
                    {
                        try
                        {
                            Assembly.LoadFile(assemblyPath);
                        }
                        catch
                        {

                        }
                    }
                }
                else if (args[i] == "-ns")
                {
                    var ns = args[++i];
                    buildProxies(ns);
                }
            }
        }

        private static void buildProxies(string ns)
        {
            var subNameSpaces = new HashSet<string>();
            var types = new List<string>();
            var dir = Directory.CreateDirectory(ns.Replace('.', '/').Replace('+', '/'));

            foreach (var type in NiL.JS.NamespaceProvider.GetTypesByPrefix(ns))
            {
                if (type.Namespace == ns)
                {
                    buildProxy(type);
                    if (!type.IsNested)
                        types.Add(type.Name);
                }
                else
                {
                    if (type.Namespace.Length > ns.Length
                        && (type.Namespace[ns.Length] == '.' || type.Namespace[ns.Length] == '+'))
                    {
                        var separatorIndex = type.Namespace.IndexOf('.', ns.Length + 1);
                        if (separatorIndex == -1)
                            separatorIndex = type.Namespace.IndexOf('+', ns.Length + 1);
                        if (separatorIndex == -1)
                            separatorIndex = type.Namespace.Length;

                        subNameSpaces.Add(type.Namespace.Substring(0, separatorIndex));
                    }
                }
            }

            using (var nsFileStream = new FileStream(dir.FullName + ".js", FileMode.Create))
            using (var nsFile = new StreamWriter(nsFileStream))
            {
                for (var i = 0; i < types.Count; i++)
                {
                    nsFile.Write("import { ");
                    nsFile.Write(types[i]);
                    nsFile.Write(" } from \"");
                    nsFile.Write(ns.Substring(ns.LastIndexOf('.') + 1));
                    nsFile.Write("/");
                    nsFile.Write(types[i]);
                    nsFile.WriteLine("\";");
                }

                nsFile.WriteLine();

                nsFile.WriteLine("export {");
                for (var i = 0; i < types.Count; i++)
                {
                    if (i > 0)
                        nsFile.WriteLine(",");
                    nsFile.Write("  ");
                    nsFile.Write(types[i]);
                }
                nsFile.WriteLine();
                nsFile.WriteLine("}");
            }
        }

        private static void buildProxy(Type type)
        {
            var fileName = type.FullName.Replace('.', '/').Replace('+', '/') + ".js";
            Directory.GetParent(fileName).Create();
            var bindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

            using (var file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(file, Encoding.UTF8, 256))
            {
                var needNl = false;
                foreach (var nestedType in type.GetNestedTypes(bindingFlags))
                {
                    needNl = true;
                    writer.Write("import { ");
                    writer.Write(nestedType.Name);
                    writer.Write(" } from \"");
                    writer.Write(nestedType.Name);
                    writer.WriteLine("\";");
                }

                if (needNl)
                    writer.WriteLine();

                writer.Write("export class ");
                writer.Write(type.Name);
                writer.WriteLine(" {");

                foreach (var nestedType in type.GetNestedTypes(bindingFlags))
                {
                    writer.Write("  static get ");
                    writer.Write(nestedType.Name);
                    writer.WriteLine("() {");
                    writer.Write("    return ");
                    writer.Write(nestedType.Name);
                    writer.WriteLine(";");
                    writer.WriteLine("  }");
                    writer.WriteLine();
                }

                var properties = new HashSet<string>();
                foreach (var property in type.GetProperties(bindingFlags))
                {
                    if (properties.Contains(property.Name))
                        continue;
                    properties.Add(property.Name);

                    var getMethod = property.GetGetMethod(false);
                    var setMethod = property.GetSetMethod(false);

                    if (getMethod != null)
                    {
                        if (getMethod.IsStatic)
                            writer.Write("  static get ");
                        else
                            writer.Write("  get ");

                        writer.Write(property.Name); writer.WriteLine("() {");
                        writer.Write("    return this.$$entity."); writer.Write(property.Name); writer.WriteLine(";");
                        writer.WriteLine("  }");
                        writer.WriteLine();
                    }

                    if (setMethod != null)
                    {
                        if (setMethod.IsStatic)
                            writer.Write("  static set ");
                        else
                            writer.Write("  set ");

                        writer.Write(property.Name); writer.WriteLine("(value) {");
                        writer.Write("    this.$$entity.");
                        writer.Write(property.Name);
                        if (property.PropertyType.IsPrimitive)
                            writer.WriteLine(" = value;");
                        else
                            writer.WriteLine(" = value && (value.$$entity || value);");
                        writer.WriteLine("  }");
                        writer.WriteLine();
                    }
                }

                writer.WriteLine("  constructor(...params) {");
                writer.Write("    this.$$entity = $nil.GetCtor('"); writer.Write(type.FullName); writer.WriteLine("')(...params);");
                writer.WriteLine("  }");
                writer.WriteLine();

                var methods = new HashSet<string>();
                var prevLen = 0L;
                var skip = false;
                writer.Flush();
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).OrderBy(x => x.Name))
                {
                    if (method.IsSpecialName && !method.Name.StartsWith("add_") && !method.Name.StartsWith("remove_"))
                        continue;

                    if (methods.Contains(method.Name))
                    {
                        if (skip)
                            continue;
                        skip = true;
                        file.Position = prevLen;
                    }
                    else
                    {
                        skip = false;
                        methods.Add(method.Name);
                    }

                    prevLen = file.Position;

                    if (method.IsStatic)
                        writer.Write("  static ");
                    else
                        writer.Write("  ");

                    writer.Write(method.Name);
                    writer.Write("(");

                    if (skip)
                    {
                        writer.Write("...prms");
                    }
                    else
                    {
                        var first = true;
                        foreach (var prm in method.GetParameters())
                        {
                            if (!first)
                                writer.Write(", ");
                            first = false;
                            writer.Write("/*"); writer.Write(prm.ParameterType); writer.Write("*/ ");
                            writer.Write(prm.Name);
                        }
                    }

                    writer.WriteLine(") {");

                    if (method.ReturnType != typeof(void))
                        writer.Write("    return ");
                    else
                        writer.Write("    ");
                    writer.Write("this.$$entity.");
                    writer.Write(method.Name);
                    writer.Write("(");

                    if (skip)
                    {
                        writer.Write("...prms");
                    }
                    else
                    {
                        var first = true;
                        foreach (var prm in method.GetParameters())
                        {
                            if (!first)
                                writer.Write(", ");
                            first = false;
                            if (prm.ParameterType.IsPrimitive)
                                writer.Write(prm.Name);
                            else
                                writer.Write($"{prm.Name} && ({prm.Name}.$$entity || {prm.Name})");
                        }
                    }

                    writer.WriteLine(");");

                    writer.WriteLine("  }");
                    writer.WriteLine();

                    writer.Flush();
                }

                writer.WriteLine("}");
                writer.Flush();
                writer.BaseStream.SetLength(writer.BaseStream.Position);
            }
        }
    }
}
