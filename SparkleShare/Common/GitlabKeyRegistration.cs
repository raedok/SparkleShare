using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace SparkleShare
{
    public class GitlabKeyRegistration
    {
        public static void RegisterKey(string urlRoot, string username, string password, string key)
        {
            if (urlRoot.IndexOf("://") < 0) urlRoot = "https://" + urlRoot;
            var cookies = new CookieContainer();
            var loginPage = Get(urlRoot + "/users/sign_in", cookies);
            var token = getAuthenticityToken(loginPage);
            var loginResult = Post(urlRoot + "/users/sign_in", buildFormContent(new[] {
                Tuple.Create("utf8", "✓"),
                Tuple.Create("authenticity_token", token),
                Tuple.Create("user[login]", username),
                Tuple.Create("user[password]", password),
                Tuple.Create("user[remember_me]", "0"),
            }), "application/x-www-form-urlencoded", cookies);

            if (parseLoginResult(loginResult))
            {
                var keysPage = Get(urlRoot + "/profile/keys/", cookies);
                token = getAuthenticityToken(keysPage);
                var keyTitle = key.Substring(key.IndexOf(" ", key.IndexOf(" ") + 1) + 1);
                var keysResult = Post(urlRoot + "/profile/keys/", buildFormContent(new[] {
                    Tuple.Create("utf8", "✓"),
                    Tuple.Create("authenticity_token", token),
                    Tuple.Create("key[key]", key),
                    Tuple.Create("key[title]", keyTitle),
                }), "application/x-www-form-urlencoded", cookies);

                if (!keysResult.Contains(keyTitle))
                {
                    throw new Exception("Key add failed.");
                }
            }
            else
            {
                throw new Exception("Login failed.");
            }
        }

        private static bool parseLoginResult(string loginResult)
        {
            try
            {
                getAuthenticityToken(loginResult);
                return false;
            }
            catch
            {
                return true;
            }
        }

        private static byte[] buildFormContent(IEnumerable<Tuple<string, string>> kvps)
        {
            StringBuilder builder = new StringBuilder();
            var glue = "";
            foreach (var kvp in kvps)
            {
                builder.Append(glue).Append(HttpUtility.UrlEncode(kvp.Item1)).Append("=").Append(HttpUtility.UrlEncode(kvp.Item2));
                glue = "&";
            }
            return Encoding.UTF8.GetBytes(builder.ToString());
        }

        private static string getAuthenticityToken(string page)
        {
            var key = @"<input type=""hidden"" name=""authenticity_token"" value=""";
            var index = page.IndexOf(key);
            if (index < 0) throw new Exception("Authenticity token missing from web page!");
            var tokenStart = page.Substring(index + key.Length);
            return tokenStart.Substring(0, tokenStart.IndexOf("\""));
        }

        private static string Get(string url, CookieContainer container)
        {
            var request = WebRequest.Create(url) as HttpWebRequest;
            request.CookieContainer = container;
            var response = request.GetResponse();
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
                return reader.ReadToEnd();
        }

        private static string Post(string url, byte[] data, string contentType, CookieContainer container)
        {
            var request = WebRequest.Create(url) as HttpWebRequest;
            request.CookieContainer = container;
            request.ContentType = contentType;
            request.ContentLength = data.Length;
            request.Method = "POST";
            using (var stream = request.GetRequestStream())
                stream.Write(data, 0, data.Length);
            var response = request.GetResponse();
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
                return reader.ReadToEnd();
        }
    }
}
