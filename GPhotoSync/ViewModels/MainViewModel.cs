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
        public GPhotoViewModel GPhoto { get; private set; }

        public IDialogManager DialogManager { get; private set; }
        #endregion Properties

        #region Ctor
        public MainViewModel(IMessenger messenger, IViewModelLocator viewModelLocator, IDialogManager dialogManager)
            : base(messenger)
        {
            if (viewModelLocator == null)
                throw new ArgumentNullException("viewModelLocator");
            if (dialogManager == null)
                throw new ArgumentNullException("dialogManager");

            _viewModelLocator = viewModelLocator;
            GPhoto = _viewModelLocator.Locate<GPhotoViewModel>();
            DialogManager = dialogManager;
        }


        #endregion Ctor

        #region Methods


        public async void TryLoadAlbums()
        {
            try
            {
                await GPhoto.LoadAlbums();
            }
            catch (Exception ex)
            {
                GPhoto.Login();
            }
            //GPhoto.LoadAlbums()
            //    .ContinueWith(r =>
            //    {
            //        if (r.IsFaulted)
            //            GPhoto.Login();
            //    }, TaskScheduler.FromCurrentSynchronizationContext()); ;
        }


        #endregion Methods
    }
}
