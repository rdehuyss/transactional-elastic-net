using System;
using System.Threading;
using System.Transactions;
using Elasticsearch.Net;
using FluentAssertions;
using Nest;
using NUnit.Framework;

namespace Elastic.Transactions.Test
{
    [TestFixture]
    public class TransactionalElasticClientIntegrationTest : AbstractIntegrationTest
    {

        [Test]
        public void ActionsFromMultipleClientsAreCombined()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                TestTransactionalElasticClient client1 = new TestTransactionalElasticClient(ElasticClient);
                TestTransactionalElasticClient client2 = new TestTransactionalElasticClient(ElasticClient);

                client1.Index(new TestObject() {Id = "id1"});
                client2.Index(new TestObject() {Id = "id2"});
                client2.Delete<TestObject>("id1");

                txSc.Complete();
            }
            ElasticClient.Get<TestObject>("id1").Source.Should().BeNull();
            ElasticClient.Get<TestObject>("id2").Source.Should().NotBeNull();
        }

        [Test]
        public void ActionsAreThreadLocal()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                TestTransactionalElasticClient client1 = new TestTransactionalElasticClient(ElasticClient);
                TestTransactionalElasticClient client2 = new TestTransactionalElasticClient(ElasticClient);

                client1.Index(new TestObject() {Id = "id1"});
                client2.Index(new TestObject() {Id = "id2"});
                client2.Delete<TestObject>("id1");

                client1.GetNumberOfActions().Should().Be(3);
                client2.GetNumberOfActions().Should().Be(3);
            }
        }

        [Test]
        public void ActionsAreNotLeakedToOtherThreads()
        {
            CountdownEvent countdownEvent1 = new CountdownEvent(1);
            CountdownEvent countdownEvent2 = new CountdownEvent(1);
            CountdownEvent countdownEventCombined = new CountdownEvent(2);

            int numberOfActions1 = 0;
            int numberOfActions2 = 0;
            var t1 = new Thread(() =>
            {
                using (TransactionScope txSc = new TransactionScope())
                {
                    Thread.CurrentThread.IsBackground = true;
                    TestTransactionalElasticClient client1 = new TestTransactionalElasticClient(ElasticClient);
                    client1.Index(new TestObject() {Id = "id1"});
                    countdownEvent1.Signal();
                    countdownEvent2.Wait();
                    numberOfActions1 = client1.GetNumberOfActions();
                    countdownEventCombined.Signal();
                }
            });

            var t2 = new Thread(() =>
            {
                using (TransactionScope txSc = new TransactionScope())
                {
                    Thread.CurrentThread.IsBackground = true;
                    TestTransactionalElasticClient client2 = new TestTransactionalElasticClient(ElasticClient);
                    client2.Index(new TestObject() {Id = "id2"});
                    client2.Delete<TestObject>("id1");
                    countdownEvent2.Signal();
                    countdownEvent1.Wait();
                    numberOfActions2 = client2.GetNumberOfActions();
                    countdownEventCombined.Signal();
                }
            });

            t1.Start();
            t2.Start();

            countdownEventCombined.Wait();
            numberOfActions1.Should().Be(1);
            numberOfActions2.Should().Be(2);
        }
    }

    public class TestTransactionalElasticClient : TransactionalElasticClient
    {
        public TestTransactionalElasticClient(ElasticClient client) : base(client)
        {
        }

        public int GetNumberOfActions()
        {
            return Actions.Count;
        }
    }
}