/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Contexts;
using OneScript.DebugProtocol;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using Variable = OneScript.DebugProtocol.Variable;
using MachineVariable = ScriptEngine.Machine.Variable;

namespace OneScript.DebugServices
{
    public class DefaultVariableVisualizer : IVariableVisualizer
    {
        public Variable GetVariable(IVariable value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            
            string presentation;
            string typeName;

            //На случай проблем, подобных:
            //https://github.com/EvilBeaver/OneScript/issues/918

            try
            {
                presentation = value.AsString();
            }
            catch (Exception e)
            {
                presentation = e.Message;
            }

            try
            {
                typeName = value.SystemType.Name;
            }
            catch (Exception e)
            {
                typeName = e.Message;
            }

            if (presentation.Length > DebuggerSettings.MAX_PRESENTATION_LENGTH)
                presentation = presentation.Substring(0, DebuggerSettings.MAX_PRESENTATION_LENGTH) + "...";

            return new Variable()
            {
                Name = value.Name,
                Presentation = presentation,
                TypeName = typeName,
                IsStructured = IsStructured(value)
            };
        }

        public IEnumerable<IVariable> GetChildVariables(IValue value)
        {
            var presenter = new DefaultValueVisitor();
            
            if (value.GetRawValue() is IRuntimeContextInstance)
            {
                var objectValue = value.AsObject();
                if (objectValue is IDebugPresentationAcceptor customPresenter)
                {
                    customPresenter.Accept(presenter);
                }
                else
                {
                    if (HasProperties(objectValue))
                    {
                        presenter.ShowProperties(objectValue);
                    }

                    if (HasIndexes(objectValue as ICollectionContext))
                    {
                        var context = value.AsObject();
                        if (context is IEnumerable<IValue> collection)
                        {
                            presenter.ShowCollectionItems(collection);
                        }
                    }
                }
            }
            
            return presenter.Result;
        }

        private bool IsStructured(IVariable variable)
        {
            var rawValue = variable?.GetRawValue();
            return HasProperties(rawValue as IRuntimeContextInstance) 
                   || HasIndexes(rawValue as ICollectionContext);
        }

        private bool HasIndexes(ICollectionContext collection)
        {
            return collection?.Count() > 0;
        }

        private static bool HasProperties(IRuntimeContextInstance value)
        {
            return value?.GetPropCount() > 0;
        }
    }
}