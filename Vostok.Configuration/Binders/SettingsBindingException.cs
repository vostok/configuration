using System;

namespace Vostok.Configuration.Binders
{
    internal class SettingsBindingException : Exception
    {
        public SettingsBindingException(string message)
            : base(message)
        {
        }
    }
}