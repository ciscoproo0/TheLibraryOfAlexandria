using TheLibraryOfAlexandria.Utils;

namespace TheLibraryOfAlexandria.Services
{
    /// <summary>
    /// IAuthenticationService defines the contract for user authentication operations.
    /// This service handles user login and JWT token generation for secure API access.
    /// Implementation must handle password verification against bcrypt-hashed passwords and token creation.
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Authenticates a user by verifying their credentials and returns a JWT token.
        /// </summary>
        /// <param name="username">The username of the user attempting to authenticate.</param>
        /// <param name="password">The plaintext password to be verified against the stored bcrypt hash.</param>
        /// <returns>
        /// A ServiceResponse containing:
        /// - Success: JWT bearer token as string if authentication succeeds
        /// - Failure: Error message if user not found or password verification fails
        /// </returns>
        /// <remarks>
        /// Password verification uses bcrypt with a workFactor of 12 (4096 iterations) for cryptographic security.
        /// The returned JWT token is signed with HS256 algorithm and contains user identity claims.
        /// </remarks>
        Task<ServiceResponse<string>> AuthenticateAsync(string username, string password);
    }
}

