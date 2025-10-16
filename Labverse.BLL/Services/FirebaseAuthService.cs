using FirebaseAdmin.Auth;
using Labverse.BLL.Interfaces;

namespace Labverse.BLL.Services;

public class FirebaseAuthService : IFirebaseAuthService
{
    public async Task<FirebaseToken> VerifyIdTokenAsync(string idToken)
    {
        try
        {
            return await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
        }
        catch (Exception ex)
        {
            throw new UnauthorizedAccessException("Invalid Firebase token", ex);
        }
    }
}
