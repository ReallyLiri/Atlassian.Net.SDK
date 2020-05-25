﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira.Remote
{
    /// <summary>
    /// JsonConverter that deserializes a JSON user into a JiraUser object and serializes a JiraUser object
    /// into a single identifier.
    /// </summary>
    public class JiraUserJsonConverter : JsonConverter
    {
        /// <summary>
        /// Whether user privacy mode is enabled (uses 'accountId' insead of 'name' for serialization).
        /// </summary>
        public bool UserPrivacyEnabled { get; set; }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(JiraUser);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var remoteUser = serializer.Deserialize<RemoteJiraUser>(reader);
            return new JiraUser()
            {
                AccountId = remoteUser.accountId,
                DisplayName = remoteUser.displayName,
                Email = remoteUser.emailAddress,
                IsActive = remoteUser.active,
                Key = remoteUser.key,
                Locale = remoteUser.locale,
                Self = remoteUser.self,
                Username = remoteUser.name,
                InternalIdentifier = UserPrivacyEnabled ? remoteUser.accountId : remoteUser.name,
                AvatarUrl = remoteUser.avatarUrls.Values.FirstOrDefault()
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var user = value as JiraUser;

            if (user != null)
            {
                var outerObject = new JObject(new JProperty(
                    UserPrivacyEnabled ? "accountId" : "name",
                    user.InternalIdentifier));

                outerObject.WriteTo(writer);
            }
        }
    }
}
