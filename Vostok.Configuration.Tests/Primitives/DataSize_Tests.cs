using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Primitives;

namespace Vostok.Configuration.Tests.Primitives
{
    [TestFixture]
    internal class DataSize_Tests
    {
        [Test]
        public void Should_return_FromKilobytes()
        {
            var val = 100;
            DataSize.FromKilobytes(val).Should().Be(new DataSize(val * DataSizeConstants.Kilobyte));
        }

        [Test]
        public void Should_return_FromMegabytes()
        {
            var val = 100;
            DataSize.FromMegabytes(val).Should().Be(new DataSize(val * DataSizeConstants.Megabyte));
        }

        [Test]
        public void Should_return_FromGigabytes()
        {
            var val = 100;
            DataSize.FromGigabytes(val).Should().Be(new DataSize(val * DataSizeConstants.Gigabyte));
        }

        [Test]
        public void Should_return_FromTerabytes()
        {
            var val = 100;
            DataSize.FromTerabytes(val).Should().Be(new DataSize(val * DataSizeConstants.Terabyte));
        }

        [Test]
        public void Should_return_FromPetabytes()
        {
            var val = 100;
            DataSize.FromPetabytes(val).Should().Be(new DataSize(val * DataSizeConstants.Petabyte));
        }

        [Test]
        public void Should_TryParse()
        {
            DataSize.TryParse("10 gb", out var res).Should().BeTrue().And.Be(res == new DataSize(10 * DataSizeConstants.Gigabyte));
        }

        [Test]
        public void Should_Parse()
        {
            DataSize.Parse("10 gb").Should().Be(new DataSize(10 * DataSizeConstants.Gigabyte));
        }

        [Test]
        public void Properties_should_return_correct_values()
        {
            var val = 10;
            var bytes = 100L * val;
            new DataSize(bytes).Bytes.Should().Be(bytes);
            bytes = val * DataSizeConstants.Kilobyte;
            new DataSize(bytes).TotalKilobytes.Should().Be(val);
            bytes = val * DataSizeConstants.Megabyte;
            new DataSize(bytes).TotalMegabytes.Should().Be(val);
            bytes = val * DataSizeConstants.Gigabyte;
            new DataSize(bytes).TotalGigabytes.Should().Be(val);
            bytes = val * DataSizeConstants.Terabyte;
            new DataSize(bytes).TotalTerabytes.Should().Be(val);
            bytes = val * DataSizeConstants.Petabyte;
            new DataSize(bytes).TotalPetabytes.Should().Be(val);
        }

        [TestCase(true, 10 * DataSizeConstants.Petabyte, "10 PB")]
        [TestCase(false, 10 * DataSizeConstants.Petabyte, "10 petabytes")]
        [TestCase(true, 10 * DataSizeConstants.Terabyte, "10 TB")]
        [TestCase(false, 10 * DataSizeConstants.Terabyte, "10 terabytes")]
        [TestCase(true, 10 * DataSizeConstants.Gigabyte, "10 GB")]
        [TestCase(false, 10 * DataSizeConstants.Gigabyte, "10 gigabytes")]
        [TestCase(true, 10 * DataSizeConstants.Megabyte, "10 MB")]
        [TestCase(false, 10 * DataSizeConstants.Megabyte, "10 megabytes")]
        [TestCase(true, 10 * DataSizeConstants.Kilobyte, "10 KB")]
        [TestCase(false, 10 * DataSizeConstants.Kilobyte, "10 kilobytes")]
        [TestCase(true, 10, "10 B")]
        [TestCase(false, 10, "10 bytes")]
        public void ToString_should_return_correct_string(bool format, long bytes, string output)
        {
            new DataSize(bytes).ToString(format).Should().Be(output);
        }

        [Test]
        public void Operator_long_should_return_bytes()
        {
            var bytes = 1000L;
            ((long)new DataSize(bytes)).Should().Be(bytes);
        }

        [Test]
        public void Operator_add_should_work_correctly()
        {
            (new DataSize(10) + new DataSize(20)).Should().Be(new DataSize(30));
        }

        [Test]
        public void Operator_sub_should_work_correctly()
        {
            (new DataSize(30) - new DataSize(10)).Should().Be(new DataSize(20));
        }

        [Test]
        public void Operator_mul_should_work_correctly()
        {
            var val1 = new DataSize(30);
            int val2I = 2;
            long val2L = 2;
            double val2D = 2;
            var res = 60;
            (val1 * val2I).Should().Be(new DataSize(res));
            (val2I * val1).Should().Be(new DataSize(res));
            (val1 * val2L).Should().Be(new DataSize(res));
            (val2L * val1).Should().Be(new DataSize(res));
            (val1 * val2D).Should().Be(new DataSize(res));
            (val2D * val1).Should().Be(new DataSize(res));
        }

        [Test]
        public void Operator_div_should_work_correctly()
        {
            var val1 = new DataSize(30);
            int val2I = 2;
            long val2L = 2;
            double val2D = 2;
            var val2TS = TimeSpan.FromSeconds(2);
            var val2DR = DataRate.FromBytesPerSecond(2);
            var res = 15;
            (val1 / val2I).Should().Be(new DataSize(res));
            (val1 / val2L).Should().Be(new DataSize(res));
            (val1 / val2D).Should().Be(new DataSize(res));
            (val1 / val2TS).Should().Be(new DataRate(res));
            (val1 / val2DR).Should().Be(new TimeSpan(0, 0, 0, res));
        }

        [Test]
        public void Operator_minus_should_work_correctly()
        {
            (-new DataSize(30)).Should().Be(new DataSize(-30));
        }

        [TestCase(10, 10, true)]
        [TestCase(10, 20, false)]
        [TestCase(20, 10, false)]
        public void Operator_equals_should_work_correctly(int val1, int val2, bool res)
        {
            (new DataSize(val1) == new DataSize(val2)).Should().Be(res);
        }

        [TestCase(10, 10, false)]
        [TestCase(10, 20, true)]
        [TestCase(20, 10, true)]
        public void Operator_not_equals_should_work_correctly(int val1, int val2, bool res)
        {
            (new DataSize(val1) != new DataSize(val2)).Should().Be(res);
        }

        [TestCase(10, 10, false)]
        [TestCase(10, 20, false)]
        [TestCase(10, 0, true)]
        public void Operator_greater_should_work_correctly(int val1, int val2, bool res)
        {
            (new DataSize(val1) > new DataSize(val2)).Should().Be(res);
        }

        [TestCase(10, 10, true)]
        [TestCase(10, 20, false)]
        [TestCase(10, 0, true)]
        public void Operator_GreaterOrEquals_should_work_correctly(int val1, int val2, bool res)
        {
            (new DataSize(val1) >= new DataSize(val2)).Should().Be(res);
        }

        [TestCase(10, 10, false)]
        [TestCase(10, 20, true)]
        [TestCase(10, 0, false)]
        public void Operator_less_should_work_correctly(int val1, int val2, bool res)
        {
            (new DataSize(val1) < new DataSize(val2)).Should().Be(res);
        }

        [TestCase(10, 10, true)]
        [TestCase(10, 20, true)]
        [TestCase(10, 00, false)]
        public void Operator_LessOrEquals_should_work_correctly(int val1, int val2, bool res)
        {
            (new DataSize(val1) <= new DataSize(val2)).Should().Be(res);
        }

        [TestCase(10, 10, 0)]
        [TestCase(10, 0, 1)]
        [TestCase(0, 10, -1)]
        public void CompareTo_should_work_correctly(int val1, int val2, int res)
        {
            new DataSize(val1).CompareTo(new DataSize(val2)).Should().Be(res);
        }

        [TestCase(10, 10, true)]
        [TestCase(10, 20, false)]
        public void Equals_should_work_correctly(int val1, int val2, bool res)
        {
            new DataSize(val1).Equals(new DataSize(val2)).Should().Be(res);
        }

        [Test]
        public void Equals_objects_should_work_correctly()
        {
            var val1 = new DataSize(10);
            var val2 = (object)new DataSize(10);
            var val3 = (object)new DataSize(20);
            var val4 = (object)val1;
            val1.Equals(val2).Should().BeTrue();
            val1.Equals(val3).Should().BeFalse();
            val1.Equals(val4).Should().BeTrue();
        }

        [Test]
        public void Should_return_HashCode_by_bytes()
        {
            new DataSize(10).GetHashCode().Should().Be(10.GetHashCode());
        }
    }
}
