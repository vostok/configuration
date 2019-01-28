using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Tests.Helpers
{
    [TestFixture]
    internal class ExceptionEqualityComparer_Tests
    {
        private IEqualityComparer<Exception> comparer;

        [SetUp]
        public void SetUp()
        {
            comparer = new ExceptionEqualityComparer();
        }

        [Test]
        public void Should_treat_nulls_as_equal()
        {
            comparer.Equals(null, null).Should().BeTrue();
        }

        [Test, TestCaseSource(nameof(EqualTestCases)), TestCaseSource(nameof(NotEqualTestCases))]
        public bool Should_compare_correctly(Exception e1, Exception e2)
        {
            return comparer.Equals(e1, e2);
        }

        [Test, TestCaseSource(nameof(EqualTestCases))]
        public bool Should_return_same_hashCodes(Exception e1, Exception e2)
        {
            return comparer.GetHashCode(e1) == comparer.GetHashCode(e2);
        }

        private static IEnumerable<TestCaseData> EqualTestCases
        {
            get
            {
                var exception = new Exception();
                yield return new TestCaseData(exception, exception)
                    .Returns(true);

                yield return new TestCaseData(new Exception(), new Exception())
                    .Returns(true);

                yield return new TestCaseData(
                        WithStackTrace(new Exception("Error")),
                        WithDeepStackTrace(new Exception("Error")))
                    .Returns(true);

                yield return new TestCaseData(
                        WithStackTrace(new Exception("Outer", WithStackTrace(new IOException("Inner")))),
                        WithDeepStackTrace(new Exception("Outer", WithDeepStackTrace(new IOException("Inner")))))
                    .Returns(true);

                yield return new TestCaseData(
                        WithStackTrace(new AggregateException("Outer", WithStackTrace(new FormatException("InnerFormat")), WithStackTrace(new IOException("InnerIO")))),
                        WithDeepStackTrace(new AggregateException("Outer", WithDeepStackTrace(new FormatException("InnerFormat")), WithDeepStackTrace(new IOException("InnerIO")))))
                    .Returns(true);
            }
        }

        private static IEnumerable<TestCaseData> NotEqualTestCases
        {
            get
            {
                yield return new TestCaseData(null, new Exception())
                    .Returns(false);
                
                yield return new TestCaseData(new Exception(), null)
                    .Returns(false);
                
                yield return new TestCaseData(new Exception(), new IOException())
                    .Returns(false);

                yield return new TestCaseData(new Exception("Error"), new Exception("Fail"))
                    .Returns(false);

                yield return new TestCaseData(new Exception("Error"), new IOException("Error"))
                    .Returns(false);

                yield return new TestCaseData(
                        new Exception("Outer", new IOException("A")),
                        new Exception("Outer", new IOException("B")))
                    .Returns(false);

                yield return new TestCaseData(
                        new AggregateException("Outer", new FormatException("InnerFormat"), new IOException("A")),
                        new AggregateException("Outer", new FormatException("InnerFormat"), new IOException("B")))
                    .Returns(false);
                
                yield return new TestCaseData(
                        new AggregateException("Outer", new FormatException("InnerFormat"), new IOException("A")),
                        new AggregateException("Outer", new FormatException("InnerFormat")))
                    .Returns(false);
            }
        }

        private static Exception WithStackTrace(Exception exception)
        {
            return CatchException(() => throw exception);
        }

        private static Exception WithDeepStackTrace(Exception exception)
        {
            return CatchException(() => new Action(() => throw exception)());
        }

        private static Exception CatchException(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                return e;
            }

            return default;
        }
    }
}