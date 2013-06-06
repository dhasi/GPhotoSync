using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GPhotoSync
{
    public class AlbumMappingViewModel : ViewModelBase
    {
        #region Fields
        private readonly AlbumMapping _mapping;
        #endregion Fields

        #region Properties
        private string _path;
        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                RaisePropertyChanged(() => Path);
            }
        }
        #endregion Properties

        #region Ctor
        public AlbumMappingViewModel(AlbumMapping mapping, IMessenger messenger)
            : base(messenger)
        {
            if (mapping == null)
                throw new ArgumentNullException("mapping");
            _mapping = mapping;
            Path = _mapping.LocalPath;
            InitializeCommands();
        }
        #endregion Ctor

        #region Methods
        private void InitializeCommands()
        {
            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
            SelectPathCommand = new RelayCommand(SelectPath);
        }

        public RelayCommand SelectPathCommand { get; private set; }

        public void SelectPath()
        {
            var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Path = dlg.SelectedPath;
            }
        }

        public RelayCommand SaveCommand { get; private set; }

        public bool CanSave() { return true; }

        public void Save()
        {
            if (!CanSave()) return;

            _mapping.LocalPath = Path;
            MessengerInstance.Send(new CloseDialogMessage(this));
            //try
            //{
            //    var service = new PicasaService("GPhotoSync");
            //    service.setUserCredentials(UserName, Password);

            //    _clientCredentials.AccessToken = service.QueryClientLoginToken();
            //    _clientCredentials.User = UserName;
            //    _clientCredentials.Save();

            //    MessengerInstance.Send(new CloseDialogMessage(this));
            //}
            //catch (Exception ex)
            //{
            //    MessengerInstance.Send(new ShowDialogMessage
            //    {
            //        Content = new ErrorViewModel(MessengerInstance)
            //        {
            //            Message = ex.Message
            //        }
            //    });
            //}
        }

        public RelayCommand CancelCommand { get; private set; }

        public void Cancel()
        {
            MessengerInstance.Send(new CloseDialogMessage(this));
        }
        #endregion Methods
    }
}
