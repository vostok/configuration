using System;
using System.Text;
using Vostok.Commons.Conversions;

namespace Vostok.Configuration.Sources
{
    public class FileSourceSettings
    {
        public Encoding Encoding { get; set; } = Encoding.UTF8;
        public TimeSpan FileWatcherPeriod { get; set; } = 5.Seconds();
    }
}