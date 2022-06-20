using System;
using JetBrains.Annotations;

namespace Vostok.Configuration.Helpers;

internal class HealthTracker
{
    private readonly string settingsType;
    private readonly string settingsSource;
    private volatile Exception lastError;

    public HealthTracker(string settingsType, string settingsSource)
    {
        this.settingsType = settingsType;
        this.settingsSource = settingsSource;
    }

    public void OnNext(Exception error = null)
    {
        lastError = error;
    }

    [CanBeNull]
    public string GetError()
    {
        var error = lastError;
        return error == null 
            ? null 
            : $"Failed to obtain settings of type {settingsType} from {settingsSource} source: {error}.";
    }
}