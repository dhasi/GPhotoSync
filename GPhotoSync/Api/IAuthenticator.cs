
namespace GPhotoSync
{
    public interface IAuthenticator
    {
        IClientCredentials GetCredentials();
    }

    public class OAuth2Authenticator : IAuthenticator
    {
        #region Fields
        public const string ClientId = "1061429631265.apps.googleusercontent.com";
        public const string ClientSecred = "Z7-FRZtdLsPJXaQORtZvTsVi";
        public const string RedirectUri = "urn:ietf:wg:oauth:2.0:oob";
        private readonly IClientCredentials _credentials;
        #endregion Fields

        #region Properties
        #endregion Properties

        #region Ctor
        public OAuth2Authenticator(IClientCredentials credentials)
        {
            _credentials = credentials;
        }
        #endregion Ctor

        #region Methods
        public IClientCredentials GetCredentials()
        {
            return _credentials;
        }
        #endregion Methods
    }
}
