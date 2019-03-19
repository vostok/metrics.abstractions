using System;
using Vostok.Metrics.Model;

namespace Vostok.Metrics.DynamicTags.Typed
{
    internal class TaggedMetricT<TFor, TMetric> : TaggedMetricBase<TMetric>, ITaggedMetricT<TFor, TMetric>
    {
        private readonly ITypeTagsConverter<TFor> converter;

        public TaggedMetricT(IMetricContext context, Func<MetricTags, TMetric> factory, ITypeTagsConverter<TFor> converter)
            : base(context, factory)
        {
            this.converter = converter;
        }

        public TaggedMetricT(IMetricContext context, Func<MetricTags, TMetric> factory, TimeSpan? scrapePeriod, ITypeTagsConverter<TFor> converter)
            : base(context, factory, scrapePeriod)
        {
            this.converter = converter;
        }

        public TMetric For(TFor value)
        {
            var tags = converter.Convert(value);
            return For(tags);
        }
    }
}