using DICREP.EcommerceSubastas.Application.Interfaces;

namespace DICREP.EcommerceSubastas.Infrastructure.Services
{
    public class BCryptPasswordHasher : IPasswordHasher
    {
        public string Hash(string password) =>
            BCrypt.Net.BCrypt.HashPassword(password);

        public bool Verify(string password, string storedHash) =>
            BCrypt.Net.BCrypt.Verify(password, storedHash);
    }
}
