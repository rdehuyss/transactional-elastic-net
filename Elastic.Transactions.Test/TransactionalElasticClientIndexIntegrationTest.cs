using System.Transactions;
using Elasticsearch.Net;
using FluentAssertions;
using Nest;
using Nito.AsyncEx.Synchronous;
using NUnit.Framework;

namespace Elastic.Transactions.Test
{
    [TestFixture]
    public class TransactionalElasticClientIndexIntegrationTest : AbstractIntegrationTest
    {
        private TransactionalElasticClient _transactionalElasticClient;

        [SetUp]
        public void SetUpClassUnderTest()
        {
            _transactionalElasticClient = new TransactionalElasticClient(ElasticClient);
        }

        [Test]
        public void IndexDescriptorWithoutTransaction()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id"});

            ElasticClient.Get<TestObject>("id").Source.Should().NotBeNull();
        }

        [Test]
        public void IndexDescriptorWithTransaction_NotCommitted_ShouldBeNull()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                var indexResponse = _transactionalElasticClient.Index(new TestObject() {Id = "id"});
                indexResponse.Should().NotBeNull();
                indexResponse.IsValid.Should().BeTrue();
                indexResponse.ApiCall.HttpStatusCode.Value.Should().Be(200);
            }

            ElasticClient.Get<TestObject>("id").Source.Should().BeNull();
        }

        [Test]
        public void IndexDescriptorWithTransaction_Committed_ShouldNotBeNull()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.Index(new TestObject() {Id = "id"});
                txSc.Complete();
            }

            ElasticClient.Get<TestObject>("id").Source.Should().NotBeNull();
        }

        [Test]
        public void IndexDescriptorWithTransaction_MultipleObjects_NotCommitted_AllShouldBeNull()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.Index(new TestObject() {Id = "id1"});
                _transactionalElasticClient.Index(new TestObject() {Id = "id2"});
            }

            ElasticClient.Get<TestObject>("id1").Source.Should().BeNull();
            ElasticClient.Get<TestObject>("id2").Source.Should().BeNull();
        }

        [Test]
        public void IndexDescriptorWithTransaction_MultipleObjects_Committed_AllShouldBeIndexed()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.Index(new TestObject() {Id = "id1"});
                _transactionalElasticClient.Index(new TestObject() {Id = "id2"});
                txSc.Complete();
            }

            ElasticClient.Get<TestObject>("id1").Source.Should().NotBeNull();
            ElasticClient.Get<TestObject>("id2").Source.Should().NotBeNull();
        }

        [Test]
        public void IndexRequestWithoutTransaction()
        {
            _transactionalElasticClient.Index((IIndexRequest) new IndexRequest<TestObject>(new TestObject() {Id = "id"}));

            ElasticClient.Get<TestObject>("id").Source.Should().NotBeNull();
        }

        [Test]
        public void IndexRequestWithTransaction_NotCommitted_ShouldBeNull()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                var indexResponse = _transactionalElasticClient.Index((IIndexRequest)new IndexRequest<TestObject>(new TestObject() {Id = "id"}));
                indexResponse.Should().NotBeNull();
                indexResponse.IsValid.Should().BeTrue();
                indexResponse.ApiCall.HttpStatusCode.Value.Should().Be(200);
            }

            ElasticClient.Get<TestObject>("id").Source.Should().BeNull();
        }

        [Test]
        public void IndexRequestWithTransaction_Committed_ShouldNotBeNull()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.Index((IIndexRequest) new IndexRequest<TestObject>(new TestObject() {Id = "id"}));
                txSc.Complete();
            }

            ElasticClient.Get<TestObject>("id").Source.Should().NotBeNull();
        }

        [Test]
        public void IndexRequestWithTransaction_MultipleObjects_NotCommitted_AllShouldBeNull()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.Index((IIndexRequest)new IndexRequest<TestObject>(new TestObject() {Id = "id1"}));
                _transactionalElasticClient.Index((IIndexRequest)new IndexRequest<TestObject>(new TestObject() {Id = "id2"}));
            }

            ElasticClient.Get<TestObject>("id1").Source.Should().BeNull();
            ElasticClient.Get<TestObject>("id2").Source.Should().BeNull();
        }

        [Test]
        public void IndexRequestWithTransaction_MultipleObjects_Committed_AllShouldBeIndexed()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.Index((IIndexRequest)new IndexRequest<TestObject>(new TestObject() {Id = "id1"}));
                _transactionalElasticClient.Index((IIndexRequest)new IndexRequest<TestObject>(new TestObject() {Id = "id2"}));
                txSc.Complete();
            }

            ElasticClient.Get<TestObject>("id1").Source.Should().NotBeNull();
            ElasticClient.Get<TestObject>("id2").Source.Should().NotBeNull();
        }

        [Test]
        public void IndexDescriptorAsyncWithoutTransaction()
        {
            _transactionalElasticClient.IndexAsync(new TestObject() {Id = "id"}).WaitAndUnwrapException();

            ElasticClient.Get<TestObject>("id").Source.Should().NotBeNull();
        }

        [Test]
        public void IndexDescriptorAsycWithTransaction_NotCommitted_ShouldBeNull()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                var indexResponse = _transactionalElasticClient.IndexAsync(new TestObject() {Id = "id"}).WaitAndUnwrapException();
                indexResponse.Should().NotBeNull();
                indexResponse.IsValid.Should().BeTrue();
                indexResponse.ApiCall.HttpStatusCode.Value.Should().Be(200);
            }

            ElasticClient.Get<TestObject>("id").Source.Should().BeNull();
        }

        [Test]
        public void IndexDescriptorAsyncWithTransaction_Committed_ShouldNotBeNull()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.IndexAsync(new TestObject() {Id = "id"}).WaitAndUnwrapException();
                txSc.Complete();
            }

            ElasticClient.Get<TestObject>("id").Source.Should().NotBeNull();
        }

        [Test]
        public void IndexDescriptorAsyncWithTransaction_MultipleObjects_NotCommitted_AllShouldBeNull()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.IndexAsync(new TestObject() {Id = "id1"}).WaitAndUnwrapException();
                _transactionalElasticClient.IndexAsync(new TestObject() {Id = "id2"}).WaitAndUnwrapException();
            }

            ElasticClient.Get<TestObject>("id1").Source.Should().BeNull();
            ElasticClient.Get<TestObject>("id2").Source.Should().BeNull();
        }

        [Test]
        public void IndexDescriptorAsyncWithTransaction_MultipleObjects_Committed_AllShouldBeIndexed()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.IndexAsync(new TestObject() {Id = "id1"}).WaitAndUnwrapException();
                _transactionalElasticClient.IndexAsync(new TestObject() {Id = "id2"}).WaitAndUnwrapException();
                txSc.Complete();
            }

            ElasticClient.Get<TestObject>("id1").Source.Should().NotBeNull();
            ElasticClient.Get<TestObject>("id2").Source.Should().NotBeNull();
        }

        [Test]
        public void IndexRequestAsyncWithoutTransaction()
        {
            _transactionalElasticClient.IndexAsync((IIndexRequest)new IndexRequest<TestObject>(new TestObject() {Id = "id"})).WaitAndUnwrapException();

            ElasticClient.Get<TestObject>("id").Source.Should().NotBeNull();
        }

        [Test]
        public void IndexRequestAsycWithTransaction_NotCommitted_ShouldBeNull()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                var indexResponse = _transactionalElasticClient.IndexAsync((IIndexRequest)new IndexRequest<TestObject>(new TestObject() {Id = "id"})).WaitAndUnwrapException();
                indexResponse.Should().NotBeNull();
                indexResponse.IsValid.Should().BeTrue();
                indexResponse.ApiCall.HttpStatusCode.Value.Should().Be(200);
            }

            ElasticClient.Get<TestObject>("id").Source.Should().BeNull();
        }

        [Test]
        public void IndexRequestAsyncWithTransaction_Committed_ShouldNotBeNull()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.IndexAsync((IIndexRequest)new IndexRequest<TestObject>(new TestObject() {Id = "id"})).WaitAndUnwrapException();
                txSc.Complete();
            }

            ElasticClient.Get<TestObject>("id").Source.Should().NotBeNull();
        }

        [Test]
        public void IndexRequestAsyncWithTransaction_MultipleObjects_NotCommitted_AllShouldBeNull()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.IndexAsync((IIndexRequest)new IndexRequest<TestObject>(new TestObject() {Id = "id1"})).WaitAndUnwrapException();
                _transactionalElasticClient.IndexAsync((IIndexRequest)new IndexRequest<TestObject>(new TestObject() {Id = "id2"})).WaitAndUnwrapException();
            }

            ElasticClient.Get<TestObject>("id1").Source.Should().BeNull();
            ElasticClient.Get<TestObject>("id2").Source.Should().BeNull();
        }

        [Test]
        public void IndexRequestAsyncWithTransaction_MultipleObjects_Committed_AllShouldBeIndexed()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.IndexAsync((IIndexRequest)new IndexRequest<TestObject>(new TestObject() {Id = "id1"})).WaitAndUnwrapException();
                _transactionalElasticClient.IndexAsync((IIndexRequest)new IndexRequest<TestObject>(new TestObject() {Id = "id2"})).WaitAndUnwrapException();
                txSc.Complete();
            }

            ElasticClient.Get<TestObject>("id1").Source.Should().NotBeNull();
            ElasticClient.Get<TestObject>("id2").Source.Should().NotBeNull();
        }
    }
}