namespace TwitterStream.Interfaces
{
    public interface ITwitterConfig
    {
        string Api { get; }
        string OAuthConsumerKey { get; }
        string OAuthConsumerSecret { get; }
    }
}