// This software may be modified and distributed under the terms of the MIT license.  
// See the LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Triggers
{

    /// <summary>
    /// Defines the frequencies available for triggers.
    /// </summary>
    public enum TriggerFrequency
    {
        Never = 0,

        At50Hz = 20,
        At25Hz = 40,
        At10Hz = 100,
        At5Hz = 200,
        At2Hz = 500,

        EverySecond = 1000,
        Every5Seconds = 5000,
        Every10Seconds = 10000,
        Every30Seconds = 30000,

        EveryMinute = 60000,
        Every5Minutes = 300000,
        Every10Minutes = 600000,
    }

    /// <summary>
    /// Provides <see cref="IObservable{T}"/> that emits a <see cref="Unit"/> on a
    /// defined frequency.
    /// </summary>
    /// <example>
    /// ```cs
    /// var trigger = triggerProvider.OnFrequency(TriggerFrequency.At10Hz)
    ///     .Subscribe(_ =&gt; DoSomething10TimesPerSecond();
    /// ```
    /// </example>
    public interface ITriggerProvider
    {

        /// <summary>
        /// Get a stream that emits a value at the requested frequency
        /// </summary>
        /// <param name="frequency">The desired trigger frequency</param>
        IObservable<Unit> OnFrequency(TriggerFrequency frequency);
    }

    /// <inheritdoc cref="ITriggerProvider"/>
    /// <remarks>
    /// Rather than run multiple intervals the provider runs a single interval at 50Hz
    /// and will use the <see cref="Observable.Buffer"/> method to decimate the events 
    /// down to the required frequency.
    /// </remarks>
    public sealed class TriggerProvider : ITriggerProvider
    {
        readonly int BaseInterval = 20;

        /// <inheritdoc />
        public IObservable<Unit> OnFrequency(TriggerFrequency frequency)
        {
            if (frequency == TriggerFrequency.Never)
            {
                return voidPulse;
            }
            if (!this.triggerCache.TryGetValue(frequency, out var result))
            {
                var cycles = (int)frequency / BaseInterval;
                result = this.basePulse
                    .Buffer(cycles)
                    .Select(_ => Unit.Default);
                this.triggerCache.Add(frequency, result);
            }
            return result;
        }

        /// <summary>
        /// Create a new instance of the <see cref="TriggerProvider"/> class
        /// </summary>
        /// <param name="scheduler">
        /// Optional <see cref="IScheduler"/> that the trigger interval should
        /// use. Defaults to <see cref="TaskPoolScheduler.Default"/>.
        /// </param>
        public TriggerProvider(IScheduler scheduler = null)
        {
            scheduler ??= TaskPoolScheduler.Default;
            var interval = TimeSpan.FromMilliseconds(BaseInterval);
            this.basePulse = Observable
                .Interval(interval, scheduler)
                .Select(_ => Unit.Default);
        }
        readonly IObservable<Unit> basePulse;
        readonly IObservable<Unit> voidPulse = Observable.Empty<Unit>();
        readonly Dictionary<TriggerFrequency, IObservable<Unit>> triggerCache = new();

    }
}

