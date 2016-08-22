using TfsAdvanced.Data;
using TfsAdvanced.Infrastructure;
using Microsoft.Extensions.OptionsModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Attachment = System.Net.Mail.Attachment;

namespace TfsAdvanced.ServiceRequests
{
    public class WorkItemRequest
    {
        private readonly AppSettings appSettings;

        public WorkItemRequest(IOptions<AppSettings> appSettings)
        {
            this.appSettings = appSettings.Value;
        }

        public async Task<WorkItem> CreateWorkItem(RequestData requestData, WorkItem workItem, int? newWorkItemId = null)
        {
            var projectName = workItem.fields["System.TeamProject"];
            var workItemType = workItem.fields["System.WorkItemType"];
            var url =
                $"{requestData.BaseAddress}/{projectName}/_apis/wit/workitems/${Uri.EscapeUriString(workItemType)}?api-version=1.0";
            if (newWorkItemId.HasValue)
                url = $"{requestData.BaseAddress}/_apis/wit/workitems/{newWorkItemId.Value}?api-version=1.0";

            List<Update> updateFields = new List<Update>();

            foreach (var fieldKey in workItem.fields.Keys)
            {
                if (appSettings.WorkItemSettings.ReadOnlyFields.Contains(fieldKey) || appSettings.WorkItemSettings.IgnoredFields.Any(f => fieldKey.Contains(f)))
                    continue;

                var fieldValue = workItem.fields[fieldKey];

                fieldValue = GetMapedField(fieldKey, fieldValue);

                updateFields.Add(new UpdateField
                {
                    op = UpdateFieldOperation.add,
                    path = "/fields/" + fieldKey,
                    value = fieldValue
                });
            }

            var requestContent = new StringContent(JsonConvert.SerializeObject(updateFields), Encoding.UTF8,
                "application/json-patch+json");
            HttpResponseMessage request;
            if (newWorkItemId.HasValue)
            {
                HttpRequestMessage requestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), url);
                requestMessage.Content = requestContent;
                request = await requestData.HttpClient.SendAsync(requestMessage);
            }
            else
                request = await requestData.HttpClient.PostAsync(url, requestContent);
            var content = await request.Content.ReadAsStringAsync();
            if (request.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<WorkItem>(content);

            return new WorkItem
            {
                url = content
            };
        }

        public async Task<List<int>> GetBackLogItemIds(RequestData requestData, Project project)
        {
            return await GetWorkIds(requestData, project, "[System.WorkItemType] = 'Product Backlog Item'");
        }

        public async Task<List<int>> GetBugs(RequestData requestData, Project project)
        {
            return await GetWorkIds(requestData, project, "[System.WorkItemType] = 'Bug'");
        }

        public async Task<List<int>> GetEpics(RequestData requestData, Project project)
        {
            return await GetWorkIds(requestData, project, "[System.WorkItemType] = 'Epic'");
        }

        public async Task<List<int>> GetFeatures(RequestData requestData, Project project)
        {
            return await GetWorkIds(requestData, project, "[System.WorkItemType] = 'Feature'");
        }

        public async Task<List<int>> GetLinks(RequestData requestData, int parentId)
        {
            var parentTask = (await GetWorkItems(requestData, new List<int> { parentId }, true)).First();
            var children = parentTask.relations.Where(r => r.rel == "System.LinkTypes.Hierarchy-Forward").ToList();

            var childrenWorkIds = new List<int>();
            children.ForEach(c =>
            {
                var match = Regex.Match(c.url, @"(?<=(\D|^))\d+(?=\D*$)");
                if (match.Success)
                {
                    childrenWorkIds.Add(int.Parse(match.Value));
                }
            });

            return childrenWorkIds;
        }

        public async Task<List<int>> GetTasks(RequestData requestData, Project project)
        {
            return await GetWorkIds(requestData, project, "[System.WorkItemType] = 'Task'");
        }

        public async Task<List<int>> GetWorkIds(RequestData requestData, Project project, string filter = null)
        {
            List<int> workIds = new List<int>();
            var index = 0;

            while (true)
            {
                var query =
                    $"SELECT id FROM WorkItems WHERE [System.Id] > {index}";
                if (filter != null)
                    query += $" && {filter}";
                query += $" && [System.TeamProject] = '{project.name}'";

                var FormUrlEncodedContent = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("wiql",query)
                });
                var response =
                    await
                        requestData.HttpClient.PostAsync($"{requestData.BaseAddress}/{project.id}/_api/_wit/query?__v=5",
                            FormUrlEncodedContent);

                var content = await response.Content.ReadAsStringAsync();
                var queryResponse = JsonConvert.DeserializeObject<WorkQuery>(content);
                if (queryResponse.payload == null || queryResponse.payload.rows.Length == 0)
                    break;

                index += 200;
                queryResponse.payload.rows.ToList().ForEach(r => workIds.Add(int.Parse(r[0].ToString())));
            }

            return workIds.Distinct().OrderBy(x => x).ToList();
        }

        public async Task<WorkItem> GetWorkItem(RequestData requestData, int workId, bool expand = false)
        {
            var url = $"{requestData.BaseAddress}/_apis/wit/workitems/{workId}?api-version=1.0";
            if (expand)
                url += "&$expand=all";
            var response =
                await
                    requestData.HttpClient.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<WorkItem>(content);
            }
            return null;
        }

        public async Task<WorkItem> GetWorkItem(RequestData requestData, Project project, WorkItem workItem, int? updatedWorkId = null)
        {
            var query = $"[System.Title] = '{workItem.fields["System.Title"]}' && [System.WorkItemType] = '{workItem.fields["System.WorkItemType"]}'";
            if (updatedWorkId.HasValue)
                query += $"  && [System.Id] <> {updatedWorkId.Value}";
            var workIds = await GetWorkIds(requestData, project, query);
            if (workIds.Count == 1)
                return await GetWorkItem(requestData, workIds.First());
            return null;
        }

        public async Task<WorkItem> GetWorkItemFromUrl(RequestData requestData, string url)
        {
            var response = await requestData.HttpClient.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<WorkItem>(content);
            }
            return null;
        }

        public async Task<List<WorkItem>> GetWorkItems(RequestData requestData, List<int> workIds, bool expand = false)
        {
            List<WorkItem> workItems = new List<WorkItem>();
            foreach (var partitionList in partition(workIds, 200))
            {
                var url = $"{appSettings.OnSiteBaseAddress}/_apis/wit/workitems?ids={string.Join(",", partitionList)}&api-version=1.0";
                if (expand)
                    url += "&$expand=all";
                var response =
                    await
                        requestData.HttpClient.GetAsync(url);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var iteration = JsonConvert.DeserializeObject<Response<IEnumerable<WorkItem>>>(content);
                    workItems.AddRange(iteration.value);
                }
            }
            return workItems;
        }

        public async Task<List<WorkItem>> GetWorkItemWithRevisions(RequestData requestData, int workId,
                    bool expand = false)
        {
            var url = $"{appSettings.OnSiteBaseAddress}/_apis/wit/workitems/{workId}/revisions?api-version=1.0";
            if (expand)
                url += "&$expand=all";

            var response =
                await
                    requestData.HttpClient.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var workItems = JsonConvert.DeserializeObject<Response<IEnumerable<WorkItem>>>(content);
                return workItems.value.ToList();
            }
            return new List<WorkItem>();
        }

        public async Task<WorkItem> LinkWorkItem(RequestData requestData, WorkItem workItem, int newWorkItemId)
        {
            var projectName = workItem.fields["System.TeamProject"];
            var workItemType = workItem.fields["System.WorkItemType"];
            var url = $"{requestData.BaseAddress}/_apis/wit/workitems/{newWorkItemId}?api-version=1.0";

            List<Update> updateFields = new List<Update>();

            if (workItem.relations != null)
            {
                foreach (var relation in workItem.relations)
                {
                    if (relation.workItem != null)
                    {
                        updateFields.Add(new UpdateRelation
                        {
                            op = UpdateFieldOperation.add,
                            path = "/relations/-",
                            value = new UpdateRelationValue
                            {
                                rel = relation.rel,
                                url = relation.url,
                                attributes = relation.attributes
                            }
                        });
                    }
                }
            }

            if (!updateFields.Any())
            {
                var msg =
                    $"Failed to updated work item {workItem.id} {workItem.fields["System.Title"]} because it did not have any links.";
                Debug.WriteLine(msg);
                return new WorkItem
                {
                    url = msg
                };
            }

            var requestContent = new StringContent(JsonConvert.SerializeObject(updateFields), Encoding.UTF8, "application/json-patch+json");
            HttpRequestMessage requestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), url);
            requestMessage.Content = requestContent;
            HttpResponseMessage request = await requestData.HttpClient.SendAsync(requestMessage);
            var content = await request.Content.ReadAsStringAsync();
            if (request.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<WorkItem>(content);

            return new WorkItem
            {
                url = content
            };
        }

        private string GetMapedField(string key, string value)
        {
            var userMap = appSettings.WorkItemSettings.UserMappings.FirstOrDefault(u => u.From == value);
            if (userMap != null)
            {
                var mapped = userMap.To;
                if (mapped == null)
                    throw new Exception($"User {value} has not been mapped over yet.");
                return mapped;
            }

            if (key == "Microsoft.VSTS.Common.Priority")
            {
                var priority = int.Parse(value);
                if (priority > appSettings.WorkItemSettings.MaxPriority)
                    value = appSettings.WorkItemSettings.MaxPriority.ToString();
            }

            return value;
        }

        private List<List<int>> partition(List<int> locations, int nSize = 30)
        {
            var list = new List<List<int>>();

            for (int i = 0; i < locations.Count; i += nSize)
            {
                list.Add(locations.GetRange(i, Math.Min(nSize, locations.Count - i)));
            }

            return list;
        }

        public async Task<WorkItem> AddAttachment(RequestData requestData, int workItemId, AttachmentUploadResult uploadedAttachment)
        {
            var url = $"{requestData.BaseAddress}/_apis/wit/workitems/{workItemId}?api-version=1.0";

            List<Update> updateFields = new List<Update>();

                        updateFields.Add(new UpdateRelation
                        {
                            op = UpdateFieldOperation.add,
                            path = "/relations/-",
                            value = new UpdateRelationValue
                            {
                                rel = "AttachedFile",
                                url = uploadedAttachment.url
                            }
                        });
                    
                
            

            var requestContent = new StringContent(JsonConvert.SerializeObject(updateFields), Encoding.UTF8, "application/json-patch+json");
            HttpRequestMessage requestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), url);
            requestMessage.Content = requestContent;
            HttpResponseMessage request = await requestData.HttpClient.SendAsync(requestMessage);
            var content = await request.Content.ReadAsStringAsync();
            if (request.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<WorkItem>(content);

            return new WorkItem
            {
                url = content
            };
        }
    }
}