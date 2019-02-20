using System;
using Vostok.Configuration.Parsers;

namespace Vostok.Configuration.Binders
{
    internal interface ISettingsBinderProvider
    {
        ISettingsBinder<T> CreateFor<T>();

        ISettingsBinder<object> CreateFor(Type type);

        void SetupCustomBinder<TValue>(ISettingsBinder<TValue> binder);

        void SetupCustomBinder(Type binderType, Predicate<Type> condition);

        void SetupParserFor<T>(ITypeParser parser);
    }
}