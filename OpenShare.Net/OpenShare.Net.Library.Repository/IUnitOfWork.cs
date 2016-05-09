using System;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;

namespace OpenShare.Net.Library.Repository
{
    /// <summary>
    /// Implements a Unit of Work pattern.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Return the database reference for this Unit of Work.
        /// </summary>
        DbContext DbContext { get; }

        /// <summary>
        /// Call this to commit the Unit of Work.
        /// </summary>
        void Commit();

        /// <summary>
        /// Call this to commit the Unit of Work.
        /// </summary>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
