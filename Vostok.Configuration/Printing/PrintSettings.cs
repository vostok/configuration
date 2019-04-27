using JetBrains.Annotations;

namespace Vostok.Configuration.Printing
{
    [PublicAPI]
    public class PrintSettings
    {
        internal static readonly PrintSettings Default = new PrintSettings();

        /// <summary>
        /// If set to <c>true</c>, <see cref="ConfigurationPrinter"/> will hide values of secret fields and properties.
        /// </summary>
        public bool HideSecretValues { get; set; } = true;
    }
}
