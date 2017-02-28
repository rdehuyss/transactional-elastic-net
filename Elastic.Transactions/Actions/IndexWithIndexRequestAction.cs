using System.Threading.Tasks;
using Elastic.Transactions.Infrastructure;
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

        public override IResponse Commit(ElasticClient client)
        {
            return Index(client);
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

    public class IndexWithIndexRequestAsyncAction : AbstractTransactionableAsyncAction<IndexWithIndexRequestAsyncAction>
    {
        private readonly IIndexRequest _indexRequest;

        public IndexWithIndexRequestAsyncAction(IIndexRequest indexRequest)
        {
            _indexRequest = indexRequest;
        }

        public override Task<IResponse> Commit(ElasticClient client)
        {
            return Index(client).Then(r => (IResponse)r);
        }

        public Task<IIndexResponse> TestWithInMemoryClient(ElasticClient inMemoryClient)
        {
            return Index(inMemoryClient);
        }

        private Task<IIndexResponse> Index(ElasticClient client)
        {
            return client.IndexAsync(_indexRequest);
        }
    }
}