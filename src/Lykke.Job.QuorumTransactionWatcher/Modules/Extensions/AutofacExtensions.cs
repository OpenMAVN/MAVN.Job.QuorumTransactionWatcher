using Autofac;
using Lykke.Common.Log;
using Lykke.Job.QuorumTransactionWatcher.Contract;
using Lykke.Job.QuorumTransactionWatcher.Settings.Job.Rabbit;
using Lykke.RabbitMqBroker.Publisher;

namespace Lykke.Job.QuorumTransactionWatcher.Modules.Extensions
{
    public static class AutofacExtensions
    {
        public static void RegisterEventPublishingService<T>(
            this ContainerBuilder builder,
            PublisherSettings settings)
            where T : ITransactionEvent
        {
            builder
                .Register(ctx => new JsonRabbitPublisher<T>
                (
                    ctx.Resolve<ILogFactory>(),
                    settings.ConnectionString,
                    Context.GetEndpointName<T>()
                ))
                .As<IRabbitPublisher<T>>()
                .SingleInstance();
        }

        public static IRabbitPublisher<T> ResolveEventPublishingService<T>(
            this IComponentContext context)
            where T : ITransactionEvent
        {
            return context.Resolve<IRabbitPublisher<T>>();
        }
    }
}
