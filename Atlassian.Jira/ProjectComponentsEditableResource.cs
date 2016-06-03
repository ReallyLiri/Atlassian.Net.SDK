using Atlassian.Jira.Remote;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Atlassian.Jira
{
    /// Class that encapsulates operations on the remote components collection of a project.

    public class ProjectComponentsEditableResource
    {
        private readonly Project _project;
        private readonly Jira _jira;

        /// <summary>
        /// Creates a new instance of the ProjectComponentsEditableResource.
        /// </summary>
        /// <param name="jira">Instance of the Jira client.</param>
        /// <param name="project">The project on which to target the operations.</param>
        public ProjectComponentsEditableResource(Jira jira, Project project)
        {
            _jira = jira;
            _project = project;
        }

        /// <summary>
        /// Gets the components for the current project.
        /// </summary>
        public IEnumerable<ProjectComponent> Get()
        {
            return _jira.GetProjectComponents(_project.Key);
        }

        /// <summary>
        /// Gets the components for the current project.
        /// </summary>
        /// <param name="token">Cancellation token for this operation.</param>
        public Task<IEnumerable<ProjectComponent>> GetAsync(CancellationToken token = default(CancellationToken))
        {
            var resource = String.Format("rest/api/2/project/{0}/components", _project.Key);
            return _jira.RestClient.ExecuteRequestAsync<RemoteComponent[]>(Method.GET, resource).ContinueWith(task =>
            {
                var components = task.Result.Select(remoteComponent => new ProjectComponent(remoteComponent));
                _jira.Cache.Components.AddIfMIssing(new JiraEntityDictionary<ProjectComponent>(_project.Key, components));

                return components;
            }, token, TaskContinuationOptions.None, TaskScheduler.Default);
        }

        /// <summary>
        /// Creates and adds a new component the current project.
        /// </summary>
        /// <param name="projectComponent">Information of the new component.</param>
        public ProjectComponent Add(ProjectComponentCreationInfo projectComponent)
        {
            return ExecuteAndGuard(() => AddAsync(projectComponent).Result);
        }

        /// <summary>
        /// Creates and adds a new component to a project.
        /// </summary>
        /// <param name="projectComponent">Information of the new component.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        public Task<ProjectComponent> AddAsync(ProjectComponentCreationInfo projectComponent, CancellationToken token = default(CancellationToken))
        {
            var settings = _jira.RestClient.GetSerializerSettings();
            var serializer = JsonSerializer.Create(settings);
            var resource = "/rest/api/2/component";
            var requestBody = JToken.FromObject(projectComponent, serializer);

            requestBody["project"] = _project.Key;

            return _jira.RestClient.ExecuteRequestAsync<RemoteComponent>(Method.POST, resource, requestBody, token).ContinueWith(task =>
            {
                var component = new ProjectComponent(task.Result);
                var cacheEntry = new JiraEntityDictionary<ProjectComponent>(_project.Key, new ProjectComponent[1] { component });

                if (!_jira.Cache.Components.AddIfMIssing(cacheEntry))
                {
                    // If there was already an entry for the project, add the component to its list.
                    _jira.Cache.Components[_project.Key].AddIfMIssing(component);
                }

                return component;
            }, token, TaskContinuationOptions.None, TaskScheduler.Default);
        }

        /// <summary>
        /// Deletes a component from a project.
        /// </summary>
        /// <param name="componentName">Name of the component to remove.</param>
        /// <param name="moveIssuesTo">The component to set on issues where the deleted component is the component, If null then the component is removed.</param>
        public void Delete(string componentName, string moveIssuesTo = null)
        {
            ExecuteAndGuard(() => DeleteAsync(componentName, moveIssuesTo).Wait());
        }

        /// <summary>
        /// Deletes a component from a project.
        /// </summary>
        /// <param name="componentName">Name of the component to remove.</param>
        /// <param name="moveIssuesTo">The component to set on issues where the deleted component is the component, If null then the component is removed.</param>
        /// <param name="token">Cancellation token for this operation.</param>
        public Task DeleteAsync(string componentName, string moveIssuesTo = null, CancellationToken token = default(CancellationToken))
        {
            var component = _jira.GetProjectComponents(_project.Key).First(v => v.Name.Equals(componentName, StringComparison.OrdinalIgnoreCase));

            var resource = String.Format("/rest/api/2/component/{0}?{1}",
                component.Id,
                String.IsNullOrEmpty(moveIssuesTo) ? null : "moveIssuesTo=" + Uri.EscapeDataString(moveIssuesTo));

            return _jira.RestClient.ExecuteRequestAsync(Method.DELETE, resource, null, token).ContinueWith(task =>
            {
                JiraEntityDictionary<ProjectComponent> cacheEntry;

                if (_jira.Cache.Components.TryGetValue(_project.Key, out cacheEntry) && cacheEntry.ContainsKey(component.Id))
                {
                    cacheEntry.Remove(component.Id);
                }
            }, token, TaskContinuationOptions.None, TaskScheduler.Default);
        }

        private T ExecuteAndGuard<T>(Func<T> execute)
        {
            try
            {
                return execute();
            }
            catch (AggregateException ex)
            {
                throw ex.Flatten().InnerException;
            }
        }

        private void ExecuteAndGuard(Action execute)
        {
            ExecuteAndGuard(() =>
            {
                execute();
                return false;
            });
        }
    }
}
