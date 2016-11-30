using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Policy;
using TfsAdvanced.Repository;
using TfsAdvanced.Utilities;

namespace TfsAdvanced.Tasks
{
    public class PolicyUpdater
    {

        private readonly RequestData requestData;
        private readonly ProjectRepository projectRepository;
        private readonly PolicyRepository policyRepository;
        private bool IsRunning = false;

        public PolicyUpdater(RequestData requestData, ProjectRepository projectRepository, PolicyRepository policyRepository)
        {
            this.requestData = requestData;
            this.projectRepository = projectRepository;
            this.policyRepository = policyRepository;
        }

        [AutomaticRetry(Attempts = 0)]
        public void Update()
        {
            if (IsRunning)
                return;
            IsRunning = true;
            try
            {

                ConcurrentBag<PolicyConfiguration> policies = new ConcurrentBag<PolicyConfiguration>();
                Parallel.ForEach(projectRepository.GetProjects(), new ParallelOptions {MaxDegreeOfParallelism = Startup.MAX_DEGREE_OF_PARALLELISM}, project =>
                {
                    var policyConfigurations = GetAsync.FetchResponseList<PolicyConfiguration>(requestData, $"{requestData.BaseAddress}/defaultcollection/{project.id}/_apis/policy/configurations?api-version=2.0-preview.1").Result;
                    foreach (var configuration in policyConfigurations)
                    {
                        policies.Add(configuration);
                    }

                });
                policyRepository.SetPolicies(policies);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error running policy updater.", ex);
            }
            finally
            {
                IsRunning = false;
            }

        }
    }
}
