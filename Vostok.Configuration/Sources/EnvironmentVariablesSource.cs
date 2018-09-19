using System;
using System.Collections;
using System.Reactive.Linq;
using System.Text;
using Vostok.Configuration.Abstractions;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Environment variables converter to <see cref="ISettingsNode"/> tree
    /// </summary>
    public class EnvironmentVariablesSource : IConfigurationSource
    {
        private readonly TaskSource taskSource;
        private volatile bool neverParsed;
        private (ISettingsNode, Exception) currentValue;

        /// <inheritdoc />
        /// <summary>
        /// <para>Creates a <see cref="EnvironmentVariablesSource"/> instance.</para>
        /// </summary>
        public EnvironmentVariablesSource()
        {
            taskSource = new TaskSource();
            neverParsed = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns previously parsed <see cref="ISettingsNode"/> tree.
        /// </summary>
        public ISettingsNode Get() => taskSource.Get(Observe()).settings;

        private static ISettingsNode GetSettings(string vars) => new IniStringSource(vars, false).Get();

        private static string GetVariables()
        {
            var builder = new StringBuilder();
            foreach (DictionaryEntry ev in Environment.GetEnvironmentVariables())
                builder.AppendLine($"{ev.Key}={ev.Value}");
            return builder.ToString();
        }

        public IObservable<(ISettingsNode settings, Exception error)> Observe()
        {
            if (neverParsed)
            {
                currentValue = (GetSettings(GetVariables()), null as Exception);
                neverParsed = false;
            }

            return Observable.Return(currentValue);
        }
    }
}