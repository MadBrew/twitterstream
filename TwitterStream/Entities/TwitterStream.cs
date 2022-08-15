using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using Newtonsoft.Json.Linq;
using TwitterStream.Interfaces;

namespace TwitterStream.Entities
{
    internal static class TwitterStream
    {
        public static IEnumerable<string> StreamStatuses(ITwitterConfig config)
        {
            TextReader streamReader = ReadTweets(config);

            if (streamReader == StreamReader.Null)
            {
                throw new Exception("Could not connect to twitter with credentials provided");
            }

            while (true)
            {
                string? line = null;

                try
                {
                    line = streamReader.ReadLine();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Ignoring :{e}");
                }

                if (!string.IsNullOrWhiteSpace(line))
                {
                    yield return line;
                }

                // Reconnect to the Twitter feed.
                if (line == null)
                {
                    streamReader = ReadTweets(config);
                }
            }
        }

        private static string GetBearerToken(string key, string secret)
        {
            var bearerRequest = HttpUtility.UrlEncode(key) + ":" + HttpUtility.UrlEncode(secret);
            bearerRequest = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(bearerRequest));
            // TODO: Refactor to HttpClient.
            var request = WebRequest.Create("https://api.twitter.com/oauth2/token");
            request.Headers.Add("Authorization", "Basic " + bearerRequest);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
            var requestContent = System.Text.Encoding.UTF8.GetBytes("grant_type=client_credentials");
            var requestStream = request.GetRequestStream();
            requestStream.Write(requestContent, 0, requestContent.Length);
            requestStream.Close();
            var responseJson = string.Empty;
            var response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseStream = response.GetResponseStream();
                responseJson = new StreamReader(responseStream).ReadToEnd();
            }

            var jobjectResponse = JObject.Parse(responseJson);
            return jobjectResponse["access_token"].ToString();
        }

        private static TextReader ReadTweets(ITwitterConfig config)
        {
            ServicePointManager.Expect100Continue = false;
            // TODO: Refactor to HttpClient.
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(config.Api);
            request.Headers.Add("Authorization", "Bearer " + GetBearerToken(config.OAuthConsumerKey, config.OAuthConsumerSecret));
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            request.PreAuthenticate = true;
            request.AllowWriteStreamBuffering = true;
            request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.BypassCache);
            var tresponse = request.GetResponseAsync();

            // Bail out and retry after 5 seconds.
            if (tresponse.Wait(5000))
            {
                return new StreamReader(tresponse.Result.GetResponseStream());
            }
            else
            {
                request.Abort();
                return StreamReader.Null;
            }
        }
    }
}