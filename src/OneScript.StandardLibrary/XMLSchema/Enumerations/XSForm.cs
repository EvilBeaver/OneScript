/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Xml.Schema;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Types;

namespace OneScript.StandardLibrary.XMLSchema.Enumerations
{
    public class XSForm : ClrEnumValueWrapper<XmlSchemaForm>
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

                    EnumerationXSForm enumeration = GlobalsHelper.GetEnum<EnumerationXSForm>();
                    return enumeration._valuesCache[native];

                default:
                    return null;
            } 
        }

        public static EnumerationXSForm CreateInstance(ITypeManager typeManager)
        {
 
            var type          = typeManager.RegisterType(
                "ПеречислениеФормаПредставленияXS",
                "EnumerationXSForm",  typeof(EnumerationXSForm));
            
            var enumValueType = typeManager.RegisterType(
                "ФормаПредставленияXS",
                "XSForm",             typeof(XSForm));

            EnumerationXSForm instance = new EnumerationXSForm(type, enumValueType);

            instance.AddValue("Квалифицированная",   "Qualified",   instance._valuesCache[XmlSchemaForm.Qualified]);
            instance.AddValue("Неквалифицированная", "Unqualified", instance._valuesCache[XmlSchemaForm.Unqualified]);

            return instance;
        }
    }
}
