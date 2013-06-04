using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace GPhotoSync
{
    public class Album
    {
        #region Properties
        public Stream ImageStream { get; set; }
        public string Title { get; set; }
        #endregion Properties
    }
}
