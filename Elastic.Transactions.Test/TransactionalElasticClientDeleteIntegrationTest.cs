using System.Transactions;
using Elasticsearch.Net;
using FluentAssertions;
using Nest;
using Nito.AsyncEx.Synchronous;
using NUnit.Framework;

namespace Elastic.Transactions.Test
{
    [TestFixture]
    public class TransactionalElasticClientDeleteIntegrationTest : AbstractIntegrationTest
    {
        private TransactionalElasticClient _transactionalElasticClient;

        [SetUp]
        public void SetUpClassUnderTest()
        {
            _transactionalElasticClient = new TransactionalElasticClient(ElasticClient, WaitForStatus.Green);
        }

        [Test]
        public void DeleteDescriptorWithoutTransaction()
        {
            var deleteResponse = _transactionalElasticClient.Delete<TestObject>("id");
            deleteResponse.IsValid.Should().BeTrue();
        }

        [Test]
        public void DeleteDescriptorWithTransaction_NotCommitted_ShouldNotBeDeleted()
        {
            ElasticClient.Index(new TestObject() {Id = "id"});

            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.Delete<TestObject>("id");
            }

            ElasticClient.Get<TestObject>("id").Source.Should().NotBeNull();
        }

        [Test]
        public void DeleteDescriptorWithTransaction_Committed_ShouldBeDeleted()
        {
            ElasticClient.Index(new TestObject() {Id = "id"});

            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.Delete<TestObject>("id");
                txSc.Complete();
            }

            ElasticClient.Get<TestObject>("id").Source.Should().BeNull();
        }

        [Test]
        public void DeleteDescriptorWithTransaction_MultipleObjects_NotCommitted_NothingDeleted()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id1"});
            _transactionalElasticClient.Index(new TestObject() {Id = "id2"});

            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.Delete<TestObject>("id1");
                _transactionalElasticClient.Delete<TestObject>("id2");
            }

            ElasticClient.Get<TestObject>("id1").Source.Should().NotBeNull();
            ElasticClient.Get<TestObject>("id2").Source.Should().NotBeNull();
        }

        [Test]
        public void DeleteDescriptorWithTransaction_MultipleObjects_Committed_AllShouldBeDeleted()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id1"});
            _transactionalElasticClient.Index(new TestObject() {Id = "id2"});

            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.Delete<TestObject>("id1");
                _transactionalElasticClient.Delete<TestObject>("id2");
                txSc.Complete();
            }

            ElasticClient.Get<TestObject>("id1").Source.Should().BeNull();
            ElasticClient.Get<TestObject>("id2").Source.Should().BeNull();
        }

        [Test]
        public void DeleteRequestWithoutTransaction()
        {
            var deleteResponse = _transactionalElasticClient.Delete(new DeleteRequest<TestObject>(new TestObject() {Id = "id"}));
            deleteResponse.IsValid.Should().BeTrue();
        }

        [Test]
        public void DeleteRequestWithTransaction_NotCommitted_ShouldNotBeDeleted()
        {
            ElasticClient.Index(new TestObject() {Id = "id"});

            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.Delete(new DeleteRequest<TestObject>(new TestObject() {Id = "id"}));
            }

            ElasticClient.Get<TestObject>("id").Source.Should().NotBeNull();
        }

        [Test]
        public void DeleteRequestWithTransaction_Committed_ShouldBeDeleted()
        {
            ElasticClient.Index(new TestObject() {Id = "id"});

            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.Delete(new DeleteRequest<TestObject>(new TestObject() {Id = "id"}));
                txSc.Complete();
            }

            ElasticClient.Get<TestObject>("id").Source.Should().BeNull();
        }

        [Test]
        public void DeleteRequestWithTransaction_MultipleObjects_NotCommitted_NothingDeleted()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id1"});
            _transactionalElasticClient.Index(new TestObject() {Id = "id2"});

            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.Delete(new DeleteRequest<TestObject>(new TestObject() {Id = "id1"}));
                _transactionalElasticClient.Delete(new DeleteRequest<TestObject>(new TestObject() {Id = "id2"}));
            }

            ElasticClient.Get<TestObject>("id1").Source.Should().NotBeNull();
            ElasticClient.Get<TestObject>("id2").Source.Should().NotBeNull();
        }

        [Test]
        public void DeleteRequestWithTransaction_MultipleObjects_Committed_AllShouldBeDeleted()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id1"});
            _transactionalElasticClient.Index(new TestObject() {Id = "id2"});

            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.Delete(new DeleteRequest<TestObject>(new TestObject() {Id = "id1"}));
                _transactionalElasticClient.Delete(new DeleteRequest<TestObject>(new TestObject() {Id = "id2"}));
                txSc.Complete();
            }

            ElasticClient.Get<TestObject>("id1").Source.Should().BeNull();
            ElasticClient.Get<TestObject>("id2").Source.Should().BeNull();
        }

        [Test]
        public void DeleteAsyncDescriptorWithoutTransaction()
        {
            var deleteResponse = _transactionalElasticClient.DeleteAsync<TestObject>("id").WaitAndUnwrapException();
            deleteResponse.IsValid.Should().BeTrue();
        }

        [Test]
        public void DeleteAsyncDescriptorWithTransaction_NotCommitted_ShouldNotBeDeleted()
        {
            ElasticClient.Index(new TestObject() {Id = "id"});

            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.DeleteAsync<TestObject>("id").WaitAndUnwrapException();
            }

            ElasticClient.Get<TestObject>("id").Source.Should().NotBeNull();
        }

        [Test]
        public void DeleteAsyncDescriptorWithTransaction_Committed_ShouldBeDeleted()
        {
            ElasticClient.Index(new TestObject() {Id = "id"});

            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.DeleteAsync<TestObject>("id").WaitAndUnwrapException();
                txSc.Complete();
            }

            ElasticClient.Get<TestObject>("id").Source.Should().BeNull();
        }

        [Test]
        public void DeleteAsyncDescriptorWithTransaction_MultipleObjects_NotCommitted_NothingDeleted()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id1"});
            _transactionalElasticClient.Index(new TestObject() {Id = "id2"});

            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.DeleteAsync<TestObject>("id1").WaitAndUnwrapException();
                _transactionalElasticClient.DeleteAsync<TestObject>("id2").WaitAndUnwrapException();
            }

            ElasticClient.Get<TestObject>("id1").Source.Should().NotBeNull();
            ElasticClient.Get<TestObject>("id2").Source.Should().NotBeNull();
        }

        [Test]
        public void DeleteAsyncDescriptorWithTransaction_MultipleObjects_Committed_AllShouldBeDeleted()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id1"});
            _transactionalElasticClient.Index(new TestObject() {Id = "id2"});

            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.DeleteAsync<TestObject>("id1").WaitAndUnwrapException();
                _transactionalElasticClient.DeleteAsync<TestObject>("id2").WaitAndUnwrapException();
                txSc.Complete();
            }

            ElasticClient.Get<TestObject>("id1").Source.Should().BeNull();
            ElasticClient.Get<TestObject>("id2").Source.Should().BeNull();
        }

        [Test]
        public void DeleteAsyncRequestWithoutTransaction()
        {
            var deleteResponse = _transactionalElasticClient.DeleteAsync(new DeleteRequest<TestObject>(new TestObject() {Id = "id"})).WaitAndUnwrapException();
            deleteResponse.IsValid.Should().BeTrue();
        }

        [Test]
        public void DeleteAsyncRequestWithTransaction_NotCommitted_ShouldNotBeDeleted()
        {
            ElasticClient.Index(new TestObject() {Id = "id"});

            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.DeleteAsync(new DeleteRequest<TestObject>(new TestObject() {Id = "id"})).WaitAndUnwrapException();
            }

            ElasticClient.Get<TestObject>("id").Source.Should().NotBeNull();
        }

        [Test]
        public void DeleteAsyncRequestWithTransaction_Committed_ShouldBeDeleted()
        {
            ElasticClient.Index(new TestObject() {Id = "id"});

            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.DeleteAsync(new DeleteRequest<TestObject>(new TestObject() {Id = "id"})).WaitAndUnwrapException();
                txSc.Complete();
            }

            ElasticClient.Get<TestObject>("id").Source.Should().BeNull();
        }

        [Test]
        public void DeleteAsyncRequestWithTransaction_MultipleObjects_NotCommitted_NothingDeleted()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id1"});
            _transactionalElasticClient.Index(new TestObject() {Id = "id2"});

            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.DeleteAsync(new DeleteRequest<TestObject>(new TestObject() {Id = "id1"})).WaitAndUnwrapException();
                _transactionalElasticClient.DeleteAsync(new DeleteRequest<TestObject>(new TestObject() {Id = "id2"})).WaitAndUnwrapException();
            }

            ElasticClient.Get<TestObject>("id1").Source.Should().NotBeNull();
            ElasticClient.Get<TestObject>("id2").Source.Should().NotBeNull();
        }

        [Test]
        public void DeleteAsyncRequestWithTransaction_MultipleObjects_Committed_AllShouldBeDeleted()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id1"});
            _transactionalElasticClient.Index(new TestObject() {Id = "id2"});

            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.DeleteAsync(new DeleteRequest<TestObject>(new TestObject() {Id = "id1"})).WaitAndUnwrapException();
                _transactionalElasticClient.DeleteAsync(new DeleteRequest<TestObject>(new TestObject() {Id = "id2"})).WaitAndUnwrapException();
                txSc.Complete();
            }

            ElasticClient.Get<TestObject>("id1").Source.Should().BeNull();
            ElasticClient.Get<TestObject>("id2").Source.Should().BeNull();
        }
    }
}