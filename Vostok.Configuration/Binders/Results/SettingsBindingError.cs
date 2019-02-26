namespace Vostok.Configuration.Binders.Results
{
    internal abstract class SettingsBindingError
    {
        public static SettingsBindingError Message(string message) =>
            new ErrorMessage(message);

        public static SettingsBindingError Property(string name, SettingsBindingError inner) =>
            new ErrorProperty(name, inner);

        public static SettingsBindingError Index(string key, SettingsBindingError inner) =>
            new ErrorIndex(key, inner);

        protected abstract string Prefix { get; }

        private class ErrorMessage : SettingsBindingError
        {
            private readonly string message;

            public ErrorMessage(string message) => this.message = message;

            public override string ToString() => message;

            protected override string Prefix => ": ";
        }

        private class ErrorProperty : SettingsBindingError
        {
            private readonly string name;
            private readonly SettingsBindingError inner;

            public ErrorProperty(string name, SettingsBindingError inner)
            {
                this.name = name;
                this.inner = inner;
            }

            public override string ToString() => $"{name}{inner.Prefix}{inner}";

            protected override string Prefix => ".";
        }

        private class ErrorIndex : SettingsBindingError
        {
            private readonly string key;
            private readonly SettingsBindingError inner;

            public ErrorIndex(string key, SettingsBindingError inner)
            {
                this.key = key;
                this.inner = inner;
            }

            public override string ToString() => $"[{key}]{inner.Prefix}{inner}";

            protected override string Prefix => "";
        }
    }
}