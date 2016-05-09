using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenShare.Net.Library.Common;

namespace OpenShare.Net.Library.Repository
{
    /// <summary>
    /// Implements a Unit of Work pattern.
    /// </summary>
    public abstract class BaseUnitOfWork : IUnitOfWork
    {
        private readonly DbContext _dbContext;
        private bool _disposed;

        /// <summary>
        /// Specific constructor.
        /// </summary>
        /// <param name="dbContext">The DBContext to use for the Unit of Work.</param>
        protected BaseUnitOfWork(DbContext dbContext)
        {
            _dbContext = dbContext;
            _dbContext.Configuration.AutoDetectChangesEnabled = true;
            _dbContext.Configuration.ValidateOnSaveEnabled = false;
        }

        /// <summary>
        /// Return the database reference for this Unit of Work.
        /// </summary>
        public DbContext DbContext
        {
            get { return _dbContext; }
        }

        /// <summary>
        /// Call this to commit the Unit of Work.
        /// </summary>
        public virtual void Commit()
        {
            _dbContext.SaveChanges();
            PurgeCache();
        }

        /// <summary>
        /// Call this to commit the Unit of Work.
        /// </summary>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        public virtual async Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _dbContext.SaveChangesAsync(cancellationToken);
            PurgeCache();
        }

        /// <summary>
        /// This is called automatically on Commit(), but is available for override.
        /// </summary>
        public virtual void PurgeCache()
        {
            var objectContext = ((IObjectContextAdapter)DbContext).ObjectContext;
            var changes = objectContext.ObjectStateManager.GetObjectStateEntries(
                EntityState.Added | EntityState.Deleted | EntityState.Deleted | EntityState.Modified | EntityState.Unchanged);
            changes.Where(p => p.State != EntityState.Detached).ForEach(p =>
            {
                try
                {
                    objectContext.Detach(p.Entity);
                }
                catch (Exception)
                {
                    // Do nothing.
                }
            });
        }

        /// <summary>
        /// Internal dispose, to be called from IDisposable override.
        /// </summary>
        /// <param name="disposing">If Dispose() is manually invoked.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
                _dbContext.Dispose();

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
