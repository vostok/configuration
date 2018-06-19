using System;
using System.Collections;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using Vostok.Commons;
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
        private bool neverParsed;

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
        public ISettingsNode Get() => taskSource.Get(Observe());

        /// <inheritdoc />
        /// <summary>
        /// <para>Subscribtion to <see cref="ISettingsNode"/> tree changes.</para>
        /// <para>Returns current value immediately on subscribtion.</para>
        /// </summary>
        public IObservable<ISettingsNode> Observe()
        {
            if (neverParsed) // CR(krait): Use AtomicBoolean from core-infra here.
            {
                neverParsed = false;
                currentValue = GetSettings(GetVariables());
            }

            // CR(krait): Observable.Return(currentValue);
            return Observable.Create<ISettingsNode>(
                observer =>
                {
                    observer.OnNext(currentValue);
                    return Disposable.Empty;
                });
        }

        public void Dispose()
        {
        }

        private static ISettingsNode GetSettings(string vars)
        {
            using (var source = new IniStringSource(vars, false)) // CR(krait): Why don't you allow multiple level values here? It should be possible to fill complex types using environment variables with dots in names.
                return source.Get();
        }

        private static string GetVariables()
        {
            var builder = new StringBuilder();
            foreach (DictionaryEntry ev in Environment.GetEnvironmentVariables())
                builder.AppendLine($"{ev.Key}={ev.Value}");
            return builder.ToString();
        }
    }
}