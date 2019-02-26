using System;
using Vostok.Configuration.Parsers;

namespace Vostok.Configuration.Binders
{
    internal interface ISettingsBinderProvider
    {
        ISafeSettingsBinder<T> CreateFor<T>();

        ISafeSettingsBinder<object> CreateFor(Type type);

        void SetupCustomBinder<TValue>(ISafeSettingsBinder<TValue> binder);

        void SetupCustomBinder(Type binderType, Predicate<Type> condition);

        void SetupParserFor<T>(ITypeParser parser);
    }
}