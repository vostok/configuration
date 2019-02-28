using System;
using JetBrains.Annotations;
using Vostok.Configuration.Parsers;

namespace Vostok.Configuration.Primitives
{
    /// <summary>
    /// Represents a rate of data transfer.
    /// </summary>
    [PublicAPI]
    [Serializable]
    public struct DataRate : IEquatable<DataRate>, IComparable<DataRate>
    {
        /// <summary>
        /// Creates a new instance of <see cref="DataRate"/> class.
        /// </summary>
        public DataRate(long bytesPerSecond) => BytesPerSecond = bytesPerSecond;

        /// <summary>
        /// Creates a new <see cref="DataRate"/> from given number of bytes per second.
        /// </summary>
        public static DataRate FromBytesPerSecond(long bytes) =>
            new DataRate(bytes);

        /// <summary>
        /// Creates a new <see cref="DataRate"/> from given number of kilobytes per second.
        /// </summary>
        public static DataRate FromKilobytesPerSecond(double kilobytes) =>
            new DataRate((long)(kilobytes * DataSizeConstants.Kilobyte));

        /// <summary>
        /// Creates a new <see cref="DataRate"/> from given number of megabytes per second.
        /// </summary>
        public static DataRate FromMegabytesPerSecond(double megabytes) =>
            new DataRate((long)(megabytes * DataSizeConstants.Megabyte));

        /// <summary>
        /// Creates a new <see cref="DataRate"/> from given number of gigabytes per second.
        /// </summary>
        public static DataRate FromGigabytesPerSecond(double gigabytes) =>
            new DataRate((long)(gigabytes * DataSizeConstants.Gigabyte));

        /// <summary>
        /// Creates a new <see cref="DataRate"/> from given number of terabytes per second.
        /// </summary>
        public static DataRate FromTerabytesPerSecond(double terabytes) =>
            new DataRate((long)(terabytes * DataSizeConstants.Terabyte));

        /// <summary>
        /// Creates a new <see cref="DataRate"/> from given number of petabytes per second.
        /// </summary>
        public static DataRate FromPetabytesPerSecond(double petabytes) =>
            new DataRate((long)(petabytes * DataSizeConstants.Petabyte));

        /// <summary>
        /// Attempts to parse <see cref="DataRate"/> from a string.
        /// </summary>
        public static bool TryParse(string input, out DataRate result) =>
            DataRateParser.TryParse(input, out result);

        /// <summary>
        /// <para>Attempts to parse <see cref="DataRate"/> from a string.</para>
        /// <para>In case of failure a <see cref="FormatException"/> is thrown.</para>
        /// </summary>
        public static DataRate Parse(string input) =>
            DataRateParser.Parse(input);

        /// <summary>
        /// Returns the total number of bytes per second in current <see cref="DataRate"/>.
        /// </summary>
        public long BytesPerSecond { get; }

        /// <summary>
        /// Returns the total number of kilobytes per second in current <see cref="DataRate"/>.
        /// </summary>
        public double KilobytesPerSecond => BytesPerSecond / (double)DataSizeConstants.Kilobyte;

        /// <summary>
        /// Returns the total number of megabytes per second in current <see cref="DataRate"/>.
        /// </summary>
        public double MegabytesPerSecond => BytesPerSecond / (double)DataSizeConstants.Megabyte;

        /// <summary>
        /// Returns the total number of gigabytes per second in current <see cref="DataRate"/>.
        /// </summary>
        public double GigabytesPerSecond => BytesPerSecond / (double)DataSizeConstants.Gigabyte;

        /// <summary>
        /// Returns the total number of terabytes per second in current <see cref="DataRate"/>.
        /// </summary>
        public double TerabytesPerSecond => BytesPerSecond / (double)DataSizeConstants.Terabyte;

        /// <summary>
        /// Returns the total number of petabytes per second in current <see cref="DataRate"/>.
        /// </summary>
        public double PetabytesPerSecond => BytesPerSecond / (double)DataSizeConstants.Petabyte;

        /// <inheritdoc cref="ToString()"/>
        public string ToString(bool shortFormat)
        {
            if (PetabytesPerSecond >= 1) return $"{PetabytesPerSecond:0.####} {(shortFormat ? "PB/sec" : "petabytes/second")}";
            if (TerabytesPerSecond >= 1) return $"{TerabytesPerSecond:0.####} {(shortFormat ? "TB/sec" : "terabytes/second")}";
            if (GigabytesPerSecond >= 1) return $"{GigabytesPerSecond:0.####} {(shortFormat ? "GB/sec" : "gigabytes/second")}";
            if (MegabytesPerSecond >= 1) return $"{MegabytesPerSecond:0.####} {(shortFormat ? "MB/sec" : "megabytes/second")}";
            if (KilobytesPerSecond >= 1) return $"{KilobytesPerSecond:0.####} {(shortFormat ? "KB/sec" : "kilobytes/second")}";

            return $"{BytesPerSecond} {(shortFormat ? "B/sec" : "bytes/second")}";
        }

        /// <summary>
        /// Returns a string representation of current <see cref="DataRate"/>.
        /// </summary>
        public override string ToString() => ToString(true);

        #region Operators

        public static DataRate operator+(DataRate speed1, DataRate speed2) =>
            new DataRate(speed1.BytesPerSecond + speed2.BytesPerSecond);

        public static DataRate operator-(DataRate speed1, DataRate speed2) =>
            new DataRate(speed1.BytesPerSecond - speed2.BytesPerSecond);

        public static DataRate operator*(DataRate speed, int multiplier) =>
            new DataRate(speed.BytesPerSecond * multiplier);

        public static DataRate operator*(DataRate speed, long multiplier) =>
            new DataRate(speed.BytesPerSecond * multiplier);

        public static DataRate operator*(DataRate speed, double multiplier) =>
            new DataRate((long)(speed.BytesPerSecond * multiplier));

        public static DataSize operator*(DataRate speed, TimeSpan time) =>
            new DataSize((long)(speed.BytesPerSecond * time.TotalSeconds));

        public static DataSize operator*(TimeSpan time, DataRate speed) =>
            new DataSize((long)(speed.BytesPerSecond * time.TotalSeconds));

        public static DataRate operator/(DataRate speed, int divider) =>
            new DataRate(speed.BytesPerSecond / divider);

        public static DataRate operator/(DataRate speed, long divider) =>
            new DataRate(speed.BytesPerSecond / divider);

        public static DataRate operator/(DataRate speed, double divider) =>
            new DataRate((long)(speed.BytesPerSecond / divider));

        #endregion

        #region Equality

        /// <inheritdoc />
        public bool Equals(DataRate other) =>
            BytesPerSecond == other.BytesPerSecond;

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is DataRate speed && Equals(speed);
        }

        /// <inheritdoc />
        public override int GetHashCode() =>
            BytesPerSecond.GetHashCode();

        public static bool operator==(DataRate left, DataRate right) =>
            left.Equals(right);

        public static bool operator!=(DataRate left, DataRate right) =>
            !left.Equals(right);

        /// <inheritdoc />
        public int CompareTo(DataRate other) =>
            BytesPerSecond.CompareTo(other.BytesPerSecond);

        #endregion
    }
}