using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NiL.JS.Core
{
    [Serializable]
    internal sealed class GenericType : Type, IEnumerable
    {
        private class Pinfo : ParameterInfo
        {
            private string name;

            public override string Name
            {
                get
                {
                    return name;
                }
            }

            public override Type ParameterType
            {
                get
                {
                    return typeof(Type);
                }
            }

            public Pinfo(string name, int position)
            {
                this.name = name;
                PositionImpl = position;
            }
        }

        private class Ctor : ConstructorInfo
        {
            private GenericType owner;
            private Type target;

            public Ctor(GenericType owner, Type target)
            {
                this.owner = owner;
                this.target = target;
            }

            public override object Invoke(BindingFlags invokeAttr, Binder binder, object[] parameters, System.Globalization.CultureInfo culture)
            {
                Type[] args = new Type[parameters.Length];
                for (var i = 0; i < args.Length; i++)
                    args[i] = parameters[i] as Type;
                return TypeProxy.GetConstructor(target.MakeGenericType(args));
            }

            public override Type DeclaringType
            {
                get { throw new NotImplementedException(); }
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                return new object[0];
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                return Activator.CreateInstance(attributeType.MakeArrayType(), new object[]{ 0 }) as object[];
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
                get { return owner; }
            }

            public override MethodAttributes Attributes
            {
                get { throw new NotImplementedException(); }
            }

            public override MethodImplAttributes GetMethodImplementationFlags()
            {
                throw new NotImplementedException();
            }

            public override ParameterInfo[] GetParameters()
            {
                var pa = target.GetGenericArguments();
                var res = new ParameterInfo[pa.Length];
                for (var i = 0; i < pa.Length; i++)
                    res[i] = new Pinfo(pa[i].Name, i);
                return res;
            }

            public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }

            public override RuntimeMethodHandle MethodHandle
            {
                get { throw new NotImplementedException(); }
            }

            public override string ToString()
            {
                var res = "Void .ctor(";
                var pa = target.GetGenericArguments();
                for (var i = 0; i < pa.Length; i++)
                    res += typeof(Type);
                res += ")";
                return res;
            }
        }

        private class Record
        {
            public Type type;
            public ConstructorInfo constructor;
        }

        private BinaryTree<int, Record> types;
        private string name;

        private void check()
        {
            if (types.Count == 0)
                throw new NotSupportedException("Collection of types is empty");
        }

        public void Add(Type type)
        {
            types.Add(type.GetGenericArguments().Length, new Record { type = type, constructor = new Ctor(this, type) });
        }

        public IEnumerator GetEnumerator()
        {
            foreach (var t in types)
                yield return t.Value.type;
        }

        public GenericType(string name)
        {
            types = new BinaryTree<int, Record>();
            this.name = name;
        }

        public override System.Reflection.Assembly Assembly
        {
            get
            {
                return null;
            }
        }

        public override string AssemblyQualifiedName
        {
            get
            {
                return null;
            }
        }

        public override Type BaseType
        {
            get
            {
                return null;
            }
        }

        public override bool ContainsGenericParameters
        {
            get
            {
                check();
                return false;
            }
        }

        public override System.Reflection.MethodBase DeclaringMethod
        {
            get
            {
                check();
                return types.First().Value.type.DeclaringMethod;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                check();
                return types.First().Value.type.DeclaringType;
            }
        }

        public override Type[] FindInterfaces(System.Reflection.TypeFilter filter, object filterCriteria)
        {
            check();
            return System.Type.EmptyTypes;
        }

        public override System.Reflection.MemberInfo[] FindMembers(System.Reflection.MemberTypes memberType, System.Reflection.BindingFlags bindingAttr, System.Reflection.MemberFilter filter, object filterCriteria)
        {
            List<MemberInfo> res = new List<MemberInfo>();
            foreach (var record in types.Values)
            {
                if (filter == null || filter(record.constructor, filterCriteria))
                    res.Add(record.constructor);
            }
            return res.ToArray();
        }

        public override string FullName
        {
            get
            {
                return name;
            }
        }

        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            return TypeAttributes.AutoClass | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.Serializable | TypeAttributes.SpecialName;
        }

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            if (bindingAttr != (BindingFlags.Public | BindingFlags.Instance))
                return null;
            for (var i = 0; i < types.Length; i++)
            {
                if (!(types[i] is Type))
                    return null;
            }
            foreach (var record in this.types.Values)
            {
                if (record.type.GetGenericArguments().Length == types.Length)
                    return record.constructor;
            }
            return null;
        }

        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            var res = new ConstructorInfo[types.Count];
            int i = 0;
            foreach (var r in types)
                res[i++] = r.Value.constructor;
            return res;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return new object[0];
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return new object[0];
        }

        public override Type GetElementType()
        {
            throw new NotSupportedException();
        }

        public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
        {
            return null;
        }

        public override EventInfo[] GetEvents(BindingFlags bindingAttr)
        {
            return new EventInfo[0];
        }

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            return null;
        }

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            return null;
        }

        public override Type[] GetGenericArguments()
        {
            return new Type[0];
        }

        public override Type[] GetGenericParameterConstraints()
        {
            return new Type[0];
        }

        public override Type GetGenericTypeDefinition()
        {
            return null;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override Type GetInterface(string name, bool ignoreCase)
        {
            return null;
        }

        public override InterfaceMapping GetInterfaceMap(Type interfaceType)
        {
            return base.GetInterfaceMap(interfaceType);
        }

        public override Type[] GetInterfaces()
        {
            return new Type[0];
        }

        public override MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
        {
            if (name == ".ctor")
                return GetConstructors(bindingAttr);
            else return null;
        }

        public override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
        {
            return null;
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            return GetConstructors(bindingAttr);
        }

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            //if (name == ".ctor")
            //    return GetConstructorImpl(bindingAttr, binder, callConvention, types, modifiers);
            return null;
        }

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            return null;
        }

        public override Type GetNestedType(string name, BindingFlags bindingAttr)
        {
            return null;
        }

        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            return new Type[0];
        }

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            return new PropertyInfo[0];
        }

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            return null;
        }

        public override Guid GUID
        {
            get { return Guid.Empty; }
        }

        protected override bool HasElementTypeImpl()
        {
            return false;
        }

        public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, System.Globalization.CultureInfo culture, string[] namedParameters)
        {
            throw new NotImplementedException();
        }

        protected override bool IsArrayImpl()
        {
            return false;
        }

        public override bool IsAssignableFrom(Type c)
        {
            return false;
        }

        protected override bool IsByRefImpl()
        {
            return true;
        }

        protected override bool IsCOMObjectImpl()
        {
            return false;
        }

        protected override bool IsContextfulImpl()
        {
            return true;
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return false;
        }

        protected override bool IsPointerImpl()
        {
            return false;
        }

        protected override bool IsPrimitiveImpl()
        {
            return false;
        }

        public override string Namespace
        {
            get { return ""; }
        }

        public override string Name
        {
            get { return name; }
        }

        public override Module Module
        {
            get { return null; }
        }

        public override Type UnderlyingSystemType
        {
            get { return this; }
        }
    }
}
