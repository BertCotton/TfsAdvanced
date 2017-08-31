using System;
using System.Collections.Generic;
using System.Text;
using ServiceConnect.Interfaces;

namespace TfsAdvanced.MessageBus.Messages
{
    public class UpdatedPullRequestMessage : Message
    {
        public UpdatedPullRequestMessage(Guid correlationId) : base(correlationId)
        {
        }
    }
}
