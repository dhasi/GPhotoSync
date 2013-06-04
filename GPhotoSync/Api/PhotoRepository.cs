using Google.GData.Photos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace GPhotoSync
{
    public interface IPhotoRepository
    {
        List<Album> GetAlbums();
    }

    public class PhotoRepository : IPhotoRepository
    {
        #region Fields
        private readonly IClientCredentials _credentials;
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
        public List<Album> GetAlbums()
        {
            var service = new PicasaService("GPhotoSync");
            service.SetAuthenticationToken(_credentials.AccessToken);
            var query = new AlbumQuery();
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
                            var thumb = x.Media.Thumbnails[0];
                            using (var stream = service.Query(new Uri(thumb.Attributes["url"] as string)))
                            {
                                var ms = new MemoryStream();
                                stream.CopyTo(ms);
                                ms.Seek(0, SeekOrigin.Begin);
                                return new Album
                                {
                                    Title = x.Title.Text,
                                    ImageStream = ms
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
            //return Regex.IsMatch(entry.Title.Text, "^(19|20)\\d\\d([- /.])(0[1-9]|1[012])\\2(0[1-9]|[12][0-9]|3[01])$") ||
            //    Regex.IsMatch(entry.Title.Text, "^(0[1-9]|1[012])       (19|20)\\d\\d([- /.])(0[1-9]|1[012])\\2(0[1-9]|[12][0-9]|3[01])$");
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
