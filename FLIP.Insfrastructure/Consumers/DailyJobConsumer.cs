using BuildingBlock.Contracts;
using MassTransit;

namespace FLIP.Infrastructure.Consumers;

public class DailyJobConsumer : IConsumer<DailyJobMessage>
{
    public Task Consume(ConsumeContext<DailyJobMessage> context)
    {
        throw new NotImplementedException();
    }
}
