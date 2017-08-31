using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using ServiceConnect;
using ServiceConnect.Interfaces;

namespace TfsAdvanced.MessageBus
{
    public static class MessageBusExtensions
    {
        public static void AddMessageBus(this ContainerBuilder builder)
        {
            var bus = Bus.Initialize();
            builder.RegisterInstance(bus).As<IBus>();
        }
    }
}
