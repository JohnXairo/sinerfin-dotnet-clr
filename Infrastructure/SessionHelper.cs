using System;
using System.Text;
using Microsoft.Owin;

namespace Sinerfin.Infrastructure
{
    public static class SessionHelper
    {
        private const string CookieName = "sinerfin_sess";

        public static void SetSession(IOwinResponse response, string user, string dbType)
        {
            var value = Convert.ToBase64String(
                Encoding.UTF8.GetBytes("user=" + Encode(user) + "|dbType=" + Encode(dbType)));
            response.Cookies.Append(CookieName, value,
                new CookieOptions { HttpOnly = true, Path = "/" });
        }

        public static void ClearSession(IOwinResponse response)
        {
            // CookieOptions.Expires is DateTime? in Microsoft.Owin 4.x
            response.Cookies.Append(CookieName, "",
                new CookieOptions { HttpOnly = true, Path = "/",
                    Expires = DateTime.UtcNow.AddDays(-1) });
        }

        public static bool TryGetSession(IOwinRequest request, out string user, out string dbType)
        {
            user = null; dbType = null;
            var raw = request.Cookies[CookieName];
            if (string.IsNullOrEmpty(raw)) return false;
            try
            {
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(raw));
                foreach (var part in decoded.Split('|'))
                {
                    var idx = part.IndexOf('=');
                    if (idx < 0) continue;
                    var k = part.Substring(0, idx);
                    var v = Decode(part.Substring(idx + 1));
                    if (k == "user")   user   = v;
                    if (k == "dbType") dbType = v;
                }
                return !string.IsNullOrEmpty(user);
            }
            catch { return false; }
        }

        private static string Encode(string s) => s == null ? "" : s.Replace("|", "%7C").Replace("=", "%3D");
        private static string Decode(string s) => s == null ? "" : s.Replace("%7C", "|").Replace("%3D", "=");
    }
}
