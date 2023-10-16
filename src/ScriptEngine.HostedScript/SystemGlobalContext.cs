﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using OneScript.Contexts;
using OneScript.StandardLibrary;
using OneScript.StandardLibrary.Collections;
using OneScript.Sources;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library
{
    /// <summary>
    /// Глобальный контекст. Представляет глобально доступные свойства и методы.
    /// </summary>
    [GlobalContext(Category="Процедуры и функции взаимодействия с системой", ManualRegistration=true)]
    public class SystemGlobalContext : IAttachableContext
    {
        private static readonly ContextMethodsMapper<SystemGlobalContext> _methods 
            = new ContextMethodsMapper<SystemGlobalContext>();

        private static readonly ContextPropertyMapper<SystemGlobalContext> _properties =
            new ContextPropertyMapper<SystemGlobalContext>();

        private IVariable[] _state;
        private FixedArrayImpl  _args;

        public ScriptingEngine EngineInstance{ get; set; }

        public void InitInstance()
        {
            InitContextVariables();
        }

        private void InitContextVariables()
        {
            _state = new IVariable[_properties.Count];
            
            for (int i = 0; i < _properties.Count; i++)
            {
                _state[i] = Variable.CreateContextPropertyReference(this, i, _properties.GetProperty(i).Name);
            }
        }

        public IHostApplication ApplicationHost { get; set; }
        public SourceCode CodeSource { get; set; }

        /// <summary>
        /// Выдает сообщение в консоль.
        /// </summary>
        /// <param name="message">Выдаваемое сообщение.</param>
        /// <param name="status">Статус сообщения. В зависимости от статуса изменяется цвет вывода сообщения.</param>
        [ContextMethod("Сообщить", "Message")]
		public void Echo(string message, MessageStatusEnum status = MessageStatusEnum.Ordinary)
        {
            ApplicationHost.Echo(message ?? "", status);
        }

        /// <summary>
        /// Возвращает информацию о сценарии, который был точкой входа в программу.
        /// Можно выделить два вида сценариев: те, которые были подключены, как классы и те, которые запущены непосредственно. Метод СтартовыйСценарий возвращает информацию о сценарии, запущенном непосредственно.
        /// Для получения информации о текущем выполняемом сценарии см. метод ТекущийСценарий()
        /// </summary>
        /// <returns>Объект ИнформацияОСценарии</returns>
        [ContextMethod("СтартовыйСценарий", "EntryScript")]
        public IRuntimeContextInstance StartupScript()
        {
            return new ScriptInformationContext(CodeSource);
        }

        /// <summary>
        /// Прерывает выполнение текущего скрипта.
        /// </summary>
        /// <param name="exitCode">Код возврата (ошибки), возвращаемый операционной системе.</param>
        [ContextMethod("ЗавершитьРаботу", "Exit")]
        public void Quit(int exitCode)
        {
            throw new ScriptInterruptionException(exitCode);
        }

        /// <summary>
        /// Ввод строки пользователем. Позволяет запросить у пользователя информацию.
        /// </summary>
        /// <param name="resut">Выходной параметр. Введенные данные в виде строки.</param>
        /// <param name="prompt">Строка, выводимая в качестве подсказки. Необязательный, по умолчанию - пустая строка.</param>
        /// <param name="len">Максимальная длина вводимой строки. Необязательный, по умолчанию - 0 (неограниченная длина).
        /// Указание неограниченной длины может не поддерживаться хост-приложением.</param>
        /// <param name="multiline">Булево, определяет режим ввода многострочного текста. Необязательный, по умолчанию - Ложь.</param>
        /// <returns>Булево. Истина, если пользователь ввел данные, Ложь, если отказался.</returns>
        [ContextMethod("ВвестиСтроку", "InputString")]
        public bool InputString([ByRef] IVariable resut, string prompt = null, int? len = null, bool? multiline = null)
        {
            string input;
            bool inputIsDone;
            
            string strPrompt = null;
            int length = 0;
            bool flagML = false;

            if (prompt != null)
            {
                strPrompt = prompt;
            }

            if (len != null)
            {
                length = (int)len;
            }

            if (multiline != null)
            {
                flagML = (bool)multiline;
            }

            inputIsDone = ApplicationHost.InputString(out input, strPrompt, length, flagML);

            if (inputIsDone)
            {
                resut.Value = ValueFactory.Create(input);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Доступ к аргументам командной строки.
        /// Объект АргументыКоманднойСтроки представляет собой массив в режиме "только чтение".
        /// </summary>
        [ContextProperty("АргументыКоманднойСтроки", "CommandLineArguments", CanWrite = false)]
        public FixedArrayImpl CommandLineArguments
        {
            get
            {
                if (_args == null)
                {
                    var argsArray = new ArrayImpl();
                    if (ApplicationHost != null)
                    {
                        foreach (var arg in ApplicationHost.GetCommandLineArguments())
                        {
                            argsArray.Add(ValueFactory.Create(arg));
                        }
                    }
                    _args = new FixedArrayImpl(argsArray);
                }

                return _args;
            }

        }

        /// <summary>
        /// Каталог исполняемых файлов OneScript
        /// </summary>
        /// <returns></returns>
        [ContextMethod("КаталогПрограммы","ProgramDirectory")]
        public string ProgramDirectory()
        {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            var filename = asm.Location;

            return System.IO.Path.GetDirectoryName(filename);
        }

#region IAttachableContext Members

        public void OnAttach(out IVariable[] variables, 
            out BslMethodInfo[] methods)
        {
            if (_state == null)
                InitContextVariables();

            variables = _state;
            methods = new BslMethodInfo[_methods.Count];
            for (int i = 0; i < _methods.Count; i++)
            {
                methods[i] = _methods.GetRuntimeMethod(i);
            }
        }

#endregion

#region IRuntimeContextInstance Members

        public bool IsIndexed
        {
            get 
            { 
                return false; 
            }
        }

        public bool DynamicMethodSignatures
        {
            get
            {
                return false;
            }
        }

        public IValue GetIndexedValue(IValue index)
        {
            throw new NotImplementedException();
        }

        public void SetIndexedValue(IValue index, IValue val)
        {
            throw new NotImplementedException();
        }

        public int GetPropertyNumber(string name)
        {
            return _properties.FindProperty(name);
        }

        public bool IsPropReadable(int propNum)
        {
            return _properties.GetProperty(propNum).CanRead;
        }

        public bool IsPropWritable(int propNum)
        {
            return _properties.GetProperty(propNum).CanWrite;
        }

        public IValue GetPropValue(int propNum)
        {
            return _properties.GetProperty(propNum).Getter(this);
        }

        public void SetPropValue(int propNum, IValue newVal)
        {
            throw new InvalidOperationException("global props are not writable");
        }

        public int GetPropCount()
        {
            return _properties.Count;
        }

        public string GetPropName(int index)
        {
            return _properties.GetProperty(index).Name;
        }

        public int GetMethodNumber(string name)
        {
            return _methods.FindMethod(name);
        }

        public virtual BslMethodInfo GetMethodInfo(int methodNumber)
        {
            return _methods.GetRuntimeMethod(methodNumber);
        }
        
        public BslPropertyInfo GetPropertyInfo(int propertyNumber)
        {
            return _properties.GetProperty(propertyNumber).PropertyInfo;
        }

        public int GetMethodsCount()
        {
            return _methods.Count;
        }

        public void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            _methods.GetCallableDelegate(methodNumber)(this, arguments);
        }

        public void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            retValue = _methods.GetCallableDelegate(methodNumber)(this, arguments);
        }

#endregion

    }
}
