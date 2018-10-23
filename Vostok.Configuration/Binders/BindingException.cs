using System;

namespace Vostok.Configuration.Binders
{
    internal class BindingException : Exception
    {
        public BindingException(string message)
            : base(message)
        {
        }
    }
}