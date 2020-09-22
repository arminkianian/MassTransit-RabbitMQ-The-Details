using System;
using System.Threading.Tasks;
using MassTransit;
using Rabbit.Details.Commands;

namespace Rabbit.Details.CommandHandlers
{
    public class AnotherUpdateAccountCommandHandler : IConsumer<UpdateAccountCommand>
    {
        public Task Consume(ConsumeContext<UpdateAccountCommand> context)
        {
            Console.WriteLine("Another Command received: {0}", context.Message.AccountNumber);
            return Task.CompletedTask;
        }
    }
}