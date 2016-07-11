using System;

namespace ExamplesFramework
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class LevelAttribute : Attribute
    {
        public int Level { get; private set; }

        public LevelAttribute(int level)
        {
            this.Level = level;
        }        
    }
}
