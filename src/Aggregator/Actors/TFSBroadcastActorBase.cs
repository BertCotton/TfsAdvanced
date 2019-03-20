using System.Collections.Generic;
using Aggregator.Messages;
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.DI.Core;
using Akka.Routing;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;

namespace TFSAdvancedAggregator.Actors
{
    public abstract class TFSBroadcastActorBase<T, TObject> : TFSActorBase where T: TFSActorBase
    {
        protected IList<TObject> UpdatedModels;
        protected TFSBroadcastActorBase(ILogger logger, int poolSize, params string[] messageContexts) : base(logger)
        {
            UpdatedModels = new List<TObject>();
            Receive<SimpleMessages.SUBSCRIBE>(_ =>
            {
                foreach (var messageContext in messageContexts)
                {
                    logger.LogInformation($"{GetType().Name} Is subscribing to the {messageContext} topic");
                    Mediator.Tell(new Subscribe(messageContext, Self), Self);    
                }
            });
            Receive<SubscribeAck>(ack =>
            {
                logger.LogInformation($"{GetType().Name} Subscribed to {ack.Subscribe.Topic}");
            });

            Receive<TObject>(model => { UpdatedModels.Add(model); });

            Receive<SimpleMessages.GET>(_ => Sender.Tell(UpdatedModels));
            
            var prop = Context.System.DI().Props<T>().WithRouter(new RoundRobinPool(poolSize));
            var actor = Context.System.ActorOf(prop, typeof(T).Name);
            ReceiveAny(message =>
            {
                actor.Tell(message,Self);
            });
        }

    }
}