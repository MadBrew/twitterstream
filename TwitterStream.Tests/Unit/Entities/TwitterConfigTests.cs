using TwitterStream.Entities;

namespace TwitterStream.Tests.Entities
{
    internal class TwitterConfigTests
    {
        [Test]
        public void TwitterConfig_Returns_PopulatedConfigObject()
        {
            // Arrange
            const string api = "api";
            const string key = "key";
            const string secret = "secret";

            // Act.
            var objectUnderTest = new TwitterConfig(api, key, secret);

            // Assert.
            Assert.Multiple(() =>
            {
                Assert.That(objectUnderTest.Api, Is.EqualTo(api));
                Assert.That(objectUnderTest.OAuthConsumerKey, Is.EqualTo(key));
                Assert.That(objectUnderTest.OAuthConsumerSecret, Is.EqualTo(secret));
            });
        }
    }
}