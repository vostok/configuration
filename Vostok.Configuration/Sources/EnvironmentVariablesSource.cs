using System;
using System.Collections;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Environment variables converter to <see cref="IRawSettings"/> tree
    /// </summary>
    public class EnvironmentVariablesSource : IConfigurationSource
    {
        private readonly TaskSource taskSource;
        private IRawSettings currentValue;
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
        /// Returns previously parsed <see cref="IRawSettings"/> tree.
        /// </summary>
        public IRawSettings Get() => taskSource.Get(Observe());

        /// <inheritdoc />
        /// <summary>
        /// <para>Subscribtion to <see cref="IRawSettings"/> tree changes.</para>
        /// <para>Returns current value immediately on subscribtion.</para>
        /// </summary>
        public IObservable<IRawSettings> Observe()
        {
            if (neverParsed)
            {
                neverParsed = false;
                currentValue = GetSettings(GetVariables());
            }

            return Observable.Create<IRawSettings>(
                observer =>
                {
                    observer.OnNext(currentValue);
                    return Disposable.Empty;
                });
        }

        public void Dispose()
        {
        }

        private static IRawSettings GetSettings(string vars)
        {
            using (var source = new IniStringSource(vars))
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