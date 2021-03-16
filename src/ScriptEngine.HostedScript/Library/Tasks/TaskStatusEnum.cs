/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.Tasks
{
    [EnumerationType("СостояниеЗадания", "TaskStatus")]
    public enum TaskStatusEnum
    {
        [EnumItem("НеВыполнялось", "NotRunned")]
        NotRunned,
        [EnumItem("Выполняется", "Running")]
        Running,
        [EnumItem("Завершено", "Completed")]
        Completed,
        [EnumItem("ЗавершеноСОшибками", "CompletedWithErrors")]
        CompletedWithErrors
    }
}