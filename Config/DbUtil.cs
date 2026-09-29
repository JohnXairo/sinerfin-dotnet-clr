using Sinerfin.Models;

namespace Sinerfin.Config
{
    public static class DbUtil
    {
        public static string T(DbProvider provider, string tabla) =>
            provider == DbProvider.ORACLE ? tabla.ToUpper() : tabla.ToLower();

        public static string Param(DbProvider provider, string name) => "@" + name;
    }
}
