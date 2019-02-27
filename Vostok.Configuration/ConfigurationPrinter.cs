using System;
using JetBrains.Annotations;

namespace Vostok.Configuration.Printing
{
    [PublicAPI]
    public static class ConfigurationPrinter
    {
        /// <summary>
        /// Pretty-prints given settings object in YAML-like human-readable syntax. Useful for logging purposes.
        /// </summary>
        [NotNull]
        public static string Print([CanBeNull] object item)
        {
            try
            {
                var token = PrintTokenFactory.Create(item);
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