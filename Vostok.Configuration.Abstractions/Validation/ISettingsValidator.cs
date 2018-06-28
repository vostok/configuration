namespace Vostok.Configuration.Abstractions.Validation
{
    public interface ISettingsValidator<in T>
    {
        void Validate(T value, ISettingsValidationErrors errors);
    }
}