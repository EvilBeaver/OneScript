/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using ScriptEngine.Machine;
using Xunit;

namespace OneScript.Core.Tests
{
    public class CodeStatHubTests
    {
        [Fact]
        public void Hits_Go_To_Every_Active_Session()
        {
            var hub = new CodeStatHub();
            var first = hub.StartSession();
            var second = hub.StartSession();
            var entry = new CodeStatEntry("script.os", "Method", 1);

            hub.MarkEntryReached(entry, 0);
            hub.MarkPrepared("script.os");
            hub.MarkEntryReached(entry);

            CountOf(first, entry).Should().Be(1);
            CountOf(second, entry).Should().Be(1);
        }

        [Fact]
        public void New_Session_Gets_Prepared_Zeros_Without_History()
        {
            var hub = new CodeStatHub();
            var first = hub.StartSession();
            var entry = new CodeStatEntry("script.os", "Method", 1);

            hub.MarkEntryReached(entry, 0);
            hub.MarkPrepared("script.os");
            hub.MarkEntryReached(entry);
            hub.MarkEntryReached(entry);

            var second = hub.StartSession();

            CountOf(first, entry).Should().Be(2);
            CountOf(second, entry).Should().Be(0);

            hub.MarkEntryReached(entry);

            CountOf(first, entry).Should().Be(3);
            CountOf(second, entry).Should().Be(1);
        }

        [Fact]
        public void Paused_Session_Does_Not_Receive_Hits()
        {
            var hub = new CodeStatHub();
            var session = hub.StartSession();
            var entry = new CodeStatEntry("script.os", "Method", 1);

            hub.MarkEntryReached(entry, 0);
            hub.MarkPrepared("script.os");
            hub.MarkEntryReached(entry);

            hub.PauseSession(session);
            hub.MarkEntryReached(entry);
            hub.MarkEntryReached(entry);

            CountOf(session, entry).Should().Be(1);

            hub.ResumeSession(session);
            hub.MarkEntryReached(entry);

            CountOf(session, entry).Should().Be(2);
        }

        [Fact]
        public void Paused_Session_Receives_Prepared_Zeros()
        {
            var hub = new CodeStatHub();
            var session = hub.StartSession();
            hub.PauseSession(session);

            var entry = new CodeStatEntry("other.os", "Other", 2);
            hub.MarkEntryReached(entry, 0);
            hub.MarkPrepared("other.os");

            CountOf(session, entry).Should().Be(0);
            session.GetStatData().Should().Contain(x => x.Entry.Equals(entry));
        }

        [Fact]
        public void Finished_Session_Does_Not_Receive_Further_Events()
        {
            var hub = new CodeStatHub();
            var finished = hub.StartSession();
            var alive = hub.StartSession();
            var first = new CodeStatEntry("script.os", "Method", 1);

            hub.MarkEntryReached(first, 0);
            hub.MarkPrepared("script.os");
            hub.MarkEntryReached(first);
            hub.FinishSession(finished);

            hub.MarkEntryReached(first);

            var second = new CodeStatEntry("later.os", "Later", 3);
            hub.MarkEntryReached(second, 0);
            hub.MarkPrepared("later.os");

            CountOf(finished, first).Should().Be(1);
            finished.GetStatData().Should().NotContain(x => x.Entry.Equals(second));
            CountOf(alive, first).Should().Be(2);
            CountOf(alive, second).Should().Be(0);
        }

        [Fact]
        public void StartSession_Reads_Zeros_From_Shared_Catalog()
        {
            var hub = new CodeStatHub();
            for (var line = 1; line <= 50; line++)
            {
                hub.MarkEntryReached(new CodeStatEntry("script.os", "Method", line), 0);
            }

            hub.MarkPrepared("script.os");
            var session = hub.StartSession();
            var data = session.GetStatData();

            data.Count.Should().Be(50);
            data.Should().OnlyContain(x => x.ExecutionCount == 0 && x.TimeElapsed == 0);
        }

        [Fact]
        public void ResumeSession_Does_Not_Revive_Finished_Session()
        {
            var hub = new CodeStatHub();
            var session = hub.StartSession();
            var entry = new CodeStatEntry("script.os", "Method", 1);

            hub.MarkEntryReached(entry, 0);
            hub.MarkPrepared("script.os");
            hub.MarkEntryReached(entry);
            hub.FinishSession(session);

            hub.ResumeSession(session);
            hub.MarkEntryReached(entry);
            hub.MarkEntryReached(entry);

            CountOf(session, entry).Should().Be(1);
        }

        [Fact]
        public void Concurrent_ResumeWatch_Does_Not_Restart_After_Pause_Or_Finish()
        {
            AssertWatchDoesNotResumeAfterControl(pause: true);
            AssertWatchDoesNotResumeAfterControl(pause: false);
        }

        [Fact]
        public void Concurrent_Hits_Do_Not_Apply_After_Pause_Or_Finish()
        {
            AssertHitsDoNotApplyAfterControl(pause: true);
            AssertHitsDoNotApplyAfterControl(pause: false);
        }

        private static void AssertWatchDoesNotResumeAfterControl(bool pause)
        {
            var hub = new CodeStatHub();
            var session = hub.StartSession();
            var entry = new CodeStatEntry("script.os", "Method", 1);
            hub.MarkEntryReached(entry, 0);
            hub.MarkPrepared("script.os");
            hub.MarkEntryReached(entry);

            using var stopResuming = new ManualResetEventSlim(false);
            var start = new Barrier(2);

            var resumer = Task.Run(() =>
            {
                start.SignalAndWait();
                while (!stopResuming.IsSet)
                    hub.ResumeWatch(entry);
            });

            start.SignalAndWait();
            if (pause)
                hub.PauseSession(session);
            else
                hub.FinishSession(session);

            var timeWhenControlReturned = TimeOf(session, entry);
            Thread.Sleep(40);
            stopResuming.Set();
            resumer.Wait();

            TimeOf(session, entry).Should().Be(timeWhenControlReturned);
        }

        private static void AssertHitsDoNotApplyAfterControl(bool pause)
        {
            var hub = new CodeStatHub();
            var session = hub.StartSession();
            var entry = new CodeStatEntry("script.os", "Method", 1);
            hub.MarkEntryReached(entry, 0);
            hub.MarkPrepared("script.os");

            using var stopHitting = new ManualResetEventSlim(false);
            var start = new Barrier(2);

            var hitter = Task.Run(() =>
            {
                start.SignalAndWait();
                while (!stopHitting.IsSet)
                    hub.MarkEntryReached(entry);
            });

            start.SignalAndWait();
            if (pause)
                hub.PauseSession(session);
            else
                hub.FinishSession(session);

            var countWhenControlReturned = CountOf(session, entry);
            var timeWhenControlReturned = TimeOf(session, entry);

            Thread.Sleep(30);
            stopHitting.Set();
            hitter.Wait();

            CountOf(session, entry).Should().Be(countWhenControlReturned);
            TimeOf(session, entry).Should().Be(timeWhenControlReturned);
        }

        private static int CountOf(CodeStatProcessor session, CodeStatEntry entry)
        {
            return session.GetStatData().Single(x => x.Entry.Equals(entry)).ExecutionCount;
        }

        private static long TimeOf(CodeStatProcessor session, CodeStatEntry entry)
        {
            return session.GetStatData().Single(x => x.Entry.Equals(entry)).TimeElapsed;
        }
    }
}
