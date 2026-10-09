/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;

namespace OneScript.StandardLibrary.Tasks
{
    /// <summary>
    /// Реестр заданий менеджера. Если заданий больше заданного числа, вытесняет задания,
    /// завершившиеся раньше всех. Выполняющиеся не трогает, даже если их больше этого числа.
    /// </summary>
    internal class BackgroundTasksRegistry
    {
        private readonly int _capacity;

        private readonly object _lock = new object();

        private readonly Dictionary<int, LinkedListNode<Entry>> _index =
            new Dictionary<int, LinkedListNode<Entry>>();

        // Сначала выполняющиеся задания в порядке запуска, за ними завершенные в порядке завершения
        private readonly LinkedList<Entry> _order = new LinkedList<Entry>();

        // Первое завершенное задание в _order, новые задания встают перед ним
        private LinkedListNode<Entry> _firstCompleted;

        public BackgroundTasksRegistry(int capacity)
        {
            _capacity = capacity;
        }

        /// <summary>
        /// Снимок заданий: выполняющиеся в порядке запуска, завершенные в порядке завершения
        /// </summary>
        public BackgroundTask[] Values
        {
            get
            {
                lock (_lock)
                {
                    var result = new BackgroundTask[_order.Count];
                    var i = 0;
                    foreach (var entry in _order)
                    {
                        result[i++] = entry.Task;
                    }

                    return result;
                }
            }
        }

        public bool TryAdd(int taskId, BackgroundTask task)
        {
            lock (_lock)
            {
                if (_index.ContainsKey(taskId))
                    return false;

                var entry = new Entry(task);
                var node = _firstCompleted == null
                    ? _order.AddLast(entry)
                    : _order.AddBefore(_firstCompleted, entry);

                _index.Add(taskId, node);
                if (_index.Count > _capacity)
                    EvictCompleted();

                return true;
            }
        }

        /// <summary>
        /// Переносит завершенное задание в конец, чтобы вытеснялись те, что завершились раньше.
        /// Если выполнявшихся заданий было больше емкости, лишние вытесняются сразу.
        /// </summary>
        public void MarkCompleted(BackgroundTask task)
        {
            lock (_lock)
            {
                if (_index.TryGetValue(task.TaskId, out var node)
                    && node.Value.Task == task
                    && !node.Value.Completed)
                {
                    MoveToEnd(node);
                    if (_index.Count > _capacity)
                        EvictCompleted();
                }
            }
        }

        public bool TryGetValue(int taskId, out BackgroundTask task)
        {
            lock (_lock)
            {
                if (_index.TryGetValue(taskId, out var node))
                {
                    task = node.Value.Task;
                    return true;
                }
            }

            task = default;
            return false;
        }

        public bool TryRemove(int taskId, out BackgroundTask task)
        {
            lock (_lock)
            {
                if (_index.Remove(taskId, out var node))
                {
                    if (node == _firstCompleted)
                        _firstCompleted = node.Next;

                    _order.Remove(node);
                    task = node.Value.Task;
                    return true;
                }
            }

            task = default;
            return false;
        }

        public void Clear()
        {
            lock (_lock)
            {
                _index.Clear();
                _order.Clear();
                _firstCompleted = null;
            }
        }

        private void MoveToEnd(LinkedListNode<Entry> node)
        {
            node.Value.Completed = true;
            _order.Remove(node);
            _order.AddLast(node);
            _firstCompleted ??= node;
        }

        private void EvictCompleted()
        {
            while (_firstCompleted != null && _index.Count > _capacity)
            {
                var node = _firstCompleted;
                _firstCompleted = node.Next;
                _index.Remove(node.Value.Task.TaskId);
                _order.Remove(node);
            }
        }

        private class Entry
        {
            public Entry(BackgroundTask task)
            {
                Task = task;
            }

            public BackgroundTask Task { get; }

            public bool Completed { get; set; }
        }
    }
}
