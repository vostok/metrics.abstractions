using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Metrics.Models;

// ReSharper disable AssignNullToNotNullAttribute
// ReSharper disable ObjectCreationAsStatement

namespace Vostok.Metrics.Tests.Models
{
    [TestFixture]
    internal class MetricEvent_Tests
    {
        [Test]
        public void Should_not_accept_null_tags()
        {
            Action action = () => new MetricEvent(default, default, default, default, default, default);

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Should_not_accept_empty_tags()
        {
            Action action = () => new MetricEvent(default, MetricTags.Empty, default, default, default, default);

            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void Equals_should_ignore_timezone()
        {
            var event1 = new MetricEvent(12.34d, MetricTags.Empty.Append("k", "v").Append("k2", "v2"), DateTimeOffset.Now, WellKnownUnits.Seconds, WellKnownAggregationTypes.Timer, null);
            var event2 = new MetricEvent(event1.Value, event1.Tags, event1.Timestamp.ToUniversalTime(), event1.Unit, event1.AggregationType, event1.AggregationParameters);

            event2.Should().Be(event1);
        }
    }
}