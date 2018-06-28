using System;
using System.Linq;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Configuration.SettingsTree
{
    internal static class SettingsNode
    {
        /// <summary>
        /// Checks <see cref="settings"/>. Throws exeption if something is wrong.
        /// </summary>
        /// <param name="settings">Settings you're going to check</param>
        /// <param name="checkValues">Check inner values</param>
        /// <exception cref="ArgumentNullException">If <paramref name="settings"/> is null</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="settings"/> fields values are null and <paramref name="checkValues"/> is true</exception>
        public static void CheckSettings(ISettingsNode settings, bool checkValues = true)
        {
            if (settings == null)
                throw new ArgumentNullException($"{nameof(ISettingsNode)} checker: parameter \"{nameof(settings)}\" is null");
            if (checkValues && settings.Value == null && !settings.Children.Any())
                throw new ArgumentNullException($"{nameof(ISettingsNode)} checker: parameter \"{nameof(settings)}\" is empty");
        }
    }
}