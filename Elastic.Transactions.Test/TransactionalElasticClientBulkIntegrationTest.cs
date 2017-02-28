using System.Collections.Generic;
using System.Transactions;
using Elasticsearch.Net;
using FluentAssertions;
using Nest;
using Nito.AsyncEx.Synchronous;
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
        public void BulkDescriptorWithoutTransaction()
        {
            var list = new List<TestObject>{new TestObject {Id = "id1"}, new TestObject {Id = "id2"}};
            _transactionalElasticClient.Bulk(idx => idx.IndexMany(list));

            ElasticClient.Refresh(CurrentTestIndexName());
            ElasticClient.Count<TestObject>().Count.Should().Be(2);
        }

        [Test]
        public void BulkDescriptorWithTransaction_NotCommitted_ShouldNotBeIndexed()
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
        public void BulkDescriptorWithTransaction_Committed_ShouldNotBeNull()
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
        public void BulkDescriptorWithTransaction_MultipleObjects_NotCommitted_AllShouldBeNull()
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
        public void BulkDescriptorWithTransaction_MultipleObjects_Committed_AllShouldBeIndexed()
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

        [Test]
        public void BulkRequestWithoutTransaction()
        {
            _transactionalElasticClient.Bulk(new BulkRequest() {Refresh = true, Consistency = Consistency.One, Operations = new List<IBulkOperation>
                {
                    { new BulkIndexOperation<TestObject>(new TestObject {Id = "id1"})},
                    { new BulkIndexOperation<TestObject>(new TestObject {Id = "id2"}) }
                }
            });

            ElasticClient.Refresh(CurrentTestIndexName());
            ElasticClient.Count<TestObject>().Count.Should().Be(2);
        }

        [Test]
        public void BulkRequestWithTransaction_NotCommitted_ShouldNotBeIndexed()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.Bulk(new BulkRequest() {Refresh = true, Consistency = Consistency.One, Operations = new List<IBulkOperation>
                    {
                        { new BulkIndexOperation<TestObject>(new TestObject {Id = "id1"})},
                        { new BulkIndexOperation<TestObject>(new TestObject {Id = "id2"}) }
                    }
                });
            }

            ElasticClient.Refresh(CurrentTestIndexName());
            ElasticClient.Count<TestObject>().Count.Should().Be(0);
        }

        [Test]
        public void BulkRequestWithTransaction_Committed_ShouldNotBeNull()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.Bulk(new BulkRequest() {Refresh = true, Consistency = Consistency.One, Operations = new List<IBulkOperation>
                    {
                        { new BulkIndexOperation<TestObject>(new TestObject {Id = "id1"})},
                        { new BulkIndexOperation<TestObject>(new TestObject {Id = "id2"}) }
                    }
                });
                txSc.Complete();
            }

            ElasticClient.Refresh(CurrentTestIndexName());
            ElasticClient.Count<TestObject>().Count.Should().Be(2);
        }

        [Test]
        public void BulkRequestWithTransaction_MultipleObjects_NotCommitted_AllShouldBeNull()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.Bulk(new BulkRequest() {Refresh = true, Consistency = Consistency.One, Operations = new List<IBulkOperation>
                    {
                        { new BulkIndexOperation<TestObject>(new TestObject {Id = "id1"})},
                        { new BulkIndexOperation<TestObject>(new TestObject {Id = "id2"}) }
                    }
                });
                _transactionalElasticClient.Bulk(new BulkRequest() {Refresh = true, Consistency = Consistency.One, Operations = new List<IBulkOperation>
                    {
                        { new BulkIndexOperation<TestObject>(new TestObject {Id = "id3"})},
                        { new BulkIndexOperation<TestObject>(new TestObject {Id = "id4"}) }
                    }
                });
            }

            ElasticClient.Refresh(CurrentTestIndexName());
            ElasticClient.Count<TestObject>().Count.Should().Be(0);
        }

        [Test]
        public void BulkRequestWithTransaction_MultipleObjects_Committed_AllShouldBeIndexed()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.Bulk(new BulkRequest() {Refresh = true, Consistency = Consistency.One, Operations = new List<IBulkOperation>
                    {
                        { new BulkIndexOperation<TestObject>(new TestObject {Id = "id1"})},
                        { new BulkIndexOperation<TestObject>(new TestObject {Id = "id2"}) }
                    }
                });
                _transactionalElasticClient.Bulk(new BulkRequest() {Refresh = true, Consistency = Consistency.One, Operations = new List<IBulkOperation>
                    {
                        { new BulkIndexOperation<TestObject>(new TestObject {Id = "id3"})},
                        { new BulkIndexOperation<TestObject>(new TestObject {Id = "id4"}) }
                    }
                });
                txSc.Complete();
            }

            ElasticClient.Refresh(CurrentTestIndexName());
            ElasticClient.Count<TestObject>().Count.Should().Be(4);
        }

        [Test]
        public void BulkDescriptorAsyncWithoutTransaction()
        {
            var list = new List<TestObject>{new TestObject {Id = "id1"}, new TestObject {Id = "id2"}};
            _transactionalElasticClient.BulkAsync(idx => idx.IndexMany(list)).WaitAndUnwrapException();

            ElasticClient.Refresh(CurrentTestIndexName());
            ElasticClient.Count<TestObject>().Count.Should().Be(2);
        }

        [Test]
        public void BulkDescriptorAsyncWithTransaction_NotCommitted_ShouldNotBeIndexed()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                var list = new List<TestObject>{new TestObject {Id = "id1"}, new TestObject {Id = "id2"}};
                _transactionalElasticClient.BulkAsync(idx => idx.IndexMany(list)).WaitAndUnwrapException();
            }

            ElasticClient.Refresh(CurrentTestIndexName());
            ElasticClient.Count<TestObject>().Count.Should().Be(0);
        }

        [Test]
        public void BulkDescriptorAsyncWithTransaction_Committed_ShouldNotBeNull()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                var list = new List<TestObject>{new TestObject {Id = "id1"}, new TestObject {Id = "id2"}};
                _transactionalElasticClient.BulkAsync(idx => idx.IndexMany(list)).WaitAndUnwrapException();
                txSc.Complete();
            }

            ElasticClient.Refresh(CurrentTestIndexName());
            ElasticClient.Count<TestObject>().Count.Should().Be(2);
        }

        [Test]
        public void BulkDescriptorAsyncWithTransaction_MultipleObjects_NotCommitted_AllShouldBeNull()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                var list1 = new List<TestObject>{new TestObject {Id = "id1"}, new TestObject {Id = "id2"}};
                _transactionalElasticClient.BulkAsync(idx => idx.IndexMany(list1)).WaitAndUnwrapException();
                var list2 = new List<TestObject>{new TestObject {Id = "id3"}, new TestObject {Id = "id4"}};
                _transactionalElasticClient.BulkAsync(idx => idx.IndexMany(list2)).WaitAndUnwrapException();
            }

            ElasticClient.Refresh(CurrentTestIndexName());
            ElasticClient.Count<TestObject>().Count.Should().Be(0);
        }

        [Test]
        public void BulkDescriptorAsyncWithTransaction_MultipleObjects_Committed_AllShouldBeIndexed()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                var list1 = new List<TestObject>{new TestObject {Id = "id1"}, new TestObject {Id = "id2"}};
                _transactionalElasticClient.BulkAsync(idx => idx.IndexMany(list1)).WaitAndUnwrapException();
                var list2 = new List<TestObject>{new TestObject {Id = "id3"}, new TestObject {Id = "id4"}};
                _transactionalElasticClient.BulkAsync(idx => idx.IndexMany(list2)).WaitAndUnwrapException();
                txSc.Complete();
            }

            ElasticClient.Refresh(CurrentTestIndexName());
            ElasticClient.Count<TestObject>().Count.Should().Be(4);
        }

        [Test]
        public void BulkRequestAsyncWithoutTransaction()
        {
            _transactionalElasticClient.BulkAsync(new BulkRequest() {Consistency = Consistency.One, Operations = new List<IBulkOperation>
                {
                    { new BulkIndexOperation<TestObject>(new TestObject {Id = "id1"})},
                    { new BulkIndexOperation<TestObject>(new TestObject {Id = "id2"}) }
                }
            }).WaitAndUnwrapException();

            ElasticClient.Refresh(CurrentTestIndexName());
            ElasticClient.Count<TestObject>().Count.Should().Be(2);
        }

        [Test]
        public void BulkRequestAsyncWithTransaction_NotCommitted_ShouldNotBeIndexed()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.BulkAsync(new BulkRequest() {Consistency = Consistency.One, Operations = new List<IBulkOperation>
                    {
                        { new BulkIndexOperation<TestObject>(new TestObject {Id = "id1"})},
                        { new BulkIndexOperation<TestObject>(new TestObject {Id = "id2"}) }
                    }
                }).WaitAndUnwrapException();
            }

            ElasticClient.Refresh(CurrentTestIndexName());
            ElasticClient.Count<TestObject>().Count.Should().Be(0);
        }

        [Test]
        public void BulkRequestAsyncWithTransaction_Committed_ShouldNotBeNull()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.BulkAsync(new BulkRequest() {Consistency = Consistency.One, Operations = new List<IBulkOperation>
                    {
                        { new BulkIndexOperation<TestObject>(new TestObject {Id = "id1"})},
                        { new BulkIndexOperation<TestObject>(new TestObject {Id = "id2"}) }
                    }
                }).WaitAndUnwrapException();
                txSc.Complete();
            }

            ElasticClient.Refresh(CurrentTestIndexName());
            ElasticClient.Count<TestObject>().Count.Should().Be(2);
        }

        [Test]
        public void BulkRequestAsyncWithTransaction_MultipleObjects_NotCommitted_AllShouldBeNull()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.BulkAsync(new BulkRequest() {Consistency = Consistency.One, Operations = new List<IBulkOperation>
                    {
                        { new BulkIndexOperation<TestObject>(new TestObject {Id = "id1"})},
                        { new BulkIndexOperation<TestObject>(new TestObject {Id = "id2"}) }
                    }
                }).WaitAndUnwrapException();
                _transactionalElasticClient.BulkAsync(new BulkRequest() {Consistency = Consistency.One, Operations = new List<IBulkOperation>
                    {
                        { new BulkIndexOperation<TestObject>(new TestObject {Id = "id3"})},
                        { new BulkIndexOperation<TestObject>(new TestObject {Id = "id4"}) }
                    }
                }).WaitAndUnwrapException();
            }

            ElasticClient.Refresh(CurrentTestIndexName());
            ElasticClient.Count<TestObject>().Count.Should().Be(0);
        }

        [Test]
        public void BulkRequestAsyncWithTransaction_MultipleObjects_Committed_AllShouldBeIndexed()
        {
            using (TransactionScope txSc = new TransactionScope())
            {
                _transactionalElasticClient.BulkAsync(new BulkRequest() {Consistency = Consistency.One, Operations = new List<IBulkOperation>
                    {
                        { new BulkIndexOperation<TestObject>(new TestObject {Id = "id1"})},
                        { new BulkIndexOperation<TestObject>(new TestObject {Id = "id2"}) }
                    }
                }).WaitAndUnwrapException();
                _transactionalElasticClient.BulkAsync(new BulkRequest() {Consistency = Consistency.One, Operations = new List<IBulkOperation>
                    {
                        { new BulkIndexOperation<TestObject>(new TestObject {Id = "id3"})},
                        { new BulkIndexOperation<TestObject>(new TestObject {Id = "id4"}) }
                    }
                }).WaitAndUnwrapException();
                txSc.Complete();
            }

            ElasticClient.Refresh(CurrentTestIndexName());
            ElasticClient.Count<TestObject>().Count.Should().Be(4);
        }
    }
}