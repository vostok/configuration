namespace Vostok.Configuration.Abstractions.Validation
{
    public interface ISettingsValidator
    {
        SettingsValidationErrors Validate(object value, string prefix = "");
    }
}