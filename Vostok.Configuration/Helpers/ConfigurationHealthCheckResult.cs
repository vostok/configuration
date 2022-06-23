using JetBrains.Annotations;

namespace Vostok.Configuration.Helpers;

[PublicAPI]
public class ConfigurationHealthCheckResult
{
    public static readonly ConfigurationHealthCheckResult Successful = new ConfigurationHealthCheckResult(null);

    public ConfigurationHealthCheckResult(string error) =>
        Error = error;

    [CanBeNull]
    public string Error { get; }

    public override string ToString() => Error ?? "Successful";
}