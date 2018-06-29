using System;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration.Tests.Sources
{
    [TestFixture]
    public class JsonStringSource_Tests : Sources_Test
    {
        [Test]
        public void Should_return_null_if_null_or_whitespace_string()
        {
            var jss = new JsonStringSource(null);
            jss.Get().Should().BeNull();

            jss = new JsonStringSource(" ");
            jss.Get().Should().BeNull();
        }

        [Test]
        public void Should_parse_String_value()
        {
            const string value = "{ 'StringValue': 'string' }";

            var jss = new JsonStringSource(value);
            var result = jss.Get();
            result["StringValue"].Value.Should().Be("string");
        }

        [Test]
        public void Should_parse_Integer_value()
        {
            const string value = "{ 'IntValue': 123 }";

            var jss = new JsonStringSource(value);
            var result = jss.Get();
            result["IntValue"].Value.Should().Be("123");
        }

        [Test]
        public void Should_parse_Double_value()
        {
            const string value = "{ 'DoubleValue': 123.321 }";

            var jss = new JsonStringSource(value);
            var result = jss.Get();
            result["DoubleValue"].Value.Should().Be(123.321d.ToString(CultureInfo.CurrentCulture));
        }

        [Test]
        public void Should_parse_Boolean_value()
        {
            const string value = "{ 'BooleanValue': true }";

            var jss = new JsonStringSource(value);
            var result = jss.Get();
            result["BooleanValue"].Value.Should().Be("True");
        }

        [Test]
        public void Should_parse_Null_value()
        {
            const string value = "{ 'NullValue': null }";

            var jss = new JsonStringSource(value);
            var result = jss.Get();
            result["NullValue"].Value.Should().Be(null);
        }

        [Test]
        public void Should_parse_Array_value()
        {
            const string value = "{ 'IntArray': [1, 2, 3] }";

            var jss = new JsonStringSource(value);
            var result = jss.Get();
            result["IntArray"].Children.Select(c => c.Value).Should().Equal("1", "2", "3");
        }

        [Test]
        public void Should_parse_Object_value()
        {
            const string value = "{ 'Object': { 'StringValue': 'str' } }";

            var jss = new JsonStringSource(value);
            var result = jss.Get();
            result["Object"]["StringValue"].Value.Should().Be("str");
        }

        [Test]
        public void Should_parse_ArrayOfObjects_value()
        {
            const string value = "{ 'Array': [{ 'StringValue': 'str' }, { 'IntValue': 123 }] }";

            var jss = new JsonStringSource(value);
            var result = jss.Get();
            result["Array"].Children
                .SelectMany(c => c.Children)
                .Select(c => c.Value)
                .Should().Equal("str", "123");
        }

        [Test]
        public void Should_parse_ArrayOfNulls_value()
        {
            const string value = "{ 'Array': [null, null] }";

            var jss = new JsonStringSource(value);
            var result = jss.Get();
            result["array"].Children.Select(c => c.Value).Should().Equal(null, null);
        }

        [Test]
        public void Should_parse_ArrayOfArrays_value()
        {
            const string value = "{ 'Array': [['s', 't'], ['r']] }";

            var jss = new JsonStringSource(value);
            var result = jss.Get();
            result["Array"].Children.First().Children.Select(c => c.Value).Should().Equal("s", "t");
            result["Array"].Children.Last().Children.Select(c => c.Value).Should().Equal("r");
        }

        [Test]
        public void Should_subscribe_and_get_parsed_tree()
        {
            new Action(() => ShouldSubscribeAndGetParsedTreeTest_ReturnsCountOfReceives().Should().Be(1)).ShouldPassIn(1.Seconds());
        }

        private int ShouldSubscribeAndGetParsedTreeTest_ReturnsCountOfReceives()
        {
            const string value = "{ 'IntValue': 123 }";
            var val = 0;

            var jss = new JsonStringSource(value);
            var sub = jss.Observe().Subscribe(
                settings =>
                {
                    val++;
                    settings["IntValue"].Value.Should().Be("123");
                });
            sub.Dispose();

            return val;
        }
    }
}