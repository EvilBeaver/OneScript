/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Linq;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.StandardLibrary.Collections.ValueTable;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary
{
    /// <summary>
    /// Сессия сбора статистики исполнения кода. Создаётся через СборСтатистики.НачатьСбор().
    /// </summary>
    [ContextClass("СборщикСтатистикиКода", "CodeStatisticsCollector")]
    public sealed class CodeStatisticsCollector : AutoContext<CodeStatisticsCollector>
    {
        private enum SessionState
        {
            Active,
            Paused,
            Finished
        }

        private readonly CodeStatHub _hub;
        private readonly CodeStatProcessor _session;
        private SessionState _state = SessionState.Active;

        internal CodeStatisticsCollector(CodeStatHub hub, CodeStatProcessor session)
        {
            _hub = hub;
            _session = session;
        }

        [ContextMethod("Приостановить", "Pause")]
        public void Pause()
        {
            EnsureState(SessionState.Active);
            _hub.PauseSession(_session);
            _state = SessionState.Paused;
        }

        [ContextMethod("Восстановить", "Resume")]
        public void Resume()
        {
            EnsureState(SessionState.Paused);
            _hub.ResumeSession(_session);
            _state = SessionState.Active;
        }

        [ContextMethod("Завершить", "Finish")]
        public ValueTable Finish()
        {
            if (_state == SessionState.Finished)
                ThrowInvalidState();

            _hub.FinishSession(_session);
            _state = SessionState.Finished;
            return ToValueTable(_session.GetStatData());
        }

        private void EnsureState(SessionState expected)
        {
            if (_state != expected)
                ThrowInvalidState();
        }

        private static void ThrowInvalidState()
        {
            throw new RuntimeException(
                "Неверное состояние сборщика статистики кода",
                "Invalid code statistics collector state");
        }

        private static ValueTable ToValueTable(CodeStatDataCollection data)
        {
            var table = new ValueTable();
            var pathColumn = table.Columns.Add("Путь");
            var methodColumn = table.Columns.Add("Метод");
            var lineColumn = table.Columns.Add("НомерСтроки");
            var countColumn = table.Columns.Add("Количество");
            var timeColumn = table.Columns.Add("Время");

            foreach (var item in data
                         .OrderBy(x => x.Entry.ScriptFileName)
                         .ThenBy(x => x.Entry.SubName)
                         .ThenBy(x => x.Entry.LineNumber))
            {
                var row = table.Add();
                row.Set(pathColumn, ValueFactory.Create(item.Entry.ScriptFileName ?? string.Empty));
                row.Set(methodColumn, ValueFactory.Create(item.Entry.SubName ?? string.Empty));
                row.Set(lineColumn, ValueFactory.Create(item.Entry.LineNumber));
                row.Set(countColumn, ValueFactory.Create(item.ExecutionCount));
                row.Set(timeColumn, ValueFactory.Create((decimal)item.TimeElapsed));
            }

            return table;
        }
    }
}
