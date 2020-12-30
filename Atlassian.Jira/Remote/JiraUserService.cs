﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Atlassian.Jira.Remote
{
    internal class JiraUserService : IJiraUserService
    {
        private readonly Jira _jira;

        public JiraUserService(Jira jira)
        {
            _jira = jira;
        }

        public async Task<JiraUser> CreateUserAsync(JiraUserCreationInfo user, CancellationToken token = default(CancellationToken))
        {
            var resource = "rest/api/2/user";
            var requestBody = JToken.FromObject(user);

            return await _jira.RestClient.ExecuteRequestAsync<JiraUser>(Method.POST, resource, requestBody, token).ConfigureAwait(false);
        }

        public Task DeleteUserAsync(string usernameOrAccountId, CancellationToken token = default(CancellationToken))
        {
            var queryString = _jira.RestClient.Settings.EnableUserPrivacyMode ? "accountId" : "username";
            var resource = String.Format($"rest/api/2/user?{queryString}={Uri.EscapeUriString(usernameOrAccountId)}");
            return _jira.RestClient.ExecuteRequestAsync(Method.DELETE, resource, null, token);
        }

        public Task<JiraUser> GetUserAsync(string usernameOrAccountId, CancellationToken token = default(CancellationToken))
        {
            var queryString = _jira.RestClient.Settings.EnableUserPrivacyMode ? "accountId" : "username";
            var resource = String.Format($"rest/api/2/user?{queryString}={Uri.EscapeUriString(usernameOrAccountId)}");
            return _jira.RestClient.ExecuteRequestAsync<JiraUser>(Method.GET, resource, null, token);
        }

        public Task<IEnumerable<JiraUser>> SearchUsersAsync(string query, JiraUserStatus userStatus = JiraUserStatus.Active, int maxResults = 50, int startAt = 0, CancellationToken token = default(CancellationToken))
        {
            var resource = String.Format(
                "rest/api/2/user/search?{0}={1}&includeActive={2}&includeInactive={3}&startAt={4}&maxResults={5}",
                _jira.RestClient.Settings.EnableUserPrivacyMode ? "query" : "username",
                Uri.EscapeUriString(query),
                userStatus.HasFlag(JiraUserStatus.Active),
                userStatus.HasFlag(JiraUserStatus.Inactive),
                startAt,
                maxResults);

            return _jira.RestClient.ExecuteRequestAsync<IEnumerable<JiraUser>>(Method.GET, resource, null, token);
        }

        public async Task<JiraUser> GetMyselfAsync(CancellationToken token = default(CancellationToken))
        {
            var resource = "rest/api/2/myself";
            var jiraUser = await _jira.RestClient.ExecuteRequestAsync<JiraUser>(Method.GET, resource, null, token);
            return jiraUser;
        }
    }
}
