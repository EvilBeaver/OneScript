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

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    public class XSComplexFinal : EnumerationValue
    {
        private readonly XmlSchemaDerivationMethod _derivationMethod;
        public XSComplexFinal(EnumerationContext owner, XmlSchemaDerivationMethod derivationMethod)
            : base(owner) => _derivationMethod = derivationMethod;

        public static XSComplexFinal FromNativeValue(XmlSchemaDerivationMethod native)
            => EnumerationXSComplexFinal.FromNativeValue(native);

        public static XmlSchemaDerivationMethod ToNativeValue(XSComplexFinal wrapper)
             => wrapper._derivationMethod;
    }

    [SystemEnum("ЗавершенностьСоставногоТипаXS", "XSComplexFinal")]
    public class EnumerationXSComplexFinal : EnumerationContext
    {
        private readonly Dictionary<XmlSchemaDerivationMethod, XSComplexFinal> _valuesCache;

        private EnumerationXSComplexFinal(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
            _valuesCache = new Dictionary<XmlSchemaDerivationMethod, XSComplexFinal>
            {
                { XmlSchemaDerivationMethod.All, new XSComplexFinal(this, XmlSchemaDerivationMethod.All) },
                { XmlSchemaDerivationMethod.Restriction, new XSComplexFinal(this, XmlSchemaDerivationMethod.Restriction) },
                { XmlSchemaDerivationMethod.Extension, new XSComplexFinal(this, XmlSchemaDerivationMethod.Extension) }
            };
        }
        internal static XSComplexFinal FromNativeValue(XmlSchemaDerivationMethod native)
        {
            EnumerationXSComplexFinal enumeration = GlobalsManager.GetEnum<EnumerationXSComplexFinal>();
            enumeration._valuesCache.TryGetValue(native, out XSComplexFinal value);
            return value;
        }

        public static EnumerationXSComplexFinal CreateInstance()
        {
            TypeDescriptor type = TypeManager.RegisterType("ПеречислениеЗавершенностьСоставногоТипаXS", typeof(EnumerationXSComplexFinal));
            TypeDescriptor enumValueType = TypeManager.RegisterType("ЗавершенностьСоставногоТипаXS", typeof(XSComplexFinal));

            TypeManager.RegisterAliasFor(type, "EnumerationXSComplexFinal");
            TypeManager.RegisterAliasFor(enumValueType, "XSComplexFinal");

            EnumerationXSComplexFinal instance = new EnumerationXSComplexFinal(type, enumValueType);
            instance.AddValue("Расширение", "Extension", new XSComplexFinal(instance, XmlSchemaDerivationMethod.Extension));
            instance.AddValue("Ограничение", "Restriction", new XSComplexFinal(instance, XmlSchemaDerivationMethod.Restriction));
            instance.AddValue("Все", "All", new XSComplexFinal(instance, XmlSchemaDerivationMethod.All));

            return instance;
        }
    }



}