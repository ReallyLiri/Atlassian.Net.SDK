﻿namespace Atlassian.Jira.OAuth
{
    /// <summary>
    /// Access token settings to help obtain the access token.
    /// </summary>
    public class OAuthAccessTokenSettings
    {
        public string Url;
        public string ConsumerKey;
        public string ConsumerSecret;
        public string OAuthRequestToken;
        public string OAuthTokenSecret;
        public JiraOAuthSignatureMethod SignatureMethod;
        public string AccessTokenUrl;

        /// <summary>
        /// Creates a Access token settings to obtain the access token.
        /// </summary>
        /// <param name="url">The url of the Jira instance to request to.</param>
        /// <param name="consumerKey">The consumer key provided by the Jira application link.</param>
        /// <param name="consumerSecret">The consumer public key in XML format.</param>
        /// <param name="oAuthRequestToken">The OAuth request token generated by Jira.</param>
        /// <param name="oAuthTokenSecret">The OAuth token secret generated by Jira.</param>
        /// <param name="signatureMethod">The signature method used to sign the request.</param>
        /// <param name="accessTokenUrl">The relative path to the url to request the access token to Jira.</param>
        public OAuthAccessTokenSettings(
            string url,
            string consumerKey,
            string consumerSecret,
            string oAuthRequestToken,
            string oAuthTokenSecret,
            JiraOAuthSignatureMethod signatureMethod = JiraOAuthSignatureMethod.RsaSha1,
            string accessTokenUrl = "plugins/servlet/oauth/access-token")
        {
            Url = url;
            ConsumerKey = consumerKey;
            ConsumerSecret = consumerSecret;
            OAuthRequestToken = oAuthRequestToken;
            OAuthTokenSecret = oAuthTokenSecret;
            SignatureMethod = signatureMethod;
            AccessTokenUrl = accessTokenUrl;
        }

        /// <summary>
        /// Creates a Access token settings to obtain the access token.
        /// </summary>
        /// <param name="oAuthRequestTokenSettings">The settings used to generate the request token.</param>
        /// <param name="oAuthRequestToken">The request token object returned by <see cref="OAuthTokenHelper.GenerateRequestToken"/>.</param>
        /// <param name="accessTokenUrl">The relative path to the url to request the access token to Jira.</param>
        public OAuthAccessTokenSettings(
            OAuthRequestTokenSettings oAuthRequestTokenSettings,
            OAuthRequestToken oAuthRequestToken,
            string accessTokenUrl = "plugins/servlet/oauth/access-token")
        {
            Url = oAuthRequestTokenSettings.Url;
            ConsumerKey = oAuthRequestTokenSettings.ConsumerKey;
            ConsumerSecret = oAuthRequestTokenSettings.ConsumerSecret;
            SignatureMethod = oAuthRequestTokenSettings.SignatureMethod;
            OAuthRequestToken = oAuthRequestToken.OAuthToken;
            OAuthTokenSecret = oAuthRequestToken.OAuthTokenSecret;
            AccessTokenUrl = accessTokenUrl;
        }
    }
}
