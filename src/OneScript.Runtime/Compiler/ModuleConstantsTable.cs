using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneScript.Core;
using OneScript.Language;

namespace OneScript.Runtime.Compiler
{
    public class ModuleConstantsTable : ModuleEntityTable<ConstDefinition>
    {
        private IValue[] _loadedValues;

        public IValue[] Values
        {
            get
            {
                return _loadedValues;
            }
        }

        public void LoadEntities()
        {
            if (_loadedValues != null)
                return;

            _loadedValues = this.Select(x => ValueFactory.Parse(x.Presentation, ConvertToRuntimeType(x.Type))).ToArray();
        }

        private DataType ConvertToRuntimeType(ConstType type)
        {
            switch (type)
            {
                case ConstType.Undefined:
                    return BasicTypes.Undefined;
                case ConstType.String:
                    return BasicTypes.String;
                case ConstType.Number:
                    return BasicTypes.Number;
                case ConstType.Boolean:
                    return BasicTypes.Boolean;
                case ConstType.Date:
                    return BasicTypes.Date;
                case ConstType.Null:
                    return BasicTypes.Null;
                default:
                    throw new ArgumentException();
            }
        }
    }
}
