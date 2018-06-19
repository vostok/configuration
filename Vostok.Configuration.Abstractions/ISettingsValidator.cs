namespace Vostok.Configuration.Abstractions
{
    public interface ISettingsValidator<in T>
    {
        SettingsValidationErrors Validate(T value);
    }
}