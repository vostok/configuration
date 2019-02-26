using System;
using System.Collections.Generic;
using System.Linq;

namespace Vostok.Configuration.Binders.Results
{
    internal class SettingsBindingResult<TSettings>
    {
        internal SettingsBindingResult(TSettings value, IList<SettingsBindingError> errors)
        {
            Value = value;
            Errors = errors;
        }

        public TSettings Value { get; }

        public IList<SettingsBindingError> Errors { get; }

        public TSettings UnwrapIfNoErrors()
        {
            if (Errors.Any())
                throw new SettingsBindingException(
                    $"Failed to bind settings to type '{typeof(TSettings)}':{Environment.NewLine}" +
                    string.Join(Environment.NewLine, Errors.Select(e => "\t- " + e)));

            return Value;
        }
    }
}