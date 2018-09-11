using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Vostok.Configuration.Abstractions.Attributes;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration
{
    public class ConfigGeneratorTask : Task
    {
        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }

        [Microsoft.Build.Framework.Required]
        public string AssemblyPath { get; set; }

        [Microsoft.Build.Framework.Required]
        public string ConfigType { get; set; } = "json";

        public override bool Execute()
        {
            Assembly assembly;
            try
            {
                assembly = Assembly.LoadFrom(AssemblyPath);
            }
            catch (ReflectionTypeLoadException e)
            {
                foreach (var loaderException in e.LoaderExceptions)
                    Console.Out.WriteLine(loaderException);
                throw;
            }

            HandleAssembly(assembly);
            return true;
        }

        private void HandleAssembly(Assembly assembly)
        {
            var neededAttributes = new[] {typeof(ValidateByAttribute).FullName, typeof(RequiredByDefaultAttribute).FullName};

            foreach (var type in assembly.GetTypes().Where(t => t.IsPublic && t.CustomAttributes.Any(a => neededAttributes.Contains(a.AttributeType.FullName))))
            {
                var instance = Activator.CreateInstance(type);
                var configFile = type.Name;
                var configType = ConfigType.ToLower();
                var exampleFileName = configFile + ".example." + configType;
                var path = Path.Combine(Path.GetDirectoryName(AssemblyPath) ?? "", "settings");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                path = Path.Combine(path, exampleFileName);

                string str;
                switch (configType)
                {
                    case "json":
                        str = JsonSerializer.Serialize(instance, SerializeOption.Readable);
                        break;
                    case "ini":
                        str = IniSerializer.Serialize(instance);
                        break;
                    default:
                        throw new ArgumentException("Wrong type. Set 'json' or 'ini'.");
                }
                File.WriteAllText(path, str, Encoding.UTF8);
            }
        }
    }
}