using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Threading.Tasks;

namespace GPhotoSync
{
    public class MainViewModel : ViewModelBase, IViewModel
    {
        #region Fields
        private readonly IViewModelLocator _viewModelLocator;
        #endregion Fields

        #region Properties
        public RelayCommand LoginCommand { get; private set; }

        public RelayCommand ReloadCommand { get; private set; }

        private LoginViewModel _loginVIewModel;
        public LoginViewModel LoginViewModel
        {
            get { return _loginVIewModel; }
            set
            {
                _loginVIewModel = value;
                RaisePropertyChanged(() => LoginViewModel);
            }
        }

        public GPhotoViewModel GPhoto { get; set; }
        //public AlbumListViewModel AlbumList { get; private set; }
        #endregion Properties

        #region Ctor
        public MainViewModel(IMessenger messenger, IViewModelLocator viewModelLocator)
            : base(messenger)
        {
            if (viewModelLocator == null)
                throw new ArgumentNullException("viewModelLocator");
            _viewModelLocator = viewModelLocator;

            //AlbumList = _viewModelLocator.Locate<AlbumListViewModel>();
            LoginViewModel = _viewModelLocator.Locate<LoginViewModel>();
            GPhoto = _viewModelLocator.Locate<GPhotoViewModel>();
            //AlbumList.LoadAlbumsAsync();
            InitializeCommands();
        }


        #endregion Ctor

        #region Methods
        private void InitializeCommands()
        {
            LoginCommand = new RelayCommand(Login, CanLogin);
            ReloadCommand = new RelayCommand(Reload, CanReload);
        }

        public void TryLoadAlbums()
        {
            GPhoto.LoadAlbums()
                .ContinueWith(r =>
                {
                    if (r.IsFaulted)
                        Login();
                }, TaskScheduler.FromCurrentSynchronizationContext()); ;
        }

        private bool CanLogin() { return true; }

        public void Login()
        {
            if (CanLogin())
                LoginViewModel.IsVisible = true;
        }

        public bool CanReload() { return true; }

        public void Reload()
        {
            if (CanReload())
                GPhoto.LoadAlbums();
        }
        #endregion Methods
    }
}
