using System;
using JetBrains.Annotations;
using Vostok.Metrics.Model;

namespace Vostok.Metrics.Primitives.Gauge
{
    [PublicAPI]
    public static class FuncGaugeFactoryExtensions
    {
        /// <inheritdoc cref="IGauge"/>
        /// <param name="context">Context this metric will belong to.</param>
        /// <param name="name">Name of the metric. It will be added to event's <see cref="MetricEvent.Tags"/> with key set to <see cref="WellKnownTagKeys.Name"/>.</param>
        /// <param name="getValue">Value provider delegate that will be called each time when the value of this gauge is observed.</param>
        /// <param name="config">Optional config.</param>
        [NotNull]
        public static IDisposable Gauge(
            [NotNull] this IMetricContext context,
            [NotNull] string name,
            [NotNull] Func<double> getValue, 
            [CanBeNull] FuncGaugeConfig config = null)
            => new FuncGauge(context, MetricTagsMerger.Merge(context.Tags, name), getValue, config ?? FuncGaugeConfig.Default);
    }
}