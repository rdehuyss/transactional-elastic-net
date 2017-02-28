using System.Threading.Tasks;
using Elastic.Transactions.Infrastructure;
using Nest;

namespace Elastic.Transactions.Actions
{
    public class BulkWithBulkRequestAction : AbstractTransactionableAction<BulkWithBulkRequestAction>
    {
        private IBulkRequest _request;

        public BulkWithBulkRequestAction(IBulkRequest request)
        {
            _request = request;
        }

        public override IResponse Commit(ElasticClient client)
        {
            return Bulk(client);
        }

        public IBulkResponse TestWithInMemoryClient(ElasticClient inMemoryClient)
        {
            return Bulk(inMemoryClient);
        }

        private IBulkResponse Bulk(ElasticClient client)
        {
            return client.Bulk(_request);
        }
    }

    public class BulkWithBulkRequestAsyncAction : AbstractTransactionableAsyncAction<BulkWithBulkRequestAsyncAction>
    {
        private IBulkRequest _request;

        public BulkWithBulkRequestAsyncAction(IBulkRequest request)
        {
            _request = request;
        }

        public override Task<IResponse> Commit(ElasticClient client)
        {
            return Bulk(client).Then(r => (IResponse)r);
        }

        public Task<IBulkResponse> TestWithInMemoryClient(ElasticClient inMemoryClient)
        {
            return Bulk(inMemoryClient);
        }

        private Task<IBulkResponse> Bulk(ElasticClient client)
        {
            return client.BulkAsync(_request);
        }
    }
}