using Google.GData.Photos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace GPhotoSync
{
    public interface IAlbumRepository
    {
        List<Album> GetList();
    }

    public class AlbumRepository : IAlbumRepository
    {
        #region Fields
        private readonly IClientCredentials _credentials;
        private readonly IPhotoRepository _photoRepository;
        #endregion Fields

        #region Properties
        #endregion Properties

        #region Ctor
        public AlbumRepository(IClientCredentials credentials, IPhotoRepository photoRepository)
        {
            if (credentials == null)
                throw new ArgumentNullException("credentials");
            if (photoRepository == null)
                throw new ArgumentNullException("photoRepository");
            _credentials = credentials;
            _photoRepository = photoRepository;
        }
        #endregion Ctor

        #region Methods
        public List<Album> GetList()
        {
            var service = new PicasaService("GPhotoSync");
            service.SetAuthenticationToken(_credentials.AccessToken);

            var query = new AlbumQuery();
            query.Thumbsize = "180";
            query.Uri = new Uri(PicasaQuery.CreatePicasaUri(_credentials.User));

            var feed = service.Query(query);

            if (feed != null)
            {
                var list = feed.Entries
                    .OfType<PicasaEntry>()
                    .Where(x => !IsPostEntry(x))
                    .OrderBy(x => x.Title.Text)
                    .Select(x =>
                    {
                        var accessor = new AlbumAccessor(x);
                       
                        var thumb = x.Media.Thumbnails[0];
                        using (var stream = service.Query(new Uri(thumb.Attributes["url"] as string)))
                        {
                            var ms = new MemoryStream();
                            stream.CopyTo(ms);
                            ms.Seek(0, SeekOrigin.Begin);
                            return new Album
                            {
                                Id = accessor.Id,
                                Title = accessor.AlbumTitle,
                                ImageStream = ms,
                                PhotoCount = (int)accessor.NumPhotos
                            };
                        }
                    })
                    .ToList();
                return list;
            }
            else
                return new List<Album>();
        }

        private bool IsPostEntry(PicasaEntry entry)
        {
            var regExA = @"^(0[1-9]|[12][0-9]|3[01])[- /.](0[1-9]|1[012])[- /.](19|20)\d\d$";
            var regExB = @"^(0[1-9]|[12][0-9]|3[01])[- /.](0[1-9]|1[012])[- /.]\d\d$";
            var regExC = @"^(19|20)\d\d[- /.](0[1-9]|1[012])[- /.](0[1-9]|[12][0-9]|3[01])$";
            return Regex.IsMatch(entry.Title.Text, regExA) ||
                Regex.IsMatch(entry.Title.Text, regExB) ||
                Regex.IsMatch(entry.Title.Text, regExC);
        }
        #endregion Methods
    }
}
