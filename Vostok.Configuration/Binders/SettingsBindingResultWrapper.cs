using Vostok.Configuration.Binders.Results;

namespace Vostok.Configuration.Binders
{
    internal class SettingsBindingResultWrapper<TInterface, TImplementation> : SettingsBindingResult<TInterface>
        where TImplementation : TInterface
    {
        public SettingsBindingResultWrapper(SettingsBindingResult<TImplementation> implResult)
            : base(implResult.Value, implResult.Errors)
        {
        }
    }
}