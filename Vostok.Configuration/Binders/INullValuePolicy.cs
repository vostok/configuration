using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.Binders
{
    internal interface INullValuePolicy
    {
        bool IsNullValue([CanBeNull] ISettingsNode node);
    }
}