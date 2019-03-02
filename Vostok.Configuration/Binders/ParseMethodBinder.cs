using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Binders.Results;
using Vostok.Configuration.Parsers;

namespace Vostok.Configuration.Binders
{
    internal class ParseMethodBinder<T> : ISafeSettingsBinder<T>
    {
        public SettingsBindingResult<T> Bind(ISettingsNode rawSettings)
        {
            var tryParseMethod = ParseMethodFinder.FindTryParseMethod(typeof(T));
            if (tryParseMethod != null)
                return new PrimitiveBinder<T>(new TryParseMethodParser(tryParseMethod)).Bind(rawSettings);

            var parseMethod = ParseMethodFinder.FindParseMethod(typeof(T));
            if (parseMethod != null)
                return new PrimitiveBinder<T>(new ParseMethodParser(parseMethod)).Bind(rawSettings);

            return SettingsBindingResult.Error<T>($"Failed to find a '{nameof(int.TryParse)}' or '{nameof(int.Parse)}' method on '{typeof(T)}' type.");
        }
    }
}