using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenShare.Net.Library.Threading
{
    /// <summary>
    /// A simple thread safe in-memory cache mechanism for quick access to commonly
    /// used values. This class supports specifiying custom equality comparers for
    /// determining the key, as well as customized expiration durations. This class
    /// also supports sliding expirations for cache values that are not already
    /// expired as well as non-sliding expirations.
    /// </summary>
    /// <typeparam name="TKey">The key type to use for finding values in cache.</typeparam>
    /// <typeparam name="TValue">The value type to store in cache.</typeparam>
    public class ConcurrentCache<TKey, TValue>
        : IDictionary<TKey, TValue>, IDisposable
    {
        /// <summary>
        /// This is a wrapper class for storing usage and expiration data
        /// on each item in cache.
        /// </summary>
        /// <typeparam name="T">The value type to store in cache.</typeparam>
        private class CacheWrapper<T>
        {
            /// <summary>
            /// Specific constructor.
            /// </summary>
            /// <param name="value">The cache value to wrap.</param>
            public CacheWrapper(T value)
            {
                Value = value;
            }

            /// <summary>
            /// The cache value.
            /// </summary>
            public T Value { get; set; }

            /// <summary>
            /// The number of times this value was accessed in cache.
            /// </summary>
            public long Uses { get; set; }

            /// <summary>
            /// The last time the value was accessed in cache.
            /// </summary>
            public DateTime LastUsedOn { get; set; }

            /// <summary>
            /// The date the value expires.
            /// </summary>
            public DateTime ExpiresOn { get; set; }
        }

        /// <summary>
        /// This class provides equality comparison for the concurrent cache contains method.
        /// </summary>
        private class ConcurrentCacheContainsEqualityComparer
            : EqualityComparer<KeyValuePair<TKey, CacheWrapper<TValue>>>
        {
            private IEqualityComparer<TKey> Comparer { get; }

            /// <summary>
            /// Specific constructor
            /// </summary>
            /// <param name="comparer">Comparer for TKey</param>
            public ConcurrentCacheContainsEqualityComparer(
                IEqualityComparer<TKey> comparer)
            {
                if (comparer == null)
                    comparer = EqualityComparer<TKey>.Default;

                Comparer = comparer;
            }

            /// <summary>
            /// Determines whether the specified objects are equal.
            /// </summary>
            /// <param name="x">The first object to compare.</param>
            /// <param name="y">The second object to compare.</param>
            /// <returns>
            /// true if the specified objects are equal; otherwise, false.
            /// </returns>
            public override bool Equals(
                KeyValuePair<TKey, CacheWrapper<TValue>> x,
                KeyValuePair<TKey, CacheWrapper<TValue>> y)
            {
                return Comparer.Equals(x.Key, y.Key)
                    && x.Value.Value.Equals(y.Value.Value);
            }

            /// <summary>
            /// Returns a hash code for the specified object.
            /// </summary>
            /// <param name="obj">The <see cref="T:System.Object" /> for which a hash code is to be returned.</param>
            /// <exception cref="T:System.ArgumentNullException">
            /// The type of <paramref name="obj" /> is a reference type and <paramref name="obj" /> is null.
            /// </exception>
            /// <returns>
            /// A hash code for the specified object.
            /// </returns>
            public override int GetHashCode(
                KeyValuePair<TKey, CacheWrapper<TValue>> obj)
            {
                return obj.Value.Value.GetHashCode();
            }
        }

        /// <summary>
        /// Cache manager to poll for cache that should be removed when expired.
        /// </summary>
        protected class ConcurrentCacheManager : IDisposable
        {
            /// <summary>
            /// Field to determine if this class has already been disposed.
            /// </summary>
            private bool _disposed;

            /// <summary>
            /// The lock object for thread safety.
            /// </summary>
            private readonly object _lockObject = new object();

            /// <summary>
            /// Reference to cache.
            /// <seealso cref="T:ConcurrentCache{TKey,TValue}"/>
            /// </summary>
            private ConcurrentCache<TKey, TValue> _concurrentCache;

            public ConcurrentCacheManager(
                ConcurrentCache<TKey, TValue> concurrentCache)
            {
                if (concurrentCache == null)
                    throw new ArgumentNullException(
                        nameof(concurrentCache),
                        $"{nameof(concurrentCache)} parameter can not be null.");

                lock (_lockObject)
                {
                    lock (concurrentCache.LockObject)
                    {
                        _concurrentCache = concurrentCache;
                    }
                }
            }

            /// <summary>
            /// Starts a new asynchronous thread with a polling loop based on the current
            /// <paramref name="pollingIntervalInMilliseconds"/> interval. This polling loop will
            /// only stop if cancellation is requested from the <see cref="T:System.Threading.CancellationTokenSource"/>
            /// for the <paramref name="cancellationToken"/> parameter,
            /// or if this object becomes disposed.
            /// <remarks>
            /// This method will not run if the <paramref name="pollingIntervalInMilliseconds"/>
            /// parameter is greater than 0, and the first poll will not happen until after
            /// <paramref name="pollingIntervalInMilliseconds"/> parameters value in milliseconds.
            /// </remarks>
            /// </summary>
            /// <param name="pollingIntervalInMilliseconds"></param>
            /// <param name="cancellationToken"></param>
            public void StartPolling(
                double pollingIntervalInMilliseconds,
                CancellationToken cancellationToken)
            {
                lock (_lockObject)
                {
                    if (_disposed)
                        throw new ObjectDisposedException("This object has already been disposed.");
                }

                if (pollingIntervalInMilliseconds <= 0)
                    return;

                Task.Run(async () => {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            lock (_lockObject)
                            {
                                if (_disposed
                                    || _concurrentCache == null
                                    || _concurrentCache.IsDisposed)
                                    break;
                            }

                            var pollingIterval = TimeSpan.FromMilliseconds(pollingIntervalInMilliseconds);
                            await Task.Delay(pollingIterval, cancellationToken);

                            if (cancellationToken.IsCancellationRequested)
                                break;

                            lock (_lockObject)
                            {
                                if (_disposed
                                    || _concurrentCache == null
                                    || _concurrentCache.IsDisposed)
                                    break;
                            }

                            _concurrentCache?.ClearExpiredCache();
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    }
                }, cancellationToken);
            }
            
            /// <summary>
            /// Internal dispose, to be called from IDisposable override.
            /// </summary>
            /// <param name="disposing">If <see cref="Dispose"/> is manually invoked.</param>
            protected virtual void Dispose(bool disposing)
            {
                lock (_lockObject)
                {
                    if (!_disposed && disposing)
                    {
                        // Release the (reference to the) kraken
                        _concurrentCache = null;
                    }

                    _disposed = true;
                }
            }

            /// <summary>
            /// Implements Dispose from IDisposable interface.
            /// <remarks>
            /// Based on the Dispose Pattern described on MSDN, found at the following link:
            /// https://msdn.microsoft.com/en-us/library/b1yfkh5e(v=vs.110).aspx
            /// the article suggests to not make the parameterless Displose method virtual.
            /// </remarks>
            /// </summary>
            public void Dispose()
            {
                lock (_lockObject)
                {
                    Dispose(true);
                    GC.SuppressFinalize(this);
                }
            }
        }

        /// <summary>
        /// Field to determine if this class has already been disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Method to determine if this object has already been disposed.
        /// </summary>
        public bool IsDisposed
        {
            get
            {
                lock (LockObject)
                {
                    return _disposed;
                }
            }
        }

        /// <summary>
        /// The lock object for thread safety.
        /// </summary>
        public readonly object LockObject = new object();

        /// <summary>
        /// The equality comparer for the key of type TKey.
        /// <remarks>
        /// For a TKey of type <see cref="T:System.String"/> a comparer of type
        /// <see cref="T:System.StringComparer.InvariantCultureIgnoreCase"/> could be used.
        /// </remarks>
        /// </summary>
        public IEqualityComparer<TKey> Comparer { get; }

        /// <summary>
        /// The default setting for <see cref="MaxCachedValues"/>.
        /// </summary>
        private const long DefaultMaxCachedValues = 1024L;
        private long _maxCachedValues;

        /// <summary>
        /// The maximum number of cached values to store.
        /// </summary>
        public long MaxCachedValues
        {
            get
            {
                lock (LockObject)
                {
                    return _maxCachedValues;
                }
            }
            set
            {
                lock (LockObject)
                {
                    if (value <= 0)
                    {
                        _maxCachedValues = DefaultMaxCachedValues;
                        return;
                    }

                    _maxCachedValues = value;
                }
            }
        }

        /// <summary>
        /// The minimum cache expiration in milliseconds.
        /// </summary>
        private const double MinCacheExpirationInMilliseconds = 1D;
        private double _cacheExpirationInMilliseconds;

        /// <summary>
        /// The duration in milliseconds that cache will expire.
        /// <remarks>
        /// Changing this property at runtime will only effect values added to the cache
        /// after the value has been changed. Existing cache values will expire under the
        /// duration they were set at when they were originally added.
        /// If <see cref="UseSlidingExpiration"/> is set to true, then the new expiration
        /// for an accessed value in cache will respect the
        /// <see cref="CacheExpirationInMilliseconds"/> property as long as the
        /// cached item has not already expired.
        /// </remarks>
        /// </summary>
        public double CacheExpirationInMilliseconds
        {
            get
            {
                lock (LockObject)
                {
                    return _cacheExpirationInMilliseconds;
                }
            }
            set
            {
                lock (LockObject)
                {
                    if (value <= 0D)
                    {
                        _cacheExpirationInMilliseconds = MinCacheExpirationInMilliseconds;
                        return;
                    }

                    _cacheExpirationInMilliseconds = value;
                }
                
            }
        }

        private bool _useSlidingExpiration;

        /// <summary>
        /// Should cache mechanism use a sliding timeout for expiration duration.
        /// <remarks>
        /// If this property is set to true, as values are accessed in cache,
        /// the expiration date is incremented by the amount set in the
        /// <see cref="CacheExpirationInMilliseconds"/> property, as long as the
        /// cached values are not already expired. Accessing any expired values results
        /// in a failed lookup and the values are removed from cache.
        /// </remarks>
        /// </summary>
        public bool UseSlidingExpiration
        {
            get
            {
                lock (LockObject)
                {
                    return _useSlidingExpiration;
                }
            }
            set
            {
                lock (LockObject)
                {
                    _useSlidingExpiration = value;
                }
            }
        }

        private bool _stopPollingCalled;

        private double _pollingIntervalInMilliseconds;

        /// <summary>
        /// The duration in milliseconds that a cache manager will check for and remove
        /// expired cache values. Setting the value equal to or below 0 will stop
        /// polling and leaves removing expired or least used cache values to the
        /// add methods and indexer set method.
        /// </summary>
        public double PollingIntervalInMilliseconds
        {
            get
            {
                lock (LockObject)
                {
                    return _pollingIntervalInMilliseconds;
                }
            }
            set
            {
                lock (LockObject)
                {
                    if (value <= 0D)
                    {
                        _pollingIntervalInMilliseconds = 0D;
                        
                        try
                        {
                            _cancellationTokenSource?.Cancel();
                        }
                        catch (Exception)
                        {
                            return;
                        }

                        return;
                    }

                    if (_cache.IsEmpty || _stopPollingCalled)
                    {
                        _pollingIntervalInMilliseconds = value;
                        return;
                    }

                    try
                    {
                        _cancellationTokenSource?.Cancel();
                    }
                    finally
                    {
                        _pollingIntervalInMilliseconds = value;

                        _cancellationTokenSource = new CancellationTokenSource();
                        var cancellationToken = _cancellationTokenSource.Token;
                        _cacheManager.StartPolling(_pollingIntervalInMilliseconds, cancellationToken);
                    }
                }
            }
        }

        /// <summary>
        /// The internal dictionary for storing all cached values.
        /// </summary>
        private readonly ConcurrentDictionary<TKey, CacheWrapper<TValue>> _cache;

        /// <summary>
        /// The equality comparer for the contains method.
        /// </summary>
        private readonly ConcurrentCacheContainsEqualityComparer _containsEqualityComparer;

        /// <summary>
        /// The cache manager for expiration polling automation.
        /// </summary>
        private readonly ConcurrentCacheManager _cacheManager;

        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// This specific constructor allows for a custom EqualityComparer for type TKey and
        /// has a default for the max values to cache set at 1,024 values of type TValue.
        /// The default expiration is set at 15 minutes i.e., 900,000 milliseconds.
        /// Cached values that are expired will be removed upon adding new values
        /// or accessing existing expired values during runtime. Expired values are
        /// removed regardless of the <see cref="UseSlidingExpiration"/> property setting.
        /// </summary>
        /// <param name="comparer">
        /// The equality comparer for the key of type TKey.
        /// <remarks>
        /// For a TKey of type <see cref="T:System.String"/> a comparer of type
        /// <see cref="T:System.StringComparer.InvariantCultureIgnoreCase"/> could be used.
        /// </remarks>
        /// </param>
        /// <param name="maxCachedValues">
        /// The maximum number of cached values to store.
        /// <remarks>
        /// This value can also be adjusted at runtime by changing the
        /// <see cref="MaxCachedValues"/> property.
        /// </remarks>
        /// </param>
        /// <param name="cacheExpirationInMilliseconds">
        /// The duration in milliseconds that cache will expire.
        /// <remarks>
        /// Default is set at 900,000 milliseconds or 15 minutes.
        /// This value can also be adjusted at runtime by changing the 
        /// <see cref="CacheExpirationInMilliseconds"/> property.
        /// Changing the <see cref="CacheExpirationInMilliseconds"/> property at runtime
        /// will only effect values added to the cache after the value has been changed.
        /// Existing cache values will expire under the duration they were set at when they
        /// were originally added. If <see cref="UseSlidingExpiration"/> is set to true,
        /// then the new expiration for an accessed value in cache will respect the
        /// <see cref="CacheExpirationInMilliseconds"/> property as long as the
        /// cached item has not already expired.
        /// </remarks>
        /// </param>
        /// <param name="useSlidingExpiration">
        /// Should cache mechanism use a sliding timeout for expiration duration.
        /// <remarks>
        /// Default is set to true.
        /// If <see cref="UseSlidingExpiration"/> is set to true, as values are accessed
        /// in cache, the expiration date is incremented by the amount set in the
        /// <see cref="CacheExpirationInMilliseconds"/> property, as long as the
        /// cached values are not already expired. Accessing any expired values, results
        /// in a failed lookup and the values are removed from cache.
        /// </remarks>
        /// </param>
        /// <param name="pollingIntervalInMilliseconds">
        /// The duration in milliseconds that a cache manager will check for and remove
        /// expired cache values. Setting the value equal to or below 0 will stop
        /// polling and leaves removing expired or least used cache values to the
        /// add methods and indexer set method.
        /// <remarks>
        /// Default is set at 60,000 milliseconds or 1 minute.
        /// </remarks>
        /// </param>
        public ConcurrentCache(
            IEqualityComparer<TKey> comparer = null,
            long maxCachedValues = 1024L,
            double cacheExpirationInMilliseconds = 900000D,
            bool useSlidingExpiration = true,
            double pollingIntervalInMilliseconds = 60000D)
        {
            lock (LockObject)
            {
                if (comparer == null)
                    comparer = EqualityComparer<TKey>.Default;

                Comparer = comparer;
                MaxCachedValues = maxCachedValues;
                CacheExpirationInMilliseconds = cacheExpirationInMilliseconds;
                UseSlidingExpiration = useSlidingExpiration;

                _containsEqualityComparer = new ConcurrentCacheContainsEqualityComparer(Comparer);
                _cache = new ConcurrentDictionary<TKey, CacheWrapper<TValue>>(Comparer);
                _cacheManager = new ConcurrentCacheManager(this);
                PollingIntervalInMilliseconds = pollingIntervalInMilliseconds;
            }
        }

        /// <summary>
        /// Helper method to return a generic dictionary representation of the cache.
        /// </summary>
        /// <returns></returns>
        private Dictionary<TKey, TValue> GetInnerDictionary()
        {
            lock (LockObject)
            {
                if (_disposed)
                    throw new ObjectDisposedException("This object has already been disposed.");
            }

            return _cache.ToDictionary(p => p.Key, p => p.Value.Value, Comparer);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lock (LockObject)
            {
                if (_disposed)
                    throw new ObjectDisposedException("This object has already been disposed.");
            }

            return GetInnerDictionary().GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate
        /// through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Adds a value to cache or overwrite an existing value if it exists.
        /// For overwrites, the expiration is updated if <see cref="UseSlidingExpiration"/>
        /// property is set to true.
        /// </summary>
        /// <param name="item">The key value pair to add to cache.</param>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            lock (LockObject)
            {
                if (_disposed)
                    throw new ObjectDisposedException("This object has already been disposed.");

                // If item already exists in cache update it.
                if (_cache.ContainsKey(item.Key))
                {
                    var now = DateTime.UtcNow;
                    CacheWrapper<TValue> cacheValue;
                    if (_cache.TryGetValue(item.Key, out cacheValue))
                    {
                        cacheValue.Value = item.Value;
                        cacheValue.Uses++;
                        cacheValue.LastUsedOn = now;
                        if (UseSlidingExpiration)
                            cacheValue.ExpiresOn = now.AddMilliseconds(CacheExpirationInMilliseconds);
                        return;
                    }
                }

                // If cache exceeds the maximum then make room for one more item.
                if (_cache.Count >= MaxCachedValues)
                {
                    // Make an assessment of cache value to remove.
                    var valueToRemove = _cache.
                        OrderBy(p => p.Value.ExpiresOn)
                        .ThenBy(p => p.Value.LastUsedOn)
                        .ThenBy(p => p.Value.Uses)
                        .First();

                    CacheWrapper<TValue> cacheValue;
                    _cache.TryRemove(valueToRemove.Key, out cacheValue);
                }

                // Add new item to cache
                var date = DateTime.UtcNow;
                var newCacheValue = new CacheWrapper<TValue>(item.Value)
                {
                    LastUsedOn = date,
                    ExpiresOn = date.AddMilliseconds(CacheExpirationInMilliseconds)
                };

                var isCacheEmpty = _cache.IsEmpty;

                _cache.TryAdd(item.Key, newCacheValue);

                // If cache was previously empty and polling interval is greater than 0
                // start back up the cache manager by reseting the polling interval.
                if (!_stopPollingCalled
                    && isCacheEmpty
                    && PollingIntervalInMilliseconds > 0)
                    PollingIntervalInMilliseconds = PollingIntervalInMilliseconds;
            }
        }

        /// <summary>
        /// Clears all cache values.
        /// </summary>
        public void Clear()
        {
            lock (LockObject)
            {
                if (_disposed)
                    throw new ObjectDisposedException("This object has already been disposed.");

                try
                {
                    // Try to stop polling by using the cancellation token source
                    // as it will no longer be necessary to use extra resources.
                    _cancellationTokenSource?.Cancel();
                }
                finally 
                {
                    // Clear out the cache.
                    _cache.Clear();
                }
            }
        }

        /// <summary>
        /// Remove all expired cache values.
        /// </summary>
        public void ClearExpiredCache()
        {
            lock (LockObject)
            {
                if (_disposed)
                    throw new ObjectDisposedException("This object has already been disposed.");

                if (_cache.IsEmpty)
                    return;

                var now = DateTime.UtcNow;

                // Fetch expired values to remove.
                var valuesToRemove = _cache
                    .Where(p => p.Value.ExpiresOn <= now);

                // Remove expired values.
                foreach (var keyValuePair in valuesToRemove)
                {
                    CacheWrapper<TValue> cacheValue;
                    _cache.TryRemove(keyValuePair.Key, out cacheValue);
                }

                if (!_cache.IsEmpty)
                    return;

                try
                {
                    // Try to stop polling by using the cancellation token source
                    // as it will no longer be necessary to use extra resources.
                    _cancellationTokenSource?.Cancel();
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        /// <summary>
        /// Determines whether the cache contains a specific value.
        /// </summary>
        /// <param name="item">The value to locate in cache.</param>
        /// <returns>
        /// true if <paramref name="item" /> is found in cache; otherwise, false.
        /// </returns>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            lock (LockObject)
            {
                if (_disposed)
                    throw new ObjectDisposedException("This object has already been disposed.");

                if (_cache.IsEmpty)
                    return false;

                var cacheValue = new CacheWrapper<TValue>(item.Value);
                var keyValuePair = new KeyValuePair<TKey, CacheWrapper<TValue>>(item.Key, cacheValue);
                return _cache.Contains(keyValuePair, _containsEqualityComparer);
            }
        }

        /// <summary>
        /// Copies the values from cache to an <see cref="T:System.Array" />,
        /// starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="T:System.Array" /> that is the destination of
        /// the values copied from cache. The <see cref="T:System.Array" /> must have
        /// zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="array" /> is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex" /> is less than 0.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The number of values in cache is greater than the available space from <paramref name="arrayIndex" />
        /// to the end of the destination <paramref name="array" />.
        /// </exception>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(
                    nameof(array),
                    $"The {nameof(array)} parameter should not be null.");

            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(arrayIndex),
                    arrayIndex,
                    $"The {nameof(arrayIndex)} parameter is out of range for {nameof(array)}.");

            Dictionary<TKey, TValue> internalDictionary;
            lock (LockObject)
            {
                if (_disposed)
                    throw new ObjectDisposedException("This object has already been disposed.");

                if (_cache.IsEmpty)
                    return;

                if (_cache.Count + Convert.ToInt64(arrayIndex) > array.LongLength)
                    throw new ArgumentException(
                        "The number of values in cache is greater than the available space from index to the end of the destination array.",
                        nameof(array));

                internalDictionary = GetInnerDictionary();
            }

            internalDictionary
                .ToArray()
                .CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the first occurrence of a specific value from cache.</summary>
        /// <param name="item">The value to remove from cache.</param>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from cache; otherwise, false.
        /// This method also returns false if <paramref name="item" /> is not found in cache.
        /// </returns>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            lock (LockObject)
            {
                if (_disposed)
                    throw new ObjectDisposedException("This object has already been disposed.");

                if (_cache.IsEmpty)
                    return false;

                // If cache value cannot be found return false.
                var cacheValue = new CacheWrapper<TValue>(item.Value);
                var keyValuePair = new KeyValuePair<TKey, CacheWrapper<TValue>>(item.Key, cacheValue);
                if (!_cache.Contains(keyValuePair, _containsEqualityComparer))
                    return false;

                // Remove value from cache.
                CacheWrapper<TValue> cacheValueToRemove;
                var isRemoved = _cache.TryRemove(item.Key, out cacheValueToRemove);

                if (!_cache.IsEmpty)
                    return isRemoved;

                try
                {
                    // Try to stop polling by using the cancellation token source
                    // as it will no longer be necessary to use extra resources.
                    _cancellationTokenSource?.Cancel();
                }
                catch (Exception)
                {
                    // ignored
                }

                return isRemoved;
            }
        }

        /// <summary>
        /// The number of values in cache.
        /// </summary>
        public int Count => _cache.Count;

        /// <summary>
        /// If the cache is readonly.
        /// <remarks>
        /// This property will always be false.
        /// </remarks>
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Determines whether the cache contains a value with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the cache.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is null.
        /// </exception>
        /// <returns>
        /// true if the cache contains an value with the key; otherwise, false.
        /// </returns>
        public bool ContainsKey(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException(
                    nameof(key),
                    $"The parameter {nameof(key)} cannot be null.");

            lock (LockObject)
            {
                if (_disposed)
                    throw new ObjectDisposedException("This object has already been disposed.");

                return !_cache.IsEmpty
                    && _cache.ContainsKey(key);
            }
        }

        /// <summary>
        /// Determines whether the cache contains a value with the specified key.
        /// </summary>
        /// <param name="value">The value to locate in the cache.</param>
        /// <returns>
        /// true if the cache contains the value; otherwise, false.
        /// </returns>
        public bool ContainsValue(TValue value)
        {
            lock (LockObject)
            {
                if (_disposed)
                    throw new ObjectDisposedException("This object has already been disposed.");

                if (_cache.IsEmpty)
                    return false;

                return _cache.Values
                    .Select(p => p.Value)
                    .Contains(value);
            }
        }

        /// <summary>
        /// Adds a value with the provided key to cache or updates an existing value for the specified key.
        /// For overwrites, the expiration is updated if <see cref="UseSlidingExpiration"/>
        /// property is set to true.
        /// <remarks>
        /// This method can be thought of as put in most cache terminolgy or an upsert in
        /// repository terms, however the intention wit this class was to keep familiarity
        /// with method names from the common generic dictionary.
        /// </remarks>
        /// </summary>
        /// <param name="key">The key of the value to add.</param>
        /// <param name="value">The value to add.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is null.
        /// </exception>
        public void Add(TKey key, TValue value)
        {
            if (key == null)
                throw new ArgumentNullException(
                    nameof(key),
                    $"The parameter {nameof(key)} cannot be null.");

            Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        /// <summary>
        /// Removes the value with the specified key from the cache.
        /// </summary>
        /// <param name="key">The key of the value to remove.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is null.
        /// </exception>
        /// <returns>
        /// true if the value is successfully removed; otherwise, false.
        /// This method also returns false if <paramref name="key" /> was not found in cache.
        /// </returns>
        public bool Remove(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException(
                    nameof(key),
                    $"The parameter {nameof(key)} cannot be null.");

            lock (LockObject)
            {
                if (_disposed)
                    throw new ObjectDisposedException("This object has already been disposed.");

                if (_cache.IsEmpty)
                    return false;

                CacheWrapper<TValue> cacheValue;
                var isRemoved = _cache.TryRemove(key, out cacheValue);

                if (!_cache.IsEmpty)
                    return isRemoved;

                try
                {
                    // Try to stop polling by using the cancellation token source
                    // as it will no longer be necessary to use extra resources.
                    _cancellationTokenSource?.Cancel();
                }
                catch (Exception)
                {
                    // ignored
                }

                return isRemoved;
            }
        }

        /// <summary>
        /// Gets the value associated with the specified key from cache.
        /// <remarks>
        /// The expiration is updated if <see cref="UseSlidingExpiration"/>
        /// property is set to true.
        /// </remarks>
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">
        /// When this method returns, the value associated with the specified key, if the key is found;
        /// otherwise, the default value for the type of the <paramref name="value" /> parameter.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is null.
        /// </exception>
        /// <returns>
        /// true if the value is found in cache with the specified key; otherwise, false.
        /// </returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null)
                throw new ArgumentNullException(
                    nameof(key),
                    $"The parameter {nameof(key)} cannot be null.");

            lock (LockObject)
            {
                if (_disposed)
                    throw new ObjectDisposedException("This object has already been disposed.");

                // If cache does not contain the key return a default value.
                if (_cache.IsEmpty ||
                    !_cache.ContainsKey(key))
                {
                    value = default(TValue);
                    return false;
                }

                // Try to caputre the cache value or return a defalt value.
                CacheWrapper<TValue> cacheValue;
                if (!_cache.TryGetValue(key, out cacheValue))
                {
                    value = default(TValue);
                    return false;
                }

                // If the value has expired, remove it and return a default value.
                var now = DateTime.UtcNow;
                if (cacheValue.ExpiresOn <= now)
                {
                    Remove(key);
                    value = default(TValue);
                    return false;
                }

                // Update cache usage and return the value.
                cacheValue.Uses++;
                cacheValue.LastUsedOn = now;
                if (UseSlidingExpiration)
                    cacheValue.ExpiresOn = now.AddMilliseconds(CacheExpirationInMilliseconds);
                value = cacheValue.Value;
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// When getting values, adding a new value, or overwrites using the indexer,
        /// the expiration is updated if <see cref="UseSlidingExpiration"/>
        /// property is set to true.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is null.
        /// </exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">
        /// The property is retrieved and <paramref name="key" /> does not exist in cache.
        /// </exception>
        /// <returns>
        /// The value associated with the specified key.
        /// If the specified key is not found, a get operation throws a
        /// <see cref="T:System.Collections.Generic.KeyNotFoundException" />,
        /// and a set operation creates a new value with the specified key.
        /// </returns>
        public TValue this[TKey key]
        {
            get
            {
                if (key == null)
                    throw new ArgumentNullException(
                        nameof(key),
                        $"The parameter {nameof(key)} cannot be null.");

                TValue value;
                if (TryGetValue(key, out value))
                    return value;

                throw new KeyNotFoundException("The key provided does not exist in cache.");
            }
            set
            {
                if (key == null)
                    throw new ArgumentNullException(
                        nameof(key),
                        $"The parameter {nameof(key)} cannot be null.");

                Add(new KeyValuePair<TKey, TValue>(key, value));
            }
        }

        /// <summary>
        /// Gets a collection that contains the keys in the cache.
        /// </summary>
        /// <returns>
        /// A collection that contains the keys in the cache.
        /// </returns>
        public ICollection<TKey> Keys => GetInnerDictionary().Keys;

        /// <summary>
        /// Gets a collection that contains the values in the cache.
        /// </summary>
        /// <returns>
        /// A collection that contains the values in the cache.
        /// </returns>
        public ICollection<TValue> Values => GetInnerDictionary().Values;

        /// <summary>
        /// Determines whether or not cache manager is polling.
        /// </summary>
        public bool IsPolling
        {
            get
            {
                lock (LockObject)
                {
                    if (_disposed)
                        throw new ObjectDisposedException("This object has already been disposed.");

                    return !_stopPollingCalled
                        && !_cache.IsEmpty
                        && PollingIntervalInMilliseconds > 0
                        && _cancellationTokenSource != null
                        && !_cancellationTokenSource.IsCancellationRequested;
                }
            }
        }

        /// <summary>
        /// Iterate through the cache and update the expiration for every value,
        /// included already expired cache. This will update expiration periods
        /// for every value regardless of whether or not <see cref="UseSlidingExpiration"/>
        /// property is set to true.
        /// <remarks>
        /// If polling was stopped by calling the <see cref="StopPolling"/> method
        /// prior to calling <see cref="Revive"/>, this method will not start
        /// polling again after updating the expiration date for each
        /// cache entry.
        /// </remarks>
        /// </summary>
        public void Revive()
        {
            lock (LockObject)
            {
                if (_disposed)
                    throw new ObjectDisposedException("This object has already been disposed.");

                if (_cache.IsEmpty)
                    return;

                // Stop polling if it has not already been called.
                var isStopPollingCalled = _stopPollingCalled;
                if (!_stopPollingCalled)
                    StopPolling();

                // Update cache expiration date for each item.
                var expiresOn = DateTime.UtcNow.AddMilliseconds(CacheExpirationInMilliseconds);
                foreach (var cacheWrapper in _cache)
                    cacheWrapper.Value.ExpiresOn = expiresOn;

                // If stop polling had not originally been called,
                // start polling again.
                if (!isStopPollingCalled)
                    StartPolling();
            }
        }

        /// <summary>
        /// Starts cache manager polling for removing expired cache.
        /// <remarks>
        /// If the cache is empty, polling will not begin until the first cache value is added,
        /// and the <see cref="PollingIntervalInMilliseconds"/> property is greater than 0.
        /// </remarks>
        /// </summary>
        public void StartPolling()
        {
            lock (LockObject)
            {
                if (_disposed)
                    throw new ObjectDisposedException("This object has already been disposed.");

                _stopPollingCalled = false;
                // If polling interval is greater than 0 start back up the cache manager
                // by reseting the polling interval.
                if (!_cache.IsEmpty && PollingIntervalInMilliseconds > 0)
                    PollingIntervalInMilliseconds = PollingIntervalInMilliseconds;
            }
        }

        /// <summary>
        /// Stops cache manager from polling for and removing expired cache.
        /// <remarks>
        /// This method prevents changes to <see cref="PollingIntervalInMilliseconds"/>
        /// property from starting polling through the cache manager, even if
        /// the value of <see cref="PollingIntervalInMilliseconds"/> is greater than 0.
        /// </remarks>
        /// </summary>
        public void StopPolling()
        {
            lock (LockObject)
            {
                if (_disposed)
                    throw new ObjectDisposedException("This object has already been disposed.");

                _stopPollingCalled = true;

                try
                {
                    _cancellationTokenSource?.Cancel();
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        /// <summary>
        /// Internal dispose, to be called from IDisposable override.
        /// </summary>
        /// <param name="disposing">If <see cref="Dispose"/> is manually invoked.</param>
        protected virtual void Dispose(bool disposing)
        {
            lock (LockObject)
            {
                if (!_disposed && disposing)
                {
                    try
                    {
                        // Stop polling using cancellation token
                        _cancellationTokenSource?.Cancel();
                    }
                    finally
                    {
                        _cacheManager.Dispose();
                        _cache.Clear();
                    }
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Implements Dispose from IDisposable interface.
        /// <remarks>
        /// Based on the Dispose Pattern described on MSDN, found at the following link:
        /// https://msdn.microsoft.com/en-us/library/b1yfkh5e(v=vs.110).aspx
        /// the article suggests to not make the parameterless Displose method virtual.
        /// </remarks>
        /// </summary>
        public void Dispose()
        {
            lock (LockObject)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
