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
        /// Pretty-prints given settings object in YAML-like or JSON-like human-readable syntax. Useful for logging purposes.
        /// </summary>
        [NotNull]
        public static string Print([CanBeNull] object item, [CanBeNull] PrintSettings settings)
        {
            try
            {
                settings = settings ?? PrintSettings.Default;

                var token = PrintTokenFactory.Create(item, settings);

                var context = new PrintContext(settings);
                if (settings.InitialIndent)
                    context.IncreaseDepth();

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