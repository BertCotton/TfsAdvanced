using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TfsAdvanced.DataStore.Repository;
using TfsAdvanced.Models;
using TfsAdvanced.Models.Infrastructure;
using TfsAdvanced.Updater;
using TFSAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;

namespace TFSAdvanced.Updater.Tasks
{
    public class ReleaseUpdater : UpdaterBase
    {
        private readonly ReleaseRepository releaseRepository;
        private readonly UpdateStatusRepository updateStatusRepository;
        private readonly ProjectRepository projectRepository;
        private readonly RequestData requestData;

        public ReleaseUpdater(ILogger<ReleaseUpdater> logger, ReleaseRepository releaseRepository, UpdateStatusRepository updateStatusRepository, ProjectRepository projectRepository, RequestData requestData) : base(logger)
        {
            this.releaseRepository = releaseRepository;
            this.updateStatusRepository = updateStatusRepository;
            this.projectRepository = projectRepository;
            this.requestData = requestData;
        }

        protected override async Task Update(bool initialize)
        {
            if (initialize && !releaseRepository.IsEmpty())
                return;


            IList<Release> releases = new List<Release>();
            foreach (var project in projectRepository.GetAll())
            {
                    foreach (var release in await GetAsync.FetchResponseList<Models.Releases.Release>(requestData, $"{requestData.BaseReleaseManagerAddress}/{project.ProjectId}/_apis/Release/releases?api-version=3.0-preview.1"))
                    {
                        
                        var releaseDto = new Release
                        {
                            Id = release.id,
                            Created = release.createdOn,
                            CreatedBy = new User
                            {
                                UniqueName = release.createdBy.uniqueName,
                                IconUrl = release.createdBy.imageUrl,
                                Name = release.createdBy.displayName
                            },
                            Name = release.name,
                            Description = release.description,
                            Reason = release.reason,
                            ReleaseDefinition = new ReleaseDefinition
                            {
                                Id = release.releaseDefinition.id
                            },
                            Project = new Project
                            {
                                ProjectId = release.projectReference.id
                            }
                        };
                        releases.Add(releaseDto);
                    }
            }
            await releaseRepository.Update(releases);
            updateStatusRepository.UpdateStatus(new UpdateStatus { LastUpdate = DateTime.Now, UpdatedRecords = releases.Count, UpdaterName = GetType().Name });

        }
    }
}
