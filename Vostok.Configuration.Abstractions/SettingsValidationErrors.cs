using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vostok.Configuration.Abstractions
{
    public class SettingsValidationErrors
    {
        private readonly List<string> errors = new List<string>();

        public void ReportError(string error)
        {
            errors.Add(error);
        }

        public void MergeWith(SettingsValidationErrors other)
        {
            errors.AddRange(other.errors);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            foreach (var error in errors)
                builder.AppendLine(error);

            return builder.ToString();
        }

        public Exception ToException()
        {
            return new SettingsValidationException(ToString());
        }

        public bool HasErrors => errors.Any();
    }
}