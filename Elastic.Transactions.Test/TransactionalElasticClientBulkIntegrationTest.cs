using System.Collections.Generic;
using System.Transactions;
using Elasticsearch.Net;
using FluentAssertions;
using Nest;
using NUnit.Framework;

namespace Elastic.Transactions.Test
{
    [TestFixture]
    public class TransactionalElasticClientBulkIntegrationTest : AbstractIntegrationTest
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
            var list = new List<TestObject>{new TestObject {Id = "id1"}, new TestObject {Id = "id2"}};
            _transactionalElasticClient.Bulk(idx => idx.IndexMany(list));

            ElasticClient.Refresh(CurrentTestIndexName());
            ElasticClient.Count<TestObject>().Count.Should().Be(2);
        }

        [Test]
        public void TestWithTransaction_NotCommitted_ShouldNotBeIndexed()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                var list = new List<TestObject>{new TestObject {Id = "id1"}, new TestObject {Id = "id2"}};
                _transactionalElasticClient.Bulk(idx => idx.IndexMany(list));
            }

            ElasticClient.Refresh(CurrentTestIndexName());
            ElasticClient.Count<TestObject>().Count.Should().Be(0);
        }

        [Test]
        public void TestWithTransaction_Committed_ShouldNotBeNull()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                var list = new List<TestObject>{new TestObject {Id = "id1"}, new TestObject {Id = "id2"}};
                _transactionalElasticClient.Bulk(idx => idx.IndexMany(list));
                txSc.Complete();
            }

            ElasticClient.Refresh(CurrentTestIndexName());
            ElasticClient.Count<TestObject>().Count.Should().Be(2);
        }

        [Test]
        public void TestWithTransaction_MultipleObjects_NotCommitted_AllShouldBeNull()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                var list1 = new List<TestObject>{new TestObject {Id = "id1"}, new TestObject {Id = "id2"}};
                _transactionalElasticClient.Bulk(idx => idx.IndexMany(list1));
                var list2 = new List<TestObject>{new TestObject {Id = "id3"}, new TestObject {Id = "id4"}};
                _transactionalElasticClient.Bulk(idx => idx.IndexMany(list2));
            }

            ElasticClient.Refresh(CurrentTestIndexName());
            ElasticClient.Count<TestObject>().Count.Should().Be(0);
        }

        [Test]
        public void TestWithTransaction_MultipleObjects_Committed_AllShouldBeIndexed()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                var list1 = new List<TestObject>{new TestObject {Id = "id1"}, new TestObject {Id = "id2"}};
                _transactionalElasticClient.Bulk(idx => idx.IndexMany(list1));
                var list2 = new List<TestObject>{new TestObject {Id = "id3"}, new TestObject {Id = "id4"}};
                _transactionalElasticClient.Bulk(idx => idx.IndexMany(list2));
                txSc.Complete();
            }

            ElasticClient.Refresh(CurrentTestIndexName());
            ElasticClient.Count<TestObject>().Count.Should().Be(4);
        }
    }
}