using System;
#if !UNITY_WEBGL || UNITY_EDITOR
using Firebase.Firestore;
#endif

namespace YuankunHuang.Unity.FirebaseCore
{
    /// <summary>
    /// Platform-agnostic timestamp wrapper that works across all Unity platforms
    /// </summary>
    [System.Serializable]
    public struct FirebaseTimestamp : IComparable<FirebaseTimestamp>, IEquatable<FirebaseTimestamp>
    {
        private readonly long _ticks;

        public DateTime DateTime => new DateTime(_ticks);
        public long Ticks => _ticks;

        public FirebaseTimestamp(DateTime dateTime)
        {
            _ticks = dateTime.Ticks;
        }

        public FirebaseTimestamp(long ticks)
        {
            _ticks = ticks;
        }

        public static FirebaseTimestamp Now => new FirebaseTimestamp(DateTime.UtcNow);

#if !UNITY_WEBGL || UNITY_EDITOR
        public FirebaseTimestamp(Timestamp firebaseTimestamp)
        {
            _ticks = firebaseTimestamp.ToDateTime().Ticks;
        }

        public Timestamp ToFirebaseTimestamp()
        {
            return Timestamp.FromDateTime(DateTime);
        }

        public static implicit operator FirebaseTimestamp(Timestamp timestamp)
        {
            return new FirebaseTimestamp(timestamp);
        }

        public static implicit operator Timestamp(FirebaseTimestamp timestamp)
        {
            return timestamp.ToFirebaseTimestamp();
        }
#endif

        public static implicit operator FirebaseTimestamp(DateTime dateTime)
        {
            return new FirebaseTimestamp(dateTime);
        }

        public static implicit operator DateTime(FirebaseTimestamp timestamp)
        {
            return timestamp.DateTime;
        }

        public int CompareTo(FirebaseTimestamp other)
        {
            return _ticks.CompareTo(other._ticks);
        }

        public bool Equals(FirebaseTimestamp other)
        {
            return _ticks == other._ticks;
        }

        public override bool Equals(object obj)
        {
            return obj is FirebaseTimestamp other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _ticks.GetHashCode();
        }

        public override string ToString()
        {
            return DateTime.ToString("yyyy-MM-dd HH:mm:ss UTC");
        }

        public static bool operator ==(FirebaseTimestamp left, FirebaseTimestamp right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FirebaseTimestamp left, FirebaseTimestamp right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(FirebaseTimestamp left, FirebaseTimestamp right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(FirebaseTimestamp left, FirebaseTimestamp right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(FirebaseTimestamp left, FirebaseTimestamp right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(FirebaseTimestamp left, FirebaseTimestamp right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}