/*using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Sources;
using Vostok.Configuration.Tests.Helper;

namespace Vostok.Configuration.Tests.Sources
{
    [TestFixture]
    public class IniFileSource_Tests
    {
        private const string TestName = nameof(IniFileSource);
        
        [TearDown]
        public void Cleanup()
        {
            TestHelper.DeleteAllFiles(TestName);
        }

        [Test]
        public void Should_return_null_if_file_not_exists()
        {
            using (var ifs = new IniFileSource("some_file"))
                ifs.Get().Should().BeNull();
        }
        
        [Test]
        public void Should_parse_simple()
        {
            var fileName = TestHelper.CreateFile(TestName, "value = 123 \n value2 = 321");

            using (var ifs = new IniFileSource(fileName))
            {
                var result = ifs.Get();
                result["value"].Value.Should().Be("123");
                result["value2"].Value.Should().Be("321");
            }
        }

        [Test]
        public void Should_throw_exception_if_exception_was_thrown_and_has_no_observers()
        {
            var fileName = TestHelper.CreateFile(TestName, "wrong file format");
            new Action(() =>
            {
                using (var ifs = new IniFileSource(fileName))
                    ifs.Get();
            }).Should().Throw<Exception>();
        }
    }
}*/