using System;

namespace Vostok.Configuration.Binders
{
    internal interface IBinderWrapper
    {
        Type BinderType { get; }
    }
}