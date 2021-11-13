/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.Tasks
{
    [EnumerationType("СостояниеФоновогоЗадания", "BackgroundJobState")]
    public enum TaskStateEnum
    {
        [EnumItem("Активно", "Active")]
        Running,
        [EnumItem("Завершено", "Completed")]
        Completed,
        [EnumItem("ЗавершеноАварийно", "Failed")]
        CompletedWithErrors,
        [EnumItem("Отменено", "Canceled")]
        Canceled,
        [EnumItem("НеВыполнялось", "NotRunned")]
        NotRunned
    }
}