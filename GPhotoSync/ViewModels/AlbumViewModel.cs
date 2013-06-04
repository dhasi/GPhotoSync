using GalaSoft.MvvmLight;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GPhotoSync
{
    public class AlbumViewModel : ViewModelBase, IViewModel
    {
        #region Properties
        private ImageSource _imageSource;
        public ImageSource ImageSource
        {
            get
            {
                if (_imageSource == null)
                {
                    //var bitmap = new Bitmap(Model.ImageStream);
                    var bi = new BitmapImage();
                    bi.BeginInit();
                    //bi.DecodePixelWidth = 30;
                    bi.StreamSource = Model.ImageStream;
                    bi.EndInit();
                    _imageSource = bi;
                }
                return _imageSource;

            }
        }
        
        public Album Model { get; private set; }
        #endregion Properties

        #region Ctor
        public AlbumViewModel(Album model)
        {
            Model = model;
        }
        #endregion Ctor
    }
}
