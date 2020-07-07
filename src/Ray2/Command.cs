using System;

namespace Ray2
{
    public abstract class Command<TStateKey> : Event<TStateKey>, ICommand
    {
        public Command()
        {
            
        }

        public Command(Guid commandId):this(commandId.ToString("N"))
        {
        }
        public Command(string correlationId)
        {
            RelationEvent = correlationId;
        }

    }

    public interface ICommand
    {
    }

}