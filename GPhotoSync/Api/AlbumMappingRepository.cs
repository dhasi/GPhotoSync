using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPhotoSync
{
    public interface IAlbumMappingRepository
    {
        List<AlbumMapping> GetList();
        void Save(AlbumMapping album);
        void Delete(Guid id);
    }

    public class AlbumMappingRepository : IAlbumMappingRepository
    {
        #region Fields
        private readonly ISessionFactory _sessionFactory;
        #endregion Fields

        #region Properties
        #endregion Properties

        #region Ctor
        public AlbumMappingRepository(ISessionFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");
            _sessionFactory = factory;

        }
        #endregion Ctor

        #region Methods
        public List<AlbumMapping> GetList()
        {
            using (var session = _sessionFactory.OpenSession())
                return session.QueryOver<AlbumMapping>().List().ToList();
        }

        public void Save(AlbumMapping album)
        {
            using (var session = _sessionFactory.OpenSession())
            {
                session.SaveOrUpdate(album);
                session.Flush();
            }
        }

        public void Delete(Guid id)
        {
            using (var session = _sessionFactory.OpenSession())
            {
                var album = session.Get<AlbumMapping>(id);
                if (album != null)
                {
                    session.Delete(album);
                    session.Flush();
                }
            }
        }
        #endregion Methods
    }
}
