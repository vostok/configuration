using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Primitives;

namespace Vostok.Configuration.Tests.Primitives
{
    [TestFixture]
    internal class DataRate_Tests
    {
        [Test]
        public void Should_return_FromBytesPerSecond()
        {
            var val = 100L;
            DataRate.FromBytesPerSecond(val).Should().Be(new DataRate(val));
        }

        [Test]
        public void Should_return_FromKilobytesPerSecond()
        {
            var val = 100;
            DataRate.FromKilobytesPerSecond(val).Should().Be(new DataRate(val * DataSizeConstants.Kilobyte));
        }

        [Test]
        public void Should_return_FromMegabytesPerSecond()
        {
            var val = 100;
            DataRate.FromMegabytesPerSecond(val).Should().Be(new DataRate(val * DataSizeConstants.Megabyte));
        }

        [Test]
        public void Should_return_FromGigabytesPerSecond()
        {
            var val = 100;
            DataRate.FromGigabytesPerSecond(val).Should().Be(new DataRate(val * DataSizeConstants.Gigabyte));
        }

        [Test]
        public void Should_return_FromTerabytesPerSecond()
        {
            var val = 100;
            DataRate.FromTerabytesPerSecond(val).Should().Be(new DataRate(val * DataSizeConstants.Terabyte));
        }

        [Test]
        public void Should_return_FromPetabytesPerSecond()
        {
            var val = 100;
            DataRate.FromPetabytesPerSecond(val).Should().Be(new DataRate(val * DataSizeConstants.Petabyte));
        }

        [Test]
        public void Should_TryParse()
        {
            DataRate.TryParse("10 /sec", out var res).Should().BeTrue().And.Be(res == new DataRate(10));
        }

        [Test]
        public void Should_Parse()
        {
            DataRate.Parse("10 /sec").Should().Be(new DataRate(10));
        }

        [Test]
        public void Properties_should_return_correct_values()
        {
            var val = 10;
            var bytes = 100L * val;
            new DataRate(bytes).BytesPerSecond.Should().Be(bytes);
            bytes = val * DataSizeConstants.Kilobyte;
            new DataRate(bytes).KilobytesPerSecond.Should().Be(val);
            bytes = val * DataSizeConstants.Megabyte;
            new DataRate(bytes).MegabytesPerSecond.Should().Be(val);
            bytes = val * DataSizeConstants.Gigabyte;
            new DataRate(bytes).GigabytesPerSecond.Should().Be(val);
            bytes = val * DataSizeConstants.Terabyte;
            new DataRate(bytes).TerabytesPerSecond.Should().Be(val);
            bytes = val * DataSizeConstants.Petabyte;
            new DataRate(bytes).PetabytesPerSecond.Should().Be(val);
        }

        [TestCase(true, 10 * DataSizeConstants.Petabyte, "10 PB/sec")]
        [TestCase(false, 10 * DataSizeConstants.Petabyte, "10 petabytes/second")]
        [TestCase(true, 10 * DataSizeConstants.Terabyte, "10 TB/sec")]
        [TestCase(false, 10 * DataSizeConstants.Terabyte, "10 terabytes/second")]
        [TestCase(true, 10 * DataSizeConstants.Gigabyte, "10 GB/sec")]
        [TestCase(false, 10 * DataSizeConstants.Gigabyte, "10 gigabytes/second")]
        [TestCase(true, 10 * DataSizeConstants.Megabyte, "10 MB/sec")]
        [TestCase(false, 10 * DataSizeConstants.Megabyte, "10 megabytes/second")]
        [TestCase(true, 10 * DataSizeConstants.Kilobyte, "10 KB/sec")]
        [TestCase(false, 10 * DataSizeConstants.Kilobyte, "10 kilobytes/second")]
        [TestCase(true, 10, "10 B/sec")]
        [TestCase(false, 10, "10 bytes/second")]
        public void ToString_should_return_correct_string(bool format, long bytes, string output)
        {
            new DataRate(bytes).ToString(format).Should().Be(output);
        }

        [Test]
        public void Operator_add_should_work_correctly()
        {
            (new DataRate(10) + new DataRate(20)).Should().Be(new DataRate(30));
        }

        [Test]
        public void Operator_sub_should_work_correctly()
        {
            (new DataRate(30) - new DataRate(10)).Should().Be(new DataRate(20));
        }

        [Test]
        public void Operator_mul_should_work_correctly()
        {
            var val1 = new DataRate(30);
            int val2I = 2;
            long val2L = 2;
            double val2D = 2;
            var val2TS = TimeSpan.FromSeconds(2);
            var res = 60;
            (val1 * val2I).Should().Be(new DataRate(res));
            (val1 * val2L).Should().Be(new DataRate(res));
            (val1 * val2D).Should().Be(new DataRate(res));
            (val1 * val2TS).Should().Be(new DataSize(res));
            (val2TS * val1).Should().Be(new DataSize(res));
        }

        [Test]
        public void Operator_div_should_work_correctly()
        {
            var val1 = new DataRate(30);
            int val2I = 2;
            long val2L = 2;
            double val2D = 2;
            var res = 15;
            (val1 / val2I).Should().Be(new DataRate(res));
            (val1 / val2L).Should().Be(new DataRate(res));
            (val1 / val2D).Should().Be(new DataRate(res));
        }

        [TestCase(10, 10, true)]
        [TestCase(10, 20, false)]
        [TestCase(20, 10, false)]
        public void Operator_equals_should_work_correctly(int val1, int val2, bool res)
        {
            (new DataRate(val1) == new DataRate(val2)).Should().Be(res);
        }

        [TestCase(10, 10, false)]
        [TestCase(10, 20, true)]
        [TestCase(20, 10, true)]
        public void Operator_not_equals_should_work_correctly(int val1, int val2, bool res)
        {
            (new DataRate(val1) != new DataRate(val2)).Should().Be(res);
        }

        [TestCase(10, 10, 0)]
        [TestCase(10, 0, 1)]
        [TestCase(0, 10, -1)]
        public void CompareTo_should_work_correctly(int val1, int val2, int res)
        {
            new DataRate(val1).CompareTo(new DataRate(val2)).Should().Be(res);
        }

        [TestCase(10, 10, true)]
        [TestCase(10, 20, false)]
        public void Equals_should_work_correctly(int val1, int val2, bool res)
        {
            new DataRate(val1).Equals(new DataRate(val2)).Should().Be(res);
        }

        [Test]
        public void Equals_objects_should_work_correctly()
        {
            var val1 = new DataRate(10);
            var val2 = (object)new DataRate(10);
            var val3 = (object)new DataRate(20);
            var val4 = (object)val1;
            val1.Equals(val2).Should().BeTrue();
            val1.Equals(val3).Should().BeFalse();
            val1.Equals(val4).Should().BeTrue();
        }

        [Test]
        public void Should_return_HashCode_by_bytes_per_second()
        {
            new DataRate(10).GetHashCode().Should().Be(10.GetHashCode());
        }
    }
}