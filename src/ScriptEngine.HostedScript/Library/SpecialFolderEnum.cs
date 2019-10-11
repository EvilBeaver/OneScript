/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

using sysFolder = System.Environment.SpecialFolder;

namespace ScriptEngine.HostedScript.Library
{
    /// <summary>
    /// Системное перечисление для специальных папок.
    /// </summary>
    [SystemEnum("СпециальнаяПапка", "SpecialFolder")]
    public class SpecialFolderEnum : EnumerationContext
    {
        private SpecialFolderEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {

        }

        public static SpecialFolderEnum CreateInstance()
        {
            SpecialFolderEnum instance;
            var type = TypeManager.RegisterType("ПеречислениеСпециальнаяПапка", typeof(SpecialFolderEnum));
            var enumValueType = TypeManager.RegisterType("СпециальнаяПапка", typeof(CLREnumValueWrapper<sysFolder>));

            instance = new SpecialFolderEnum(type, enumValueType);

            instance.AddValue("МоиДокументы", "MyDocuments", new CLREnumValueWrapper<sysFolder>(instance, sysFolder.Personal));
            instance.AddValue("ДанныеПриложений", "ApplicationData", new CLREnumValueWrapper<sysFolder>(instance, sysFolder.ApplicationData));
            instance.AddValue("ЛокальныйКаталогДанныхПриложений", "LocalApplicationData", new CLREnumValueWrapper<sysFolder>(instance, sysFolder.LocalApplicationData));
            instance.AddValue("РабочийСтол", "Desktop", new CLREnumValueWrapper<sysFolder>(instance, sysFolder.Desktop));
            instance.AddValue("КаталогРабочийСтол", "DesktopDirectory", new CLREnumValueWrapper<sysFolder>(instance, sysFolder.DesktopDirectory));
            instance.AddValue("МояМузыка", "MyMusic", new CLREnumValueWrapper<sysFolder>(instance, sysFolder.MyMusic));
            instance.AddValue("МоиРисунки", "MyPictures", new CLREnumValueWrapper<sysFolder>(instance, sysFolder.MyPictures));
            instance.AddValue("Шаблоны", "Templates", new CLREnumValueWrapper<sysFolder>(instance, sysFolder.Templates));
            instance.AddValue("МоиВидеозаписи", "MyVideos", new CLREnumValueWrapper<sysFolder>(instance, sysFolder.MyVideos));
            instance.AddValue("ОбщиеШаблоны", "CommonTemplates", new CLREnumValueWrapper<sysFolder>(instance, sysFolder.CommonTemplates));
            instance.AddValue("ПрофильПользователя", "UserProfile", new CLREnumValueWrapper<sysFolder>(instance, sysFolder.UserProfile));
            instance.AddValue("ОбщийКаталогДанныхПриложения", "CommonApplicationData", new CLREnumValueWrapper<sysFolder>(instance, sysFolder.CommonApplicationData));

            return instance;
        }
    }

}
