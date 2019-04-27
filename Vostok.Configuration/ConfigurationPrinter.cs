using System;
using JetBrains.Annotations;
using Vostok.Configuration.Printing;

namespace Vostok.Configuration
{
    [PublicAPI]
    public static class ConfigurationPrinter
    {
        /// <inheritdoc cref="Print(object,PrintSettings)"/>
        [NotNull]
        public static string Print([CanBeNull] object item)
            => Print(item, PrintSettings.Default);

        /// <summary>
        /// Pretty-prints given settings object in YAML-like human-readable syntax. Useful for logging purposes.
        /// </summary>
        [NotNull]
        public static string Print([CanBeNull] object item, [CanBeNull] PrintSettings settings)
        {
            try
            {
                var token = PrintTokenFactory.Create(item, settings ?? PrintSettings.Default);
                var context = new PrintContext();

                token.Print(context);

                return context.Content;
            }
            catch (Exception error)
            {
                return $"<error of type '{error.GetType().Name}': {error.Message}>";
            }
        }
    }
}
