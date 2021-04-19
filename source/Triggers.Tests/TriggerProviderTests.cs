// This software may be modified and distributed under the terms of the MIT license.  
// See the LICENSE file in the project root for details.

using System;

using Microsoft.Reactive.Testing;
using Xunit;

namespace Triggers.Tests
{
    public class TriggerProviderTests
    {

        TestScheduler Scheduler { get; }

        ITriggerProvider Triggers { get; }

        public TriggerProviderTests()
        {
            Scheduler = new TestScheduler();
            Triggers = new TriggerProvider(Scheduler);
        }

        [Theory]
        [InlineData(TriggerFrequency.Never, 1, 0)]
        [InlineData(TriggerFrequency.At50Hz, 1, 50)]
        [InlineData(TriggerFrequency.At25Hz, 1, 25)]
        [InlineData(TriggerFrequency.At10Hz, 1, 10)]
        [InlineData(TriggerFrequency.At5Hz, 1, 5)]
        [InlineData(TriggerFrequency.At2Hz, 1, 2)]
        [InlineData(TriggerFrequency.EverySecond, 10, 10)]
        [InlineData(TriggerFrequency.Every5Seconds, 30, 6)]
        [InlineData(TriggerFrequency.Every10Seconds, 60, 6)]
        [InlineData(TriggerFrequency.Every30Seconds, 60, 2)]
        [InlineData(TriggerFrequency.EveryMinute, 600, 10)]
        [InlineData(TriggerFrequency.Every5Minutes, 3600, 12)]
        [InlineData(TriggerFrequency.Every10Minutes, 3600, 6)]
        public void EmitsAtRequestedFrequency(TriggerFrequency frequency, int periodSeconds, int expected)
        {
            var received = 0;
            Triggers.OnFrequency(frequency)
                .Subscribe(_ => received++);
            
            Scheduler.AdvanceTo(TimeSpan.FromSeconds(periodSeconds).Ticks);
            
            Assert.Equal(expected, received);
        }


        [Theory]
        [InlineData(TriggerFrequency.At50Hz, 1, 50)]
        [InlineData(TriggerFrequency.At25Hz, 1, 25)]
        [InlineData(TriggerFrequency.At10Hz, 1, 10)]
        [InlineData(TriggerFrequency.At5Hz, 1, 5)]
        [InlineData(TriggerFrequency.At2Hz, 1, 2)]
        [InlineData(TriggerFrequency.EverySecond, 10, 10)]
        [InlineData(TriggerFrequency.Every5Seconds, 30, 6)]
        [InlineData(TriggerFrequency.Every10Seconds, 60, 6)]
        [InlineData(TriggerFrequency.Every30Seconds, 60, 2)]
        [InlineData(TriggerFrequency.EveryMinute, 600, 10)]
        [InlineData(TriggerFrequency.Every5Minutes, 3600, 12)]
        [InlineData(TriggerFrequency.Every10Minutes, 3600, 6)]
        public void EmitsAtRequestedFrequencyWhenShared(TriggerFrequency frequency, int periodSeconds, int expected)
        {
            var first = 0;
            var firstTrigger = Triggers.OnFrequency(frequency)
                .Subscribe(_ => first++);

            var second = 0;
            var secondTrigger = Triggers.OnFrequency(frequency)
                .Subscribe(_ => second++);
            
            Scheduler.AdvanceTo(TimeSpan.FromSeconds(periodSeconds).Ticks);
            
            Assert.Equal(expected, first);
            Assert.Equal(expected, second);
        }

        [Theory]
        [InlineData(TriggerFrequency.At50Hz, 50, TriggerFrequency.At25Hz, 25, 1)]
        [InlineData(TriggerFrequency.At50Hz, 50, TriggerFrequency.At10Hz, 10, 1)]
        [InlineData(TriggerFrequency.At50Hz, 50, TriggerFrequency.At5Hz, 5, 1)]
        [InlineData(TriggerFrequency.At50Hz, 50, TriggerFrequency.At2Hz, 2, 1)]
        [InlineData(TriggerFrequency.At50Hz, 500, TriggerFrequency.EverySecond, 10, 10)]
        [InlineData(TriggerFrequency.EverySecond, 60, TriggerFrequency.Every5Seconds, 12, 60)]
        [InlineData(TriggerFrequency.EverySecond, 60, TriggerFrequency.Every10Seconds, 6, 60)]
        [InlineData(TriggerFrequency.EverySecond, 60, TriggerFrequency.Every30Seconds, 2, 60)]
        [InlineData(TriggerFrequency.EveryMinute, 60, TriggerFrequency.Every5Minutes, 12, 3600)]
        [InlineData(TriggerFrequency.EveryMinute, 60, TriggerFrequency.Every10Minutes, 6, 3600)]
        public void EmitsAtMultipleRequestedFrequencies(
            TriggerFrequency firstFrequency, int firstExpected,
            TriggerFrequency secondFrequency, int secondExpected,
            int periodSeconds)
        {
            var first = 0;
            var firstTrigger = Triggers.OnFrequency(firstFrequency)
                .Subscribe(_ => first++);

            var second = 0;
            var secondTrigger = Triggers.OnFrequency(secondFrequency)
                .Subscribe(_ => second++);

            Scheduler.AdvanceTo(TimeSpan.FromSeconds(periodSeconds).Ticks);
            
            Assert.Equal(firstExpected, first);
            Assert.Equal(secondExpected, second);
        }

    }
}
