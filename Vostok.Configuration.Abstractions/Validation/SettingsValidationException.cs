using System;

namespace Vostok.Configuration.Abstractions.Validation
{
    public class SettingsValidationException : Exception
    {
        public SettingsValidationException(string message)
            : base(message)
        {
        }
    }
}