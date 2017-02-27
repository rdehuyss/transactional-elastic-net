using System.Transactions;
using Elasticsearch.Net;
using FluentAssertions;
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
        public void TestWithoutTransaction()
        {
            var deleteResponse = _transactionalElasticClient.Delete<TestObject>("id");
            deleteResponse.IsValid.Should().BeTrue();
        }

        [Test]
        public void TestWithTransaction_NotCommitted_ShouldNotBeDeleted()
        {
            ElasticClient.Index(new TestObject() {Id = "id"});

            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.Delete<TestObject>("id");
            }

            ElasticClient.Get<TestObject>("id").Source.Should().NotBeNull();
        }

        [Test]
        public void TestWithTransaction_Committed_ShouldBeDeleted()
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
        public void TestWithTransaction_MultipleObjects_NotCommitted_NothingDeleted()
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
        public void TestWithTransaction_MultipleObjects_Committed_AllShouldBeDeleted()
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
    }
}