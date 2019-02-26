using System;
using JetBrains.Annotations;
using Vostok.Configuration.Parsers;

namespace Vostok.Configuration.Primitives
{
    /// <summary>
    /// Represents an amount of data.
    /// </summary>
    [PublicAPI]
    [Serializable]
    public struct DataSize : IEquatable<DataSize>, IComparable<DataSize>
    {
        /// <summary>
        /// Creates a new instance of <see cref="DataSize"/> class.
        /// </summary>
        public DataSize(long bytes) => Bytes = bytes;

        /// <summary>
        /// Creates a new <see cref="DataSize"/> from given number of bytes.
        /// </summary>
        public static DataSize FromBytes(long bytes) =>
            new DataSize(bytes);

        /// <summary>
        /// Creates a new <see cref="DataSize"/> from given number of kilobytes.
        /// </summary>
        public static DataSize FromKilobytes(double kilobytes) =>
            new DataSize((long)(kilobytes * DataSizeConstants.Kilobyte));

        /// <summary>
        /// Creates a new <see cref="DataSize"/> from given number of megabytes.
        /// </summary>
        public static DataSize FromMegabytes(double megabytes) =>
            new DataSize((long)(megabytes * DataSizeConstants.Megabyte));

        /// <summary>
        /// Creates a new <see cref="DataSize"/> from given number of gigabytes.
        /// </summary>
        public static DataSize FromGigabytes(double gigabytes) =>
            new DataSize((long)(gigabytes * DataSizeConstants.Gigabyte));

        /// <summary>
        /// Creates a new <see cref="DataSize"/> from given number of terabytes.
        /// </summary>
        public static DataSize FromTerabytes(double terabytes) =>
            new DataSize((long)(terabytes * DataSizeConstants.Terabyte));

        /// <summary>
        /// Creates a new <see cref="DataSize"/> from given number of petabytes.
        /// </summary>
        public static DataSize FromPetabytes(double petabytes) =>
            new DataSize((long)(petabytes * DataSizeConstants.Petabyte));

        /// <summary>
        /// Attempts to parse <see cref="DataSize"/> from a string.
        /// </summary>
        public static bool TryParse(string input, out DataSize result) =>
            DataSizeParser.TryParse(input, out result);

        /// <summary>
        /// <para>Attempts to parse <see cref="DataSize"/> from a string.</para>
        /// <para>In case of failure a <see cref="FormatException"/> is thrown.</para>
        /// </summary>
        public static DataSize Parse(string input) =>
            DataSizeParser.Parse(input);

        /// <summary>
        /// Returns the total number of bytes in current <see cref="DataSize"/>.
        /// </summary>
        public long Bytes { get; }

        /// <summary>
        /// Returns the total number of kilobytes in current <see cref="DataSize"/>.
        /// </summary>
        public double TotalKilobytes => Bytes / (double)DataSizeConstants.Kilobyte;

        /// <summary>
        /// Returns the total number of megabytes in current <see cref="DataSize"/>.
        /// </summary>
        public double TotalMegabytes => Bytes / (double)DataSizeConstants.Megabyte;

        /// <summary>
        /// Returns the total number of gigabytes in current <see cref="DataSize"/>.
        /// </summary>
        public double TotalGigabytes => Bytes / (double)DataSizeConstants.Gigabyte;

        /// <summary>
        /// Returns the total number of terabytes in current <see cref="DataSize"/>.
        /// </summary>
        public double TotalTerabytes => Bytes / (double)DataSizeConstants.Terabyte;

        /// <summary>
        /// Returns the total number of petabytes in current <see cref="DataSize"/>.
        /// </summary>
        public double TotalPetabytes => Bytes / (double)DataSizeConstants.Petabyte;

        /// <inheritdoc cref="ToString()" />
        public string ToString(bool shortFormat)
        {
            if (Math.Abs(TotalPetabytes) >= 1) return $"{TotalPetabytes:0.##} {(shortFormat ? "PB" : "petabytes")}";
            if (Math.Abs(TotalTerabytes) >= 1) return $"{TotalTerabytes:0.##} {(shortFormat ? "TB" : "terabytes")}";
            if (Math.Abs(TotalGigabytes) >= 1) return $"{TotalGigabytes:0.##} {(shortFormat ? "GB" : "gigabytes")}";
            if (Math.Abs(TotalMegabytes) >= 1) return $"{TotalMegabytes:0.##} {(shortFormat ? "MB" : "megabytes")}";
            if (Math.Abs(TotalKilobytes) >= 1) return $"{TotalKilobytes:0.##} {(shortFormat ? "KB" : "kilobytes")}";

            return $"{Bytes} {(shortFormat ? "B" : "bytes")}";
        }

        /// <summary>
        /// Returns a string representation of current <see cref="DataSize"/>.
        /// </summary>
        public override string ToString() => ToString(true);

        #region Operators

        public static explicit operator long(DataSize size) => size.Bytes;

        public static DataSize operator+(DataSize size1, DataSize size2) =>
            new DataSize(size1.Bytes + size2.Bytes);

        public static DataSize operator-(DataSize size1, DataSize size2) =>
            new DataSize(size1.Bytes - size2.Bytes);

        public static DataSize operator*(DataSize size, int multiplier) =>
            new DataSize(size.Bytes * multiplier);

        public static DataSize operator*(int multiplier, DataSize size) =>
            size * multiplier;

        public static DataSize operator*(DataSize size, long multiplier) =>
            new DataSize(size.Bytes * multiplier);

        public static DataSize operator*(long multiplier, DataSize size) =>
            size * multiplier;

        public static DataSize operator*(DataSize size, double multiplier) =>
            new DataSize((long)(size.Bytes * multiplier));

        public static DataSize operator*(double multiplier, DataSize size) =>
            size * multiplier;

        public static DataSize operator/(DataSize size, int divider) =>
            new DataSize(size.Bytes / divider);

        public static DataSize operator/(DataSize size, long divider) =>
            new DataSize(size.Bytes / divider);

        public static DataSize operator/(DataSize size, double divider) =>
            new DataSize((long)(size.Bytes / divider));

        public static DataSize operator-(DataSize size) =>
            new DataSize(-size.Bytes);

        public static DataRate operator/(DataSize size, TimeSpan time) =>
            new DataRate((size / time.TotalSeconds).Bytes);

        public static TimeSpan operator/(DataSize size, DataRate speed) =>
            TimeSpan.FromSeconds(size.Bytes / (double)speed.BytesPerSecond);

        #endregion

        #region Equality

        /// <inheritdoc />
        public bool Equals(DataSize other) =>
            Bytes == other.Bytes;

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is DataSize size && Equals(size);
        }

        /// <inheritdoc />
        public override int GetHashCode() =>
            Bytes.GetHashCode();

        public static bool operator==(DataSize left, DataSize right) =>
            left.Equals(right);

        public static bool operator!=(DataSize left, DataSize right) =>
            !left.Equals(right);

        /// <inheritdoc />
        public int CompareTo(DataSize other) =>
            Bytes.CompareTo(other.Bytes);

        public static bool operator>(DataSize size1, DataSize size2) =>
            size1.Bytes > size2.Bytes;

        public static bool operator>=(DataSize size1, DataSize size2) =>
            size1.Bytes >= size2.Bytes;

        public static bool operator<(DataSize size1, DataSize size2) =>
            size1.Bytes < size2.Bytes;

        public static bool operator<=(DataSize size1, DataSize size2) =>
            size1.Bytes <= size2.Bytes;

        #endregion
    }
}
