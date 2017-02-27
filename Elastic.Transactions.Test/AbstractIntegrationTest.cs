using System;
using Nest;
using NUnit.Framework;

namespace Elastic.Transactions.Test
{
    public class AbstractIntegrationTest
    {
        protected ElasticClient ElasticClient;

        [SetUp]
        public void SetUp()
        {
            var connectionSettings = new ConnectionSettings(new Uri("http://localhost:9200"))
                .DefaultIndex(CurrentTestIndexName());
            ElasticClient = new ElasticClient(connectionSettings);
            ElasticClient.CreateIndex(CurrentTestIndexName(),
                idx => idx.Settings(ids => ids.NumberOfShards(1).NumberOfReplicas(0)));
        }

        [TearDown]
        public void TearDown()
        {
            ElasticClient.DeleteIndex(CurrentTestIndexName());
        }

        protected string CurrentTestIndexName()
        {
            return TestContext.CurrentContext.Test.Name.ToLowerInvariant();
        }
    }
}