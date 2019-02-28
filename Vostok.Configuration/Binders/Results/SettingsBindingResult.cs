using System;
using System.Collections.Generic;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Binders.Results
{
    internal static class SettingsBindingResult
    {
        public static SettingsBindingResult<TSettings> NodeTypeMismatch<TSettings>(ISettingsNode node) =>
            Error<TSettings>($"A {node.GetType().Name} cannot be bound to '{typeof(TSettings)}'");

        public static SettingsBindingResult<TSettings> ParsingError<TSettings>(string value) =>
            Error<TSettings>($"Value '{value}' cannot be parsed as '{typeof(TSettings)}'.");

        public static SettingsBindingResult<TSettings> RequiredPropertyIsNull<TSettings>(string name) =>
            Error<TSettings>($"Required field or property '{name}' must have a non-null value.");

        public static SettingsBindingResult<TSettings> DictionaryKeyIsNull<TSettings>() =>
            Error<TSettings>("Dictionary key cannot be null.");

        public static SettingsBindingResult<TSettings> BinderNotFound<TSettings>(Type type) =>
            Error<TSettings>($"Could not find suitable binder for type '{type}'.");

        public static SettingsBindingResult<TSettings> Error<TSettings>(string error) =>
            new SettingsBindingResult<TSettings>(default, new[] {SettingsBindingError.Message(error)});

        public static SettingsBindingResult<TSettings> Errors<TSettings>(IList<SettingsBindingError> errors) =>
            new SettingsBindingResult<TSettings>(default, errors);

        public static SettingsBindingResult<TSettings> Success<TSettings>(TSettings result) =>
            new SettingsBindingResult<TSettings>(result, Array.Empty<SettingsBindingError>());

        public static SettingsBindingResult<TSettings> Catch<TSettings>(Func<SettingsBindingResult<TSettings>> action)
        {
            try
            {
                return action();
            }
            catch (Exception error)
            {
                return Error<TSettings>(error.ToString());
            }
        }
    }
}