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

        /// <summary>
        /// Format to print objects in.
        /// </summary>
        public PrintFormat Format = PrintFormat.YAML;

        /// <summary>
        /// If set to <c>true</c> initial indent will be applied.
        /// </summary>
        public bool InitialIndent { get; set; }
    }
}
