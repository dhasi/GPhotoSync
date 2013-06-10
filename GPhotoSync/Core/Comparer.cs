using System;
using System.Collections.Generic;

namespace GPhotoSync
{
    public class EqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _comparer;

        public EqualityComparer(Func<T, T, bool> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            _comparer = comparer;
        }

        public bool Equals(T x, T y)
        {
            return _comparer(x, y);
        }

        public int GetHashCode(T obj)
        {
            return obj.ToString().ToLower().GetHashCode();
        }
    }

    public class PhotoComparer : IEqualityComparer<Photo>
    {
        public bool Equals(Photo x, Photo y)
        {
            return string.CompareOrdinal(x.Title, y.Title) == 0;
        }

        public int GetHashCode(Photo obj)
        {
            return obj.Title.ToLowerInvariant().GetHashCode();
        }
    }

}
