using System;
using System.Threading.Tasks;
using Aggregator.Messages;
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Microsoft.Extensions.Logging;
using TfsAdvanced.Models;

namespace TFSAdvancedAggregator.Actors
{
    public abstract class TFSActorBase : ReceiveActor
    {
        protected readonly ILogger logger;
        protected IActorRef Mediator { get; } 
        protected TFSActorBase(ILogger logger)
        {
            this.logger = logger;
            Mediator = DistributedPubSub.Get(Context.System).Mediator;
        }

        protected override void Unhandled(object message)
        {
            logger.LogWarning($"{GetType().Name} received a message {message.GetType().Name} and could not handle it.");
            base.Unhandled(message);
        }

        protected void BroadcastMessage(object message)
        {
            var topic = message.GetType().Name;
            logger.LogTrace($"{GetType().Name} is Broadcasting message type {message.GetType().Name} message on topic {topic}");
            Mediator.Tell(new Publish(topic, message), Self);
        }

        protected async Task HandleMessageAsync<T>(T message, Func<T, Task> pred) where T : MessageBase
        {
            try
            {
                
                LogTrace($"[{message.MessageId}] Message Handling for {message.GetType().Name}");
                var start = DateTime.Now;
                await pred(message);
                LogTrace($"[{message.MessageId}] Message Finished for {message.GetType().Name} in {DateTime.Now-start}");
            }
            catch (Exception e)
            {
                LogError(e);
                throw;
            }

        }
        
        protected void HandleMessage<T>(T message, Predicate<T> pred) where T : MessageBase
        {
            try
            {
                
                LogTrace($"[{message.MessageId}] Message Handling for {message.GetType().Name}");
                var start = DateTime.Now;
                pred(message);
                LogTrace($"[{message.MessageId}] Message Finished for {message.GetType().Name} in {DateTime.Now-start}");
            }
            catch (Exception e)
            {
                LogError(e);
                throw;
            }

        }
        
        protected void LogError(Exception e)
        {
            logger.LogError(e, $"[{Self.Path.Address}/{Self.Path.Uid}] {e.Message}");
        }


        protected void LogInformation(string message)
        {
            logger.LogInformation($"[{Self.Path.Address}/{Self.Path.Uid}] {message}");
        }
        
        protected void LogDebug(string message)
        {
            logger.LogDebug($"[{Self.Path.ToStringWithUid()}] {message}");
        }
        
        protected void LogTrace(string message)
        {
            logger.LogTrace($"[{Self.Path.Address}/{Self.Path.Uid}] {message}");
        }
    }
}