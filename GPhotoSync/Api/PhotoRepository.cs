using Google.GData.Photos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPhotoSync
{
    public interface IPhotoRepository
    {
        List<Photo> GetListFor(string albumId);
    }

    public class PhotoRepository : IPhotoRepository
    {
        #region Fields
        private IClientCredentials _credentials;
        #endregion Fields

        #region Properties
        #endregion Properties

        #region Ctor
        public PhotoRepository(IClientCredentials credentials)
        {
            if (credentials == null)
                throw new ArgumentNullException("credentials");
            _credentials = credentials;
        }
        #endregion Ctor

        #region Methods
        public List<Photo> GetListFor(string albumId)
        {
            var service = new PicasaService("GPhotoSync");
            service.SetAuthenticationToken(_credentials.AccessToken);

            var query = new PhotoQuery(PicasaQuery.CreatePicasaUri(_credentials.User, albumId));
            var feed = service.Query(query);

            if (feed != null)
            {
                var list = feed.Entries
                    .OfType<PicasaEntry>()
                    .Select(x =>
                    {
                        var accessor = new PhotoAccessor(x);
                        
                        return new Photo
                        {
                            Id = accessor.Id,
                            Title = accessor.PhotoTitle,
                            Size = accessor.Size
                        };
                    })
                    .ToList();
                return list;
            }
            else
                return new List<Photo>();
        }
        #endregion Methods
    }
}
