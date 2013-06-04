using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;

namespace GPhotoSync
{
    public class GPhotoViewModel : ViewModelBase, IViewModel
    {
        #region Fields
        private readonly IPhotoRepository _repository;
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
                RaisePropertyChanged(() => IsAlbumSelected);
            }
        }

        public bool IsAlbumSelected { get { return SelectedAlbum != null; } }

        #endregion Properties

        #region Ctor
        public GPhotoViewModel(IMessenger messenger, IPhotoRepository repository)
            : base(messenger)
        {
            if (repository == null)
                throw new ArgumentNullException("repository");

            _repository = repository;
            Albums = new ObservableCollection<AlbumViewModel>();
            MessengerInstance.Register<LoginMessage>(this, OnLogin);
        }

        #endregion Ctor

        #region Methods
        private void OnLogin(LoginMessage msg)
        {
            LoadAlbums();
        }

        public Task LoadAlbums()
        {
            Albums.Clear();

            return Task.Factory.StartNew(() =>
            {
                return _repository
                  .GetAlbums()
                  .Select(a => new AlbumViewModel(a))
                  .ToList();
            }).ContinueWith(r =>
            {
                if (!r.IsFaulted)
                    r.Result.ForEach(a => Albums.Add(a));
                else
                    throw r.Exception;
            }, TaskScheduler.FromCurrentSynchronizationContext());

        }
        #endregion Methods
    }
}
