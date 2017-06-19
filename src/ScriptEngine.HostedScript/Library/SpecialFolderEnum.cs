/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript.Library
{
    [SystemEnum("СпециальнаяПапка", "SpecialFolder")]
    class SpecialFolderEnum : EnumerationContext
    {
        private SpecialFolderEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {

        }

        public static SpecialFolderEnum CreateInstance()
        {
            SpecialFolderEnum instance;
            var type = TypeManager.RegisterType("ПеречислениеСпециальнаяПапка", typeof(SpecialFolderEnum));
            var enumValueType = TypeManager.RegisterType("СпециальнаяПапка", typeof(CLREnumValueWrapper<SpecialFolder>));

            instance = new SpecialFolderEnum(type, enumValueType);

            instance.AddValue("МоиДокументы", "Personal", new CLREnumValueWrapper<SpecialFolder>(instance, SpecialFolder.Personal));
            instance.AddValue("ДанныеПриложений", "ApplicationData", new CLREnumValueWrapper<SpecialFolder>(instance, SpecialFolder.ApplicationData));
            instance.AddValue("ЛокальныйКаталогДанныхПриложений", "LocalApplicationData", new CLREnumValueWrapper<SpecialFolder>(instance, SpecialFolder.LocalApplicationData));
            instance.AddValue("РабочийСтол", "Desktop", new CLREnumValueWrapper<SpecialFolder>(instance, SpecialFolder.Desktop));
            instance.AddValue("КаталогРабочийСтол", "DesktopDirectory", new CLREnumValueWrapper<SpecialFolder>(instance, SpecialFolder.DesktopDirectory));
            instance.AddValue("МояМузыка", "MyMusic", new CLREnumValueWrapper<SpecialFolder>(instance, SpecialFolder.MyMusic));
            instance.AddValue("МоиРисунки", "MyPictures", new CLREnumValueWrapper<SpecialFolder>(instance, SpecialFolder.MyPictures));
            instance.AddValue("Шаблоны", "Templates", new CLREnumValueWrapper<SpecialFolder>(instance, SpecialFolder.Templates));
            instance.AddValue("МоиВидеозаписи", "MyVideos", new CLREnumValueWrapper<SpecialFolder>(instance, SpecialFolder.MyVideos));
            instance.AddValue("ОбщиеШаблоны", "CommonTemplates", new CLREnumValueWrapper<SpecialFolder>(instance, SpecialFolder.CommonTemplates));
            instance.AddValue("ПрофильПользователя", "UserProfile", new CLREnumValueWrapper<SpecialFolder>(instance, SpecialFolder.UserProfile));
            instance.AddValue("ОбщийКаталогДанныхПриложения", "CommonApplicationData", new CLREnumValueWrapper<SpecialFolder>(instance, SpecialFolder.CommonApplicationData));

            return instance;
        }
    }

    public enum SpecialFolder
    {
        Personal = 0x05,
        ApplicationData = 0x1a,
        LocalApplicationData = 0x1c,
        Desktop = 0x00,
        DesktopDirectory = 0x10,
        MyMusic = 0x0d,
        MyPictures = 0x27,
        Templates = 0x15,
        MyVideos = 0x0e,
        CommonTemplates = 0x2d,
        Fonts = 0x14,
        UserProfile = 0x28,
        CommonApplicationData = 0x23
    }
}
