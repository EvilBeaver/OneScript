using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Xml.Schema;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    public class XSForm : CLREnumValueWrapper<XmlSchemaForm>
    {
        internal XSForm(EnumerationXSForm instance, XmlSchemaForm realValue) : base(instance, realValue )
        {
        }

        public static XSForm FromNativeValue(XmlSchemaForm native) => EnumerationXSForm.FromNativeValue(native);

        public static XmlSchemaForm ToNativeValue(XSForm wrapper) => wrapper.UnderlyingValue;
    }
       
    [SystemEnum("ФормаПредставленияXS", "XSForm")]
    public class EnumerationXSForm : EnumerationContext
    {
        private readonly Dictionary<XmlSchemaForm, XSForm> _valuesCache;

        private EnumerationXSForm(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
            _valuesCache = new Dictionary<XmlSchemaForm, XSForm>
            {
                { XmlSchemaForm.Qualified, new XSForm(this, XmlSchemaForm.Qualified) },
                { XmlSchemaForm.Unqualified, new XSForm(this, XmlSchemaForm.Unqualified) }
            };
        }

        internal static XSForm FromNativeValue(XmlSchemaForm native)
        {
            switch (native)
            {
                case XmlSchemaForm.Qualified:
                case XmlSchemaForm.Unqualified:

                    EnumerationXSForm enumeration = GlobalsManager.GetEnum<EnumerationXSForm>();
                    return enumeration._valuesCache[native];

                default:
                    return null;
            } 
        }

        public static EnumerationXSForm CreateInstance()
        {
 
            TypeDescriptor type          = TypeManager.RegisterType("EnumerationXSForm",  typeof(EnumerationXSForm));
            TypeDescriptor enumValueType = TypeManager.RegisterType("XSForm",             typeof(XSForm));

            TypeManager.RegisterAliasFor(type,          "ПеречислениеФормаПредставленияXS"); 
            TypeManager.RegisterAliasFor(enumValueType, "ФормаПредставленияXS");

            EnumerationXSForm instance = new EnumerationXSForm(type, enumValueType);

            instance.AddValue("Квалифицированная",   "Qualified",   instance._valuesCache[XmlSchemaForm.Qualified]);
            instance.AddValue("Неквалифицированная", "Unqualified", instance._valuesCache[XmlSchemaForm.Unqualified]);

            return instance;
        }
    }
}
