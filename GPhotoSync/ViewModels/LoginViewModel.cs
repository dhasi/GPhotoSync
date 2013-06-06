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

        public Action OnLogin { get; set; }
        #endregion Properties

        #region Ctor
        public LoginViewModel(IMessenger messenger, IClientCredentials credentials)
            : base(messenger)
        {
            _clientCredentials = credentials;
            InitializeCommands();
        }
        #endregion Ctor

        #region Methods
        private void InitializeCommands()
        {
            LoginCommand = new RelayCommand(Login, CanLogin);
            CancelCommand = new RelayCommand(Cancel);
        }

        public RelayCommand LoginCommand { get; private set; }

        public bool CanLogin() { return !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password); }

        public void Login()
        {
            if (!CanLogin()) return;

            try
            {
                var service = new PicasaService("GPhotoSync");
                service.setUserCredentials(UserName, Password);

                _clientCredentials.AccessToken = service.QueryClientLoginToken();
                _clientCredentials.User = UserName;
                _clientCredentials.Save();

                MessengerInstance.Send(new CloseDialogMessage(this));
                if (OnLogin != null)
                    OnLogin();
            }
            catch (Exception ex)
            {
                MessengerInstance.Send(new ShowDialogMessage
                {
                    Content = new ErrorViewModel(MessengerInstance)
                    {
                        Message = ex.Message
                    }
                });
            }
        }

        public RelayCommand CancelCommand { get; private set; }

        public void Cancel()
        {
            MessengerInstance.Send(new CloseDialogMessage(this));
        }
        #endregion Methods
    }
}
