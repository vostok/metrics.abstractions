using System;
using JetBrains.Annotations;
using Vostok.Metrics.Model;

namespace Vostok.Metrics.Primitives.Gauge
{
    [PublicAPI]
    public class GaugeConfig
    {
        internal static readonly GaugeConfig Default = new GaugeConfig();

        /// <summary>
        /// See <see cref="MetricEvent.Unit"/> and <see cref="WellKnownUnits"/> for more info.
        /// </summary>
        [CanBeNull]
        [ValueProvider("Vostok.Metrics.WellKnownUnits")]
        public string Unit { get; set; }

        /// <summary>
        /// Period of scraping gauge's current value. If left <c>null</c>, context default period will be used.
        /// </summary>
        [CanBeNull]
        public TimeSpan? ScrapePeriod { get; set; }

        /// <summary>
        /// If set to <c>true</c>, gauge value will be reset to <see cref="InitialValue"/> after each scrape.
        /// </summary>
        public bool ResetOnScrape { get; set; }

        /// <summary>
        /// Initial value of the gauge. Zero by default.
        /// </summary>
        public double InitialValue { get; set; }
    }
}