using NUnit.Framework;
using Vostok.Configuration.Sources.Watchers;

namespace Vostok.Configuration.Tests.Sources
{
    public class Sources_Test
    {
        [TearDown]
        public void TearDown() => SettingsFileWatcher.ClearCache();
    }
}