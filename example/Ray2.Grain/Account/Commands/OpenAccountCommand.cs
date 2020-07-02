using System;

namespace Ray2.Grain.Account.Commands
{
    public class OpenAccountCommand : Command<long>
    {
        public OpenAccountCommand() { }
        public OpenAccountCommand(Guid commandId) : base(commandId)
        { }
    }
}