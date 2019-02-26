using JetBrains.Annotations;

namespace Vostok.Configuration.Primitives
{
    [PublicAPI]
    public static class DataRateConversionExtensions
    {
        /// <summary>
        /// Creates a new <see cref="DataRate"/> from given bytes per second value.
        /// </summary>
        public static DataRate BytesPerSecond(this ushort value) =>
            DataRate.FromBytesPerSecond(value);

        /// <summary>
        /// Creates a new <see cref="DataRate"/> from given bytes per second value.
        /// </summary>
        public static DataRate BytesPerSecond(this int value) =>
            DataRate.FromBytesPerSecond(value);

        /// <summary>
        /// Creates a new <see cref="DataRate"/> from given bytes per second value.
        /// </summary>
        public static DataRate BytesPerSecond(this long value) =>
            DataRate.FromBytesPerSecond(value);

        /// <summary>
        /// Creates a new <see cref="DataRate"/> from given kilobytes per second value.
        /// </summary>
        public static DataRate KilobytesPerSecond(this ushort value) =>
            DataRate.FromKilobytesPerSecond(value);

        /// <summary>
        /// Creates a new <see cref="DataRate"/> from given kilobytes per second value.
        /// </summary>
        public static DataRate KilobytesPerSecond(this int value) =>
            DataRate.FromKilobytesPerSecond(value);

        /// <summary>
        /// Creates a new <see cref="DataRate"/> from given kilobytes per second value.
        /// </summary>
        public static DataRate KilobytesPerSecond(this long value) =>
            DataRate.FromKilobytesPerSecond(value);

        /// <summary>
        /// Creates a new <see cref="DataRate"/> from given kilobytes per second value.
        /// </summary>
        public static DataRate KilobytesPerSecond(this double value) =>
            DataRate.FromKilobytesPerSecond(value);

        /// <summary>
        /// Creates a new <see cref="DataRate"/> from given megabytes per second value.
        /// </summary>
        public static DataRate MegabytesPerSecond(this ushort value) =>
            DataRate.FromMegabytesPerSecond(value);

        /// <summary>
        /// Creates a new <see cref="DataRate"/> from given megabytes per second value.
        /// </summary>
        public static DataRate MegabytesPerSecond(this int value) =>
            DataRate.FromMegabytesPerSecond(value);

        public static DataRate MegabytesPerSecond(this long value) =>
            DataRate.FromMegabytesPerSecond(value);

        /// <summary>
        /// Creates a new <see cref="DataRate"/> from given megabytes per second value.
        /// </summary>
        public static DataRate MegabytesPerSecond(this double value) =>
            DataRate.FromMegabytesPerSecond(value);

        /// <summary>
        /// Creates a new <see cref="DataRate"/> from given gigabytes per second value.
        /// </summary>
        public static DataRate GigabytesPerSecond(this ushort value) =>
            DataRate.FromGigabytesPerSecond(value);

        /// <summary>
        /// Creates a new <see cref="DataRate"/> from given gigabytes per second value.
        /// </summary>
        public static DataRate GigabytesPerSecond(this int value) =>
            DataRate.FromGigabytesPerSecond(value);

        /// <summary>
        /// Creates a new <see cref="DataRate"/> from given gigabytes per second value.
        /// </summary>
        public static DataRate GigabytesPerSecond(this long value) =>
            DataRate.FromGigabytesPerSecond(value);

        /// <summary>
        /// Creates a new <see cref="DataRate"/> from given gigabytes per second value.
        /// </summary>
        public static DataRate GigabytesPerSecond(this double value) =>
            DataRate.FromGigabytesPerSecond(value);

        /// <summary>
        /// Creates a new <see cref="DataRate"/> from given terabytes per second value.
        /// </summary>
        public static DataRate TerabytesPerSecond(this ushort value) =>
            DataRate.FromTerabytesPerSecond(value);

        /// <summary>
        /// Creates a new <see cref="DataRate"/> from given terabytes per second value.
        /// </summary>
        public static DataRate TerabytesPerSecond(this int value) =>
            DataRate.FromTerabytesPerSecond(value);

        /// <summary>
        /// Creates a new <see cref="DataRate"/> from given terabytes per second value.
        /// </summary>
        public static DataRate TerabytesPerSecond(this long value) =>
            DataRate.FromTerabytesPerSecond(value);

        /// <summary>
        /// Creates a new <see cref="DataRate"/> from given terabytes per second value.
        /// </summary>
        public static DataRate TerabytesPerSecond(this double value) =>
            DataRate.FromTerabytesPerSecond(value);

        /// <summary>
        /// Creates a new <see cref="DataRate"/> from given petabytes per second value.
        /// </summary>
        public static DataRate PetabytesPerSecond(this ushort value) =>
            DataRate.FromPetabytesPerSecond(value);

        /// <summary>
        /// Creates a new <see cref="DataRate"/> from given petabytes per second value.
        /// </summary>
        public static DataRate PetabytesPerSecond(this int value) =>
            DataRate.FromPetabytesPerSecond(value);

        /// <summary>
        /// Creates a new <see cref="DataRate"/> from given petabytes per second value.
        /// </summary>
        public static DataRate PetabytesPerSecond(this long value) =>
            DataRate.FromPetabytesPerSecond(value);

        /// <summary>
        /// Creates a new <see cref="DataRate"/> from given petabytes per second value.
        /// </summary>
        public static DataRate PetabytesPerSecond(this double value) =>
            DataRate.FromPetabytesPerSecond(value);
    }
}
