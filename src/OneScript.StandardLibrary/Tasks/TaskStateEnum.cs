/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.StandardLibrary.Tasks
{
    [EnumerationType("СостояниеФоновогоЗадания", "BackgroundJobState")]
    public enum TaskStateEnum
    {
        [EnumItem("НеВыполнялось", "NotRunned")]
        NotRunned,
        [EnumItem("Активно", "Active")]
        Running,
        [EnumItem("Завершено", "Completed")]
        Completed,
        [EnumItem("ЗавершеноАварийно", "Failed")]
        CompletedWithErrors
    }
}