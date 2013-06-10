using Google.GData.Client;
using Google.GData.Photos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace GPhotoSync
{
    public interface IPhotoRepository
    {
        List<Photo> GetListFor(string albumId);

        void UploadPhotoTo(string albumId, string filename);

        void DownloadPhoto(string url, string filename);
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
            query.ExtraParameters = "imgmax=d";
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
                            Path = x.Media.Content.Url
                        };
                    });

                return list.OfType<Photo>().ToList();
            }
            else
                return new List<Photo>();
        }

        public void UploadPhotoTo(string albumId, string filename)
        {
            var service = new PicasaService("GPhotoSync");
            service.SetAuthenticationToken(_credentials.AccessToken);

            var query = new PhotoQuery(PicasaQuery.CreatePicasaUri(_credentials.User, albumId));
            var feed = service.Query(query);

            var media = new MediaFileSource(filename, MimeTypes.GetMimeType(Path.GetExtension(filename)));
            var photo = new PhotoEntry();
            photo.Title = new AtomTextConstruct { Text = Path.GetFileNameWithoutExtension(filename) };
            photo.MediaSource = media;

            service.Insert(feed, photo);
        }

        public void DownloadPhoto(string url, string filename)
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(url, filename);
            }
        }
        #endregion Methods



    }
}
