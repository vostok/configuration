using System.Collections.Generic;
using System.Linq;

namespace Vostok.Configuration.Binders.Results
{
    internal static class SettingsBindingResultExtensions
    {
        public static IEnumerable<SettingsBindingError> ForProperty(this IEnumerable<SettingsBindingError> errors, string property) =>
            errors.Select(e => SettingsBindingError.Property(property, e));

        public static IEnumerable<SettingsBindingError> ForIndex<T>(this IEnumerable<SettingsBindingError> errors, T index) =>
            errors.Select(e => SettingsBindingError.Index(index?.ToString() ?? "<null>", e));

        public static SettingsBindingResult<TTarget> Convert<TSource, TTarget>(this SettingsBindingResult<TSource> result)
            where TSource : TTarget =>
            result.Errors.Any() ? SettingsBindingResult.Errors<TTarget>(result.Errors) : SettingsBindingResult.Success<TTarget>(result.Value);

        public static SettingsBindingResult<TSource?> ConvertToNullable<TSource>(this SettingsBindingResult<TSource> result)
            where TSource : struct =>
            result.Errors.Any() ? SettingsBindingResult.Errors<TSource?>(result.Errors) : SettingsBindingResult.Success<TSource?>(result.Value);
    }
}