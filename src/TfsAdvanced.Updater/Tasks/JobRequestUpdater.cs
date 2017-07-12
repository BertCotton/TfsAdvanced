using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using TfsAdvanced.DataStore.Repository;
using TfsAdvanced.Models;
using TfsAdvanced.Models.Builds;
using TfsAdvanced.Models.Infrastructure;
using TfsAdvanced.Models.JobRequests;

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

                ConcurrentBag<JobRequest> jobRequests = new ConcurrentBag<JobRequest>();

                Parallel.ForEach(poolRepository.GetAll(), new ParallelOptions {MaxDegreeOfParallelism = AppSettings.MAX_DEGREE_OF_PARALLELISM}, pool =>
                {

                    var poolJobRequests = GetAsync.FetchResponseList<JobRequest>(requestData, $"{requestData.BaseAddress}/_apis/distributedtask/pools/{pool.id}/jobrequests?api-version=1.0").Result;
                    if (poolJobRequests != null)
                    {
                        foreach (var poolJobRequest in poolJobRequests)
                        {
                            if (poolJobRequest.planType == PlanTypes.Build)
                            {
                                var build = buildRepository.GetBuild(poolJobRequest.owner.id);
                                if (build != null)
                                {
                                    poolJobRequest.owner = build;
                                    poolJobRequest.startedTime = build.startTime;
                                    poolJobRequest.definition = build.definition;
                                    if (build.status == BuildStatus.completed)
                                    {
                                        switch (build.result)
                                        {
                                            case BuildResult.succeeded:
                                                poolJobRequest.status = JobRequestStatus.succeeded;
                                                break;
                                            case BuildResult.abandoned:
                                                poolJobRequest.status = JobRequestStatus.abandoned;
                                                break;
                                            case BuildResult.canceled:
                                                poolJobRequest.status = JobRequestStatus.canceled;
                                                break;
                                            case BuildResult.failed:
                                            case BuildResult.partiallySucceeded:
                                                poolJobRequest.status = JobRequestStatus.failed;
                                                break;
                                        }
                                    }
                                    else if (build.status == BuildStatus.inProgress)
                                    {
                                        if (build.startTime.HasValue)
                                            poolJobRequest.status = JobRequestStatus.started;
                                        else
                                            poolJobRequest.status = JobRequestStatus.queued;
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
                                            poolJobRequest.status = JobRequestStatus.succeeded;
                                            break;
                                        case BuildResult.abandoned:
                                            poolJobRequest.status = JobRequestStatus.abandoned;
                                            break;
                                        case BuildResult.canceled:
                                            poolJobRequest.status = JobRequestStatus.canceled;
                                            break;
                                        case BuildResult.failed:
                                        case BuildResult.partiallySucceeded:
                                            poolJobRequest.status = JobRequestStatus.failed;
                                            break;
                                    }
                                }
                            }



                            jobRequests.Add(poolJobRequest);
                        }
                    }
                });

                DateTime yesterday = DateTime.Now.Date.AddDays(-1);
                var jobRequestsLists = jobRequests.Where(x => !x.startedTime.HasValue || x.startedTime.Value >= yesterday).ToList();
                jobRequestRepository.UpdateJobRequests(jobRequestsLists);
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
