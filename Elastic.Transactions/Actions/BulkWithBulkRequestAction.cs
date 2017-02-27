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
}