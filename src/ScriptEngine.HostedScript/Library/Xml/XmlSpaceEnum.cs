/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace ScriptEngine.HostedScript.Library.Xml
{
    [SystemEnum("ПробельныеСимволыXML", "XMLSpace")]
    public class XMLSpaceEnum : EnumerationContext
    {
        readonly Dictionary<XmlSpace, IValue> _valuesCache = new Dictionary<XmlSpace, IValue>();

        private XMLSpaceEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {

        }

        public IValue FromNativeValue(XmlSpace native)
        {
            if (native == XmlSpace.None)
                native = XmlSpace.Default;

            if (_valuesCache.TryGetValue(native, out IValue val))
            {
                return val;
            }
            else
            {
                val = this.ValuesInternal.First(x => ((CLREnumValueWrapper<XmlSpace>)x).UnderlyingValue == native);
                _valuesCache.Add(native, val);
            }

            return val;
        }

        public static XMLSpaceEnum CreateInstance()
        {
            XMLSpaceEnum instance;
            var type = TypeManager.RegisterType("ПеречислениеПробельныеСимволыXML", typeof(XMLSpaceEnum));
            var enumValueType = TypeManager.RegisterType("ПробельныеСимволыXML", typeof(CLREnumValueWrapper<XmlSpace>));

            instance = new XMLSpaceEnum(type, enumValueType);

            instance.AddValue("ПоУмолчанию", "Default", new CLREnumValueWrapper<XmlSpace>(instance, XmlSpace.Default));
            instance.AddValue("Сохранять", "Preserve", new CLREnumValueWrapper<XmlSpace>(instance, XmlSpace. Preserve));

            return instance;
        }
   }
}
