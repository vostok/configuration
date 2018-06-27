namespace Vostok.Configuration.Abstractions.Validation
{
    public interface ISettingsValidator<in T> where T : class
    {
        ISettingsValidationErrors Validate(T value);
    }
}