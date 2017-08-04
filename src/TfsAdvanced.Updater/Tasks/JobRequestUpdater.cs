using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using TfsAdvanced.DataStore.Repository;
using TfsAdvanced.Models;
using TfsAdvanced.Models.Infrastructure;
using TFSAdvanced.Models.DTO;
using TFSAdvanced.Updater.Models.Builds;
using TFSAdvanced.Updater.Models.JobRequests;
using BuildStatus = TFSAdvanced.Updater.Models.Builds.BuildStatus;

namespace TfsAdvanced.Updater.Tasks
{
    public class JobRequestUpdater
    {
        private readonly JobRequestRepository jobRequestRepository;
        private readonly UpdateStatusRepository updateStatusRepository;
        private readonly PoolRepository poolRepository;
        private readonly BuildRepository buildRepository;
        private readonly RequestData requestData;
        private bool IsRunning;


        public JobRequestUpdater(JobRequestRepository jobRequestRepository, RequestData requestData, PoolRepository poolRepository, BuildRepository buildRepository, UpdateStatusRepository updateStatusRepository)
        {
            this.jobRequestRepository = jobRequestRepository;
            this.requestData = requestData;
            this.poolRepository = poolRepository;
            this.buildRepository = buildRepository;
            this.updateStatusRepository = updateStatusRepository;
        }

        [AutomaticRetry(Attempts = 0)]
        public void Update()
        {
            if (IsRunning)
                return;
            IsRunning = true;
            try
            {

                ConcurrentBag<QueueJob> jobRequests = new ConcurrentBag<QueueJob>();

                Parallel.ForEach(poolRepository.GetAll(), new ParallelOptions {MaxDegreeOfParallelism = AppSettings.MAX_DEGREE_OF_PARALLELISM}, pool =>
                {

                    var poolJobRequests = GetAsync.FetchResponseList<JobRequest>(requestData, $"{requestData.BaseAddress}/_apis/distributedtask/pools/{pool.id}/jobrequests?api-version=1.0").Result;
                    if (poolJobRequests != null)
                    {
                        foreach (var poolJobRequest in poolJobRequests)
                        {
                            QueueJob queueJob = new QueueJob
                            {
                                RequestId = poolJobRequest.requestId,
                                QueuedTime = poolJobRequest.queueTime,
                                AssignedTime = poolJobRequest.assignTime,
                                FinishedTime = poolJobRequest.finishTime
                            };
                            if (poolJobRequest.planType == PlanTypes.Build)
                            {
                                var build = buildRepository.GetBuild(poolJobRequest.owner.id);
                                if (build != null)
                                {
                                    queueJob.LaunchedBy = build.Creator;
                                    queueJob.StartedTime = build.StartedDate;
                                    queueJob.FinishedTime = build.FinishedDate;
                                    switch (build.BuildStatus)
                                    {
                                        case TFSAdvanced.Models.DTO.BuildStatus.NotStarted:
                                            queueJob.QueueJobStatus = QueueJobStatus.Queued;
                                            break;
                                        case TFSAdvanced.Models.DTO.BuildStatus.Abandonded:
                                            queueJob.QueueJobStatus = QueueJobStatus.Abandonded;
                                            break;
                                        case TFSAdvanced.Models.DTO.BuildStatus.Building:
                                            queueJob.QueueJobStatus = QueueJobStatus.Building;
                                            break;
                                        case TFSAdvanced.Models.DTO.BuildStatus.Cancelled:
                                            queueJob.QueueJobStatus = QueueJobStatus.Cancelled;
                                            break;
                                        case TFSAdvanced.Models.DTO.BuildStatus.Expired:
                                        case TFSAdvanced.Models.DTO.BuildStatus.Failed:
                                            queueJob.QueueJobStatus = QueueJobStatus.Failed;
                                            break;
                                        case TFSAdvanced.Models.DTO.BuildStatus.Succeeded:
                                            queueJob.QueueJobStatus = QueueJobStatus.Succeeded;
                                            break;
                                    }
                                }

                            }
                            else if (poolJobRequest.planType == PlanTypes.Release)
                            {
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
                            }



                            jobRequests.Add(queueJob);
                        }
                    }
                });

                DateTime yesterday = DateTime.Now.Date.AddDays(-1);
                var jobRequestsLists = jobRequests.Where(x => !x.StartedTime.HasValue || x.StartedTime.Value >= yesterday).ToList();
                jobRequestRepository.Update(jobRequestsLists);
                updateStatusRepository.UpdateStatus(new UpdateStatus {LastUpdate = DateTime.Now, UpdatedRecords = jobRequestsLists.Count, UpdaterName = nameof(JobRequestUpdater)});

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error running Job Request Updater ", ex);
            }
            finally
            {
                IsRunning = false;
            }

        }
    }
}
