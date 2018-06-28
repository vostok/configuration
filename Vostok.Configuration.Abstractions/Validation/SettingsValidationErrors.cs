using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vostok.Configuration.Abstractions.Validation
{
    internal class SettingsValidationErrors : ISettingsValidationErrors
    {
        private readonly List<string> errors = new List<string>();
        public string Prefix { get; set; }

        public bool HasErrors => errors.Any();

        public void ReportError(string error) => errors.Add(error);

        public void MergeWith(SettingsValidationErrors other)
        {
            if (other == null) return;
            errors.AddRange(other.errors.Select(e => other.Prefix + e));
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var error in errors)
                builder.AppendLine(error);
            return builder.ToString();
        }

        public Exception ToException() => new SettingsValidationException(ToString());
    }
}