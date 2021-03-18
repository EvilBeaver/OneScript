/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine.Values;

namespace OneScript.StandardLibrary
{
    /// <summary>
    /// Общие встроенные свойства и методы, присутствующие во всех контекстах исполнения
    /// </summary>
    [GlobalContext(Category = "Общие функции глобального контекста")]
    public class StandardGlobalContext : GlobalContextBase<StandardGlobalContext>
    {
        public StandardGlobalContext()
        {
            Chars = new SymbolsContext();
        }

        /// <summary>
        /// Содержит набор системных символов.
        /// </summary>
        /// <value>Набор системных символов.</value>
        [ContextProperty("Символы", "Chars")]
        public SymbolsContext Chars { get; set; }
        
        /// <summary>
        /// Явное освобождение ресурса через интерфейс IDisposable среды CLR.
        /// 
        /// OneScript не выполняет подсчет ссылок на объекты, а полагается на сборщик мусора CLR.
        /// Это значит, что объекты автоматически не освобождаются при выходе из области видимости. 
        /// 
        /// Метод ОсвободитьОбъект можно использовать для детерминированного освобождения ресурсов. Если объект поддерживает интерфейс IDisposable, то данный метод вызовет Dispose у данного объекта.
        /// 
        /// Как правило, интерфейс IDisposable реализуется различными ресурсами (файлами, соединениями с ИБ и т.п.)
        /// </summary>
        /// <param name="obj">Объект, ресурсы которого требуется освободить.</param>
        [ContextMethod("ОсвободитьОбъект", "FreeObject")]
        public void DisposeObject(IRuntimeContextInstance obj)
        {
            var disposable = obj as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        /// <summary>
        /// OneScript не выполняет подсчет ссылок на объекты, а полагается на сборщик мусора CLR.
        /// Это значит, что объекты автоматически не освобождаются при выходе из области видимости.
        /// 
        /// С помощью данного метода можно запустить принудительную сборку мусора среды CLR.
        /// Данные метод следует использовать обдуманно, поскольку вызов данного метода не гарантирует освобождение всех объектов.
        /// Локальные переменные, например, до завершения текущего метода очищены не будут,
        /// поскольку до завершения текущего метода CLR будет видеть, что они используются движком 1Script.
        /// 
        /// </summary>
        [ContextMethod("ВыполнитьСборкуМусора", "RunGarbageCollection")]
        public void RunGarbageCollection()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        
        /// <summary>
        /// Приостанавливает выполнение скрипта.
        /// </summary>
        /// <param name="delay">Время приостановки в миллисекундах</param>
        [ContextMethod("Приостановить", "Sleep")]
        public void Sleep(int delay)
        {
            System.Threading.Thread.Sleep(delay);
        }
        
        [ContextMethod("КраткоеПредставлениеОшибки", "BriefErrorDescription")]
        public string BriefErrorDescription(ExceptionInfoContext errInfo)
        {
            return errInfo.Description;
        }

        [ContextMethod("ПодробноеПредставлениеОшибки", "DetailErrorDescription")]
        public string DetailErrorDescription(ExceptionInfoContext errInfo)
        {
            return errInfo.DetailedDescription;
        }

        [ContextMethod("ТекущаяУниверсальнаяДата", "CurrentUniversalDate")]
        public IValue CurrentUniversalDate()
        {
            return ValueFactory.Create(DateTime.UtcNow);
        }

        [ContextMethod("ТекущаяУниверсальнаяДатаВМиллисекундах", "CurrentUniversalDateInMilliseconds")]
        public long CurrentUniversalDateInMilliseconds()
        {
            return DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
        }

        /// <summary>
        /// Проверяет заполненность значения по принципу, заложенному в 1С:Предприятии
        /// </summary>
        /// <param name="inValue"></param>
        /// <returns></returns>
        [ContextMethod("ЗначениеЗаполнено","ValueIsFilled")]
        public bool ValueIsFilled(IValue inValue)
        {
            var value = inValue?.GetRawValue();
            if (value == null)
            {
                return false;
            }

            switch (value)
            {
                case IEmptyValueCheck emptyHandler:
                    return !emptyHandler.IsEmpty;
                case ICollectionContext collection:
                    return collection.Count() != 0;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Заполняет одноименные значения свойств одного объекта из другого
        /// </summary>
        /// <param name="acceptor">Объект-приемник</param>
        /// <param name="source">Объект-источник</param>
        /// <param name="filledProperties">Заполняемые свойства (строка, через запятую)</param>
        /// <param name="ignoredProperties">Игнорируемые свойства (строка, через запятую)</param>
        [ContextMethod("ЗаполнитьЗначенияСвойств","FillPropertyValues")]
        public void FillPropertyValues(IRuntimeContextInstance acceptor, IRuntimeContextInstance source, IValue filledProperties = null, IValue ignoredProperties = null)
        {
            string strFilled;
            string strIgnored;

            if (filledProperties == null || filledProperties.DataType == DataType.Undefined)
            {
                strFilled = null;
            }
            else if (filledProperties.DataType == DataType.String)
            {
                strFilled = filledProperties.AsString();
            }
            else
            {
                throw RuntimeException.InvalidArgumentType(3, nameof(filledProperties));
            }

            if (ignoredProperties == null || ignoredProperties.DataType == DataType.Undefined)
            {
                strIgnored = null;
            }
            else if (ignoredProperties.DataType == DataType.String)
            {
                strIgnored = ignoredProperties.AsString();
            }
            else
            {
                throw RuntimeException.InvalidArgumentType(4, nameof(ignoredProperties));
            }

            FillPropertyValuesStr(acceptor, source, strFilled, strIgnored);
        }

        private static void FillPropertyValuesStr(IRuntimeContextInstance acceptor, IRuntimeContextInstance source, string filledProperties = null, string ignoredProperties = null)
        {
            IEnumerable<string> sourceProperties;

            if (filledProperties == null)
            {
                string[] names = new string[source.GetPropCount()];
                for (int i = 0; i < names.Length; i++)
                {
                    names[i] = source.GetPropName(i);
                }

                if (ignoredProperties == null)
                {
                    sourceProperties = names;
                }
                else
                {
                    IEnumerable<string> ignoredPropCollection = ignoredProperties.Split(',')
                        .Select(x => x.Trim())
                        .Where(x => x.Length > 0);

                    sourceProperties = names.Where(x => !ignoredPropCollection.Contains(x));
                }
            }
            else
            {
                sourceProperties = filledProperties.Split(',')
                    .Select(x => x.Trim())
                    .Where(x => x.Length > 0);

                // Проверка существования заявленных свойств
                foreach (var item in sourceProperties)
                {
                    acceptor.FindProperty(item); // бросает PropertyAccessException если свойства нет
                }
            }


            foreach (var srcProperty in sourceProperties)
            {
                try
                {
                    var srcPropIdx = source.FindProperty(srcProperty);
                    var accPropIdx = acceptor.FindProperty(srcProperty); // бросает PropertyAccessException если свойства нет

                    if (source.IsPropReadable(srcPropIdx) && acceptor.IsPropWritable(accPropIdx))
                        acceptor.SetPropValue(accPropIdx, source.GetPropValue(srcPropIdx));

                }
                catch (PropertyAccessException)
                {
                    // игнорировать свойства Источника, которых нет в Приемнике
                }
            }
        }

        /// <summary>
        /// Получает объект класса COM по его имени или пути. Подробнее см. синтакс-помощник от 1С.
        /// </summary>
        /// <param name="pathName">Путь к библиотеке</param>
        /// <param name="className">Имя класса</param>
        /// <returns>COMОбъект</returns>
        [ContextMethod("ПолучитьCOMОбъект", "GetCOMObject")]
        public IValue GetCOMObject(string pathName = null, string className = null)
        {
            var comObject = GetCOMObjectInternal(pathName, className);

            return COMWrapperContext.Create(comObject);
        }

        /// <summary>
        /// Ported from Microsoft.VisualBasic, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
        /// By JetBrains dotPeek decompiler
        /// </summary>
        private object GetCOMObjectInternal(string pathName = null, string className = null)
        {
            if (String.IsNullOrEmpty(className))
            {
                return Marshal.BindToMoniker(pathName);
            }
            else if (pathName == null)
            {
#if NETSTANDARD2_0 || NETSTANDARD2_1
                throw new NotSupportedException("Getting object by classname not supported on netstandard2");
#else
                return Marshal.GetActiveObject(className);
#endif
            }
            else if (pathName.Length == 0)
            {
                return Activator.CreateInstance(System.Type.GetTypeFromProgID(className));
            }
            else
            {
#if NETSTANDARD2_0 || NETSTANDARD2_1
                throw new NotSupportedException("Getting object by classname not supported on netstandard2");
#else
                var persistFile = (IPersistFile)Marshal.GetActiveObject(className);
                persistFile.Load(pathName, 0);
                
                return (object)persistFile;
#endif
            }
        }
        
        #region Static infrastructure

        public static IAttachableContext CreateInstance()
        {
            return new StandardGlobalContext();
        }

        #endregion
    }
}