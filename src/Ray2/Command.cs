using System;

namespace Ray2
{
    public abstract class Command<TStateKey> : Event<TStateKey>, ICommand, ICommand<TStateKey>
    {
        public Command()
        {
            
        }

        public Command(Guid commandId):this(commandId.ToString("N"))
        {
        }
        public Command(string commandId)
        {
            RelationEvent = commandId;
        }

    }

    public interface ICommand
    {
    }

    public interface ICommand<TStateKey>
    {

    }
}