using TwitterStream.Interfaces;

namespace TwitterStream.Entities
{
    public class TwitterConfig: ITwitterConfig
    {
        public string Api { get; }
        public string OAuthConsumerKey { get; }
        public string OAuthConsumerSecret { get; }

        public TwitterConfig(string api, string oauthConsumerKey, string oauthConsumerSecret)
        {
            Api = api;
            OAuthConsumerKey = oauthConsumerKey;
            OAuthConsumerSecret = oauthConsumerSecret;
        }
    }
}