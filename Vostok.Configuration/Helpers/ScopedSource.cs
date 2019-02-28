using System;
using System.Reactive.Linq;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Helpers
{
    /// <summary>
    /// Copy from Vostok.Configuration.Sources module to avoid dependency from it
    /// </summary>
    internal class ScopedSource : IConfigurationSource
    {
        private readonly IConfigurationSource source;
        private readonly string[] scope;

        public ScopedSource(IConfigurationSource source, params string[] scope)
        {
            this.source = source ?? throw new ArgumentNullException(nameof(source));
            this.scope = scope ?? throw new ArgumentNullException(nameof(scope));
        }

        public IObservable<(ISettingsNode settings, Exception error)> Observe() =>
            source.Observe().Select(pair => (pair.settings?.ScopeTo(scope), pair.error));
    }
}