using JetBrains.Annotations;

namespace Vostok.Configuration.Helpers;

public class ConfigurationHealthCheckResult
{
    public static readonly ConfigurationHealthCheckResult Successful = new ConfigurationHealthCheckResult(null);

    public ConfigurationHealthCheckResult(string error) =>
        Error = error;

    [CanBeNull]
    public string Error { get; }

    public override string ToString() => Error ?? "Successful";
}