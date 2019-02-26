using System;
using System.Collections.Generic;
using System.Linq;

namespace Vostok.Configuration.Binders.Results
{
    internal class SettingsBindingResult<TSettings>
    {
        private readonly TSettings value;

        public SettingsBindingResult(TSettings value, IList<SettingsBindingError> errors)
        {
            this.value = value;
            Errors = errors;
        }

        public TSettings Value
        {
            get
            {
                if (Errors.Any())
                    throw new SettingsBindingException(
                        $"Failed to bind settings to type '{typeof(TSettings)}':{Environment.NewLine}" +
                        string.Join(Environment.NewLine, Errors.Select(e => "\t- " + e)));

                return value;
            }
        }

        public IList<SettingsBindingError> Errors { get; }
    }
}