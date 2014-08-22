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
        private TypeManager _manager;
        private Dictionary<TypeId, List<MethodInfo>> _constructors = new Dictionary<TypeId, List<MethodInfo>>();

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

            if(!typeof(ComponentBase).IsAssignableFrom(type))
            {
                throw new ArgumentException("Type " + type.ToString() + " is not inherited from ComponentBase");
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

            var constructors = type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where((x) => x.IsDefined(typeof(TypeConstructorAttribute), false));

            foreach (var method in constructors)
            {
                if(IsValidConstructor(method))
                {
                    AddConstructorFor(newType.ID, method);
                }
            }

            return newType;
        }

        private bool IsValidConstructor(MethodInfo method)
        {
            return typeof(IValue).IsAssignableFrom(method.ReturnType);
        }

        private void AddConstructorFor(TypeId id, MethodInfo method)
        {
            List<MethodInfo> constrList;
            if (!_constructors.TryGetValue(id, out constrList))
            {
                constrList = new List<MethodInfo>();
                _constructors[id] = constrList;
            }

            constrList.Add(method);
        }
        
        private IValue CreateInstanceOf(TypeId id, IValue[] arguments)
        {
            List<MethodInfo> constructors;

            if (!_constructors.TryGetValue(id, out constructors))
            {
                throw new EngineException("Конструктор не найден: " + _manager.GetById(id).Name);
            }

            if (arguments == null)
                arguments = new IValue[0];

            int argCount = arguments.Length;

            foreach (var ctor in constructors)
            {
                ParameterInfo[] parameters = ctor.GetParameters();
                List<object> argsToPass = new List<object>();

                bool success = (parameters.Length == 0 && argCount == 0)
                        || (parameters.Length > 0 && parameters[0].ParameterType.IsArray);

                if (parameters.Length > 0 && parameters.Length < argCount
                    && !parameters[parameters.Length - 1].ParameterType.IsArray)
                {
                    success = false;
                    continue;
                }

                for (int i = 0; i < parameters.Length; i++)
                {
                    Type paramType = parameters[i].ParameterType;

                    if (parameters[i].ParameterType.IsArray)
                    {
                        // captures all remained args
                        IValue[] varArgs = new IValue[argCount - i];
                        for (int j = i, k = 0; k < varArgs.Length; j++, k++)
                        {
                            varArgs[k] = arguments[j];
                        }

                        var convertedArgs = varArgs.Select((x) => x.ToCLRType(paramType));
                        try
                        {
                            argsToPass.Add(varArgs.ToArray());
                            success = true;
                            break;
                        }
                        catch (EngineException)
                        {
                            success = false;
                            break;
                        }
                    }
                    else
                    {
                        if (i < argCount)
                        {
                            try
                            {
                                argsToPass.Add(arguments[i].ToCLRType(paramType));
                                success = true;
                            }
                            catch (EngineException)
                            {
                                success = false;
                                break;
                            }
                        }
                        else
                        {
                            success = false;
                            break; // no match
                        }
                    }
                }

                if (success)
                {
                    var instance = (ComponentBase)ctor.Invoke(null, argsToPass.ToArray());
                    instance.SetDataType(_manager.GetById(id));
                    return (IValue)instance;
                }
            }

            throw new EngineException("Конструктор не найден: " + _manager.GetById(id).Name);

        }

        private IValue ImportedClassesConstructor(DataType constructedType, IValue[] arguments)
        {
            return CreateInstanceOf(constructedType.ID, arguments);
        }
    }
}
