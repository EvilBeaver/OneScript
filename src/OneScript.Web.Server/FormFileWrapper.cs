/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using Microsoft.AspNetCore.Http;
using OneScript.Contexts;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using OneScript.Values;
using OneScript.StandardLibrary.Binary;

namespace OneScript.Web.Server
{
    [ContextClass("ФайлФормы", "FormFile")]
    public class FormFileWrapper : AutoContext<FormFileWrapper>
    {
        private readonly IFormFile _item;

        internal FormFileWrapper(IFormFile item)
        {
            _item = item;
        }

        [ContextProperty("ТипКонтента", "ContentType", CanWrite = false)]
        public IValue ContentType => BslStringValue.Create(_item.ContentType);

        [ContextProperty("РасположениеКонтента", "ContentDisposition", CanWrite = false)]
        public IValue ContentDisposition => BslStringValue.Create(_item.ContentDisposition);

        [ContextProperty("Заголовки", "Headers", CanWrite = false)]
        public HeaderDictionaryWrapper Headers => new(_item.Headers);

        [ContextProperty("Длина", "Length", CanWrite = false)]
        public IValue Length => BslNumericValue.Create(_item.Length);

        [ContextProperty("Имя", "Name", CanWrite = false)]
        public IValue Name => BslStringValue.Create(_item.Name);

        [ContextProperty("ИмяФайла", "FileName", CanWrite = false)]
        public IValue FileName => BslStringValue.Create(_item.FileName);

        [ContextMethod("ОткрытьПотокЧтения", "OpenReadStream")]
        public GenericStream OpenReadStream() => new(_item.OpenReadStream());
    }
}
