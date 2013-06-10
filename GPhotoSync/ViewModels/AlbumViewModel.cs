using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
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

        public List<Photo> Photos { get; set; }

        public List<Photo> LocalPhotos { get; set; }

        private bool _loadingPhotos;
        public bool LoadingPhotos
        {
            get { return _loadingPhotos; }
            set
            {
                _loadingPhotos = value;
                RaisePropertyChanged(() => LoadingPhotos);
                if (LoadingPhotos)
                    PhotosLoaded = false;
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

        private bool _synchronizingPhotos;
        public bool SynchronizingPhotos
        {
            get { return _synchronizingPhotos; }
            set
            {
                _synchronizingPhotos = value;
                RaisePropertyChanged(() => SynchronizingPhotos);
            }
        }

        public bool HasChanges { get { return HasRemoteChanges || HasLocalChanges; } }

        private bool _hasRemoveChanges;
        public bool HasRemoteChanges
        {
            get { return _hasRemoveChanges; }
            set
            {
                _hasRemoveChanges = value;
                RaisePropertyChanged(() => HasRemoteChanges);
                RaisePropertyChanged(() => HasChanges);
            }
        }

        private bool _hasLocalChanges;
        public bool HasLocalChanges
        {
            get { return _hasLocalChanges; }
            set
            {
                _hasLocalChanges = value;
                RaisePropertyChanged(() => HasLocalChanges);
                RaisePropertyChanged(() => HasChanges);
            }
        }

        public string ToolTipMessage { get { return "xxx"; } }
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
