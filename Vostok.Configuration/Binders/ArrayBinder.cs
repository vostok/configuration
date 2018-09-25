using System;
using System.Linq;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Binders
{
    internal class ArrayBinder<T> : ISettingsBinder<T>
    {
        private readonly ISettingsBinderProvider binderProvider;

        public ArrayBinder(ISettingsBinderProvider binderProvider) =>
            this.binderProvider = binderProvider;

        public T Bind(ISettingsNode settings)
        {
            var subType = typeof(T).GetElementType();
            var binder = binderProvider.CreateFor(subType);

            var i = 0;
            var instance = Array.CreateInstance(subType, settings.Children.Count());
            foreach (var value in settings.Children.Select(n => binder.Bind(n)))
                instance.SetValue(value, i++);

            return (T)(object)instance;
        }
    }
}