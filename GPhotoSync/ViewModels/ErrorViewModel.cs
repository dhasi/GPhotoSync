using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace GPhotoSync
{
    public class ErrorViewModel : ViewModelBase
    {
        #region Fields
        #endregion Fields

        #region Properties
        public string Message { get; set; }
        #endregion Properties

        #region Ctor
        public ErrorViewModel(IMessenger messenger)
            : base(messenger)
        {
            InitializeCommands();
        }
        #endregion Ctor

        #region Methods
        private void InitializeCommands()
        {
            CloseCommand = new RelayCommand(Close);
        }

        public RelayCommand CloseCommand { get; private set; }

        public void Close()
        {
            MessengerInstance.Send(new CloseDialogMessage(this));
        }
        #endregion Methods
    }
}
