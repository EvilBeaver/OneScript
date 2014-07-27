using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace OneScript.ComponentModel
{
    public class TypeImporter
    {
        TypeManager _manager;
        public TypeImporter(TypeManager manager)
        {
            _manager = manager;
        }

        public DataType ImportType(Type type)
        {
            if (!type.IsDefined(typeof(ImportedClassAttribute), false))
            {
                throw new ArgumentException("Type " + type.ToString() + " is not marked with ContextClass attribute");
            }

            if(!typeof(IValue).IsAssignableFrom(type))
            {
                throw new ArgumentException("Type " + type.ToString() + " is not inherited from IValue");
            }

            var attrib = (ImportedClassAttribute)type.GetCustomAttributes(typeof(ImportedClassAttribute), false)[0];
            string typeName;
            string typeAlias = null;
            if(attrib.Name == null)
            {
                typeName = type.Name;
            }
            else
            {
                typeName = attrib.Name;
                typeAlias = attrib.Alias;
            }

            DataType newType;
            if (attrib.IsSimpleType)
                newType = _manager.RegisterSimpleType(typeName, typeAlias, ImportedClassesConstructor);
            else
                newType = _manager.RegisterObjectType(typeName, typeAlias, ImportedClassesConstructor);

            var constructors = type.GetMethods(BindingFlags.Static)
                .Where((x) => x.IsDefined(typeof(TypeConstructorAttribute), false));

            foreach (var method in constructors)
            {
                if(IsValidConstructor(method))
                {
                    _manager.AddConstructorFor(typeName, method);
                }
            }

            return newType;
        }

        private bool IsValidConstructor(MethodInfo method)
        {
            if(!typeof(IValue).IsAssignableFrom(method.ReturnType))
                return false;

            var parameters = method.GetParameters();
            if(parameters.Length < 1)
                return false;
            
            if(parameters[0].ParameterType == typeof(DataType))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        private static IValue ImportedClassesConstructor(DataType constructedType, IValue[] arguments)
        {
            throw new NotImplementedException();
        }
    }
}
