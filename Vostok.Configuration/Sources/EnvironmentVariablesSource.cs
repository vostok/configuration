using System;
using System.Collections;
using System.Text;
using Vostok.Commons.Conversions;

namespace Vostok.Configuration.Sources
{
    /// <inheritdoc />
    /// <summary>
    /// Ini converter to RawSettings tree from file
    /// </summary>
    public class EnvironmentVariablesSource : IConfigurationSource
    {
        private static readonly TimeSpan MinObservationPeriod = 100.Milliseconds();

        private readonly TimeSpan observePeriod;

        /// <param name="observePeriod">Observation period (min 100 ms)</param>
        public EnvironmentVariablesSource(TimeSpan observePeriod = default) //todo: callback?
        {
            this.observePeriod = observePeriod < MinObservationPeriod ? MinObservationPeriod : observePeriod;
            FixedPeriodSettingsWatcher.StartFixedPeriodSettingsWatcher(1.Seconds(), 10.Seconds(), Get, this.observePeriod);
        }

        public RawSettings Get() => Get(GetVariables());

        public IObservable<RawSettings> Observe() => 
            FixedPeriodSettingsWatcher.Observe(observePeriod);

        public void Dispose()
        {
            FixedPeriodSettingsWatcher.RemoveObservers(observePeriod);
        }

        private static RawSettings Get(string vars)
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