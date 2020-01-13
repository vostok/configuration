using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Tests.Helpers
{
    [TestFixture]
    internal class AttributeHelper_Tests
    {
        [Test]
        public void Helper_should_give_null_on_get_for_type_if_none_attributes_were_set()
        {
            AttributeHelper
                .Get<ExampleAttribute>(typeof(WithoutAttributes))
                .Should().BeNull();
        }

        [Test]
        public void Helper_should_give_empty_array_on_select_for_type_if_none_attributes_were_set()
        {
            AttributeHelper
                .Select<ExampleAttribute>(typeof(WithoutAttributes))
                .Should().BeEmpty();
        }

        [Test]
        public void Helper_should_give_null_on_get_for_member_if_none_attributes_were_set()
        {
            var propertyInfo = typeof(WithoutAttributes).GetInstanceProperties().Single(p => p.Name == nameof(WithoutAttributes.Property));

            AttributeHelper
                .Get<ExampleAttribute>(propertyInfo)
                .Should().BeNull();
        }

        [Test]
        public void Helper_should_give_empty_array_on_select_for_member_if_none_attributes_were_set()
        {
            var propertyInfo = typeof(WithoutAttributes).GetInstanceProperties().Single(p => p.Name == nameof(WithoutAttributes.Property));

            AttributeHelper
                .Select<ExampleAttribute>(propertyInfo)
                .Should().BeEmpty();
        }

        [Test]
        public void Helper_should_give_one_on_get_for_type_if_attributes_were_set()
        {
            AttributeHelper
                .Get<ExampleAttribute>(typeof(WithAttributes))
                .Should().NotBeNull()
                .And.BeOfType<ExampleAttribute>();
        }

        [Test]
        public void Helper_should_give_all_on_select_for_type_if_attributes_were_set()
        {
            AttributeHelper
                .Select<ExampleAttribute>(typeof(WithAttributes))
                .Should().HaveCount(2)
                .And.AllBeOfType<ExampleAttribute>();
        }

        [Test]
        public void Helper_should_give_one_on_get_for_member_if_attributes_were_set()
        {
            var propertyInfo = typeof(WithAttributes).GetInstanceProperties().Single(p => p.Name == nameof(WithAttributes.Property));

            AttributeHelper
                .Get<ExampleAttribute>(propertyInfo)
                .Should().NotBeNull()
                .And.BeOfType<ExampleAttribute>();
        }

        [Test]
        public void Helper_should_give_all_on_select_for_member_if_attributes_were_set()
        {
            var propertyInfo = typeof(WithAttributes).GetInstanceProperties().Single(p => p.Name == nameof(WithAttributes.Property));

            AttributeHelper
                .Select<ExampleAttribute>(propertyInfo)
                .Should().HaveCount(3)
                .And.AllBeOfType<ExampleAttribute>();
        }
        [Test]
        public void Helper_should_give_false_on_has_for_type_if_none_attributes_were_set()
        {
            AttributeHelper
                .Has<ExampleAttribute>(typeof(WithoutAttributes))
                .Should().BeFalse();
        }

        [Test]
        public void Helper_should_give_false_on_has_for_member_if_none_attributes_were_set()
        {
            var propertyInfo = typeof(WithoutAttributes).GetInstanceProperties().Single(p => p.Name == nameof(WithoutAttributes.Property));

            AttributeHelper
                .Has<ExampleAttribute>(propertyInfo)
                .Should().BeFalse();
        }

        [Test]
        public void Helper_should_give_true_on_has_for_type_if_attributes_were_set()
        {
            AttributeHelper
                .Has<ExampleAttribute>(typeof(WithAttributes))
                .Should().BeTrue();
        }

        [Test]
        public void Helper_should_give_true_on_has_for_member_if_attributes_were_set()
        {
            var propertyInfo = typeof(WithAttributes).GetInstanceProperties().Single(p => p.Name == nameof(WithAttributes.Property));

            AttributeHelper
                .Has<ExampleAttribute>(propertyInfo)
                .Should().BeTrue();
        }

        private class WithoutAttributes
        {
            public object Property { get; }
        }

        [Example]
        [Example]
        private class WithAttributes
        {
            [Example]
            [Example]
            [Example]
            public object Property { get; }
        }

        [AttributeUsage(
            AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Property,
            AllowMultiple = true
        )]
        private class ExampleAttribute : Attribute
        {
        }
    }
}