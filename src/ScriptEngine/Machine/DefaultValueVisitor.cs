/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Contexts;

namespace ScriptEngine.Machine
{
    public class DefaultValueVisitor : IDebugValueVisitor
    {
        private readonly List<IVariable> _children = new List<IVariable>();
        
        public void ShowProperties(IRuntimeContextInstance context)
        {
            var propsCount = context.GetPropCount();
            for (int i = 0; i < propsCount; i++)
            {
                var propNum = i;
                var propName = context.GetPropName(propNum);
                
                IVariable value;

                try
                {
                    value = Variable.Create(context.GetPropValue(propNum), propName);
                }
                catch (Exception e)
                {
                    value = Variable.Create(ValueFactory.Create(e.Message), propName);
                }

                _children.Add(value);
            }
        }

        public void ShowCollectionItems(IEnumerable<IValue> collection)
        {
            int index = 0;
            foreach (var collectionItem in collection)
            {
                _children.Add(Variable.Create(collectionItem, index.ToString()));
                ++index;
            }
        }

        public void ShowCustom(ICollection<IVariable> variablesToShow)
        {
            _children.AddRange(variablesToShow);
        }

        public IReadOnlyList<IVariable> Result => _children;
    }
}