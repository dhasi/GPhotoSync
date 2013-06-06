using GalaSoft.MvvmLight;
using System;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GPhotoSync
{
    public class AlbumViewModel : ViewModelBase, IViewModel
    {
        #region Properties
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                RaisePropertyChanged(() => IsSelected);
            }
        }

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
                    bi.StreamSource = Album.ImageStream;
                    bi.EndInit();
                    _imageSource = bi;
                }
                return _imageSource;

            }
        }
        
        public Album Album { get; private set; }

        public AlbumMapping AlbumMapping { get; set; }

        private bool _loadingPhotos;
        public bool LoadingPhotos
        {
            get { return _loadingPhotos; }
            set
            {
                _loadingPhotos = value;
                RaisePropertyChanged(() => LoadingPhotos);
            }
        }

        private bool _photosLoaded;
        public bool PhotosLoaded
        {
            get { return _photosLoaded; }
            set
            {
                _photosLoaded = value;
                RaisePropertyChanged(() => PhotosLoaded);
            }
        }

        private bool _isOutDated;
        public bool IsOutDated
        {
            get { return _isOutDated; }
            set
            {
                _isOutDated = value;
                RaisePropertyChanged(() => IsOutDated);
            }
        }
        #endregion Properties

        #region Ctor
        public AlbumViewModel(Album album)
        {
            if (album == null)
                throw new ArgumentNullException("album");
            Album = album;
        }
        #endregion Ctor
    }
}
