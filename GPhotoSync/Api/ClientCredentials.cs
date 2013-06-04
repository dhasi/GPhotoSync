using GPhotoSync.Properties;

namespace GPhotoSync
{
    public interface IClientCredentials
    {
        string AccessToken { get; set; }
        string User { get; set; }

        void Save();
        void Load();
    }

    public class ClientCredentials : IClientCredentials
    {
        #region Properties
        public string AccessToken { get; set; }

        public string User { get; set; }
        #endregion Properties

        #region Ctor
        public ClientCredentials()
        {
            Load();
        }
        #endregion Ctor

        #region Methods
        public void Save()
        {
            Settings.Default.Username = User;
            Settings.Default.AccessToken = AccessToken;
            Settings.Default.Save();
        }

        public void Load()
        {
            User = Settings.Default.Username;
            AccessToken = Settings.Default.AccessToken;
        }
        #endregion Methods
    }
}
