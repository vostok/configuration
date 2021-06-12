using System;
using SimpleInjector;

namespace Vostok.Configuration.Helpers
{
    internal static class ContainerExtensions
    {
        public static void RegisterConditional(
            this Container container,
            Type serviceType,
            Func<TypeFactoryContext, Type> implementationTypeFactory,
            Predicate<PredicateContext> predicate) => container.RegisterConditional(serviceType, implementationTypeFactory, Lifestyle.Transient, predicate);
    }
}