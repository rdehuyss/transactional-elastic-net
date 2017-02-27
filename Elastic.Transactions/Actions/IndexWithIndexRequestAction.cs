using System.Collections.Generic;
using Nest;

namespace Elastic.Transactions.Actions
{
    public class IndexWithIndexRequestAction : AbstractTransactionableAction<IndexWithIndexRequestAction>
    {
        private readonly IIndexRequest _indexRequest;

        public IndexWithIndexRequestAction(IIndexRequest indexRequest)
        {
            _indexRequest = indexRequest;
        }

        public override void Commit(ElasticClient client)
        {
            Index(client);
        }

        public IIndexResponse TestWithInMemoryClient(ElasticClient inMemoryClient)
        {
            return Index(inMemoryClient);
        }

        private IIndexResponse Index(ElasticClient client)
        {
            return client.Index(_indexRequest);
        }
    }
}