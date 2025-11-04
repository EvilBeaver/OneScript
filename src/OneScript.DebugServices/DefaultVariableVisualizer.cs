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
        // ВАЖНО О ПОВЕДЕНИИ В ОТЛАДКЕ:
        // Данный визуализатор для построения представления переменных читает их значения (ToString, SystemType.Name,
        // доступ к variable.Value в IsStructured и т.п.). Это может иметь побочные эффекты:
        // - Запускать геттеры и логировать предупреждения об устаревших свойствах через PropertyBag.WarnDeprecation,
        //   даже если пользовательский код явно не обращался к ним. Например, алиас StreamPosition помечен как устаревший.
        // См. также комментарии в ScriptEngine.Machine.PropertyBag.WarnDeprecation.
        public Variable GetVariable(IVariable value)
        {
            using (DeprecationWarningScope.Suppress())
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
        }

        public IEnumerable<IVariable> GetChildVariables(IValue value)
        {
            using (DeprecationWarningScope.Suppress())
            {
            // Для вывода дочерних значений мы так же обращаемся к объекту (свойства/индексы),
            // что потенциально может инициировать чтение значений и вызвать WarnDeprecation.
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
        }

        private bool IsStructured(IVariable variable)
        {
            using (DeprecationWarningScope.Suppress())
            {
            // Доступ к variable.Value здесь запускает вычисление значения и, для глобальных свойств,
            // приводит к вызову PropertyBag.GetPropValue -> WarnDeprecation.
            var rawValue = variable?.Value;
            return HasProperties(rawValue as IRuntimeContextInstance) 
                   || HasIndexes(rawValue as ICollectionContext<IValue>);
            }
        }

        private bool HasIndexes(ICollectionContext<IValue> collection)
        {
            using (DeprecationWarningScope.Suppress())
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
        }

        private static bool HasProperties(IRuntimeContextInstance value)
        {
            return value?.GetPropCount() > 0;
        }
    }
}