using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TfsAdvanced.DataStore.Repository;
using TfsAdvanced.Models;
using TfsAdvanced.Models.Infrastructure;
using TFSAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;
using TFSAdvanced.Updater.Models.Builds;
using TFSAdvanced.Updater.Models.JobRequests;
using TFSAdvanced.Updater.Tasks;
using Build = TFSAdvanced.Models.DTO.Build;
using BuildDefinition = TFSAdvanced.Models.DTO.BuildDefinition;
using BuildStatus = TFSAdvanced.Models.DTO.BuildStatus;

namespace TfsAdvanced.Updater.Tasks
{
    public class JobRequestUpdater : UpdaterBase
    {
        private readonly BuildDefinitionRepository buildDefinitionRepository;
        private readonly BuildRepository buildRepository;
        private readonly JobRequestRepository jobRequestRepository;
        private readonly PoolRepository poolRepository;
        private readonly ProjectRepository projectRepository;
        private readonly ReleaseDefinitionRepository releaseDefinitionRepository;
        private readonly RequestData requestData;
        private readonly UpdateStatusRepository updateStatusRepository;

        public JobRequestUpdater(JobRequestRepository jobRequestRepository, RequestData requestData, PoolRepository poolRepository, BuildRepository buildRepository, UpdateStatusRepository updateStatusRepository, BuildDefinitionRepository buildDefinitionRepository, ProjectRepository projectRepository, ReleaseDefinitionRepository releaseDefinitionRepository, ILogger<JobRequestUpdater> logger) : base(logger)
        {
            this.jobRequestRepository = jobRequestRepository;
            this.requestData = requestData;
            this.poolRepository = poolRepository;
            this.buildRepository = buildRepository;
            this.updateStatusRepository = updateStatusRepository;
            this.buildDefinitionRepository = buildDefinitionRepository;
            this.projectRepository = projectRepository;
            this.releaseDefinitionRepository = releaseDefinitionRepository;
        }

        protected override void Update()
        {
            var jobRequests = new ConcurrentBag<QueueJob>();

            Parallel.ForEach(poolRepository.GetAll(), new ParallelOptions { MaxDegreeOfParallelism = AppSettings.MAX_DEGREE_OF_PARALLELISM }, pool =>
              {
                  List<JobRequest> poolJobRequests = GetAsync.FetchResponseList<JobRequest>(requestData, $"{requestData.BaseAddress}/_apis/distributedtask/pools/{pool.id}/jobrequests?api-version=1.0", Logger).Result;
                  if (poolJobRequests != null)
                  {
                      foreach (JobRequest poolJobRequest in poolJobRequests)
                      {
                          var queueJob = new QueueJob
                          {
                              RequestId = poolJobRequest.requestId,
                              QueuedTime = poolJobRequest.queueTime,
                              AssignedTime = poolJobRequest.assignTime,
                              FinishedTime = poolJobRequest.finishTime,
                              Name = poolJobRequest.definition.name,
                              Url = poolJobRequest.definition._links.web.href
                          };

                          if (poolJobRequest.planType == PlanTypes.Build)
                          {
                              queueJob.JobType = JobType.Build;
                              Build build = buildRepository.GetBuild(poolJobRequest.owner.id);
                              if (build != null)
                              {
                                  queueJob.LaunchedBy = build.Creator;
                                  queueJob.StartedTime = build.StartedDate;
                                  queueJob.FinishedTime = build.FinishedDate;
                                  queueJob.BuildFolder = build.Folder;

                                  switch (build.BuildStatus)
                                  {
                                      case BuildStatus.NotStarted:
                                          queueJob.QueueJobStatus = QueueJobStatus.Queued;
                                          break;

                                      case BuildStatus.Abandonded:
                                          queueJob.QueueJobStatus = QueueJobStatus.Abandonded;
                                          break;

                                      case BuildStatus.Building:
                                          queueJob.QueueJobStatus = QueueJobStatus.Building;
                                          break;

                                      case BuildStatus.Cancelled:
                                          queueJob.QueueJobStatus = QueueJobStatus.Cancelled;
                                          break;

                                      case BuildStatus.Expired:
                                      case BuildStatus.Failed:
                                          queueJob.QueueJobStatus = QueueJobStatus.Failed;
                                          break;

                                      case BuildStatus.Succeeded:
                                          queueJob.QueueJobStatus = QueueJobStatus.Succeeded;
                                          break;
                                  }
                              }
                              BuildDefinition buildDefinition = buildDefinitionRepository.GetBuildDefinition(poolJobRequest.definition.id);
                              if (buildDefinition?.Repository != null)
                              {
                                  Project project = projectRepository.GetProject(buildDefinition.Repository.Project.Id);
                                  if (project != null)
                                  {
                                      queueJob.Project = new Project
                                      {
                                          Id = project.Id,
                                          Name = project.Name,
                                          Url = project.Url
                                      };
                                  }
                                  else
                                  {
                                      queueJob.Project = new Project
                                      {
                                          Name = "Unknown Project"
                                      };
                                  }
                              }
                              else
                              {
                                  queueJob.Project = new Project
                                  {
                                      Name = "Unknown Build Definition"
                                  };
                              }
                          }
                          else if (poolJobRequest.planType == PlanTypes.Release)
                          {
                              queueJob.JobType = JobType.Release;
                              if (poolJobRequest.finishTime.HasValue)
                              {
                                  switch (poolJobRequest.result)
                                  {
                                      case BuildResult.succeeded:
                                          queueJob.QueueJobStatus = QueueJobStatus.Succeeded;
                                          break;

                                      case BuildResult.abandoned:
                                          queueJob.QueueJobStatus = QueueJobStatus.Abandonded;
                                          break;

                                      case BuildResult.canceled:
                                          queueJob.QueueJobStatus = QueueJobStatus.Cancelled;
                                          break;

                                      case BuildResult.failed:
                                      case BuildResult.partiallySucceeded:
                                          queueJob.QueueJobStatus = QueueJobStatus.Failed;
                                          break;
                                  }
                              }

                              ReleaseDefinition releaseDefinition = releaseDefinitionRepository.GetReleaseDefinition(poolJobRequest.scopeId.ToString(), poolJobRequest.definition.id);
                              if (releaseDefinition != null)
                              {
                                  queueJob.Project = releaseDefinition.Project;
                              }
                              else
                              {
                                  queueJob.Project = new Project
                                  {
                                      Name = "Unknown Release Definition"
                                  };
                              }
                          }

                          jobRequests.Add(queueJob);
                      }
                  }
              });

            jobRequestRepository.Update(jobRequests);
            updateStatusRepository.UpdateStatus(new UpdateStatus { LastUpdate = DateTime.Now, UpdatedRecords = jobRequests.Count, UpdaterName = nameof(JobRequestUpdater) });
        }
    }
}