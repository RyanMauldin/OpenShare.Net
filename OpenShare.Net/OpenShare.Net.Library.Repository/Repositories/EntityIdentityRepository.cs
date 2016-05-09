using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenShare.Net.Library.Repository.Identities;

namespace OpenShare.Net.Library.Repository.Repositories
{
    /// <summary>
    /// Implements a Repository pattern for type T.
    /// </summary>
    /// <typeparam name="T">The type of repository.</typeparam>
    /// <typeparam name="TK">The type of primary key model for repository.</typeparam>
    public class EntityIdentityRepository<T, TK> : BaseRepository<T, TK>
        where T : class, IEntityIdentity
        where TK : class, IEntityIdentity
    {
        /// <summary>
        /// Specific Constructor.
        /// </summary>
        /// <param name="context"></param>
        public EntityIdentityRepository(DbContext context)
            : base(context)
        {

        }

        /// <summary>
        /// Retrieve a single item using it's primary key or throw exception if not found.
        /// </summary>
        /// <param name="primaryKey">The primary key of the record.</param>
        /// <returns>T</returns>
        public override T Single(TK primaryKey)
        {
            return DbSet.Single(p => p.Id == primaryKey.Id);
        }

        /// <summary>
        /// Retrieve a single item using it's primary key or throw exception if not found.
        /// </summary>
        /// <param name="primaryKey">The primary key of the record.</param>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        /// <returns>T</returns>
        public override async Task<T> SingleAsync(TK primaryKey, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await DbSet.SingleAsync(p => p.Id == primaryKey.Id, cancellationToken);
        }

        /// <summary>
        /// Retrieve a single item using it's primary key or null if not found.
        /// </summary>
        /// <param name="primaryKey">The primary key of the record.</param>
        /// <returns>T</returns>
        public override T SingleOrDefault(TK primaryKey)
        {
            return DbSet.SingleOrDefault(p => p.Id == primaryKey.Id);
        }

        /// <summary>
        /// Retrieve a single item using it's primary key or null if not found.
        /// </summary>
        /// <param name="primaryKey">The primary key of the record.</param>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        /// <returns>T</returns>
        public override async Task<T> SingleOrDefaultAsync(TK primaryKey, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await DbSet.SingleOrDefaultAsync(p => p.Id == primaryKey.Id, cancellationToken);
        }

        /// <summary>
        /// Figures out if an item of type T exist by the primary key specified.
        /// </summary>
        /// <param name="primaryKey">The primary key of the record.</param>
        /// <returns>True if T exists for primary key.</returns>
        public override bool Exists(TK primaryKey)
        {
            return DbSet.Find(primaryKey.Id) != null;
        }

        /// <summary>
        /// Figures out if an item of type T exist by the primary key specified.
        /// </summary>
        /// <param name="primaryKey">The primary key of the record.</param>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        /// <returns>True if T exists for primary key.</returns>
        public override async Task<bool> ExistsAsync(TK primaryKey, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await DbSet.FindAsync(cancellationToken, primaryKey.Id) != null;
        }

        /// <summary>
        /// Deep copies the entity in the database, given a set of specifications.
        /// </summary>
        /// <param name="entity">Entity to clone.</param>
        /// <param name="properties">Navigational properties to include in the deep copy.</param>
        /// <param name="useExisting">Use the passed in entity to clone.</param>
        /// <returns>Cloned entity.</returns>
        public override T Clone(T entity, List<string> properties, bool useExisting)
        {
            var entitySetName = GetEntitySetName();

            if (useExisting)
            {
                ((IObjectContextAdapter)Context).ObjectContext.AddObject(entitySetName, entity);
                return entity;
            }

            var query = DbSet.AsNoTracking().AsQueryable();
            query = properties.Aggregate(query, (current, child) => current.Include(child));
            var clone = query.FirstOrDefault(p => p.Id == entity.Id);

            ((IObjectContextAdapter)Context).ObjectContext.AddObject(entitySetName, clone);

            return clone;
        }

        /// <summary>
        /// Deep copies the entity in the database, given a set of specifications.
        /// </summary>
        /// <param name="entity">Entity to clone.</param>
        /// <param name="properties">Navigational properties to include in the deep copy.</param>
        /// <param name="useExisting">Use the passed in entity to clone.</param>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        /// <returns>Cloned entity.</returns>
        public override async Task<T> CloneAsync(T entity, List<string> properties, bool useExisting, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var entitySetName = GetEntitySetName();

            if (useExisting)
            {
                ((IObjectContextAdapter)Context).ObjectContext.AddObject(entitySetName, entity);
                return entity;
            }

            var query = DbSet.AsNoTracking().AsQueryable();
            query = properties.Aggregate(query, (current, child) => current.Include(child));
            var clone = await query.FirstOrDefaultAsync(p => p.Id == entity.Id, cancellationToken);

            ((IObjectContextAdapter)Context).ObjectContext.AddObject(entitySetName, clone);

            return clone;
        }

        /// <summary>
        /// Returns a copy of the Entity by the primary key specified that is detached from the context.
        /// </summary>
        /// <param name="primaryKey">The primary key of the record.</param>
        /// <param name="properties">Navigation properties to include.</param>
        /// <returns>Returns a detached entity.</returns>
        public override T GetDetached(TK primaryKey, List<string> properties)
        {
            Context.Configuration.ProxyCreationEnabled = false;
            var query = DbSet.AsNoTracking().AsQueryable();
            query = properties.Aggregate(query, (current, child) => current.Include(child));
            var clone = query.FirstOrDefault(p => p.Id == primaryKey.Id);
            Context.Configuration.ProxyCreationEnabled = true;
            return clone;
        }

        /// <summary>
        /// Returns a copy of the Entity by the primary key specified that is detached from the context.
        /// </summary>
        /// <param name="primaryKey">The primary key of the record.</param>
        /// <param name="properties">Navigation properties to include.</param>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        /// <returns>Returns a detached entity.</returns>
        public override async Task<T> GetDetachedAsync(TK primaryKey, List<string> properties, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            Context.Configuration.ProxyCreationEnabled = false;
            var query = DbSet.AsNoTracking().AsQueryable();
            query = properties.Aggregate(query, (current, child) => current.Include(child));
            var clone = await query.FirstOrDefaultAsync(p => p.Id == primaryKey.Id, cancellationToken);
            Context.Configuration.ProxyCreationEnabled = true;
            return clone;
        }
    }
}
