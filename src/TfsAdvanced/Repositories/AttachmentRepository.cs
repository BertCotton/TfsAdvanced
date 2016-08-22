using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using TfsAdvanced.Data;
using TfsAdvanced.Infrastructure;
using Microsoft.Extensions.OptionsModel;

namespace TfsAdvanced.Repositories
{
    public class AttachmentRepository
    {
        private AppSettings appSettings;

        public AttachmentRepository(IOptions<AppSettings> appSettings)
        {
            this.appSettings = appSettings.Value;
        }

        public async Task SaveAttachment(Attachment attachment)
        {
            using (var context = new DataContext(appSettings))
            {
                var existing = context.Attachments.FirstOrDefault(a => a.GUID == attachment.GUID);
                if (existing != null)
                {
                    existing.File = attachment.File;
                    existing.FileName = attachment.FileName;
                    existing.Comment = attachment.Comment;
                }
                else
                    context.Attachments.Add(attachment);

                await context.SaveChangesAsync();
            }
        }

        public List<int> GetAttachmentIds()
        {
            using (var context = new DataContext(appSettings))
            {
                return context.Attachments.Select(a => a.Id).ToList();
            }
        }

        public async Task<Attachment> GetAttachment(Guid guid)
        {
            using (var context = new DataContext(appSettings))
            {
                return await context.Attachments.FirstOrDefaultAsync(a => a.GUID == guid);
            }
        }

        public async Task<Attachment> GetAttachment(int id)
        {
            using (var context = new DataContext(appSettings))
            {
                return await context.Attachments.FirstOrDefaultAsync(a => a.Id == id);
            }
        }

        public List<Attachment> GetAttachments(int workItemId)
        {
            using (var context = new DataContext(appSettings))
            {
                return context.Attachments.Where(a => a.OriginalWorkId == workItemId).ToList();
            }
        }
    }
}
