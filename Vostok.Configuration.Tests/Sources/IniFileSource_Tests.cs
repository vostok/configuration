using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Sources;
using Vostok.Configuration.Tests.Helper;

namespace Vostok.Configuration.Tests.Sources
{
    [TestFixture]
    public class IniFileSource_Tests
    {
        [Test]
        public void Should_return_null_if_file_not_exists()
        {
            var ifs = new IniFileSource("some_file");
            ifs.Get().Should().BeNull();
        }
        
        [Test]
        public void Should_parse_simple()
        {
            const string fileName = "test.ini";
            const string content = "value = 123 \n value2 = 321";

            var ifs = new IniFileSource(fileName, f =>
            {
                var watcher = new SingleFileWatcherSubstitute(f);
                watcher.GetUpdate(content); //create file
                return watcher;
            });
            var result = ifs.Get();
            result["value"].Value.Should().Be("123");
            result["value2"].Value.Should().Be("321");
        }

        [Test]
        public void Should_throw_exception_if_exception_was_thrown_and_has_no_observers()
        {
            const string fileName = "test.ini";
            const string content = "wrong file format";

            new Action(() =>
            {
                var ifs = new IniFileSource(fileName, f =>
                {
                    var watcher = new SingleFileWatcherSubstitute(f);
                    watcher.GetUpdate(content); //create file
                    return watcher;
                });
                ifs.Get();
            }).Should().Throw<Exception>();
        }
    }
}