using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Builds;
using TfsAdvanced.Data.JobRequests;
using TfsAdvanced.Data.Projects;
using TfsAdvanced.Repository;
using TfsAdvanced.Utilities;

namespace TfsAdvanced.Tasks
{
    public class JobRequestUpdater
    {
        private readonly JobRequestRepository jobRequestRepository;
        private readonly PoolRepository poolRepository;
        private readonly BuildRepository buildRepository;
        private readonly RequestData requestData;
        private bool IsRunning;


        public JobRequestUpdater(JobRequestRepository jobRequestRepository, RequestData requestData, PoolRepository poolRepository, BuildRepository buildRepository)
        {
            this.jobRequestRepository = jobRequestRepository;
            this.requestData = requestData;
            this.poolRepository = poolRepository;
            this.buildRepository = buildRepository;
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

                Parallel.ForEach(poolRepository.GetPools(), new ParallelOptions {MaxDegreeOfParallelism = Startup.MAX_DEGREE_OF_PARALLELISM}, pool =>
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
                                }
                            }
                            else if (poolJobRequest.planType == PlanTypes.Release)
                            {

                            }
                            jobRequests.Add(poolJobRequest);
                        }
                    }
                });

                jobRequestRepository.UpdateJobRequests(jobRequests.ToList());

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
