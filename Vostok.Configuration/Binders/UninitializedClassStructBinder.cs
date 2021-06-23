using System;
using System.Runtime.Serialization;
using Vostok.Configuration.Abstractions.Attributes;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Results;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Binders
{
    internal class UninitializedClassStructBinder<T> : ISafeSettingsBinder<T>
    {
        private readonly ClassStructBinder<T> classStructBinder;

        public UninitializedClassStructBinder(ISettingsBinderProvider binderProvider) =>
            classStructBinder = new ClassStructBinder<T>(binderProvider, InstanceFactory);

        public SettingsBindingResult<T> Bind(ISettingsNode rawSettings) => classStructBinder.Bind(rawSettings);

        public static bool CanBeUsedFor(Type type) => AttributeHelper.Get<OmitConstructorsAttribute>(type) != null;

        private static object InstanceFactory(Type type, ISettingsNode _) => FormatterServices.GetUninitializedObject(type);
    }
}