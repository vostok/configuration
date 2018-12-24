using System.Collections.Generic;

namespace Vostok.Configuration.EqualityComparers
{
    internal class TupleEqualityComparer<T1, T2> : IEqualityComparer<(T1, T2)>
    {
        private readonly IEqualityComparer<T1> comparer1;
        private readonly IEqualityComparer<T2> comparer2;

        public TupleEqualityComparer(IEqualityComparer<T1> comparer1, IEqualityComparer<T2> comparer2)
        {
            this.comparer1 = comparer1;
            this.comparer2 = comparer2;
        }
        
        public bool Equals((T1, T2) x, (T1, T2) y) => comparer1.Equals(x.Item1, y.Item1) && comparer2.Equals(x.Item2, y.Item2);

        public int GetHashCode((T1, T2) obj)
        {
            unchecked
            {
                return (comparer1.GetHashCode(obj.Item1) * 397) ^ comparer2.GetHashCode(obj.Item2);
            }
        }
    }
}