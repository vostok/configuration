using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Binders.Results;

namespace Vostok.Configuration.Tests.Binders
{
    [TestFixture]
    internal class SettingsBindingResultWrapper_Tests
    {
        [Test]
        public void Should_not_throw_from_ctor_when_wrapped_result_contains_errors()
        {
            var result = SettingsBindingResult.Error<Implementation>("error");

            var wrappedResult = new SettingsBindingResultWrapper<IAbstraction, Implementation>(result);

            wrappedResult.Errors.Should().Equal(result.Errors);
        }

        private interface IAbstraction { }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class Implementation : IAbstraction { }
    }
}