using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace ScriptEngine.HostedScript.Library.Xml
{
    [GlobalContext(Category="Функции работы с XML")]
    public class XmlGlobalFunctions : IRuntimeContextInstance, IAttachableContext
    {
        [ContextMethod("XMLСтрока", "XMLString")]
        public string XMLString(IValue value)
        {
            switch(value.DataType)
            {
                case DataType.Undefined:
                    return "";
                case DataType.Boolean:
                    return XmlConvert.ToString(value.AsBoolean());
                case DataType.Date:
                    return XmlConvert.ToString(value.AsDate(), XmlDateTimeSerializationMode.Unspecified);
                case DataType.Number:
                    return XmlConvert.ToString(value.AsNumber());
                default:
                    
                    if(value.SystemType.Equals(TypeManager.GetTypeByFrameworkType(typeof(BinaryDataContext))))
                    {
                        var bdc = value.GetRawValue() as BinaryDataContext;
                        System.Diagnostics.Debug.Assert(bdc != null);

                        return Convert.ToBase64String(bdc.Buffer, Base64FormattingOptions.InsertLineBreaks);
                    }
                    else
                    {
                        return value.GetRawValue().AsString();
                    }

            }
        }

        [ContextMethod("XMLЗначение", "XMLValue")]
        public IValue XMLValue(IValue givenType, string presentation)
        {
            var typeValue = TypeManager.GetTypeDescriptorFor(givenType.GetRawValue());

            if(typeValue.Equals(TypeDescriptor.FromDataType(DataType.Boolean)))
            {
                return ValueFactory.Create(XmlConvert.ToBoolean(presentation));
            }
            else if (typeValue.Equals(TypeDescriptor.FromDataType(DataType.Date)))
            {
                return ValueFactory.Create(XmlConvert.ToDateTime(presentation, XmlDateTimeSerializationMode.Unspecified));
            }
            else if (typeValue.Equals(TypeDescriptor.FromDataType(DataType.Number)))
            {
                return ValueFactory.Create(XmlConvert.ToDecimal(presentation));
            }
            else if (typeValue.Equals(TypeDescriptor.FromDataType(DataType.String)))
            {
                return ValueFactory.Create(presentation);
            }
            else if (typeValue.Equals(TypeDescriptor.FromDataType(DataType.Undefined)) && presentation == "")
            {
                return ValueFactory.Create();
            }
            else
            {
                throw RuntimeException.InvalidArgumentValue();
            }

        }
        

        private static ContextMethodsMapper<XmlGlobalFunctions> _methods = new ContextMethodsMapper<XmlGlobalFunctions>();
        
        public static IAttachableContext CreateInstance()
        {
            return new XmlGlobalFunctions();
        }

        #region IRuntimeContextInstance members

        public bool IsIndexed
        {
            get { return false; }
        }

        public bool DynamicMethodSignatures
        {
            get { return false; }
        }

        public IValue GetIndexedValue(IValue index)
        {
            throw new NotImplementedException();
        }

        public void SetIndexedValue(IValue index, IValue val)
        {
            throw new NotImplementedException();
        }

        public int FindProperty(string name)
        {
            throw RuntimeException.PropNotFoundException(name);
        }

        public bool IsPropReadable(int propNum)
        {
            return false;
        }

        public bool IsPropWritable(int propNum)
        {
            return false;
        }

        public IValue GetPropValue(int propNum)
        {
            throw new NotImplementedException();
        }

        public void SetPropValue(int propNum, IValue newVal)
        {
            throw new NotImplementedException();
        }

        public int FindMethod(string name)
        {
            return _methods.FindMethod(name);
        }

        public MethodInfo GetMethodInfo(int methodNumber)
        {
            return _methods.GetMethodInfo(methodNumber);
        }

        public void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            _methods.GetMethod(methodNumber)(this, arguments);
        }

        public void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            retValue = _methods.GetMethod(methodNumber)(this, arguments);
        }

        #endregion

        #region IAttachableContext members

        public void OnAttach(MachineInstance machine, out IVariable[] variables, out MethodInfo[] methods, out IRuntimeContextInstance instance)
        {
            variables = new IVariable[0];
            methods = GetMethods().ToArray();
            instance = this;
        }

        public IEnumerable<VariableInfo> GetProperties()
        {
            return new VariableInfo[0];
        }

        public IEnumerable<MethodInfo> GetMethods()
        {
            var array = new MethodInfo[_methods.Count];
            for (int i = 0; i < _methods.Count; i++)
            {
                array[i] = _methods.GetMethodInfo(i);
            }

            return array;
        }

        #endregion
    }
}
