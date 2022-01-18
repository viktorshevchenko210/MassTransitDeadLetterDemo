using MassTransit;
using System;
using System.Threading.Tasks;

namespace MassTransitDeadLetterDemo
{
    public class Consumer : IConsumer<Message>
    {
        public async Task Consume(ConsumeContext<Message> context)
        {
            Console.WriteLine(context.Message.Id);
        }
    }
}
