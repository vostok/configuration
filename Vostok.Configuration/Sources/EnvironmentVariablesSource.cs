using System;
using System.Collections;
using System.Reactive.Linq;
using System.Text;
using Kontur.Synchronization;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Environment variables converter to <see cref="ISettingsNode"/> tree
    /// </summary>
    public class EnvironmentVariablesSource : IConfigurationSource
    {
        private readonly TaskSource taskSource;
        private ISettingsNode currentValue;
        private readonly AtomicBoolean neverParsed;

        /// <inheritdoc />
        /// <summary>
        /// <para>Creates a <see cref="EnvironmentVariablesSource"/> instance.</para>
        /// </summary>
        public EnvironmentVariablesSource()
        {
            taskSource = new TaskSource();
            neverParsed = new AtomicBoolean(true);
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns previously parsed <see cref="ISettingsNode"/> tree.
        /// </summary>
        public ISettingsNode Get() => taskSource.Get(Observe());

        /// <inheritdoc />
        /// <summary>
        /// <para>Subscribtion to <see cref="ISettingsNode"/> tree changes.</para>
        /// <para>Returns current value immediately on subscribtion.</para>
        /// </summary>
        public IObservable<ISettingsNode> Observe()
        {
            if (neverParsed)
            {
                neverParsed.TrySetFalse();
                currentValue = GetSettings(GetVariables());
            }

            return Observable.Return(currentValue);
        }

        // CR(krait): Why don't you allow multiple level values here? It should be possible to fill complex types using environment variables with dots in names.
        // answer: variables can be located in wrong order which can cause an exception or they can have values on every level: a, a.b, a.b.c which is not allowed.
        private static ISettingsNode GetSettings(string vars) => new IniStringSource(vars, false).Get();

        private static string GetVariables()
        {
            var builder = new StringBuilder();
            foreach (DictionaryEntry ev in Environment.GetEnvironmentVariables())
                builder.AppendLine($"{ev.Key}={ev.Value}");
            return builder.ToString();
        }
    }
}