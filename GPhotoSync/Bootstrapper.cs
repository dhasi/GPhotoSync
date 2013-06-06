using Castle.Facilities.Startable;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using GalaSoft.MvvmLight.Messaging;
using System;

namespace GPhotoSync
{
    public class Bootstrapper
    {
        #region Fields
        private readonly App _app;
        private IWindsorContainer _container;
        #endregion Fields

        #region Properties
        #endregion Properties

        #region Ctor
        public Bootstrapper(App app)
        {
            if (app == null)
                throw new ArgumentNullException("app");

            _app = app;
        }
        #endregion Ctor

        #region Methods
        public void Run()
        {
            InitializeContainer();
            var viewModel = _container.Resolve<MainViewModel>();
            _app.MainWindow = new MainWindow { DataContext = viewModel };
            _app.MainWindow.Show();
        }

        private void InitializeContainer()
        {
            _container = new WindsorContainer();
            _container.AddFacility<TypedFactoryFacility>();
            _container.AddFacility<StartableFacility>();

            _container.Register(Component.For<IViewModelLocator>().AsFactory());
            _container.Register(Component.For<IMessenger>().ImplementedBy<Messenger>());
            _container.Register(Component.For<IViewManager>().ImplementedBy<ViewManager>().Start());
            _container.Register(Component.For<IAlbumRepository>().ImplementedBy<AlbumRepository>());
            _container.Register(Component.For<IAuthenticator>().ImplementedBy<OAuth2Authenticator>());
            _container.Register(Component.For<IClientCredentials>().ImplementedBy<ClientCredentials>());
            _container.Register(Component.For<IAlbumMappingRepository>().ImplementedBy<AlbumMappingRepository>());
            _container.Register(Component.For<IDialogManager>().ImplementedBy<DialogManager>());
            _container.Register(Component.For<IPhotoRepository>().ImplementedBy<PhotoRepository>());

            //Register ViewModels           
            _container.Register(Types
                .FromAssembly(GetType().Assembly)
                .BasedOn<IViewModel>()
                .LifestyleTransient());

            _container.Install(new SQLCENHibernateInstaller());
        }
        #endregion Methods
    }
}
