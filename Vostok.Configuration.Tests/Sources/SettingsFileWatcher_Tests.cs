using NUnit.Framework;

namespace Vostok.Configuration.Tests.Sources
{
    [TestFixture]
    public class SettingsFileWatcher_Tests
    {
        [Test]
        public void Should_Observe_file()
        {
            new JsonFileSource_Tests().Should_Observe_file();
        }

        [Test]
        public void Should_not_Observe_file()
        {
            new JsonFileSource_Tests().Should_not_Observe_file_twice();
        }
    }
}