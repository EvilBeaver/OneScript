/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using OneScript.Types;
using ScriptEngine;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.XMLSchema.Collections
{
    [ContextClass("НаборСхемXML", "XMLSchemaSet")]
    public class XMLSchemaSet : AutoCollectionContext<XMLSchemaSet, Objects.XMLSchema>
    {
        private readonly XmlSchemaSet _schemaSet;
        private readonly List<Objects.XMLSchema> _items;
        private XMLSchemaSet()
        {
            _items = new List<Objects.XMLSchema>();
            _schemaSet = new XmlSchemaSet();
            _schemaSet.ValidationEventHandler += SchemaSet_ValidationError;
        }

        #region OneScript

        #region Methods

        [ContextMethod("Добавить", "Add")]
        public void Add(Objects.XMLSchema schema)
        {
            _items.Add(schema);
            _schemaSet.Add(schema.SchemaObject);
        }

        [ContextMethod("Количество", "Count")]
        public override int Count() => _schemaSet.Count;

        [ContextMethod("Получить", "Get")]
        public Objects.XMLSchema Get(IValue value)
        {
            DataType DataType = value.DataType;
            switch (DataType)
            {
                case DataType.String:
                    return _items.FirstOrDefault(x => x.TargetNamespace.Equals(value.AsString()));

                case DataType.Number:
                    return _items[(int)value.AsNumber()];

                default:
                    throw RuntimeException.InvalidArgumentType();
            }
        }

        [ContextMethod("Проверить", "Validate")]
        public bool Validate()
        {
            _schemaSet.Compile();
            return _schemaSet.IsCompiled;
        }

        [ContextMethod("Удалить", "Delete")]
        public void Delete(string namespaceUri)
        {
            Objects.XMLSchema item = _items.Find(x => x.TargetNamespace.Equals(namespaceUri));
            if (item is Objects.XMLSchema)
            {
                _items.Remove(item);
                _schemaSet.Remove(item.SchemaObject);
            }
        }

        #endregion

        #region Constructors

        [ScriptConstructor(Name = "По умолчанию")]
        public static XMLSchemaSet Constructor() => new XMLSchemaSet();

        #endregion

        #endregion

        #region IEnumerable

        public override IEnumerator<Objects.XMLSchema> GetEnumerator() => _items.GetEnumerator();

        #endregion

        #region SchemaSetEventHandlers

        private void SchemaSet_ValidationError(object sender, ValidationEventArgs args)
        {
            switch (args.Severity)
            {
                case XmlSeverityType.Error:
                    SystemLogger.Write($"ERROR:{args.Message}");
                    break;

                default:
                    SystemLogger.Write($"WARNING:{args.Message}");
                    break;
            }
        }

        #endregion
    }
}
