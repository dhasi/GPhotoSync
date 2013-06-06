using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;

namespace GPhotoSync
{
    public class GPhotoViewModel : ViewModelBase, IViewModel
    {
        #region Fields
        private readonly IAlbumRepository _albumRepository;
        private readonly IPhotoRepository _photoRepository;
        private readonly IAlbumMappingRepository _mappingRepository;
        private readonly IViewModelLocator _viewModelLocator;
        #endregion Fields

        #region Properties
        private bool _isLoggedIn;
        public bool IsLoggedIn
        {
            get { return _isLoggedIn; }
            set
            {
                _isLoggedIn = value;
                RaisePropertyChanged(() => IsLoggedIn);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                RaisePropertyChanged(() => IsBusy);
                CommandManager.InvalidateRequerySuggested();
            }
        }
        public RelayCommand LoginCommand { get; private set; }

        public RelayCommand ReloadCommand { get; private set; }

        public RelayCommand MapAlbumCommand { get; private set; }

        public ObservableCollection<AlbumViewModel> Albums { get; private set; }

        private AlbumViewModel _selectedAlbum;
        public AlbumViewModel SelectedAlbum
        {
            get { return _selectedAlbum; }
            set
            {
                _selectedAlbum = value;
                RaisePropertyChanged(() => SelectedAlbum);
                RaisePropertyChanged(() => IsAlbumSelected);
            }
        }

        public bool IsAlbumSelected { get { return SelectedAlbum != null; } }

        #endregion Properties

        #region Ctor
        public GPhotoViewModel(
            IMessenger messenger,
            IAlbumRepository albumRepository,
            IPhotoRepository photoRepository,
            IAlbumMappingRepository mappingRepository,
            IViewModelLocator viewModelLocator)
            : base(messenger)
        {
            if (albumRepository == null)
                throw new ArgumentNullException("albumRepository");
            if (photoRepository == null)
                throw new ArgumentNullException("photoRepository");
            if (mappingRepository == null)
                throw new ArgumentNullException("mappingRepository");
            if (viewModelLocator == null)
                throw new ArgumentNullException("viewModelLocator");

            _albumRepository = albumRepository;
            _photoRepository = photoRepository;
            _mappingRepository = mappingRepository;
            _viewModelLocator = viewModelLocator;

            Albums = new ObservableCollection<AlbumViewModel>();
            InitializeCommands();
        }

        #endregion Ctor

        #region Methods
        private void InitializeCommands()
        {
            LoginCommand = new RelayCommand(Login, CanLogin);
            ReloadCommand = new RelayCommand(Reload, CanReload);
            MapAlbumCommand = new RelayCommand(MapAlbum, CanMapAlbum);
        }

        public async Task LoadAlbums()
        {
            Albums.Clear();
            IsBusy = true;

            try
            {
                var albums = await Task.Factory.StartNew<List<Album>>(_albumRepository.GetList);
                var mappings = await Task.Factory.StartNew<List<AlbumMapping>>(_mappingRepository.GetList);

                albums.ForEach(a => Albums.Add(new AlbumViewModel(a)
                {
                    AlbumMapping = mappings.FirstOrDefault(m => m.AlbumId == a.Id) ?? new AlbumMapping { AlbumId = a.Id }
                }));

                var mappedAlbums = Albums.Where(x => x.AlbumMapping.IsMapped);

                if (mappedAlbums.Any())
                {
                    var tasks = mappedAlbums
                        .AsParallel()
                        .Select(async album =>
                        {
                            await LoadPhotos(album);
                            await ComparePhotos(album);
                        });
                    foreach (var task in tasks)
                        await task;
                }
                IsBusy = false;
            }
            catch (Exception ex)
            {
                IsBusy = false;
                throw ex;
            }
        }

        private async Task LoadPhotos(AlbumViewModel album)
        {
            album.LoadingPhotos = true;
            var photos = await Task.Factory.StartNew<List<Photo>>(() => _photoRepository.GetListFor(album.Album.Id));
            album.Album.Photos = photos;
            album.PhotosLoaded = true;
        }

        private async Task ComparePhotos(AlbumViewModel album)
        {
            if (!Directory.Exists(album.AlbumMapping.LocalPath)) return;

            var files = await Task.Factory.StartNew<List<FileInfo>>(() => Directory.GetFiles(album.AlbumMapping.LocalPath)
                .Select(x => new FileInfo(x))
                .Where(f => (f.Attributes & FileAttributes.Hidden) == 0)
                .ToList());

            var outDated = await Task.Factory.StartNew<bool>(() => files.Any(x => !album.Album.Photos.Any(p => p.Title == x.Name)));
            album.IsOutDated = outDated;
        }

        private bool CanLogin() { return !IsLoggedIn && !IsBusy; }

        public void Login()
        {
            if (CanLogin())
            {
                var viewModel = _viewModelLocator.Locate<LoginViewModel>();
                viewModel.OnLogin = () =>
                {
                    IsLoggedIn = true;
                    LoadAlbums();
                };
                MessengerInstance.Send(new ShowDialogMessage
                {
                    Content = viewModel
                });
            }
        }

        public bool CanReload() { return !IsBusy; }

        public void Reload()
        {
            if (CanReload())
                LoadAlbums();
        }

        public bool CanMapAlbum() { return Albums.Count(x => x.IsSelected) == 1; }

        public void MapAlbum()
        {
            if (!CanMapAlbum()) return;

            var album = Albums.FirstOrDefault(x => x.IsSelected);
            if (album != null)
            {
                var dlg = new FolderBrowserDialog();
                dlg.SelectedPath = album.AlbumMapping.LocalPath;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    var path = dlg.SelectedPath;
                    album.AlbumMapping.LocalPath = path;
                    _mappingRepository.Save(album.AlbumMapping);
                }
            }
        }
        #endregion Methods
    }
}
