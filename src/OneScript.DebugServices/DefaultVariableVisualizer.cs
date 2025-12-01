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
using OneScript.Execution;
using ScriptEngine.Machine;
using Variable = OneScript.DebugProtocol.Variable;

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
                // FIXME: В отладочном представлении не вызываются кастомные bsl-представления
                presentation = value.ToString();
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
            
            if (value is IRuntimeContextInstance)
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

                    if (HasIndexes(objectValue as ICollectionContext<IValue>))
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
            var rawValue = variable?.Value;
            return HasProperties(rawValue as IRuntimeContextInstance) 
                   || HasIndexes(rawValue as ICollectionContext<IValue>);
        }

        private bool HasIndexes(ICollectionContext<IValue> collection)
        {
            try
            {
                return collection?.Count(ForbiddenBslProcess.Instance) > 0;
            }
            catch (NotSupportedException)
            {
                // TODO разобраться с bsl-процессом для вычисления пользовательских скриптовых коллекций
                return false;
            }
        }

        private static bool HasProperties(IRuntimeContextInstance value)
        {
            return value?.GetPropCount() > 0;
        }
    }
}