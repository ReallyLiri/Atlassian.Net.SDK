﻿using Atlassian.Jira.Remote;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Atlassian.Jira
{
    /// <summary>
    /// Collection of custom fields
    /// </summary>
    public class CustomFieldValueCollection : ReadOnlyCollection<CustomFieldValue>, IRemoteIssueFieldProvider
    {
        private readonly Issue _issue;
        private Func<string, string> _getFieldIdProvider;

        internal CustomFieldValueCollection(Issue issue)
            : this(issue, new List<CustomFieldValue>())
        {
        }

        internal CustomFieldValueCollection(Issue issue, IList<CustomFieldValue> list)
            : base(list)
        {
            _issue = issue;

            // By default collection operates for edit custom fields.
            ForEdit();
        }

        /// <summary>
        /// Add a custom field by name
        /// </summary>
        /// <param name="fieldName">The name of the custom field as defined in JIRA</param>
        /// <param name="fieldValue">The value of the field</param>
        public CustomFieldValueCollection Add(string fieldName, string fieldValue)
        {
            return this.Add(fieldName, new string[] { fieldValue });
        }

        /// <summary>
        /// Add a custom field by name with an array of values
        /// </summary>
        /// <param name="fieldName">The name of the custom field as defined in JIRA</param>
        /// <param name="fildValues">The values of the field</param>
        public CustomFieldValueCollection AddArray(string fieldName, params string[] fieldValues)
        {
            return this.Add(fieldName, fieldValues);
        }

        /// <summary>
        /// Add a custom field by name
        /// </summary>
        /// <param name="fieldName">The name of the custom field as defined in JIRA</param>
        /// <param name="fieldValues">The values of the field</param>
        public CustomFieldValueCollection Add(string fieldName, string[] fieldValues)
        {
            var fieldId = _getFieldIdProvider(fieldName);
            this.Items.Add(new CustomFieldValue(fieldId, fieldName, _issue) { Values = fieldValues });
            return this;
        }

        /// <summary>
        /// Gets a custom field by name
        /// </summary>
        /// <param name="fieldName">Name of the custom field as defined in JIRA</param>
        /// <returns>CustomField instance if the field has been set on the issue, null otherwise</returns>
        public CustomFieldValue this[string fieldName]
        {
            get
            {
                var fieldId = _getFieldIdProvider(fieldName);
                return this.Items.FirstOrDefault(f => f.Id == fieldId);
            }
        }

        /// <summary>
        /// Changes context of collection to operate against fields for edit.
        /// </summary>
        /// <returns>Current collection with changed context/</returns>
        public CustomFieldValueCollection ForEdit()
        {
            _getFieldIdProvider = fieldName =>
            {
                var customField = _issue.Jira.GetFieldsForEdit(_issue)
                    .FirstOrDefault(f => f.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

                if (customField == null)
                {
                    throw new InvalidOperationException(String.Format("Could not find custom field with name '{0}' on the JIRA server. "
                        + "Make sure this field is available when editing this issue. For more information see JRA-6857", fieldName));
                }

                return customField.Id;
            };

            return this;
        }

        /// <summary>
        /// Changes context of collection to operate against fields for action.
        /// </summary>
        /// <param name="actionId">Id of action as defined in JIRA.</param>
        /// <returns>Current collection with changed context/</returns>
        public CustomFieldValueCollection ForAction(string actionId)
        {
            _getFieldIdProvider = name =>
            {
                var customField = _issue.Jira.GetFieldsForAction(_issue, actionId)
                    .FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                if (customField == null)
                {
                    throw new InvalidOperationException(
                        String.Format(
                        CultureInfo.InvariantCulture,
                        "Could not find custom field with name '{0}' and action with id '{1}' on the JIRA server. ",
                        name,
                        actionId));
                }

                return customField.Id;
            };

            return this;
        }

        RemoteFieldValue[] IRemoteIssueFieldProvider.GetRemoteFields()
        {
            return this.Items.Select(f => new RemoteFieldValue()
            {
                id = f.Id,
                values = f.Values
            }).ToArray();
        }
    }
}