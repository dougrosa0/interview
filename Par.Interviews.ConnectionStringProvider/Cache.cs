using System;

namespace Par.Interviews.ConnectionStringProvider
{
    public class Cache<T>
    {
        private readonly Func<T> _valueFactory;
        private readonly int _timeToLiveInSeconds;
        private DateTime? _expiry = null;
        private Lazy<T> _lazy;

        public T Value
        {
            get
            {
                if (_lazy == null || _expiry.HasValue && _expiry < DateTime.Now)
                {
                    _lazy = new Lazy<T>(_valueFactory);
                    _expiry = DateTime.Now.AddSeconds(_timeToLiveInSeconds);
                }
                return _lazy.Value;
            }
        }

        public Cache(Func<T> valueFactory, int timeToLiveInSeconds)
        {
            _valueFactory = valueFactory;
            _timeToLiveInSeconds = timeToLiveInSeconds;
        }
    }
}