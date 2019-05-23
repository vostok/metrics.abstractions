﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Metrics.Models;
using Vostok.Metrics.Primitives.Counter;
using Vostok.Metrics.Senders;

namespace Vostok.Metrics.Tests.Primitives.Counter
{
    [TestFixture]
    internal class Counter_Tests
    {
        private MetricContext context;

        [SetUp]
        public void SetUp()
        {
            context = new MetricContext(new MetricContextConfig(new DevNullMetricEventSender()));
        }

        [Test]
        public void Should_calculate_sum_and_reset_on_scrape()
        {
            var counter = (Metrics.Primitives.Counter.Counter)context.CreateCounter("name", new CounterConfig {ScrapePeriod = TimeSpan.MaxValue});
            counter.Add(1);
            counter.Add(2);
            counter.Add(42);
            Scrape(counter).Value.Should().Be(1 + 2 + 42);

            Scrape(counter).Value.Should().Be(0);

            counter.Add(123);
            Scrape(counter).Value.Should().Be(123);
        }

        [Test]
        public void Should_reject_negative_values()
        {
            var counter = (Metrics.Primitives.Counter.Counter)context.CreateCounter("name", new CounterConfig { ScrapePeriod = TimeSpan.MaxValue });
            Action check = () => counter.Add(-1);
            check.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Test]
        public void Should_be_same_for_same_dynamic_tags()
        {
            var counter = context.CreateCounter("name", "dynamic_tag", new CounterConfig {ScrapePeriod = TimeSpan.MaxValue});

            counter.For("tag").Add(1);
            counter.For("tag").Add(3);

            Scrape(counter.For("tag")).Value.Should().Be(4);
        }

        [Test]
        public void Should_be_not_same_for_different_dynamic_tags()
        {
            var counter = context.CreateCounter("name", "dynamic_tag", new CounterConfig {ScrapePeriod = TimeSpan.MaxValue});

            counter.For("tag1").Add(1);

            counter.For("tag2").Add(3);

            Scrape(counter.For("tag0")).Value.Should().Be(0);
            Scrape(counter.For("tag1")).Value.Should().Be(1);
            Scrape(counter.For("tag2")).Value.Should().Be(3);
        }

        [Test]
        public void Should_fill_metric_event()
        {
            var aggregationParameters = new Dictionary<string, string>
            {
                {"a", "aa"},
                {"b", "bb"}
            };

            var counter = context.CreateCounter(
                "name",
                new CounterConfig
                {
                    ScrapePeriod = TimeSpan.MaxValue,
                    Unit = "unit",
                    AggregationParameters = aggregationParameters
                });

            counter.Add(42);

            var timestamp = DateTimeOffset.Now;
            var metric = Scrape(counter, timestamp);

            metric.Should()
                .BeEquivalentTo(
                    new MetricEvent(
                        42,
                        new MetricTags(new MetricTag(WellKnownTagKeys.Name, "name")),
                        timestamp,
                        "unit",
                        WellKnownAggregationTypes.Counter,
                        aggregationParameters
                    ));
        }

        [Test]
        public void Should_be_auto_scrapable()
        {
            var sum = 0L;
            context = new MetricContext(new MetricContextConfig(new AdHocMetricEventSender(e => Interlocked.Add(ref sum, (long)e.Value))));

            var counter = (Metrics.Primitives.Counter.Counter)context.CreateCounter("name", new CounterConfig { ScrapePeriod = 10.Milliseconds() });

            counter.Add(1);
            Thread.Sleep(300.Milliseconds());

            sum.Should().Be(1);
        }

        [Test]
        public void Should_not_be_scraped_after_dispose()
        {
            var sum = 0L;
            context = new MetricContext(new MetricContextConfig(new AdHocMetricEventSender(e => Interlocked.Add(ref sum, (long)e.Value))));

            var counter = (Metrics.Primitives.Counter.Counter)context.CreateCounter("name", new CounterConfig { ScrapePeriod = 10.Milliseconds() });

            counter.Dispose();

            Thread.Sleep(100.Milliseconds());
            counter.Add(1);
            Thread.Sleep(300.Milliseconds());

            sum.Should().Be(0);
        }

        private static MetricEvent Scrape(ICounter counter, DateTimeOffset? timestamp = null)
        {
            return ((Metrics.Primitives.Counter.Counter)counter).Scrape(timestamp ?? DateTimeOffset.Now).Single();
        }
    }
}