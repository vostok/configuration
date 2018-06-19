using System;

namespace Vostok.Configuration.Abstractions
{
    public class SettingsValidationException : Exception
    {
        public SettingsValidationException(string message)
            : base(message)
        {
        }
    }
}