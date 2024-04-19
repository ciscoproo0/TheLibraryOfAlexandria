using TheLibraryOfAlexandria.Utils;

namespace TheLibraryOfAlexandria.Services
{
    public interface IAuthenticationService
    {
        Task<ServiceResponse<string>> AuthenticateAsync(string username, string password);
    }
}

