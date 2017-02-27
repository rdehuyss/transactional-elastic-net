using Nest;

namespace Elastic.Transactions.Actions
{
    public class DeleteWithDeleteRequestAction : AbstractTransactionableAction<DeleteWithDeleteRequestAction>
    {
        private readonly IDeleteRequest _indexRequest;

        public DeleteWithDeleteRequestAction(IDeleteRequest indexRequest)
        {
            _indexRequest = indexRequest;
        }

        public override IResponse Commit(ElasticClient client)
        {
            return Delete(client);
        }

        public IDeleteResponse TestWithInMemoryClient(ElasticClient inMemoryClient)
        {
            return Delete(inMemoryClient);
        }

        private IDeleteResponse Delete(ElasticClient client)
        {
            return client.Delete(_indexRequest);
        }
    }
}