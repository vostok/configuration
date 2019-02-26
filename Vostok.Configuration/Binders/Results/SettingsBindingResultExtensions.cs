using System.Collections.Generic;
using System.Linq;

namespace Vostok.Configuration.Binders.Results
{
    internal static class SettingsBindingResultExtensions
    {
        public static IEnumerable<SettingsBindingError> ForProperty(this IEnumerable<SettingsBindingError> errors, string property) =>
            errors.Select(e => SettingsBindingError.Property(property, e));

        public static IEnumerable<SettingsBindingError> ForIndex<T>(this IEnumerable<SettingsBindingError> errors, T index) =>
            errors.Select(e => SettingsBindingError.Index(index.ToString(), e));

        public static SettingsBindingResult<TTarget> Convert<TSource, TTarget>(this SettingsBindingResult<TSource> result)
            where TSource : TTarget =>
            SettingsBindingResult.Create((TTarget)result.Value, result.Errors);

        public static SettingsBindingResult<TSource?> ConvertToNullable<TSource>(this SettingsBindingResult<TSource> result)
            where TSource : struct =>
            SettingsBindingResult.Create((TSource?)result.Value, result.Errors);
    }
}