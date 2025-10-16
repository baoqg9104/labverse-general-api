using FirebaseAdmin.Auth;

namespace Labverse.BLL.Interfaces;

public interface IFirebaseAuthService
{
    Task<FirebaseToken> VerifyIdTokenAsync(string idToken);
}
