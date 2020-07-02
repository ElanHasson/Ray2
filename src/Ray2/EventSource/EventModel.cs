using System;

namespace Ray2.EventSource
{
    public class EventModel
    {
        public EventModel(IEvent @event, string typeCode, long version)
        {
            Event = @event;
            TypeCode = typeCode;
            Version = version;
            Id = @event.GetId();
        }
        public EventModel(IEvent @event):this(@event, @event.TypeCode, @event.Version)
        {
           
        }
        public IEvent Event { get; }
        public string TypeCode { get; }
        public Int64 Version { get; }
        public object Id { get; }
    }
}
