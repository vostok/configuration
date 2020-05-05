using System;
using System.Threading;
using JetBrains.Annotations;
using Vostok.Commons.Helpers.Disposable;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Binders
{
    internal static class ClassStructBinderSeed
    {
        private static readonly AsyncLocal<(ISettingsNode node, object seed)> Storage;

        static ClassStructBinderSeed()
            => Storage = new AsyncLocal<(ISettingsNode node, object seed)>();

        [CanBeNull]
        public static object Get(ISettingsNode node, Type type)
        {
            var current = Storage.Value;
            if (current.seed == null ||
                current.node == null ||
                !ReferenceEquals(current.seed.GetType(), type) ||
                !ReferenceEquals(current.node, node))
                return null;

            return current.seed;
        }

        [NotNull]
        public static IDisposable Use(ISettingsNode node, object seed)
        {
            if (node == null || seed == null)
                return new EmptyDisposable();

            var oldStorage = Storage.Value;

            Storage.Value = (node, seed);

            return new ActionDisposable(() => Storage.Value = oldStorage);
        }
    }
}
