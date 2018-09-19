using System;

namespace Vostok.Configuration
{
    internal class SettingsValidationException : Exception
    {
        public SettingsValidationException(string message)
            : base(message)
        {
        }
    }
}