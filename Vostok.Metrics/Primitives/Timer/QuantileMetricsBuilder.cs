﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Vostok.Metrics.Models;

namespace Vostok.Metrics.Primitives.Timer
{
    /// <summary>
    /// Builds array of <see cref="MetricEvent"/>'s from array of values and quantiles.
    /// </summary>
    [PublicAPI]
    public class QuantileMetricsBuilder
    {
        private readonly MetricTags tags;
        private readonly MetricTags countTags;
        private readonly MetricTags minTags;
        private readonly MetricTags maxTags;
        private readonly MetricTags averageTags;

        private double[] quantiles;
        private MetricTags[] quantileTags;
        private string unit;

        /// <summary>
        /// If <paramref name="quantiles"/> is <c>null</c>, <see cref="Quantiles.DefaultQuantiles"/> will be used.
        /// </summary>
        public QuantileMetricsBuilder([CanBeNull] double[] quantiles, [NotNull] MetricTags tags, [CanBeNull] string unit)
        {
            this.tags = tags;
            this.quantiles = quantiles = quantiles ?? Quantiles.DefaultQuantiles;
            this.unit = unit;

            quantileTags = Quantiles.QuantileTags(quantiles, tags);

            countTags = tags.Append(WellKnownTagKeys.Aggregate, WellKnownTagValues.AggregateCount);
            minTags = tags.Append(WellKnownTagKeys.Aggregate, WellKnownTagValues.AggregateMin);
            maxTags = tags.Append(WellKnownTagKeys.Aggregate, WellKnownTagValues.AggregateMax);
            averageTags = tags.Append(WellKnownTagKeys.Aggregate, WellKnownTagValues.AggregateAverage);
        }

        public IEnumerable<MetricEvent> Build(double[] values, DateTimeOffset timestamp)
            => Build(values, values.Length, values.Length, timestamp);

        // CR(iloktionov): Remove min and max by default (they're not of much use). We can specify 0 and 1 quantiles instead.
        public IEnumerable<MetricEvent> Build(double[] values, int size, int totalCount, DateTimeOffset timestamp)
        {
            Array.Sort(values, 0, size);
            
            var result = new List<MetricEvent>
            {
                new MetricEvent(totalCount, countTags, timestamp, null, null, null),
                new MetricEvent(GetMin(values, size), minTags, timestamp, unit, null, null),
                new MetricEvent(GetMax(values, size), maxTags, timestamp, unit, null, null),
                new MetricEvent(GetAverage(values, size), averageTags, timestamp, unit, null, null)
            };

            for (var i = 0; i < quantiles.Length; i++)
            {
                result.Add(new MetricEvent(
                    Quantiles.GetQuantile(quantiles[i], values, size), quantileTags[i], timestamp, unit, null, null));
            }

            return result;
        }

        private static double GetAverage(double[] values, int size)
            => size == 0 ? 0 : values.Take(size).Average();

        private static double GetMin(double[] values, int size)
            => size == 0 ? 0 : values.Take(size).Min();

        private static double GetMax(double[] values, int size)
            => size == 0 ? 0 : values.Take(size).Max();
    }
}