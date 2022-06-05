/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts.Enums;
using OneScript.Types;
using ScriptEngine.Machine.Contexts;
using sysFolder = System.Environment.SpecialFolder;

namespace OneScript.StandardLibrary
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

        public static SpecialFolderEnum CreateInstance(ITypeManager typeManager)
        {
            var instance = EnumContextHelper.CreateClrEnumInstance<SpecialFolderEnum, sysFolder>(
                typeManager,
                (t,v) => new SpecialFolderEnum(t,v));

            instance.WrapClrValue("МоиДокументы", "MyDocuments", sysFolder.Personal);
            instance.WrapClrValue("ДанныеПриложений", "ApplicationData", sysFolder.ApplicationData);
            instance.WrapClrValue("ЛокальныйКаталогДанныхПриложений", "LocalApplicationData", sysFolder.LocalApplicationData);
            instance.WrapClrValue("РабочийСтол", "Desktop", sysFolder.Desktop);
            instance.WrapClrValue("КаталогРабочийСтол", "DesktopDirectory", sysFolder.DesktopDirectory);
            instance.WrapClrValue("МояМузыка", "MyMusic", sysFolder.MyMusic);
            instance.WrapClrValue("МоиРисунки", "MyPictures", sysFolder.MyPictures);
            instance.WrapClrValue("Шаблоны", "Templates", sysFolder.Templates);
            instance.WrapClrValue("МоиВидеозаписи", "MyVideos", sysFolder.MyVideos);
            instance.WrapClrValue("ОбщиеШаблоны", "CommonTemplates", sysFolder.CommonTemplates);
            instance.WrapClrValue("ПрофильПользователя", "UserProfile", sysFolder.UserProfile);
            instance.WrapClrValue("ОбщийКаталогДанныхПриложения", "CommonApplicationData", sysFolder.CommonApplicationData);

            return instance;
        }
    }

}
