using NUnit.Framework;
using SimpleInjector;
using Vostok.Configuration.Binders;

namespace Vostok.Configuration.Tests.Binders
{
    public class Binders_Test
    {
        private DefaultSettingsBinder binder;
        protected Container Container => binder.Container;

        [SetUp]
        public void SetUp()
        {
            binder = new DefaultSettingsBinder().WithDefaultParsers();
        }
    }
}