using Vostok.Configuration.Binders.Results;

namespace Vostok.Configuration.Binders
{
    internal class SettingsBindingResultWrapper<TInterface, TImplementation> : SettingsBindingResult<TInterface>
        where TImplementation : TInterface
    {
        public SettingsBindingResultWrapper(SettingsBindingResult<TImplementation> implResult)
            : base(implResult.Errors.Count == 0 ? implResult.Value : default, implResult.Errors)
        {
        }
    }
}