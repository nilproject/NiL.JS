using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ExamplesFramework
{
    public class ExamplesLoader
    {
        public static IList<KeyValuePair<string, IList<Example>>> LoadExamples(Assembly assembly)
        {
            return assembly
                .GetTypes()
                .Where(x => x.IsSubclassOf(typeof(Example)))
                .GroupBy(x => x.Namespace)
                .OrderBy(x => x.Max(t => t.GetCustomAttribute<LevelAttribute>()?.Level))
                .Select(x => new KeyValuePair<string, IList<Example>>(PrepareExampleName(x.Key.Split('.').Last()), x.Select(Activator.CreateInstance).Cast<Example>().ToArray()))
                .ToArray();
        }

        public static string PrepareExampleName(string name)
        {
            return name
                .Replace("_T_", "<T>")
                .Replace('_', ' ');
        }
    }
}
