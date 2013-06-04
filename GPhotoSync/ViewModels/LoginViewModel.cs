using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Google.GData.Client;
using Google.GData.Photos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GPhotoSync
{
    public class LoginViewModel : ViewModelBase, IViewModel
    {
        #region Fields
        private readonly IClientCredentials _clientCredentials;
        #endregion Fields

        #region Properties
        private bool _isVisible;
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                RaisePropertyChanged(() => IsVisible);
            }
        }

        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                RaisePropertyChanged(() => UserName);
            }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                RaisePropertyChanged(() => Password);
            }
        }
        #endregion Properties

        #region Ctor
        public LoginViewModel(IMessenger messenger, IClientCredentials credentials)
            : base(messenger)
        {
            _clientCredentials = credentials;
            InitializeCommands();
            IsVisible = false;
        }
        #endregion Ctor

        #region Methods
        private void InitializeCommands()
        {
            LoginCommand = new RelayCommand(Login, CanLogin);
        }

        public RelayCommand LoginCommand { get; private set; }

        public bool CanLogin() { return !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password); }

        public void Login()
        {
            if (!CanLogin()) return;

            var service = new PicasaService("GPhotoSync");
            service.setUserCredentials(UserName, Password);

            _clientCredentials.AccessToken = service.QueryClientLoginToken();
            _clientCredentials.User = UserName;
            _clientCredentials.Save();

            MessengerInstance.Send<LoginMessage>(new LoginMessage());
            IsVisible = false;
        }
        #endregion Methods
    }
}
