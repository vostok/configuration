using System;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.Attributes;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Binders
{
    /// <summary>
    /// A binder wrapper that treats all models as if they were marked with a <see cref="SecretAttribute"/>.
    /// </summary>
    [PublicAPI]
    public class SecretBinder : ISettingsBinder
    {
        private readonly ISettingsBinder baseBinder;

        public SecretBinder([NotNull] ISettingsBinder baseBinder)
            => this.baseBinder = baseBinder ?? throw new ArgumentNullException(nameof(baseBinder));

        public TSettings Bind<TSettings>(ISettingsNode rawSettings)
        {
            using (SecurityHelper.StartSecurityScope())
                return baseBinder.Bind<TSettings>(rawSettings);
        }
    }
}