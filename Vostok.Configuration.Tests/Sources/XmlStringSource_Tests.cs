using System;
using System.Linq;
using System.Xml;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Configuration.Extensions;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration.Tests.Sources
{
    [TestFixture]
    public class XmlStringSource_Tests : Sources_Test
    {
        [Test]
        public void Should_return_null_if_null_or_whitespace_string()
        {
            var xss = new XmlStringSource(null);
            xss.Get().Should().BeNull();

            xss = new XmlStringSource(" ");
            xss.Get().Should().BeNull();
        }

        [Test]
        public void Should_parse_value()
        {
            const string value = "<value>string</value>";

            var xss = new XmlStringSource(value);
            var result = xss.Get();
            result["VALUE"].Value.Should().Be("string");
        }

        [Test]
        public void Should_parse_empty_String_value()
        {
            const string value = "<value></value>";

            var xss = new XmlStringSource(value);
            var result = xss.Get();
            result["value"].Value.Should().Be(string.Empty);
        }

        [Test]
        public void Should_parse_Array_value()
        {
            const string value = @"
<array>
    <item>1</item>
    <item>2</item>
</array>";

            var xss = new XmlStringSource(value);
            var result = xss.Get();
            result["Array"]["item"].Children.Select(c => c.Value).Should().Equal("1", "2");
        }

        [Test]
        public void Should_parse_Object_value()
        {
            const string value = @"
<object>
    <item1>value1</item1>
    <item2>value2</item2>
</object>";

            var xss = new XmlStringSource(value);
            var result = xss.Get();
            result["Object"]["item1"].Value.Should().Be("value1");
            result["Object"]["item2"].Value.Should().Be("value2");
        }

        [Test]
        public void Should_parse_Object_from_attributes()
        {
            const string value = "<object key1='val1' key2='val2' />";

            var xss = new XmlStringSource(value);
            var result = xss.Get();
            result["Object"]["key1"].Value.Should().Be("val1");
            result["Object"]["key2"].Value.Should().Be("val2");
        }

        [Test]
        public void Should_parse_ArrayOfObjects_value()
        {
            const string value = @"
<object>
    <item>
        <subitem1>value1</subitem1>
    </item>
    <item>
        <subitem2>value2</subitem2>
    </item>
</object>";

            var xss = new XmlStringSource(value);
            var result = xss.Get();
            result["Object"]["item"].Children.First()["subitem1"].Value.Should().Be("value1");
            result["Object"]["item"].Children.Last()["subitem2"].Value.Should().Be("value2");
        }

        [Test]
        public void Should_parse_ArrayOfArrays_value()
        {
            const string value = @"
<object>
    <item>
        <subitem>value1</subitem>
        <subitem>value2</subitem>
    </item>
    <item>
        <subitem>value3</subitem>
        <subitem>value4</subitem>
    </item>
</object>";

            var xss = new XmlStringSource(value);
            var result = xss.Get();
            result["Object"]["item"].Children.First()["subitem"].Children.Select(c => c.Value).Should().Equal("value1", "value2");
            result["Object"]["item"].Children.Last()["subitem"].Children.Select(c => c.Value).Should().Equal("value3", "value4");
        }

        [Test]
        public void Should_ignore_attributes_presented_in_subelements_and_add_not_presented_in_subelements()
        {
            const string value = @"
<object item='value0' attr='test'>
    <item>value1</item>
    <item>value2</item>
</object>";

            var xss = new XmlStringSource(value);
            var result = xss.Get();
            result["Object"]["item"].Children.First().Value.Should().Be("value1");
            result["Object"]["item"].Children.Last().Value.Should().Be("value2");
            result["Object"]["attr"].Value.Should().Be("test");
        }

        [Test]
        public void Should_subscribe_and_get_parsed_tree()
        {
            new Action(() => ShouldSubscribeAndGetParsedTreeTest_ReturnsCountOfReceives().Should().Be(1)).ShouldPassIn(1.Seconds());
        }

        private int ShouldSubscribeAndGetParsedTreeTest_ReturnsCountOfReceives()
        {
            const string value = "<value>123</value>";
            var val = 0;

            var xss = new XmlStringSource(value);
            var sub = xss.Observe().Subscribe(
                p =>
                {
                    val++;
                    p.settings["Value"].Value.Should().Be("123");
                });
            sub.Dispose();

            return val;
        }

        [Test]
        public void Should_throw_FormatException_on_wrong_xml_format()
        {
            const string value = "wrong file format";
            new Action(() =>
            {
                new XmlStringSource(value).Get();
            }).Should().Throw<XmlException>();
        }

        [Test]
        public void Should_invoke_OnError_for_observer_on_wrong_xml_format()
        {
            const string value = "wrong file format";
            var next = 0;
            var error = 0;
            new JsonStringSource(value).Observe().SubscribeTo(node => next++, e => error++);

            next.Should().Be(0);
            error.Should().Be(1);
        }
    }
}