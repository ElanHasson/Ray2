using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Ray2
{
    /// <summary>
    /// Event abstract class
    /// </summary>
    /// <typeparam name="TStateKey">Id Type</typeparam>
    public abstract class Event<TStateKey> : IEvent<TStateKey>
    {
        public Event()
        {
            this.TypeCode = this.GetType().FullName;
            this.Timestamp = this.GetCurrentTimeUnix();
        }
        /// <summary>
        /// Event abstract class
        /// </summary>
        /// <param name="event">Relation Event</param>
        public Event(IEvent @event) : this()
        {
            this.RelationEvent = @event.GetRelationKey();
        }
        /// <summary>
        /// State Id  <see cref="IState{TStateKey}"/>  
        /// </summary>
        public TStateKey Id { get; set; }
        /// <summary>
        ///  the version number of <see cref="IState{TStateKey}"/>  
        /// </summary>
        public long Version { get; set; }
        /// <summary>
        ///  Event release timestamp
        /// </summary>
        [JsonProperty]
        public long Timestamp { get; private set; }
        /// <summary>
        /// Event type fullname
        /// </summary>
        [JsonProperty]
        public string TypeCode { get; private set; }
        /// <summary>
        /// Relation Event
        /// </summary>
        public string RelationEvent { get;  set; }
        /// <summary>
        ///  Generate Relation key
        /// </summary>
        /// <returns></returns>
        public string GetRelationKey()
        {
            return $"{Id}-{TypeCode}-{Version}";
        }
        /// <summary>
        /// Get Id
        /// </summary>
        /// <returns></returns>
        public object GetId()
        {
            return this.Id;
        }
        public override string ToString()
        {
            return $"TypeCode:{TypeCode},Id:{Id},VersionNo:{Version},Timestamp:{Timestamp},RelationEvent:{RelationEvent}";
        }


        public long GetCurrentTimeUnix()
        {
            TimeSpan cha = (DateTime.Now - TimeZoneInfo.ConvertTimeToUtc(new System.DateTime(1970, 1, 1)));
            long t = (long)cha.TotalMilliseconds;
            return t;
        }

    }
}
