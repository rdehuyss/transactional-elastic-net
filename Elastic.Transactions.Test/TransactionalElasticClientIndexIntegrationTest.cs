using System.Transactions;
using Elasticsearch.Net;
using FluentAssertions;
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
            _transactionalElasticClient = new TransactionalElasticClient(ElasticClient, WaitForStatus.Yellow);
        }

        [Test]
        public void TestWithoutTransaction()
        {
            _transactionalElasticClient.Index(new TestObject() {Id = "id"});

            ElasticClient.Get<TestObject>("id").Source.Should().NotBeNull();
        }

        [Test]
        public void TestWithTransaction_NotCommitted_ShouldBeNull()
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
        public void TestWithTransaction_Committed_ShouldNotBeNull()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.Index(new TestObject() {Id = "id"});
                txSc.Complete();
            }

            ElasticClient.Get<TestObject>("id").Source.Should().NotBeNull();
        }

        [Test]
        public void TestWithTransaction_MultipleObjects_NotCommitted_AllShouldBeNull()
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
        public void TestWithTransaction_MultipleObjects_Committed_AllShouldBeIndexed()
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
    }
}