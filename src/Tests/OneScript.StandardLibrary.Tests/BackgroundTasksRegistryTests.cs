/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.StandardLibrary.Tasks;
using ScriptEngine.Hosting;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Types;
using Xunit;
using ExecutionContext = ScriptEngine.Machine.ExecutionContext;

namespace OneScript.StandardLibrary.Tests
{
    public class BackgroundTasksRegistryTests
    {
        [Fact]
        public void EvictsTasksCompletedEarlier()
        {
            var manager = CreateManager(3);
            var target = new TasksTarget();

            var started = Enumerable.Range(0, 5).Select(_ => RunToEnd(manager, target)).ToArray();

            Ids(manager).Should().BeEquivalentTo(Ids(started.Skip(2)));
        }

        [Fact]
        public void KeepsRunningTasks()
        {
            var manager = CreateManager(2);
            var target = new TasksTarget();

            var running = Enumerable.Range(0, 2).Select(_ => manager.Execute(target, "ЖдатьОтпускания")).ToArray();
            for (var i = 0; i < 3; i++)
            {
                RunToEnd(manager, target);
            }

            var remaining = Ids(manager);
            ReleaseAndWait(target, running);

            // Реестр переполнен выполняющимися: короткие вытесняются сразу по завершении
            remaining.Should().BeEquivalentTo(Ids(running));
        }

        [Fact]
        public void KeepsLongTaskRightAfterItCompletes()
        {
            var manager = CreateManager(2);
            var target = new TasksTarget();

            var longOne = manager.Execute(target, "ЖдатьОтпускания");
            RunToEnd(manager, target);
            RunToEnd(manager, target);

            ReleaseAndWait(target, new[] { longOne });
            var last = RunToEnd(manager, target);

            // Долгое задание запущено первым, но завершилось позже коротких: вытеснены короткие
            Ids(manager).Should().BeEquivalentTo(Ids(new[] { longOne, last }));
        }

        [Fact]
        public void EvictsExtraTasksWhenRunningOnesComplete()
        {
            var manager = CreateManager(1);
            var target = new TasksTarget();

            var running = Enumerable.Range(0, 3).Select(_ => manager.Execute(target, "ЖдатьОтпускания")).ToArray();
            ReleaseAndWait(target, running);

            // Новых заданий нет, но лишние завершенные должны уйти сами
            Ids(manager).Should().HaveCount(1);
        }

        [Fact]
        public void ListsRunningTasksBeforeCompletedOnes()
        {
            var manager = CreateManager(3);
            var target = new TasksTarget();

            var first = RunToEnd(manager, target);
            var running = manager.Execute(target, "ЖдатьОтпускания");
            var last = RunToEnd(manager, target);

            var ids = Ids(manager);
            ReleaseAndWait(target, new[] { running });

            ids.Should().Equal(running.TaskId, first.TaskId, last.TaskId);
        }

        [Fact]
        public void RejectsNonPositiveCapacity()
        {
            Action act = () => CreateManager(0);
            act.Should().Throw<RuntimeException>();
        }

        private static BackgroundTasksManager CreateManager(int capacity)
        {
            var engine = DefaultEngineBuilder.Create().SetDefaultOptions().Build();
            engine.Initialize();
            return new BackgroundTasksManager(engine.Services.Resolve<ExecutionContext>(), capacity);
        }

        private static BackgroundTask RunToEnd(BackgroundTasksManager manager, TasksTarget target)
        {
            var task = manager.Execute(target, "Пустышка");
            task.WorkerTask.Wait(5000).Should().BeTrue();
            return task;
        }

        private static void ReleaseAndWait(TasksTarget target, IEnumerable<BackgroundTask> tasks)
        {
            target.Release();
            foreach (var task in tasks)
            {
                task.WorkerTask.Wait(30000).Should().BeTrue();
                task.State.Should().Be(TaskStateEnum.Completed, "задание должно завершиться по сигналу");
            }
        }

        private static int[] Ids(BackgroundTasksManager manager)
        {
            return Ids(manager.GetBackgroundJobs().Cast<BackgroundTask>());
        }

        private static int[] Ids(IEnumerable<BackgroundTask> tasks)
        {
            return tasks.Select(x => x.TaskId).ToArray();
        }

        [ContextClass("ЦельФоновыхЗаданийТеста", "BackgroundTasksTestTarget")]
        private class TasksTarget : AutoContext<TasksTarget>
        {
            private readonly ManualResetEventSlim _release = new ManualResetEventSlim();

            public TasksTarget()
            {
                DefineType(GetType().GetTypeFromClassMarkup());
            }

            public void Release() => _release.Set();

            [ContextMethod("Пустышка", "Noop")]
            public void Noop()
            {
            }

            [ContextMethod("ЖдатьОтпускания", "WaitForRelease")]
            public void WaitForRelease()
            {
                // Не висеть вечно, если тест упал, но и не завершаться молча раньше времени
                if (!_release.Wait(30000))
                    throw new InvalidOperationException("Задание не дождалось сигнала отпустить");
            }
        }
    }
}
