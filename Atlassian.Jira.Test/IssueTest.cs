﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Atlassian.Jira.Linq;
using Moq;

namespace Atlassian.Jira.Test
{
    public class IssueTest
    {
        [Fact]
        public void Constructor_ShouldSetDefaultValues()
        {
            var issue = new Issue();

            Assert.Null(issue.DueDate);
        }

        [Fact]
        public void ToLocal_IfFieldsSet_ShouldPopulateFields()
        {
            var remoteIssue = new RemoteIssue()
            {
                created = new DateTime(2011, 1, 1),
                updated = new DateTime(2011, 2, 2),
                duedate = new DateTime(2011, 3, 3),
                priority = "High",
                resolution = "Open",
                key = "key1"
            };

            var issue = remoteIssue.ToLocal();

            Assert.Equal(new DateTime(2011, 1, 1), issue.Created);
            Assert.Equal(new DateTime(2011, 2, 2), issue.Updated);
            Assert.Equal(new DateTime(2011, 3, 3), issue.DueDate);
            Assert.Equal("High", issue.Priority.Value);
            Assert.Equal("Open", issue.Resolution.Value);
            Assert.Equal("key1", issue.Key.Value);
        }

        [Fact]
        public void ToLocal_IfFieldsNotSet_ShouldNotPopulateFields()
        {
            var remoteIssue = new RemoteIssue();

            var issue = remoteIssue.ToLocal();

            Assert.Null(issue.Created);
            Assert.Null(issue.Updated);
            Assert.Null(issue.DueDate);
            Assert.Null(issue.Priority);
            Assert.Null(issue.Resolution);
            Assert.Null(issue.Key);
        }

        [Fact]
        public void ToRemote_IfFieldsNotSet_ShouldLeaveFieldsNull()
        {
            var issue = new Issue()
            {
                Project = "TST",
                Type = "1",
                Summary = "Summary"
            };

            var remoteIssue = issue.ToRemote();

            Assert.Null(remoteIssue.priority);
            Assert.Null(remoteIssue.key);
            Assert.Null(remoteIssue.resolution);
        }

        [Fact]
        public void ToRemote_IfFieldsSet_ShouldPopulateFields()
        {
            var remoteIssue = new RemoteIssue()
            {
                created = new DateTime(2011, 1, 1),
                updated = new DateTime(2011, 2, 2),
                duedate = new DateTime(2011, 3, 3),
                priority = "High",
                resolution = "Open",
                key = "key1"
            };

            var newRemoteIssue = remoteIssue.ToLocal().ToRemote();

            Assert.Null(newRemoteIssue.created);
            Assert.Null(newRemoteIssue.updated);
            Assert.Null(newRemoteIssue.duedate);
            Assert.Equal("High", newRemoteIssue.priority);
            Assert.Equal("Open", newRemoteIssue.resolution);
            Assert.Equal("key1", newRemoteIssue.key);
        }

        [Fact]
        public void GetUpdatedFields_ReturnEmptyIfNothingChanged()
        {
            var issue = new Issue();

            Assert.Equal(0, issue.GetUpdatedFields().Length);
        }

        [Fact]
        public void GetUpdatedFields_IfString_ReturnOneFieldThatChanged()
        {
            var issue = new Issue();
            issue.Summary = "foo";

            Assert.Equal(1, issue.GetUpdatedFields().Length);
        }

        [Fact]
        public void GetUpdatedFields_IfString_ReturnAllFieldsThatChanged()
        {
            var issue = new Issue();
            issue.Summary = "foo";
            issue.Description = "foo";
            issue.Assignee = "foo";
            issue.Environment = "foo";
            issue.Project = "foo";
            issue.Reporter = "foo";
            issue.Status = "foo";
            issue.Type = "foo";

            Assert.Equal(8, issue.GetUpdatedFields().Length);
        }

        [Fact]
        public void GetUpdateFields_IfStringEqual_ReturnNoFieldsThatChanged()
        {
            var remoteIssue = new RemoteIssue()
            {
                summary = "Summary"
            };

            var issue = remoteIssue.ToLocal();

            issue.Summary = "Summary";
            issue.Status = null;

            Assert.Equal(0, issue.GetUpdatedFields().Length);
        }

        [Fact]
        public void GetUpdateFields_IfComparableEqual_ReturnNoFieldsThatChanged()
        {
            var remoteIssue = new RemoteIssue()
            {
                priority = "High",
            };

            var issue = remoteIssue.ToLocal();

            issue.Priority = "High";
            issue.Resolution = null;

            Assert.Equal(0, issue.GetUpdatedFields().Length);
        }

        [Fact]
        public void GetUpdateFields_IfComparable_ReturnsFieldsThatChanged()
        {
            var issue = new Issue();
            issue.Priority = "High";

            Assert.Equal(1, issue.GetUpdatedFields().Length);
            
        }

        [Fact]
        public void GetUpdateFields_IfDateTimeChanged_ReturnsFieldsThatChanged()
        {
            var issue = new Issue(){ DueDate = new DateTime(2011,10,10) };

            var fields = issue.GetUpdatedFields();
            Assert.Equal(1, fields.Length);
            Assert.Equal("10/Oct/11", fields[0].values[0]);
        }

        [Fact]
        public void GetUpdateFields_IfDateTimeUnChangd_ShouldNotIncludeItInFieldsThatChanged()
        {
            var remoteIssue = new RemoteIssue()
            {
                duedate = new DateTime(2011,1,1)
            };

            var issue = remoteIssue.ToLocal();
            Assert.Equal(0, issue.GetUpdatedFields().Length);
        }

        [Fact]
        public void GetAttachments_IfIssueNotCreated_ShouldThrowException()
        {
            var issue = new Issue();

            Assert.Throws(typeof(InvalidOperationException), () => issue.GetAttachments());
            
        }

        [Fact]
        public void GetAttachments_IfIssueIsCreatedAndHasAttachments_ShouldLoadAttachments()
        {
            //arrange
            var mockJiraService = new Mock<IJiraSoapServiceClient>();
            mockJiraService.Setup(j => j.Login("user", "pass")).Returns("thetoken");
            mockJiraService.Setup(j => j.GetAttachmentsFromIssue("thetoken", "key"))
                .Returns(new RemoteAttachment[1] { new RemoteAttachment() { filename = "attach.txt" } });
            
            var jira = new Jira(null, mockJiraService.Object, null, "user", "pass");
            var issue = (new RemoteIssue() { key = "key" }).ToLocal(jira);

            //act
            var attachments = issue.GetAttachments();

            //assert
            Assert.Equal(1, attachments.Count);
            Assert.Equal("attach.txt", attachments[0].FileName);
        }

        [Fact]
        public void UploadAttachment_IfIssueNotCreated_ShouldThrowAnException()
        {
            var issue = new Issue();

            Assert.Throws(typeof(InvalidOperationException), () => issue.UploadAttachments("foo"));
        }

        [Fact]
        public void Upload_IfIssueCreated_ShouldUpload()
        {
            //arrange
            var mockJiraService = new Mock<IJiraSoapServiceClient>();
            mockJiraService.Setup(j => j.Login("user", "pass")).Returns("token");
            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(f => f.FileReadAllBytes("foo.txt")).Returns(new byte[] { 1, 2, 3 });

            var jira = new Jira(null, mockJiraService.Object, mockFileSystem.Object, "user", "pass");
            var issue = (new RemoteIssue() { key = "key" }).ToLocal(jira);

            //act
            issue.UploadAttachments("foo.txt");

            //assert
            // TODO: Ugly assert, should use a proper mock to increase readability
            mockJiraService.Verify(j => j.addBase64EncodedAttachmentsToIssue(
                                                "token",
                                                "key",
                                                new string[] { "foo.txt" },
                                                new string[] { "AQID" }));
        }
    }
}
