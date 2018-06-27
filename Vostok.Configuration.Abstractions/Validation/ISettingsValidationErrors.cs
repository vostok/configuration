namespace Vostok.Configuration.Abstractions.Validation
{
    public interface ISettingsValidationErrors
    {
        void ReportError(string error);
    }
}