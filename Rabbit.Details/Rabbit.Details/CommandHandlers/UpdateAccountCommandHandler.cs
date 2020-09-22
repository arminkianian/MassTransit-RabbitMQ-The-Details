using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using Rabbit.Details.Commands;

namespace Rabbit.Details.CommandHandlers
{
    public class UpdateAccountCommandHandler : IConsumer<UpdateAccountCommand>
    {
        public Task Consume(ConsumeContext<UpdateAccountCommand> context)
        {
            Console.WriteLine("Command received: {0} on {1}", context.Message.AccountNumber, context.ReceiveContext.InputAddress);
            return Task.CompletedTask;
        }
    }
}
