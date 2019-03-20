using System;

namespace Aggregator.Messages
{
    public abstract class MessageBase
    {
        public Guid MessageId { get; }

        protected MessageBase()
        {
            MessageId = Guid.NewGuid();
        }
    }
}