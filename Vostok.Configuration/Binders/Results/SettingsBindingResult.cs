using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Binders.Results
{
    public static class SettingsBindingResult
    {
        public static SettingsBindingResult<TSettings> NodeTypeMismatch<TSettings>(ISettingsNode node) => 
            Error<TSettings>($"A {node.GetType().Name} cannot be bound to '{typeof(TSettings)}'");

        public static SettingsBindingResult<TSettings> ParsingError<TSettings>(string value) => 
            Error<TSettings>($"Value '{value}' cannot be parsed as '{typeof(TSettings)}'.");

        public static SettingsBindingResult<TSettings> RequiredPropertyIsNull<TSettings>(string name) => 
            Error<TSettings>($"Required field or property '{name}' must have a non-null value.");

        public static SettingsBindingResult<TSettings> Error<TSettings>(string error) => 
            new SettingsBindingResult<TSettings>(default, new[] {SettingsBindingError.Message(error), });

        public static SettingsBindingResult<TSettings> Success<TSettings>(TSettings result) => 
            new SettingsBindingResult<TSettings>(result, Enumerable.Empty<SettingsBindingError>());

        public static SettingsBindingResult<TSettings> Create<TSettings>(TSettings result, IEnumerable<SettingsBindingError> errors) =>
            new SettingsBindingResult<TSettings>(result, errors);

        public static IEnumerable<SettingsBindingError> ForProperty(this IEnumerable<SettingsBindingError> errors, string property) =>
            errors.Select(e => SettingsBindingError.Property(property, e));
        
        public static IEnumerable<SettingsBindingError> ForIndex<T>(this IEnumerable<SettingsBindingError> errors, T index) =>
            errors.Select(e => SettingsBindingError.Index(index.ToString(), e));

        public static SettingsBindingResult<TTarget> Convert<TSource, TTarget>(this SettingsBindingResult<TSource> result) where TSource : TTarget =>
            Create((TTarget)result.Value, result.Errors);

        public static SettingsBindingResult<TSource?> ConvertToNullable<TSource>(this SettingsBindingResult<TSource> result) where TSource : struct =>
            Create((TSource?)result.Value, result.Errors);
    }

    [PublicAPI]
    public class SettingsBindingResult<TSettings>
    {
        internal SettingsBindingResult(TSettings value, IEnumerable<SettingsBindingError> errors)
        {
            Value = value;
            Errors = errors;
        }

        public TSettings Value { get; }

        public IEnumerable<SettingsBindingError> Errors { get; }

        public TSettings UnwrapIfNoErrors()
        {
            if (Errors.Any())
                throw new SettingsBindingException(
                    $"Failed to bind settings to type '{typeof(TSettings)}':{Environment.NewLine}" +
                    string.Join(Environment.NewLine, Errors.Select(e => "\t- " + e)));

            return Value;
        }
    }
}