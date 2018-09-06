using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Build.Framework;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Sources;

namespace Vostok.Configuration
{
    public class ConfigGeneratorTask : ITask
    {
        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }

        [Microsoft.Build.Framework.Required]
        public string AssemblyPath { get; set; }

        [Microsoft.Build.Framework.Required]
        public string ConfigType { get; set; } = "json";

        public bool Execute()
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
            var neededAttributes = new[] {typeof(ValidateByAttribute), typeof(RequiredByDefaultAttribute)};

            foreach (var type in assembly.GetTypes().Where(t => t.IsPublic && t.CustomAttributes.Any(a => neededAttributes.Contains(a.AttributeType))))
            {
                var instance = Activator.CreateInstance(type);
                new ConfigurationProvider().Validate(instance, type);

                var configFile = type.Name;
                Console.Out.WriteLine(configFile);

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