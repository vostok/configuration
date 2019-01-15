using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Configuration.Helpers;

namespace Vostok.Configuration.Tests.EqualityComparers
{
    [TestFixture]
    internal class TupleEqualityComparer_Tests
    {
        private TupleEqualityComparer<object, object> tupleComparer;
        private IEqualityComparer<object>[] comparers;
        private (object, object)[] tuples;

        [SetUp]
        public void SetUp()
        {
            comparers = Enumerable.Range(0, 2).Select(_ => Substitute.For<IEqualityComparer<object>>()).ToArray();
            tupleComparer = new TupleEqualityComparer<object, object>(comparers[0], comparers[1]);

            tuples = Enumerable.Range(0, 2).Select(_ => (new object(), new object())).ToArray();
        }
        
        [Test]
        public void Equals_should_be_true_when_both_components_are_equal()
        {
            comparers[0].Equals(tuples[0].Item1, tuples[1].Item1).Returns(true);
            comparers[1].Equals(tuples[0].Item2, tuples[1].Item2).Returns(true);

            tupleComparer.Equals(tuples[0], tuples[1]).Should().BeTrue();
        }
        
        [Test]
        public void Equals_should_be_false_when_first_components_are_not_equal()
        {
            comparers[1].Equals(tuples[0].Item2, tuples[1].Item2).Returns(true);

            tupleComparer.Equals(tuples[0], tuples[1]).Should().BeFalse();
        }
        
        [Test]
        public void Equals_should_be_false_when_second_components_are_not_equal()
        {
            comparers[0].Equals(tuples[0].Item1, tuples[1].Item1).Returns(true);

            tupleComparer.Equals(tuples[0], tuples[1]).Should().BeFalse();
        }

        [Test]
        public void GetHashCode_should_return_same_hashcode_for_equal_tuples()
        {
            comparers[0].GetHashCode(tuples[0].Item1).Returns(1);
            comparers[0].GetHashCode(tuples[1].Item1).Returns(1);
            comparers[1].GetHashCode(tuples[0].Item2).Returns(3);
            comparers[1].GetHashCode(tuples[1].Item2).Returns(3);

            tupleComparer.GetHashCode(tuples[0]).Should().Be(tupleComparer.GetHashCode(tuples[1]));
        }
    }
}