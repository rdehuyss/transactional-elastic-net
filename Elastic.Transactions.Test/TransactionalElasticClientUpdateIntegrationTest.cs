using System.Transactions;
using Elasticsearch.Net;
using FluentAssertions;
using Nest;
using Nito.AsyncEx.Synchronous;
using NUnit.Framework;

namespace Elastic.Transactions.Test
{
    [TestFixture]
    public class TransactionalElasticClientUpdateIntegrationTest : AbstractIntegrationTest
    {
        private TransactionalElasticClient _transactionalElasticClient;

        [SetUp]
        public void SetUpClassUnderTest()
        {
            _transactionalElasticClient = new TransactionalElasticClient(ElasticClient);
        }

        [Test]
        public void UpdateDescriptorWithoutTransaction()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id", Value = "value1"});

            _transactionalElasticClient.Update<TestObject>("id", descriptor => descriptor.Doc(new TestObject() {Value = "value2"}));

            ElasticClient.Refresh(CurrentTestIndexName());
            ElasticClient.Get<TestObject>("id").Source.Value.Should().Be("value2");
        }

        [Test]
        public void UpdateDescriptorWithTransaction_NotCommitted_ShouldBeOldValue()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id", Value = "value1"});

            using (TransactionScope txSc = new TransactionScope())
            {
                var updateResponse = _transactionalElasticClient.Update<TestObject>("id", descriptor => descriptor.Doc(new TestObject() {Value = "value2"}));
                updateResponse.Should().NotBeNull();
                updateResponse.IsValid.Should().BeTrue();
                updateResponse.ApiCall.HttpStatusCode.Value.Should().Be(200);
            }

            ElasticClient.Source<TestObject>("id").Value.Should().Be("value1");
        }

        [Test]
        public void UpdateDescriptorWithTransaction_Committed_ShouldNotBeNewValue()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id", Value = "value1"});

            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.Update<TestObject>("id", descriptor => descriptor.Doc(new TestObject() {Value = "value2"}));
                txSc.Complete();
            }

            ElasticClient.Source<TestObject>("id").Value.Should().Be("value2");
        }

        [Test]
        public void UpdateDescriptorWithTransaction_MultipleObjects_NotCommitted_AllShouldBeNull()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id1", Value = "value1"});
            _transactionalElasticClient.Index(new TestObject() {Id = "id2", Value = "value2"});

            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.Update<TestObject>("id1", descriptor => descriptor.Doc(new TestObject() {Value = "valueA"}));
                _transactionalElasticClient.Update<TestObject>("id2", descriptor => descriptor.Doc(new TestObject() {Value = "valueB"}));
            }

            ElasticClient.Source<TestObject>("id1").Value.Should().Be("value1");
            ElasticClient.Source<TestObject>("id2").Value.Should().Be("value2");
        }

        [Test]
        public void UpdateDescriptorWithTransaction_MultipleObjects_Committed_AllShouldBeIndexed()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id1", Value = "value1"});
            _transactionalElasticClient.Index(new TestObject() {Id = "id2", Value = "value2"});

            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.Update<TestObject>("id1", descriptor => descriptor.Doc(new TestObject() {Value = "valueA"}));
                _transactionalElasticClient.Update<TestObject>("id2", descriptor => descriptor.Doc(new TestObject() {Value = "valueB"}));
                txSc.Complete();
            }

            ElasticClient.Source<TestObject>("id1").Value.Should().Be("valueA");
            ElasticClient.Source<TestObject>("id2").Value.Should().Be("valueB");
        }

        [Test]
        public void UpdateRequestWithoutTransaction()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id", Value = "value1"});

            _transactionalElasticClient.Update(new UpdateRequest<TestObject, TestObject>("id") {Doc = new TestObject() {Value = "valueA"}});

            ElasticClient.Source<TestObject>("id").Value.Should().Be("valueA");
        }

        [Test]
        public void UpdateRequestWithTransaction_NotCommitted_ShouldBeOriginalValue()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id", Value = "value1"});
            using (TransactionScope txSc = new TransactionScope())
            {
                var updateResponse = _transactionalElasticClient.Update(new UpdateRequest<TestObject, TestObject>("id") {Doc = new TestObject() {Value = "valueA"}});
                updateResponse.Should().NotBeNull();
                updateResponse.IsValid.Should().BeTrue();
                updateResponse.ApiCall.HttpStatusCode.Value.Should().Be(200);
            }

            ElasticClient.Source<TestObject>("id").Value.Should().Be("value1");
        }

        [Test]
        public void UpdateRequestWithTransaction_Committed_ShouldNotBeNewValue()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id", Value = "value1"});
            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.Update(new UpdateRequest<TestObject, TestObject>("id") {Doc = new TestObject() {Value = "valueA"}});
                txSc.Complete();
            }

            ElasticClient.Source<TestObject>("id").Value.Should().Be("valueA");
        }

        [Test]
        public void UpdateRequestWithTransaction_MultipleObjects_NotCommitted_AllShouldHaveOriginalValue()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id1", Value = "value1"});
            _transactionalElasticClient.Index(new TestObject() {Id = "id2", Value = "value2"});

            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.Update(new UpdateRequest<TestObject, TestObject>("id1") {Doc = new TestObject() {Value = "valueA"}});
                _transactionalElasticClient.Update(new UpdateRequest<TestObject, TestObject>("id2") {Doc = new TestObject() {Value = "valueB"}});
            }

            ElasticClient.Source<TestObject>("id1").Value.Should().Be("value1");
            ElasticClient.Source<TestObject>("id2").Value.Should().Be("value2");
        }

        [Test]
        public void UpdateRequestWithTransaction_MultipleObjects_Committed_AllShouldHaveNewValue()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id1", Value = "value1"});
            _transactionalElasticClient.Index(new TestObject() {Id = "id2", Value = "value2"});

            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.Update(new UpdateRequest<TestObject, TestObject>("id1") {Doc = new TestObject() {Value = "valueA"}});
                _transactionalElasticClient.Update(new UpdateRequest<TestObject, TestObject>("id2") {Doc = new TestObject() {Value = "valueB"}});
                txSc.Complete();
            }

            ElasticClient.Source<TestObject>("id1").Value.Should().Be("valueA");
            ElasticClient.Source<TestObject>("id2").Value.Should().Be("valueB");
        }

        [Test]
        public void UpdateDescriptorAsyncWithoutTransaction()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id", Value = "value1"});

            _transactionalElasticClient.UpdateAsync<TestObject>("id", descriptor => descriptor.Doc(new TestObject() {Value = "value2"})).WaitAndUnwrapException();

            ElasticClient.Refresh(CurrentTestIndexName());
            ElasticClient.Get<TestObject>("id").Source.Value.Should().Be("value2");
        }

        [Test]
        public void UpdateDescriptorAsyncWithTransaction_NotCommitted_ShouldBeOldValue()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id", Value = "value1"});

            using (TransactionScope txSc = new TransactionScope())
            {
                var updateResponse = _transactionalElasticClient.UpdateAsync<TestObject>("id", descriptor => descriptor.Doc(new TestObject() {Value = "value2"})).WaitAndUnwrapException();
                updateResponse.Should().NotBeNull();
                updateResponse.IsValid.Should().BeTrue();
                updateResponse.ApiCall.HttpStatusCode.Value.Should().Be(200);
            }

            ElasticClient.Source<TestObject>("id").Value.Should().Be("value1");
        }

        [Test]
        public void UpdateDescriptorAsyncWithTransaction_Committed_ShouldNotBeNewValue()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id", Value = "value1"});

            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.UpdateAsync<TestObject>("id", descriptor => descriptor.Doc(new TestObject() {Value = "value2"})).WaitAndUnwrapException();
                txSc.Complete();
            }

            ElasticClient.Source<TestObject>("id").Value.Should().Be("value2");
        }

        [Test]
        public void UpdateDescriptorAsyncWithTransaction_MultipleObjects_NotCommitted_AllShouldBeNull()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id1", Value = "value1"});
            _transactionalElasticClient.Index(new TestObject() {Id = "id2", Value = "value2"});

            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.UpdateAsync<TestObject>("id1", descriptor => descriptor.Doc(new TestObject() {Value = "valueA"})).WaitAndUnwrapException();
                _transactionalElasticClient.UpdateAsync<TestObject>("id2", descriptor => descriptor.Doc(new TestObject() {Value = "valueB"})).WaitAndUnwrapException();
            }

            ElasticClient.Source<TestObject>("id1").Value.Should().Be("value1");
            ElasticClient.Source<TestObject>("id2").Value.Should().Be("value2");
        }

        [Test]
        public void UpdateDescriptorAsyncWithTransaction_MultipleObjects_Committed_AllShouldBeIndexed()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id1", Value = "value1"});
            _transactionalElasticClient.Index(new TestObject() {Id = "id2", Value = "value2"});

            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.UpdateAsync<TestObject>("id1", descriptor => descriptor.Doc(new TestObject() {Value = "valueA"})).WaitAndUnwrapException();
                _transactionalElasticClient.UpdateAsync<TestObject>("id2", descriptor => descriptor.Doc(new TestObject() {Value = "valueB"})).WaitAndUnwrapException();
                txSc.Complete();
            }

            ElasticClient.Source<TestObject>("id1").Value.Should().Be("valueA");
            ElasticClient.Source<TestObject>("id2").Value.Should().Be("valueB");
        }

        [Test]
        public void UpdateRequestAsyncWithoutTransaction()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id", Value = "value1"});

            _transactionalElasticClient.UpdateAsync(new UpdateRequest<TestObject, TestObject>("id") {Doc = new TestObject() {Value = "valueA"}}).WaitAndUnwrapException();

            ElasticClient.Source<TestObject>("id").Value.Should().Be("valueA");
        }

        [Test]
        public void UpdateRequestAsyncWithTransaction_NotCommitted_ShouldBeOriginalValue()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id", Value = "value1"});
            using (TransactionScope txSc = new TransactionScope())
            {
                var updateResponse = _transactionalElasticClient.UpdateAsync(new UpdateRequest<TestObject, TestObject>("id") {Doc = new TestObject() {Value = "valueA"}}).WaitAndUnwrapException();
                updateResponse.Should().NotBeNull();
                updateResponse.IsValid.Should().BeTrue();
                updateResponse.ApiCall.HttpStatusCode.Value.Should().Be(200);
            }

            ElasticClient.Source<TestObject>("id").Value.Should().Be("value1");
        }

        [Test]
        public void UpdateRequestAsyncWithTransaction_Committed_ShouldNotBeNewValue()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id", Value = "value1"});
            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.UpdateAsync(new UpdateRequest<TestObject, TestObject>("id") {Doc = new TestObject() {Value = "valueA"}}).WaitAndUnwrapException();
                txSc.Complete();
            }

            ElasticClient.Source<TestObject>("id").Value.Should().Be("valueA");
        }

        [Test]
        public void UpdateRequestAsyncWithTransaction_MultipleObjects_NotCommitted_AllShouldHaveOriginalValue()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id1", Value = "value1"});
            _transactionalElasticClient.Index(new TestObject() {Id = "id2", Value = "value2"});

            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.UpdateAsync(new UpdateRequest<TestObject, TestObject>("id1") {Doc = new TestObject() {Value = "valueA"}}).WaitAndUnwrapException();
                _transactionalElasticClient.UpdateAsync(new UpdateRequest<TestObject, TestObject>("id2") {Doc = new TestObject() {Value = "valueB"}}).WaitAndUnwrapException();
            }

            ElasticClient.Source<TestObject>("id1").Value.Should().Be("value1");
            ElasticClient.Source<TestObject>("id2").Value.Should().Be("value2");
        }

        [Test]
        public void UpdateRequestAsyncWithTransaction_MultipleObjects_Committed_AllShouldHaveNewValue()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id1", Value = "value1"});
            _transactionalElasticClient.Index(new TestObject() {Id = "id2", Value = "value2"});

            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.UpdateAsync(new UpdateRequest<TestObject, TestObject>("id1") {Doc = new TestObject() {Value = "valueA"}}).WaitAndUnwrapException();
                _transactionalElasticClient.UpdateAsync(new UpdateRequest<TestObject, TestObject>("id2") {Doc = new TestObject() {Value = "valueB"}}).WaitAndUnwrapException();
                txSc.Complete();
            }

            ElasticClient.Source<TestObject>("id1").Value.Should().Be("valueA");
            ElasticClient.Source<TestObject>("id2").Value.Should().Be("valueB");
        }
    }
}