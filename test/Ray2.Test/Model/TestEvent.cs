﻿using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Ray2.Test.Model
{
    [EventPublish("test", "Default")]
    public class TestEvent : Event<int>
    {
        public static TestEvent Create(long v)
        {
            return new TestEvent()
            {
                Id = 100,
                Version = v
            };
        }

        public void Valid(TestEvent state)
        {
            Assert.Equal(state.Id, this.Id);
            Assert.Equal(state.Version, this.Version);
            Assert.Equal(state.TypeCode, this.TypeCode);
            Assert.Equal(state.Version, this.Version);
            Assert.Equal(state.Timestamp, this.Timestamp);
            Assert.Equal(state.RelationEvent, this.RelationEvent);
        }
    }

    public class TestEvent1 : Event<int>
    {
        public static TestEvent1 Create(long v)
        {
            return new TestEvent1()
            {
                Id = 100,
                Version = v
            };
        }
    }
}
