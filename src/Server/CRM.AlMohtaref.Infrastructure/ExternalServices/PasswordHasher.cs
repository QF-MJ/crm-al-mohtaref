using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using CRM.AlMohtaref.Application.Interfaces;


namespace CRM.AlMohtaref.Infrastructure.ExternalServices
{
    public class PasswordHasher : IPasswordHasher
    {
        // OWASP recommended parameters for general web applications
        private const int DegreeOfParallelism = 4; // Number of threads
        private const int MemorySizeInKB = 65536;  // 64 MB RAM
        private const int Iterations = 3;          // Number of passes
        private const int HashLengthBytes = 32;    // Output size


        /// <summary>
        /// Hashes a password and returns a single self-contained string.
        /// </summary>
        public string HashPassword(string password)
        {
            // 1. Generate a cryptographically secure 16-byte random salt
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            // 2. Configure Argon2id
            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = DegreeOfParallelism;
                argon2.MemorySize = MemorySizeInKB;
                argon2.Iterations = Iterations;

                // 3. Compute the raw hash bytes
                byte[] hash = argon2.GetBytes(HashLengthBytes);

                // 4. Convert to Base64 to safely store as text
                string saltBase64 = Convert.ToBase64String(salt);
                string hashBase64 = Convert.ToBase64String(hash);

                // 5. Output a standard cryptographic string:
                // Format: $argon2id$v=19$m=65536,t=3,p=4$salt$hash
                return $"$argon2id$v=19$m={MemorySizeInKB},t={Iterations},p={DegreeOfParallelism}${saltBase64}${hashBase64}";
            }
        }

        /// <summary>
        /// Automatically extracts parameters from the stored string to verify a password.
        /// </summary>
        public bool VerifyPassword(string password, string storedHashString)
        {
            try
            {
                // Break down the combined string parts
                string[] parts = storedHashString.Split('$');
                if (parts.Length < 6 || parts[1] != "argon2id") return false;

                // Parse configurations dynamically (so old passwords still work if you change specs later)
                string[] configParts = parts[3].Split(',');
                int memory = int.Parse(configParts[0].Replace("m=", ""));
                int iterations = int.Parse(configParts[1].Replace("t=", ""));
                int parallelism = int.Parse(configParts[2].Replace("p=", ""));

                byte[] salt = Convert.FromBase64String(parts[4]);
                byte[] storedHash = Convert.FromBase64String(parts[5]);

                // Hash the incoming login attempt with the extracted configurations
                using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
                {
                    argon2.Salt = salt;
                    argon2.DegreeOfParallelism = parallelism;
                    argon2.MemorySize = memory;
                    argon2.Iterations = iterations;

                    byte[] computedHash = argon2.GetBytes(storedHash.Length);

                    // Fixed-time comparison protects against timing side-channel attacks
                    return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
                }
            }
            catch
            {
                return false; // Prevent application crashes if a database string is corrupted
            }
        }
    }

}

