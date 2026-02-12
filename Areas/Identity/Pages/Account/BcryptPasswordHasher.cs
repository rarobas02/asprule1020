
using Microsoft.AspNetCore.Identity;
using BCrypt.Net;

namespace asprule1020.Areas.Identity.Pages.Account
{
    public class BcryptPasswordHasher<TUser> : IPasswordHasher<TUser> where TUser : class
    {
        private readonly int _workFactor;

        public BcryptPasswordHasher(int workFactor = 12)
        {
            _workFactor = workFactor; // 10–14 typical
        }

        public string HashPassword(TUser user, string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, _workFactor);
        }

        public PasswordVerificationResult VerifyHashedPassword(
            TUser user,
            string hashedPassword,
            string providedPassword)
        {
            if (BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword))
            {
                // Optional: rehash if work factor increased
                if (BCrypt.Net.BCrypt.PasswordNeedsRehash(hashedPassword, _workFactor))
                {
                    return PasswordVerificationResult.SuccessRehashNeeded;
                }

                return PasswordVerificationResult.Success;
            }

            return PasswordVerificationResult.Failed;
        }
    }
}
