using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenShare.Net.Library.Threading;

namespace OpenShare.Net.UnitTest.UnitTests
{
    [TestClass]
    public class OpenShareNetLibraryThreadingUnitTests : BaseUnitTest
    {
        private const string KeyOne = "one";
        private const long ValueOne = 1L;
        private const string KeyTwo = "two";
        private const long ValueTwo = 2L;
        private const string KeyThree = "three";
        private const long ValueThree = 3L;
        private const long ValueFifty = 50L;

        private readonly KeyValuePair<string, long> _keyValuePair1 = new KeyValuePair<string, long>(KeyOne, ValueOne);
        private readonly KeyValuePair<string, long> _keyValuePair2 = new KeyValuePair<string, long>(KeyTwo, ValueTwo);
        private readonly KeyValuePair<string, long> _keyValuePair3 = new KeyValuePair<string, long>(KeyThree, ValueThree);

        private const string KeyDoesNotExistErrorMessage = "The key provided does not exist in cache.";
        private const string ValueShouldHaveExpiredErrorMessage = "Value should have expired in cache.";

        [TestMethod]
        public void ConcurrentCache_ExpirationTests()
        {
            //var concurrentDictionary = new ConcurrentDictionary<string, long>(StringComparer.InvariantCultureIgnoreCase);
            var concurrentCache = new ConcurrentCache<string, long>(StringComparer.InvariantCultureIgnoreCase)
            {
                {KeyOne, ValueOne}
            };

            var concurrentCacheValue = concurrentCache[KeyOne];

            Assert.IsNotNull(concurrentCacheValue);
            Assert.AreEqual(concurrentCacheValue, ValueOne);
            
            concurrentCache[KeyOne] = ValueTwo;
            concurrentCacheValue = concurrentCache[KeyOne];

            Assert.IsNotNull(concurrentCacheValue);
            Assert.AreEqual(concurrentCacheValue, ValueTwo);
            
            concurrentCache.CacheExpirationInMilliseconds = 10D;
            concurrentCache[KeyOne] = ValueThree;

            Thread.Sleep(100);

            try
            {
                concurrentCacheValue = concurrentCache[KeyOne];
                Assert.Fail(ValueShouldHaveExpiredErrorMessage);
            }
            catch (KeyNotFoundException ex)
            {
                Assert.IsNotNull(ex);
                Assert.AreEqual(ex.Message, KeyDoesNotExistErrorMessage);
            }

            Assert.AreEqual(concurrentCacheValue, ValueTwo);

            // Polling tests for Cache Manager
            const double expirationDuration = 1000D;
            concurrentCache.CacheExpirationInMilliseconds = expirationDuration;
            concurrentCache[KeyTwo] = ValueFifty;
            var concurrentCacheValue2 = concurrentCache[KeyTwo];
            Assert.AreEqual(concurrentCacheValue2, ValueFifty);
            concurrentCache.CacheExpirationInMilliseconds = expirationDuration * 10;
            concurrentCache.PollingIntervalInMilliseconds = expirationDuration / 6;
            concurrentCache[KeyOne] = ValueFifty;

            Thread.Sleep((int)expirationDuration * 2);

            try
            {
                concurrentCacheValue2 = concurrentCache[KeyTwo];
                Assert.Fail(ValueShouldHaveExpiredErrorMessage);
            }
            catch (KeyNotFoundException ex)
            {
                Assert.IsNotNull(ex);
                Assert.AreEqual(ex.Message, KeyDoesNotExistErrorMessage);
            }

            try
            {
                concurrentCacheValue = concurrentCache[KeyOne];
            }
            catch (KeyNotFoundException ex)
            {
                Assert.IsNotNull(ex);
                Assert.AreEqual(ex.Message, KeyDoesNotExistErrorMessage);
            }

            concurrentCache.Dispose();

            Assert.AreEqual(concurrentCacheValue, ValueFifty);
        }

        [TestMethod]
        public void ConcurrentCache_FunctionalityTests()
        {
            var dictionary = new Dictionary<string, long>(StringComparer.InvariantCultureIgnoreCase);
            var concurrentCache = new ConcurrentCache<string, long>(
                StringComparer.InvariantCultureIgnoreCase,
                cacheExpirationInMilliseconds: 10L,
                pollingIntervalInMilliseconds: 5L);

            var dictionaryComparer = dictionary.Comparer;
            var concurrentCacheComparer = concurrentCache.Comparer;
            Assert.AreEqual(dictionaryComparer, concurrentCacheComparer);

            dictionary.Add(KeyOne, ValueOne);
            concurrentCache.Add(KeyOne, ValueOne);

            var dictionaryValue = dictionary[KeyOne];
            var concurrentCacheValue = concurrentCache[KeyOne];

            Assert.IsNotNull(dictionaryValue);
            Assert.IsNotNull(concurrentCacheValue);
            Assert.AreEqual(dictionaryValue, concurrentCacheValue);

            Assert.IsTrue(dictionary.ContainsKey(KeyOne));
            Assert.IsTrue(concurrentCache.ContainsKey(KeyOne));

            Assert.IsTrue(dictionary.ContainsValue(ValueOne));
            Assert.IsTrue(concurrentCache.ContainsValue(ValueOne));

            Assert.IsTrue(dictionary.Remove(KeyOne));
            Assert.IsTrue(concurrentCache.Remove(KeyOne));

            Assert.AreEqual(dictionary.Count, 0);
            Assert.AreEqual(concurrentCache.Count, 0);

            concurrentCache.Add(_keyValuePair1);
            concurrentCache.Add(_keyValuePair2);
            concurrentCache.Add(_keyValuePair3);

            Assert.IsTrue(concurrentCache.Remove(KeyTwo));
            Assert.AreEqual(concurrentCache.Count, 2);
            Assert.IsTrue(concurrentCache.Contains(_keyValuePair3));
            concurrentCache.Clear();
            Assert.AreEqual(concurrentCache.Count, 0);

            long cacheValue;
            Assert.IsFalse(concurrentCache.TryGetValue(KeyTwo, out cacheValue));
            Assert.AreEqual(concurrentCache.Count, 0);
            concurrentCache.CacheExpirationInMilliseconds = 50D;
            long existingCacheValue;
            concurrentCache[KeyOne] = ValueFifty;
            Assert.IsTrue(concurrentCache.TryGetValue(KeyOne, out existingCacheValue));
            Assert.AreEqual(existingCacheValue, ValueFifty);
            Assert.AreEqual(concurrentCache.Count, 1);
            concurrentCache.ClearExpiredCache();
            Assert.AreEqual(concurrentCache.Count, 1);

            Thread.Sleep(100);
            concurrentCache.ClearExpiredCache();
            Assert.AreEqual(concurrentCache.Count, 0);
            Assert.IsFalse(concurrentCache.Remove(_keyValuePair1));
            Assert.IsFalse(concurrentCache.Remove(KeyThree));

            // Turn off polling.
            concurrentCache.StopPolling();

            Assert.IsFalse(concurrentCache.IsPolling);

            concurrentCache[KeyOne] = ValueOne;
            concurrentCache[KeyTwo] = ValueTwo;
            concurrentCache[KeyThree] = ValueThree;

            Assert.AreEqual(concurrentCache.Keys.Count, 3);
            Assert.AreEqual(concurrentCache.Values.Sum(), 6L);

            Assert.IsTrue(concurrentCache.Remove(_keyValuePair3));
            Assert.AreEqual(concurrentCache.Values.Sum(), 3L);

            concurrentCache.Add(KeyThree, ValueThree);
            Assert.AreEqual(concurrentCache.Values.Sum(), 6L);

            var manualCount = 0;
            var manualTotal = 0L;
            foreach (var keyValuePair in concurrentCache)
            {
                manualCount++;
                manualTotal += keyValuePair.Value;
            }

            Assert.AreEqual(manualCount, 3);
            Assert.AreEqual(manualTotal, 6L);

            // Revive all cache.
            concurrentCache.Revive();

            // Turn on polling
            concurrentCache.StartPolling();

            Assert.IsTrue(concurrentCache.IsPolling);

            Assert.AreEqual(concurrentCache.Count, 3);

            concurrentCache.StopPolling();
            var array = new KeyValuePair<string, long>[4];
            concurrentCache.CopyTo(array, 1);
            array[0] = new KeyValuePair<string, long>("zero", 0);

            Assert.AreEqual(array.Sum(p => p.Value), 6);

            Assert.IsFalse(concurrentCache.IsDisposed);
            concurrentCache.Dispose();
            Assert.IsTrue(concurrentCache.IsDisposed);
            Assert.IsNotNull(concurrentCache);
        }
    }
}
