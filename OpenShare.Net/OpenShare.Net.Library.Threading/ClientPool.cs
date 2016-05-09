using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace OpenShare.Net.Library.Threading
{
    public class ClientPool<T> : IDisposable
        where T : class, IDisposable, new()
    {
        private bool _disposed;

        public object LockObject = new object();

        private const int TwentyMinutesInMilliseconds = 20 * 60 * 1000; // 20 minutes = 1,200,000 milliseconds
        private const int OneDayInMilliseconds = 24 * 60 * 60 * 1000; // 24 hours = 86,400,000 milliseconds

        public const int MinClients = 1;
        private int _maxClients = 32;
        public const int MinWaitPeriodBetweenTriesInMilliseconds = 0;
        private int _maxWaitPeriodBetweenTriesInMilliseconds = TwentyMinutesInMilliseconds;
        public const int MinWaits = 1;
        private int _maxWaits = OneDayInMilliseconds / TwentyMinutesInMilliseconds; // 72 waits at most (a whole day)

        protected ConcurrentQueue<T> Clients { get; set; }
        protected ConcurrentDictionary<int, T> ClientsInUse { get; set; }

        public int Count
        {
            get
            {
                lock (LockObject)
                {
                    return Clients.Count + ClientsInUse.Count;
                }
            }
        }

        public virtual int MaxClients
        {
            get { return _maxClients; }
            set
            {
                if (value < MinClients)
                {
                    _maxClients = MinClients;
                    return;
                }

                _maxClients = value;
            }
        }

        public virtual int MaxWaitPeriodBetweenTriesInMilliseconds
        {
            get { return _maxWaitPeriodBetweenTriesInMilliseconds; }
            set
            {
                if (value < MinWaitPeriodBetweenTriesInMilliseconds)
                {
                    _maxWaitPeriodBetweenTriesInMilliseconds = MinWaitPeriodBetweenTriesInMilliseconds;
                    return;
                }

                _maxWaitPeriodBetweenTriesInMilliseconds = value;
            }
        }

        public virtual int MaxWaits
        {
            get { return _maxWaits; }
            set
            {
                if (value < MinWaits)
                {
                    _maxWaits = MinWaits;
                    return;
                }

                _maxWaits = value;
            }
        }

        public ClientPool()
        {
            lock (LockObject)
            {
                Clients = new ConcurrentQueue<T>();
                Clients.Enqueue(new T());
                ClientsInUse = new ConcurrentDictionary<int, T>();
            }
        }

        public ClientPool(int maxClients)
        {
            lock (LockObject)
            {
                if (maxClients < MinClients)
                    maxClients = 1;

                if (maxClients > _maxClients)
                    maxClients = _maxClients;

                Clients = new ConcurrentQueue<T>();
                for (var i = 0; i < maxClients; i++)
                    Clients.Enqueue(new T());

                ClientsInUse = new ConcurrentDictionary<int, T>();
            }
        }

        public virtual T GetPooledClient()
        {
            lock (LockObject)
            {
                T client;
                if (!Clients.TryDequeue(out client))
                    return null;

                if (!ClientsInUse.TryAdd(client.GetHashCode(), client))
                    throw new Exception("Unexpected error in GetClient(). Unable to track client.");

                return client;
            }
        }

        public virtual T GetPooledClient(int maxWaits, int waitPeriodBetweenTriesInMilliseconds)
        {
            var client = GetPooledClient();
            if (client != null)
                return client;

            if (waitPeriodBetweenTriesInMilliseconds < MinWaitPeriodBetweenTriesInMilliseconds)
                waitPeriodBetweenTriesInMilliseconds = MinWaitPeriodBetweenTriesInMilliseconds;

            if (waitPeriodBetweenTriesInMilliseconds > MaxWaitPeriodBetweenTriesInMilliseconds)
                waitPeriodBetweenTriesInMilliseconds = MaxWaitPeriodBetweenTriesInMilliseconds;


            for (var i = 0; i < maxWaits; i++)
            {
                Thread.Sleep(waitPeriodBetweenTriesInMilliseconds);
                client = GetPooledClient();
                if (client != null)
                    return client;
            }

            return null;
        }

        public virtual bool ReturnPooledClient(T client)
        {
            if (client == null)
                return false;

            lock (LockObject)
            {
                if (!ClientsInUse.ContainsKey(client.GetHashCode()))
                    return false;

                T clientInUse;
                if (!ClientsInUse.TryRemove(client.GetHashCode(), out clientInUse))
                    return false;

                Clients.Enqueue(clientInUse);
                return true;
            }
        }

        public virtual bool AddPooledClients(int count = 1)
        {
            if (count < 1 || Count + count > MaxClients)
                return false;

            lock (LockObject)
            {
                for (var i = 0; i < MaxClients; i++)
                    Clients.Enqueue(new T());
                return true;
            }
        }

        public virtual bool AddPooledClient(T client)
        {
            if (client == null)
                return false;

            lock (LockObject)
            {
                if (Count >= MaxClients
                    || Clients.Any(p => p == client)
                    || ClientsInUse.ContainsKey(client.GetHashCode()))
                    return false;

                Clients.Enqueue(new T());
                return true;
            }
        }

        public virtual bool RemovePooledClientsNotInUse(int count = 1)
        {
            if (count < 1 || Clients.Count == 0)
                return false;

            lock (LockObject)
            {
                count = Math.Min(count, Clients.Count);
                for (var i = 0; i < count; i++)
                {
                    T client;
                    if (Clients.TryDequeue(out client))
                        client.Dispose();
                }
            }

            return true;
        }

        public virtual bool RemovePooledClientNotInUse(T client)
        {
            if (client == null)
                return false;

            lock (LockObject)
            {
                if (Clients.Any(p => p == client))
                {
                    var clients = new ConcurrentQueue<T>();
                    T existingClient;
                    while (Clients.TryDequeue(out existingClient))
                        if (client != existingClient)
                            clients.Enqueue(existingClient);
                    Clients = clients;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Internal dispose, to be called from IDisposable override.
        /// </summary>
        /// <param name="disposing">If Dispose() is manually invoked.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                lock (LockObject)
                {
                    if (Clients != null && Clients.Count > 0)
                    {
                        for (var i = 0; i < Clients.Count; i++)
                        {
                            T client;
                            if (Clients.TryDequeue(out client))
                                client.Dispose();
                        }

                        foreach (var key in ClientsInUse.Keys.ToList())
                        {
                            T client;
                            if (ClientsInUse.TryGetValue(key, out client)
                                && ClientsInUse.TryRemove(key, out client))
                                client.Dispose();
                        }
                    }
                }
            }

            _disposed = true;
        }

        /// <summary>
        /// Implements Dispose from IDisposable interface.
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
