namespace Sinerfin.Config
{
    /// <summary>
    /// Utilidad BCrypt identica a la version .NET Core.
    /// Normaliza prefijos $2b$/$2y$ -> $2a$ para compatibilidad
    /// con hashes generados por Java (jbcrypt) y Python/Node.
    /// </summary>
    public static class PasswordUtil
    {
        private const int WorkFactor = 12;

        private static string Normalize(string hash)
        {
            if (string.IsNullOrEmpty(hash)) return hash;
            var h = hash.Trim();
            if (h.StartsWith("$2b$") || h.StartsWith("$2y$"))
                return "$2a$" + h.Substring(4);
            return h;
        }

        public static string Hash(string plainPassword) =>
            BCrypt.Net.BCrypt.HashPassword(plainPassword, WorkFactor);

        public static bool IsValidBcryptFormat(string storedHash)
        {
            if (string.IsNullOrWhiteSpace(storedHash)) return false;
            var n = Normalize(storedHash.Trim());
            return n.Length == 60 && n.StartsWith("$2a$");
        }

        public static bool Verify(string plainPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(plainPassword) || string.IsNullOrWhiteSpace(storedHash))
                return false;

            var trimmed    = storedHash.Trim();
            var normalized = Normalize(trimmed);

            try { if (BCrypt.Net.BCrypt.Verify(plainPassword, normalized)) return true; } catch { }

            if (trimmed != normalized)
                try { return BCrypt.Net.BCrypt.Verify(plainPassword, trimmed); } catch { }

            return false;
        }
    }
}
