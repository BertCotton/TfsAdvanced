using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TfsAdvanced.Data;
using TfsAdvanced.Infrastructure;
using Microsoft.Extensions.OptionsModel;
using Newtonsoft.Json;

namespace TfsAdvanced.ServiceRequests
{
    public class AttachmentRequest
    {
        private readonly AppSettings appSettings;

        public AttachmentRequest(IOptions<AppSettings> appSettings)
        {
            this.appSettings = appSettings.Value;
        }

        public async Task<Attachment> GetAttachment(RequestData requestdata, int workItemId, Relations relation)
        {
              var name = "Attachment";
                if (relation.attributes.ContainsKey("name"))
                    name = relation.attributes["name"].ToString();

                var comment = "";
                if (relation.attributes.ContainsKey("comment"))
                    comment = relation.attributes["comment"].ToString();
                var guid = new Guid(new Uri(relation.url).Segments.Last());
                byte[] fileContents = await requestdata.HttpClient.GetByteArrayAsync(relation.url);
                return new Attachment
                {
                    File = fileContents,
                    OriginalWorkId = workItemId,
                    FileName = name,
                    Comment = comment,
                    GUID = guid
                };
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public async Task<List<Attachment>> GetAttachmentFromComment(RequestData requestData, WorkItem workItem)
        {
            var description = workItem.fields["System.Description"];
            var index = 0;
            var urls = new List<string>();
            while (index < description.Length)
            {
                var hrefIndex = description.IndexOf("href", index);
                if (hrefIndex < 0)
                    break;
                var urlStartIndex = description.IndexOf('"', hrefIndex)+1;
                if (urlStartIndex < 0)
                    break;
                var urlEndIndex = description.IndexOf('"', urlStartIndex+1);
                if (urlEndIndex < 0)
                    break;
                urls.Add(description.Substring(urlStartIndex, (urlEndIndex - urlStartIndex)));
                index = urlEndIndex;
            }

            List<Attachment> attachments = new List<Attachment>();
            foreach (var url in urls)
            {
                var name = new Uri(url).Segments.Last();
                if (appSettings.WorkItemSettings.ExtensionsFromDescription.Any(e => name.ToUpper().Contains(e.ToUpper())))
                {
                    var response = await requestData.HttpClient.GetAsync(url);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var fileContents = await response.Content.ReadAsByteArrayAsync();
                        attachments.Add(new Attachment
                        {
                            File = fileContents,
                            OriginalWorkId = workItem.id,
                            FileName = name

                        });
                    }
                }
                else
                {
                    Debug.WriteLine($"Extension for attachment {name} is not in the approved listed.");
                }
            }

            return attachments;
        }

        public async Task<AttachmentUploadResult> UploadAttachment(RequestData requestData, Attachment attachment)
        {
            var url = $"{requestData.BaseAddress}/_apis/wit/attachments?api-version=1.0&filename={attachment.FileName}";

            Stream stream = new MemoryStream(attachment.File);
            HttpContent content = new StreamContent(stream);
            var response = await requestData.HttpClient.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<AttachmentUploadResult>(responseString);
            }
                
            return new AttachmentUploadResult
            {
                url = responseString
            };
        }
        
    }
}
