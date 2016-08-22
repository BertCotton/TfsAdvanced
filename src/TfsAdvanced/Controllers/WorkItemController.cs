using TfsAdvanced.Data;
using TfsAdvanced.Infrastructure;
using TfsAdvanced.ServiceRequests;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.OptionsModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TfsAdvanced.Repositories;

namespace TfsAdvanced.Controllers
{
    [Route("data/WorkItem")]
    public class WorkItemController
    {
        private static string WorkIdey = "WorkId";
        private static string WorkItemKey = "WorkItems";
        private readonly AppSettings appSettings;
        private readonly WorkItemMigrationRepository workItemMigrationRepository;
        private readonly IMemoryCache memoryCache;
        private readonly ProjectServiceRequest projectServiceRequest;
        private readonly TfsRequest tfsRequest;
        private readonly WorkItemRequest workItemRequest;
        private readonly AttachmentRequest attachmentRequest;
        private readonly AttachmentRepository attachmentRepository;

        public WorkItemController(TfsRequest tfsRequest, WorkItemRequest workItemRequest,
            ProjectServiceRequest projectServiceRequest, IMemoryCache memoryCache, IOptions<AppSettings> appSettings,
            WorkItemMigrationRepository workItemMigrationRepository, AttachmentRequest attachmentRequest, AttachmentRepository attachmentRepository)
        {
            this.tfsRequest = tfsRequest;
            this.workItemRequest = workItemRequest;
            this.projectServiceRequest = projectServiceRequest;
            this.memoryCache = memoryCache;
            this.workItemMigrationRepository = workItemMigrationRepository;
            this.attachmentRequest = attachmentRequest;
            this.attachmentRepository = attachmentRepository;
            this.appSettings = appSettings.Value;
        }

        [HttpGet("{workItemId}")]
        public async Task<List<WorkItem>> GetAllWorkItems([FromRoute] int workItemId)
        {
            List<WorkItem> workItems = new List<WorkItem>();
            using (var httpClient = tfsRequest.GetHttpClient())
            {
                workItems = await workItemRequest.GetWorkItems(httpClient, new List<int> { workItemId });
            }

            return workItems;
        }

        [HttpGet]
        public async Task<List<WorkItem>> GetAllWorkItems()
        {
            List<WorkItem> cachedWorkItems;
            var cacheKey = WorkItemKey;
            if (memoryCache.TryGetValue(cacheKey, out cachedWorkItems))
                return cachedWorkItems;

            List<WorkItem> workItems = new List<WorkItem>();
            using (var httpClient = tfsRequest.GetHttpClient())
            {
                var backLogItemIds = GetBackLogItemIds().Result;
                workItems = await workItemRequest.GetWorkItems(httpClient, backLogItemIds);
            }

            memoryCache.Set(cacheKey, workItems,
                new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10)));

            return workItems;
        }

        [HttpGet("All")]
        public async Task<List<int>> GetAllWorkItemsIds()
        {
            List<int> cachedWorkIds;
            if (memoryCache.TryGetValue("All-IDS", out cachedWorkIds))
                return cachedWorkIds;

            List<int> workIds =
                await
                    GetBackLogItemIds(
                        (RequestData requestData, Project p) => workItemRequest.GetWorkIds(requestData, p).Result);

            memoryCache.Set("All-IDS", workIds,
                new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10)));
            return workIds;
        }

        [HttpGet("BLIs")]
        public async Task<List<int>> GetBackLogItemIds()
        {
            List<int> cachedWorkIds;
            var cacheKey = WorkIdey;
            if (memoryCache.TryGetValue(cacheKey, out cachedWorkIds))
                return cachedWorkIds;

            List<int> workIds =
                await
                    GetBackLogItemIds(
                        (RequestData requestData, Project p) => workItemRequest.GetBackLogItemIds(requestData, p).Result);

            memoryCache.Set(cacheKey, workIds,
                new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10)));
            return workIds;
        }

        [HttpGet("Bugs")]
        public async Task<List<int>> GetBugIds()
        {
            List<int> cachedWorkIds;
            if (memoryCache.TryGetValue("BUGS-IDS", out cachedWorkIds))
                return cachedWorkIds;

            List<int> workIds =
                await
                    GetBackLogItemIds(
                        (RequestData requestData, Project p) => workItemRequest.GetBugs(requestData, p).Result);

            memoryCache.Set("BUGS-IDS", workIds,
                new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10)));
            return workIds;
        }

        [HttpGet("Epics")]
        public async Task<List<int>> GetEpics()
        {
            List<int> cachedWorkIds;
            if (memoryCache.TryGetValue("EPIC-IDS", out cachedWorkIds))
                return cachedWorkIds;

            List<int> workIds =
                await
                    GetBackLogItemIds(
                        (RequestData requestData, Project p) => workItemRequest.GetEpics(requestData, p).Result);

            memoryCache.Set("EPIC-IDS", workIds,
                new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10)));
            return workIds;
        }

        [HttpGet("Features")]
        public async Task<List<int>> GetFeatures()
        {
            List<int> cachedWorkIds;
            if (memoryCache.TryGetValue("FEATURES-IDS", out cachedWorkIds))
                return cachedWorkIds;

            List<int> workIds =
                await
                    GetBackLogItemIds(
                        (RequestData requestData, Project p) => workItemRequest.GetFeatures(requestData, p).Result);

            memoryCache.Set("FEATURES-IDS", workIds,
                new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10)));
            return workIds;
        }

        [HttpGet("links/{workItemId}")]
        public async Task<List<WorkItem>> GetLinks([FromRoute] int workItemId)
        {
            List<WorkItem> workItems = new List<WorkItem>();
            using (var requestData = tfsRequest.GetHttpClient())
            {
                var linkedIds = await workItemRequest.GetLinks(requestData, workItemId);
                var projects = await projectServiceRequest.GetProjects(requestData);
                projects.ForEach(p =>
                {
                    workItems.AddRange(workItemRequest.GetWorkItems(requestData, linkedIds).Result);
                });
            }

            return workItems;
        }

        [HttpGet("Tasks")]
        public async Task<List<int>> GetTaskIds()
        {
            List<int> cachedWorkIds;
            if (memoryCache.TryGetValue("TASK-IDS", out cachedWorkIds))
                return cachedWorkIds;

            List<int> workIds =
                await
                    GetBackLogItemIds(
                        (RequestData requestData, Project p) => workItemRequest.GetTasks(requestData, p).Result);

            memoryCache.Set("TASK-IDS", workIds,
                new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10)));
            return workIds;
        }

        [HttpPost("Migrate")]
        public async Task<List<WorkItem>> Migrate()
        {
            return Migrate(await GetAllWorkItemsIds(), false);
        }

        [HttpPost("Extract/Attachments")]
        public async Task<List<Attachment>> ExtractAttachments()
        {
            return ExtractAttachments(await GetAllWorkItemsIds());
        }

        [HttpPost("Migrate/Attachments")]
        public async Task<List<AttachmentUploadResult>> MigrateAttachments()
        {
            var attachments = new List<AttachmentUploadResult>();
            using (var requestData = tfsRequest.GetCloudHttpClient())
            {
                foreach (var attachmentId in attachmentRepository.GetAttachmentIds())
                {
                    var attachment = await attachmentRepository.GetAttachment(attachmentId);
                    var workItemId = workItemMigrationRepository.GetMigratedWorkId(attachment.OriginalWorkId);
                    if (!workItemId.HasValue)
                    {
                        Debug.WriteLine($"Unable to locate migrated work item id for original id {attachment.OriginalWorkId}");
                    }
                    var uploadedAttachment = await attachmentRequest.UploadAttachment(requestData, attachment);
                    await workItemRequest.AddAttachment(requestData, workItemId.Value, uploadedAttachment);
                    attachments.Add(uploadedAttachment);

                }
            }
            return attachments;
        }

        [HttpPost("Migrate/BLIs")]
        public async Task<List<WorkItem>> MigrateBlis()
        {
            return Migrate(await GetBackLogItemIds(), true);
        }

        [HttpGet("Migrate/Bugs")]
        public async Task<List<WorkItem>> MigrateBugs([FromQuery] bool AddLinks)
        {
            return Migrate(await GetBugIds(), AddLinks);
        }

        [HttpPost("Migrate/Epics")]
        public async Task<List<WorkItem>> MigrateEpics()
        {
            return Migrate(await GetEpics(), true);
        }

        [HttpPost("Migrate/Features")]
        public async Task<List<WorkItem>> MigrateFeatures()
        {
            return Migrate(await GetFeatures(), true);
        }

        [HttpPost("Migrate/{workItemId}")]
        public async Task<List<WorkItem>> MigrateWorkItem([FromRoute] int workItemId)
        {
            var workItems = await GetAllWorkItemsIds();
            workItems = workItems.Where(x => x == workItemId).ToList();
            return Migrate(workItems, false);
        }

        private async Task<List<int>> GetBackLogItemIds(Func<RequestData, Project, List<int>> loaderFunc)
        {
            List<int> workIds = new List<int>();
            using (var requestData = tfsRequest.GetHttpClient())
            {
                var projects = await projectServiceRequest.GetProjects(requestData);
                projects.ForEach(p =>
                {
                    workIds.AddRange(loaderFunc(requestData, p));
                });
            }
            return workIds.Distinct().OrderBy(x => x).ToList();
        }

        private async Task<WorkItem> LinkWorkItem(int workItemId)
        {
            WorkItem workItem;
            List<Project> projects;
            using (var requestData = tfsRequest.GetHttpClient())
            {
                projects = await projectServiceRequest.GetProjects(requestData);
                workItem = await workItemRequest.GetWorkItem(requestData, workItemId, true);

                workItem.project = projects.First(p => p.name == workItem.fields["System.TeamProject"]);
                if (workItem.relations != null)
                {
                    foreach (var relation in workItem.relations)
                    {
                        if (relation.url.Contains("_apis/wit/workItems"))
                        {
                            relation.workItem = await workItemRequest.GetWorkItemFromUrl(requestData, relation.url);
                            // Recursively migrate and link the children
                            Debug.WriteLine(
                                $"Walking down tree to {workItem.fields["System.WorkItemType"]} {relation.workItem.fields["System.Title"]}");
                            if (relation.rel.Contains("Forward"))
                                await LinkWorkItem(relation.workItem.id);
                        }
                        else if (relation.url.Contains("attachment"))
                        {
                            //TODO: Handle attachments
                        }
                    }
                }
            }
            using (var requestData = tfsRequest.GetCloudHttpClient())
            {
                List<Project> migratedProjects = await projectServiceRequest.GetProjects(requestData);
                if (workItem.relations != null)
                {
                    foreach (var relation in workItem.relations)
                    {
                        if (relation.workItem == null)
                            continue;

                        int? migratedRelationId = workItemMigrationRepository.GetMigratedWorkId(relation.workItem.id);
                        if (migratedRelationId.HasValue)
                        {
                            var migratedRelation =
                                await workItemRequest.GetWorkItem(requestData, migratedRelationId.Value);
                            relation.url = migratedRelation.url;
                            relation.workItem = migratedRelation;
                        }
                        else
                        {
                            Debug.WriteLine($"Unable to find the migration id for relation from work item {relation.workItem.id}");
                        }
                    }
                }
                var migratedProject = migratedProjects.First(p => p.name == workItem.project.name);
                workItem.project = migratedProject;
                int? migratedWorkItemId = workItemMigrationRepository.GetMigratedWorkId(workItemId);
                if (!migratedWorkItemId.HasValue)
                {
                    Debug.WriteLine($"Unable to fine migrated work it for id {workItemId}");
                    return new WorkItem
                    {
                        url = $"Unable to find migrated work item"
                    };
                }
                // Check if the item exists
                Debug.WriteLine($"Updating {workItem.fields["System.WorkItemType"]} - [{workItem.fields["System.Title"]}] From {workItem.id} (rev: {workItem.rev}) to Id {migratedWorkItemId.Value}");

                return await workItemRequest.LinkWorkItem(requestData, workItem, migratedWorkItemId.Value);
            }
        }

        private List<WorkItem> Migrate(List<int> workIds, bool addLinks)
        {
            var migratedWorkItems = new ConcurrentStack<WorkItem>();

            var counter = 0;

            var total = workIds.Count + 0m;
            // foreach (var workId in workIds)
            Parallel.ForEach(workIds, (workId) =>
            {
                var index = Interlocked.Increment(ref counter);
                Debug.WriteLine($"{index}/{total} [{Decimal.Round((index / total) * 100, 2)}%] Migrating Work Item: " + workId);

                if (addLinks)
                    migratedWorkItems.Push(LinkWorkItem(workId).Result);
                else
                {
                    var workItems = MigrateItem(workId).Result;
                    if(workItems.Any())
                        migratedWorkItems.PushRange(workItems.ToArray());
                }
                    
            }
            );
            Debug.WriteLine("Done migrating task.");
            return migratedWorkItems.ToList();
        }

        private List<Attachment> ExtractAttachments(List<int> workIds)
        {
            var migratedAttachments = new ConcurrentStack<Attachment>();

            var counter = 0;

            var total = workIds.Count + 0m;
             foreach (var workId in workIds)
//            Parallel.ForEach(workIds, (workId) =>
            {
                var index = Interlocked.Increment(ref counter);
                Debug.WriteLine($"{index}/{total} [{Decimal.Round((index / total) * 100, 2)}%] Extract Attachment: " + workId);

                var attachments = ExtractAttachment(workId).Result;
                if (attachments.Any())
                {
                    foreach (var attachment in attachments)
                    {
                        File.WriteAllBytes($@"C:\dev\tmp\{attachment.OriginalWorkId}-{attachment.FileName}", attachment.File);
                    }
                    migratedAttachments.PushRange(attachments.ToArray());
                }
            }
//            );
            return migratedAttachments.ToList();
        }

        private async Task<List<Attachment>> ExtractAttachment(int workItemId)
        {
            var attachments = new List<Attachment>();
            using (var requestData = tfsRequest.GetHttpClient())
            {
                var projects = await projectServiceRequest.GetProjects(requestData);
                var workItem = await workItemRequest.GetWorkItem(requestData, workItemId, true);
                                                             
                workItem.project = projects.First(p => p.name == workItem.fields["System.TeamProject"]);
                if (workItem.fields.ContainsKey("System.Description") && workItem.fields["System.Description"].Contains("href"))
                    attachments.AddRange(await attachmentRequest.GetAttachmentFromComment(requestData, workItem));
                if (workItem.relations != null)
                {
                    foreach (var relation in workItem.relations)
                    {
                        if (relation.url.Contains("_apis/wit/attachments"))
                        {
                            var attachment = await attachmentRequest.GetAttachment(requestData, workItem.id, relation);
                            if (attachment != null)
                            {
                                await attachmentRepository.SaveAttachment(attachment);
                                attachments.Add(attachment);
                            }
                        }
                    }
                }
            }

            return attachments;
        }

        private async Task<List<WorkItem>> MigrateItem(int workItemId)
        {
            List<WorkItem> workItemRevisions = new List<WorkItem>();
            List<Project> projects;
            using (var requestData = tfsRequest.GetHttpClient())
            {
                projects = await projectServiceRequest.GetProjects(requestData);
                workItemRevisions = await workItemRequest.GetWorkItemWithRevisions(requestData, workItemId, false);

                foreach (var workItem in workItemRevisions)
                {
                    workItem.project = projects.First(p => p.name == workItem.fields["System.TeamProject"]);
                    if (workItem.relations == null)
                        continue;
                    foreach (var relation in workItem.relations)
                    {
                        if (relation.url.Contains("_apis/wit/workItems"))
                        {
                            relation.workItem = await workItemRequest.GetWorkItemFromUrl(requestData, relation.url);
                            // Recursively migrate and link the children
                            Debug.WriteLine(
                                $"Walking down tree to {workItem.fields["System.WorkItemType"]} {relation.workItem.fields["System.Title"]}");
                            if (relation.rel.Contains("Forward"))
                                await MigrateItem(relation.workItem.id);
                        }
                        else if (relation.url.Contains("attachment"))
                        {
                            //TODO: Handle attachments
                        }
                    }
                }
            }

            workItemRevisions = workItemRevisions.OrderBy(r => r.rev).ToList();

            List<WorkItem> migratedItems = new List<WorkItem>(workItemRevisions.Count);
            using (var requestData = tfsRequest.GetCloudHttpClient())
            {
                List<Project> migratedProjects = await projectServiceRequest.GetProjects(requestData);
                int? updatedWorkItemId = null;
                foreach (var workItem in workItemRevisions)
                {
                    var migratedProject = migratedProjects.First(p => p.name == workItem.project.name);
                    workItem.project = migratedProject;
                    // Check if the item exists
                    if (!updatedWorkItemId.HasValue)
                    {
                        updatedWorkItemId = workItemMigrationRepository.GetMigratedWorkId(workItemId);
                    }
                    if (updatedWorkItemId.HasValue)
                        Debug.WriteLine(
                            $"Updating {workItem.fields["System.WorkItemType"]} - [{workItem.fields["System.Title"]}] From {workItem.id} (rev: {workItem.rev}) to Id {updatedWorkItemId.Value}");
                    else
                        Debug.WriteLine(
                            $"Migrating {workItem.fields["System.WorkItemType"]} - [{workItem.fields["System.Title"]}] From {workItem.id} (rev: {workItem.rev})");

                    WorkItem updatedWorkItem =
                        await workItemRequest.CreateWorkItem(requestData, workItem, updatedWorkItemId);
                    if (updatedWorkItem.id == 0)
                    {
                        Debug.WriteLine("Failed to create item.");
                        Debug.WriteLine(updatedWorkItem.url);
                        return new List<WorkItem>();
                    }
                    if (!updatedWorkItemId.HasValue)
                        workItemMigrationRepository.SetMigrations(new WorkItemMigration
                        {
                            OriginalId = workItemId,
                            MigratedId = updatedWorkItem.id
                        });
                    updatedWorkItemId = updatedWorkItem.id;
                    migratedItems.Add(updatedWorkItem);
                }
            }
            return migratedItems;
        }
    }
}