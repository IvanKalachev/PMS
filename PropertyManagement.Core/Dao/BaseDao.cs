using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PropertyManagement.Core.Entities;
using NHibernate;
using NHibernate.Linq;

namespace PropertyManagement.Core.Dao
{
    public class BaseDao<TEntity, TIdentifier>
        where TIdentifier : new ()
        where TEntity : Entity<Int64>
    {
        /// <summary>
        /// NHibernate ISession to be used to manipulate data in the
        /// database.
        /// </summary>
        protected ISession CurrentSession { get; set; }

        public BaseDao(ISession session)
        {
            CurrentSession = session;
        }

        /// <summary>
        /// Load an Entity by its identifier.
        /// </summary>
        /// <param name="id">Entity´s identifier.</param>
        /// <returns>Entity Object.</returns>
        public TEntity LoadById(TIdentifier id)
        {
            TEntity entity = CurrentSession.Get<TEntity>(id);
            return entity;
        }

        /// <summary>
        /// Create an Entity.
        /// </summary>
        /// <param name="entity">Entity to be created.</param>
        /// <returns>Identfifier of the created Entity.</returns>
        public void Create(TEntity entity)
        {
            using (var transaction = CurrentSession.BeginTransaction())
            {
                CurrentSession.Save(entity);
                transaction.Commit();
            }
        }

        /// <summary>
        /// Save or Update an Entity.
        /// </summary>
        /// <param name="entity">Entity to be saved or updated.</param>
        public void SaveOrUpdate(TEntity entity)
        {
            using (var transaction = CurrentSession.BeginTransaction())
            {
                CurrentSession.SaveOrUpdate(entity);
                transaction.Commit();
            }
        }

        /// <summary>
        /// Update an existing Enitty.
        /// </summary>
        /// <param name="entity">Entity to be updated.</param>
        public void Update(TEntity entity)
        {
            using (var transaction = CurrentSession.BeginTransaction())
            {
                CurrentSession.Update(entity);
                CurrentSession.Flush();
                transaction.Commit();
            }
        }

        /// <summary>
        /// Delete an Entity based on its Instance.
        /// </summary>
        /// <param name="entity">Entity Instance.</param>
        public void Delete(TEntity entity)
        {
            using (var transaction = CurrentSession.BeginTransaction())
            {
                CurrentSession.Delete(entity);
                transaction.Commit();
            }
        }

        /// <summary>
        /// Delete an Entity based on its Identifier.
        /// </summary>
        /// <param name="entityIdentifier">Entity Identifier.</param>
        public void DeleteById(TIdentifier entityIdentifier)
        {
            using (var transaction = CurrentSession.BeginTransaction())
            {
                TEntity entity = LoadById(entityIdentifier);
                CurrentSession.Delete(entity);
                transaction.Commit();
            }
        }

        /// <summary>
        /// Retrieve all Entities from the database.
        /// </summary>
        /// <returns>List of all entities.</returns>
        public IList<TEntity> LoadAll()
        {
            return CurrentSession.Query<TEntity>().ToList();
        }
    }
}
