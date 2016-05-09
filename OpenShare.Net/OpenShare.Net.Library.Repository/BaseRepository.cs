using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenShare.Net.Library.Repository
{
    /// <summary>
    /// Implements a Repository pattern for type T.
    /// </summary>
    /// <typeparam name="T">The type of repository.</typeparam>
    /// <typeparam name="TK">The type of primary key model for repository.</typeparam>
    public abstract class BaseRepository<T, TK> : IRepository<T, TK>
        where T : class
        where TK : class
    {
        /// <summary>
        /// DbContext for Repository.
        /// </summary>
        public DbContext Context { get; private set; }

        /// <summary>
        /// DbSet from context.
        /// </summary>
        protected DbSet<T> DbSet
        {
            get { return Context.Set<T>(); }
        }

        /// <summary>
        /// Specific Constructor.
        /// </summary>
        /// <param name="context">DbContext for repopsitory.</param>
        protected BaseRepository(DbContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            Context = context;
        }

        /// <summary>
        /// Retrieve a single item using it's primary key or throw exception if not found.
        /// </summary>
        /// <param name="primaryKey">The primary key of the record.</param>
        /// <returns>T</returns>
        public abstract T Single(TK primaryKey);

        /// <summary>
        /// Retrieve a single item using it's primary key or throw exception if not found.
        /// </summary>
        /// <param name="primaryKey">The primary key of the record.</param>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        /// <returns>T</returns>
        public abstract Task<T> SingleAsync(TK primaryKey, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Retrieve a single item using it's primary key or null if not found.
        /// </summary>
        /// <param name="primaryKey">The primary key of the record.</param>
        /// <returns>T</returns>
        public abstract T SingleOrDefault(TK primaryKey);

        /// <summary>
        /// Retrieve a single item using it's primary key or null if not found.
        /// </summary>
        /// <param name="primaryKey">The primary key of the record.</param>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        /// <returns>T</returns>
        public abstract Task<T> SingleOrDefaultAsync(TK primaryKey, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Return all rows for type T.
        /// </summary>
        /// <returns>IQueryable of type T</returns>
        public virtual IQueryable<T> GetAll()
        {
            return DbSet;
        }

        /// <summary>
        /// Figures out if an item of type T exist by the primary key specified.
        /// </summary>
        /// <param name="primaryKey">The primary key of the record.</param>
        /// <returns>True if T exists for primary key.</returns>
        public abstract bool Exists(TK primaryKey);

        /// <summary>
        /// Figures out if an item of type T exist by the primary key specified.
        /// </summary>
        /// <param name="primaryKey">The primary key of the record.</param>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        /// <returns>True if T exists for primary key.</returns>
        public abstract Task<bool> ExistsAsync(TK primaryKey, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Inserts a record of type T.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        /// <returns>Entity from DbSet.Add.</returns>
        public virtual T Insert(T entity)
        {
            dynamic obj = DbSet.Add(entity);
            Context.Entry(entity).State = EntityState.Added;
            return obj;
        }

        /// <summary>
        /// Updates the entity in the database by using it's primary key.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        public virtual void Update(T entity)
        {
            if (Context.Entry(entity).State == EntityState.Detached)
                DbSet.Attach(entity);

            Context.Entry(entity).State = EntityState.Modified;
        }

        /// <summary>
        /// Deep copies the entity in the database, given a set of specifications.
        /// </summary>
        /// <param name="entity">Entity to clone.</param>
        /// <param name="properties">Navigational properties to include in the deep copy.</param>
        /// <param name="useExisting">Use the passed in entity to clone.</param>
        /// <returns>Cloned entity.</returns>
        public abstract T Clone(T entity, List<string> properties, bool useExisting);

        /// <summary>
        /// Deep copies the entity in the database, given a set of specifications.
        /// </summary>
        /// <param name="entity">Entity to clone.</param>
        /// <param name="properties">Navigational properties to include in the deep copy.</param>
        /// <param name="useExisting">Use the passed in entity to clone.</param>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        /// <returns>Cloned entity.</returns>
        public abstract Task<T> CloneAsync(T entity, List<string> properties, bool useExisting, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Gets the object contect entity type name for Repository of type T.
        /// </summary>
        /// <returns>Full EntitySet name for type T.</returns>
        protected string GetEntitySetName()
        {
            var objectContext = ((IObjectContextAdapter)Context).ObjectContext;

            var entityType = typeof(T);
            var entityTypeName = entityType.Name;

            var container = objectContext.MetadataWorkspace.GetEntityContainer(
                objectContext.DefaultContainerName, DataSpace.CSpace);
            var entitySetName = (
                    from meta in container.BaseEntitySets
                    where meta.ElementType.Name == entityTypeName
                    select meta.Name
                ).First();
            var builder = new StringBuilder(container.Name.Length + entitySetName.Length + 1);
            builder.Append(container.Name);
            builder.Append(".");
            builder.Append(entitySetName);
            var fullEntitySetName = builder.ToString();
            return fullEntitySetName;
        }

        /// <summary>
        /// Returns a copy of the Entity by the primary key specified that is detached from the context.
        /// </summary>
        /// <param name="primaryKey">The primary key of the record.</param>
        /// <param name="properties">Navigation properties to include.</param>
        /// <returns>Returns a detached entity.</returns>
        public abstract T GetDetached(TK primaryKey, List<string> properties);

        /// <summary>
        /// Returns a copy of the Entity by the primary key specified that is detached from the context.
        /// </summary>
        /// <param name="primaryKey">The primary key of the record.</param>
        /// <param name="properties">Navigation properties to include.</param>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        /// <returns>Returns a detached entity.</returns>
        public abstract Task<T> GetDetachedAsync(TK primaryKey, List<string> properties, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Deletes the entity from the database.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <returns>Deleted entity.</returns>
        public virtual T Delete(T entity)
        {
            if (Context.Entry(entity).State == EntityState.Detached)
                DbSet.Attach(entity);

            return DbSet.Remove(entity);
        }

        /// <summary>
        /// Refreshes the context by fetching the entity model again.
        /// </summary>
        /// <param name="entity">The entity to refresh.</param>
        public virtual void Refresh(T entity)
        {
            if (Context.Entry(entity).State == EntityState.Detached)
                DbSet.Attach(entity);

            Context.Entry(entity).Reload();
        }

        /// <summary>
        /// Refreshes the context by fetching the entity model again.
        /// </summary>
        /// <param name="entity">The entity to refresh.</param>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        public virtual async Task RefreshAsync(T entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (Context.Entry(entity).State == EntityState.Detached)
                DbSet.Attach(entity);

            await Context.Entry(entity).ReloadAsync(cancellationToken);
        }
    }
}
