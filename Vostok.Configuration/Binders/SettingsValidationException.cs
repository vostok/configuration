using System;
using JetBrains.Annotations;

namespace Vostok.Configuration.Binders
{
    [PublicAPI]
    public class SettingsValidationException : Exception
    {
        public SettingsValidationException(string message)
            : base(message)
        {
        }
    }
}