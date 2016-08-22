using TfsAdvanced.Data;
using TfsAdvanced.ServiceRequests;
using Microsoft.AspNet.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace TfsAdvanced.Controllers
{
    [Route("data/Iterations")]
    public class IterationController
    {
        private readonly IterationRequest iterationRequest;
        private readonly RepositoryServiceRequest repositoryServiceRequest;
        private readonly TfsRequest tfsRequest;

        public IterationController(TfsRequest tfsRequest, IterationRequest iterationRequest, RepositoryServiceRequest repositoryServiceRequest)
        {
            this.tfsRequest = tfsRequest;
            this.iterationRequest = iterationRequest;
            this.repositoryServiceRequest = repositoryServiceRequest;
        }

        [HttpGet]
        public List<Iteration> GetIterations([FromQuery] string projectName)
        {
            List<Iteration> iterations = new List<Iteration>();
            using (var requestData = tfsRequest.GetHttpClient())
            {
                var projects = repositoryServiceRequest.GetRepositories(requestData, projectName).Select(r => r.project).GroupBy(x => x.id).Select(g => g.First()).ToList();

                projects.ForEach(p =>
                {
                    var iteration = iterationRequest.GetIteration(requestData, p).Result;
                    iterations.Add(iteration);
                    if (iteration.hasChildren)
                        iterations.AddRange(iteration.children);
                });
            }
            return iterations.OrderBy(i => i.id).ToList();
        }

        [HttpGet("{iterationId}")]
        public IList<Iteration> MigrateIteration([FromRoute] int iterationId, [FromQuery] string projectName)
        {
            List<Iteration> iterations = GetIterations(projectName).Where(i => i.id == iterationId).ToList();

            List<Iteration> migratedIterations = new List<Iteration>(iterations.Count);
            using (var requestData = tfsRequest.GetCloudHttpClient())
                iterations.ForEach(i =>
                {
                    migratedIterations.Add(iterationRequest.CreateIteration(requestData, i).Result);
                });
            return migratedIterations;
            ;
        }

        [HttpGet("Migrate")]
        public IList<Iteration> MigrateIterations([FromQuery] string projectName)
        {
            List<Iteration> iterations = GetIterations(projectName);

            List<Iteration> migratedIterations = new List<Iteration>(iterations.Count);
            using (var requestData = tfsRequest.GetCloudHttpClient())
                iterations.ForEach(i =>
                {
                    migratedIterations.Add(iterationRequest.CreateIteration(requestData, i).Result);
                });
            return migratedIterations;
        }
    }
}