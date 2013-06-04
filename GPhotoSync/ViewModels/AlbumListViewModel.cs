using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace GPhotoSync
{
    public class AlbumListViewModel : ViewModelBase, IViewModel
    {
        #region Fields
        private readonly IPhotoRepository _photoRepository;
        #endregion Fields

        #region Properties
        public ObservableCollection<AlbumViewModel> Albums { get; private set; }

        private AlbumViewModel _selectedAlbum;
        public AlbumViewModel SelectedAlbum
        {
            get { return _selectedAlbum; }
            set
            {
                _selectedAlbum = value;
                RaisePropertyChanged(() => SelectedAlbum);
                Messenger.Default.Send(new AlbumChangedMessage { Album = _selectedAlbum });
            }
        }
        #endregion Properties

        #region Ctor
        public AlbumListViewModel(IMessenger messenger, IPhotoRepository repo)
            : base(messenger)
        {
            if (repo == null)
                throw new ArgumentNullException("repo");
            _photoRepository = repo;
            Albums = new ObservableCollection<AlbumViewModel>();
            MessengerInstance.Register<LoginMessage>(this, OnLogin);
        }

        #endregion Ctor

        #region Methods
        private void OnLogin(LoginMessage msg)
        {
            LoadAlbumsAsync();
        }
        public Task LoadAlbumsAsync()
        {
            Albums.Clear();
            return Task.Factory.StartNew(() =>
            {
                return _photoRepository
                  .GetAlbums()
                  .Select(a => new AlbumViewModel(a))
                  .ToList();
            }).ContinueWith(r =>
            {
                r.Result.ForEach(a => Albums.Add(a));
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        #endregion Methods
    }
}
