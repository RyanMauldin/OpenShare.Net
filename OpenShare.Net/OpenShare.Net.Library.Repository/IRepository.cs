using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenShare.Net.Library.Repository
{
    /// <summary>
    /// Implements a Repository pattern for type T with a complex key TK.
    /// </summary>
    /// <typeparam name="T">The type of repository.</typeparam>
    /// <typeparam name="TK">The type of primary key model for repository.</typeparam>
    public interface IRepository<T, in TK>
    {
        /// <summary>
        /// DbContext for Repository.
        /// </summary>
        DbContext Context { get; }

        /// <summary>
        /// Retrieve a single item using it's primary key or throw exception if not found.
        /// </summary>
        /// <param name="primaryKey">The primary key of the record.</param>
        /// <returns>T</returns>
        T Single(TK primaryKey);

        /// <summary>
        /// Retrieve a single item using it's primary key or throw exception if not found.
        /// </summary>
        /// <param name="primaryKey">The primary key of the record.</param>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        /// <returns>T</returns>
        Task<T> SingleAsync(TK primaryKey, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Retrieve a single item using it's primary key or null if not found.
        /// </summary>
        /// <param name="primaryKey">The primary key of the record.</param>
        /// <returns>T</returns>
        T SingleOrDefault(TK primaryKey);

        /// <summary>
        /// Retrieve a single item using it's primary key or null if not found.
        /// </summary>
        /// <param name="primaryKey">The primary key of the record.</param>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        /// <returns>T</returns>
        Task<T> SingleOrDefaultAsync(TK primaryKey, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Return all rows for type T.
        /// </summary>
        /// <returns>IQueryable of type T</returns>
        IQueryable<T> GetAll();

        /// <summary>
        /// Figures out if an item of type T exist by the primary key specified.
        /// </summary>
        /// <param name="primaryKey">The primary key of the record.</param>
        /// <returns>True if T exists for primary key.</returns>
        bool Exists(TK primaryKey);

        /// <summary>
        /// Figures out if an item of type T exist by the primary key specified.
        /// </summary>
        /// <param name="primaryKey">The primary key of the record.</param>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        /// <returns>True if T exists for primary key.</returns>
        Task<bool> ExistsAsync(TK primaryKey, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Inserts a record of type T.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        /// <returns>Entity from DbSet.Add.</returns>
        T Insert(T entity);

        /// <summary>
        /// Updates the entity in the database by using it's primary key.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        void Update(T entity);

        /// <summary>
        /// Deep copies the entity in the database, given a set of specifications.
        /// </summary>
        /// <param name="entity">Entity to clone.</param>
        /// <param name="properties">Navigational properties to include in the deep copy.</param>
        /// <param name="useExisting">Use the passed in entity to clone.</param>
        /// <returns>Cloned entity.</returns>
        T Clone(T entity, List<string> properties, bool useExisting);

        /// <summary>
        /// Deep copies the entity in the database, given a set of specifications.
        /// </summary>
        /// <param name="entity">Entity to clone.</param>
        /// <param name="properties">Navigational properties to include in the deep copy.</param>
        /// <param name="useExisting">Use the passed in entity to clone.</param>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        /// <returns>Cloned entity.</returns>
        Task<T> CloneAsync(T entity, List<string> properties, bool useExisting, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Returns a copy of the Entity by the primary key specified that is detached from the context.
        /// </summary>
        /// <param name="primaryKey">The primary key of the record.</param>
        /// <param name="properties">Navigation properties to include.</param>
        /// <returns>Returns a detached entity.</returns>
        T GetDetached(TK primaryKey, List<string> properties);

        /// <summary>
        /// Returns a copy of the Entity by the primary key specified that is detached from the context.
        /// </summary>
        /// <param name="primaryKey">The primary key of the record.</param>
        /// <param name="properties">Navigation properties to include.</param>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        /// <returns>Returns a detached entity.</returns>
        Task<T> GetDetachedAsync(TK primaryKey, List<string> properties, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Deletes the entity from the database.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <returns>Deleted entity.</returns>
        T Delete(T entity);

        /// <summary>
        /// Refreshes the context by fetching the entity model again.
        /// </summary>
        /// <param name="entity">The entity to refresh.</param>
        void Refresh(T entity);

        /// <summary>
        /// Refreshes the context by fetching the entity model again.
        /// </summary>
        /// <param name="entity">The entity to refresh.</param>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        Task RefreshAsync(T entity, CancellationToken cancellationToken = default(CancellationToken));
    }
}
