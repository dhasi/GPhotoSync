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
using System.Threading;

namespace GPhotoSync
{
    public class GPhotoViewModel : ViewModelBase, IViewModel
    {
        #region Fields
        private readonly IAlbumRepository _albumRepository;
        private readonly IPhotoRepository _photoRepository;
        private readonly IAlbumMappingRepository _mappingRepository;
        private readonly IViewModelLocator _viewModelLocator;
        private readonly List<AlbumViewModel> _loadAlbums = new List<AlbumViewModel>();
        private readonly PhotoComparer _photoComparer = new PhotoComparer();
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
                RaisePropertyChanged(() => LoadingAlbums);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public RelayCommand LoginCommand { get; private set; }

        public RelayCommand ReloadAlbumsCommand { get; private set; }

        public RelayCommand ReloadStatesCommand { get; private set; }

        public RelayCommand MapAlbumCommand { get; private set; }

        public RelayCommand DownloadAlbumsCommand { get; private set; }

        public RelayCommand UploadAlbumsCommand { get; private set; }

        public ObservableCollection<AlbumViewModel> Albums { get; private set; }

        public bool LoadingAlbums { get { return IsBusy && Albums.Count == 0; } }

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

        private bool _hideUnmapped;
        public bool HideUnmapped
        {
            get { return _hideUnmapped; }
            set
            {
                _hideUnmapped = value;
                RaisePropertyChanged(() => HideUnmapped);
                UpdateAlbums();
            }
        }

        private bool _hideUnchanged;
        public bool HideUnchanged
        {
            get { return _hideUnchanged; }
            set
            {
                _hideUnchanged = value;
                RaisePropertyChanged(() => HideUnchanged);
                UpdateAlbums();
            }
        }
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
            ReloadAlbumsCommand = new RelayCommand(ReloadAlbums, CanReloadAlbums);
            ReloadStatesCommand = new RelayCommand(ReloadStates, CanReloadStates);
            MapAlbumCommand = new RelayCommand(MapAlbum, CanMapAlbum);
            DownloadAlbumsCommand = new RelayCommand(DownloadAlbums, CanDownloadAlbums);
            UploadAlbumsCommand = new RelayCommand(UploadAlbums, CanUploadAlbums);
        }

        public async Task LoadAlbums()
        {
            _loadAlbums.Clear();
            Albums.Clear();
            IsBusy = true;

            try
            {
                IsLoggedIn = true;
                var albums = await Task.Factory.StartNew<List<Album>>(_albumRepository.GetList);
                var mappings = await Task.Factory.StartNew<List<AlbumMapping>>(_mappingRepository.GetList);

                albums.ForEach(a => _loadAlbums.Add(new AlbumViewModel(a)
                {
                    AlbumMapping = mappings.FirstOrDefault(m => m.AlbumId == a.Id) ?? new AlbumMapping { AlbumId = a.Id }
                }));

                UpdateAlbums();

                RaisePropertyChanged(() => LoadingAlbums);
                var mappedAlbums = Albums.Where(x => x.AlbumMapping.IsMapped);

                if (mappedAlbums.Any())
                {
                    var tasks = mappedAlbums
                        .AsParallel()
                        .Select(async album =>
                        {
                            await LoadGPhotos(album);
                            await LoadLocalPhotos(album);
                            await ComparePhotos(album);
                        });
                    foreach (var task in tasks)
                        await task;
                }
                IsBusy = false;
            }
            catch (Exception ex)
            {
                IsLoggedIn = false;
                IsBusy = false;
                throw ex;
            }
        }

        private async Task LoadGPhotos(AlbumViewModel album)
        {
            album.LoadingPhotos = true;
            var removePhotos = await Task.Factory.StartNew<List<Photo>>(() => _photoRepository.GetListFor(album.Album.Id));
            album.Photos = removePhotos;
            album.PhotosLoaded = true;
        }

        private async Task LoadLocalPhotos(AlbumViewModel album)
        {
            if (!Directory.Exists(album.AlbumMapping.LocalPath)) return;

            var files = await Task.Factory.StartNew<List<Photo>>(() => Directory.GetFiles(album.AlbumMapping.LocalPath)
               .Select(x => new FileInfo(x))
               .Where(f => (f.Attributes & FileAttributes.Hidden) == 0)
               .Select(x => new Photo { Title = x.Name })
               .ToList());

            album.LocalPhotos = files;

        }

        private async Task ComparePhotos(AlbumViewModel album)
        {
            if (!album.AlbumMapping.IsMapped) return;

            var localChanges = await Task.Factory.StartNew(() => album.LocalPhotos.Except(album.Photos, _photoComparer));
            var remoteChanges = await Task.Factory.StartNew(() => album.Photos.Except(album.LocalPhotos, _photoComparer));

            album.HasLocalChanges = localChanges.Any();
            album.HasRemoteChanges = remoteChanges.Any();
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

        public bool CanReloadAlbums() { return !IsBusy && IsLoggedIn; }

        public async void ReloadAlbums()
        {
            if (CanReloadAlbums())
                await LoadAlbums();
        }

        public bool CanReloadStates() { return !IsBusy && IsLoggedIn; }

        public async void ReloadStates()
        {
            IsBusy = true;
            var mappedAlbums = Albums.Where(x => x.AlbumMapping.IsMapped);

            if (mappedAlbums.Any())
            {
                var tasks = mappedAlbums
                    .AsParallel()
                    .Select(async album =>
                    {
                        await LoadGPhotos(album);
                        await LoadLocalPhotos(album);
                        await ComparePhotos(album);
                    });
                foreach (var task in tasks)
                    await task;
            }
            IsBusy = false;

        }

        private void UpdateAlbums()
        {
            Albums.Clear();
            _loadAlbums
                .Where(x => (HideUnmapped ? x.AlbumMapping.IsMapped : true))
                .Where(x => (HideUnchanged ? x.HasChanges : true))
                .ToList()
                .ForEach(a => Albums.Add(a));

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

        public bool CanDownloadAlbums() { return Albums.Any(x => x.IsSelected && x.HasRemoteChanges); }

        public void DownloadAlbums()
        {
            Albums
                 .Where(x => x.IsSelected && x.AlbumMapping.IsMapped)
                 .ToList()
                 .ForEach(album =>
                 {
                     //create & execute download tasks for each album
                     //each task downloads new photos from g+photos
                     //album.SynchronizingPhotos = true;
                     //var remoteChanges = await Task.Factory.StartNew(() => album.Photos.Except(album.LocalPhotos, _photoComparer));
                     //var localChanges = await Task.Factory.StartNew(() => album.LocalPhotos.Except(album.Photos, _photoComparer));
                     //remoteChanges
                     //    .AsParallel()
                     //    .ForAll(async p =>
                     //    {
                     //        Thread.Sleep(10000);
                     //        var albumLocation = album.AlbumMapping.LocalPath;
                     //        var fileName = Path.Combine(albumLocation, p.Title);
                     //        await Task.Factory.StartNew(() => _photoRepository.DownloadPhoto(p.Path, fileName));
                     //    });
                     //album.SynchronizingPhotos = false;
                 });
            ReloadStates();
        }

        public bool CanUploadAlbums() { return Albums.Any(x => x.IsSelected && x.HasLocalChanges); }

        public async void UploadAlbums()
        {

        }
        #endregion Methods


    }
}
