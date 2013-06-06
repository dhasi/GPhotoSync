using System.Collections.Generic;
using System.IO;

namespace GPhotoSync
{
    public class Album
    {
        #region Properties
        public string Id { get; set; }
        public Stream ImageStream { get; set; }
        public string Title { get; set; }
        public int PhotoCount { get; set; }
        public List<Photo> Photos { get; set; }
        #endregion Properties
    }
}
