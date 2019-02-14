using System;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Parsers;

namespace Vostok.Configuration.Binders
{
    internal interface ISettingsBinderProvider
    {
        ISettingsBinder<T> CreateFor<T>();

        ISettingsBinder<object> CreateFor(Type type);

        void SetupCustomBinder<T>(ISettingsBinder<T> binder);
        
        void SetupParserFor<T>(ITypeParser parser);
    }
}