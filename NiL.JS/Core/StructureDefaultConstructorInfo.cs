using System;
using System.Reflection;

namespace NiL.JS.Core
{
    internal sealed class StructureDefaultConstructorInfo : ConstructorInfo
    {
        private static readonly ParameterInfo[] prms = new ParameterInfo[0];
        private static readonly object[] emptyArray = new object[0];
        private static readonly object[] prmsAtrbts = new object[1] { 0 };
        private Type structureType;

        public StructureDefaultConstructorInfo(Type type)
        {
            if (type == null)
                throw new ArgumentNullException();
            if (!type.IsValueType)
                throw new ArgumentException("Type is not ValueType.");
            if (type.IsAbstract)
                throw new ArgumentException("Type is abstract.");
            if (type.IsGenericTypeDefinition)
                throw new ArgumentException("Type is generic type.");
            structureType = type;
        }

        public override object Invoke(BindingFlags invokeAttr, Binder binder, object[] parameters, System.Globalization.CultureInfo culture)
        {
            return Activator.CreateInstance(structureType);
        }

        public override MethodAttributes Attributes
        {
            get { return MethodAttributes.HideBySig | MethodAttributes.Public; }
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return MethodImplAttributes.Runtime;
        }

        public override ParameterInfo[] GetParameters()
        {
            return prms;
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, System.Globalization.CultureInfo culture)
        {
            return Activator.CreateInstance(structureType);
        }

        public override RuntimeMethodHandle MethodHandle
        {
            get { return new RuntimeMethodHandle(); }
        }

        public override Type DeclaringType
        {
            get { return structureType; }
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return (object[])Activator.CreateInstance(attributeType.MakeArrayType(), prmsAtrbts);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return emptyArray;
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return false;
        }

        public override string Name
        {
            get { return ".ctor"; }
        }

        public override Type ReflectedType
        {
            get { return structureType; }
        }
    }
}
