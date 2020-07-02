using System;
using System.Collections.Generic;
using System.Text;
using TestStack.BDDfy;
using Xunit;

namespace Ray2.PostgreSQL.Test
{
    public class StateStorageTests
    {
        private PostgreSQL.StateStorage container;
        private string TableName;
        private TestState selectState;
        private TestState deleteState;

        public StateStorageTests()
        {
            this.TableName = "st_ContainerTest";
            var sp = FakeConfig.BuildServiceProvider();
            container = new PostgreSQL.StateStorage(sp, FakeConfig.ProviderName);
        }
        [Fact]
        public void should_Success()
        {
            TestState state = new TestState()
            {
                Id = 1,
                Test = DateTime.Now.Ticks.ToString()
            };
            this.When(f => f.WhenInsert(state.Id, state))
                .When(f => f.WhenUpdate(state.Id, state))
                .When(f => f.WhenSelect(state.Id, false))
                .When(f => f.WhenDelete(state.Id))
                .When(f => f.WhenSelect(state.Id, true))
                .Then(f => f.ThenSuccess(state))
                .BDDfy();
        }

        private void WhenDelete(object id)
        {
            container.DeleteAsync(TableName, id).GetAwaiter().GetResult();
        }
        private void WhenInsert(object id, TestState state)
        {
            container.InsertAsync(TableName, id, state).GetAwaiter().GetResult();
        }
        private void WhenUpdate(object id, TestState state)
        {
            state.Test = "abc";
            container.UpdateAsync(TableName, id, state).GetAwaiter().GetResult();
        }
        private void WhenSelect(object id, bool isDelete)
        {
            if (isDelete)
                deleteState = container.ReadAsync<TestState>(TableName, id).GetAwaiter().GetResult();
            else
                selectState = container.ReadAsync<TestState>(TableName, id).GetAwaiter().GetResult();
        }

        private void ThenSuccess(TestState state)
        {
            Assert.Null(deleteState);
            Assert.NotNull(selectState);
            selectState.Valid(state);
        }
    }
}
