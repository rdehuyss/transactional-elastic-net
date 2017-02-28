using System.Threading.Tasks;
using Elastic.Transactions.Infrastructure;
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

    public class DeleteWithDeleteRequestAsyncAction : AbstractTransactionableAsyncAction<DeleteWithDeleteRequestAsyncAction>
    {
        private readonly IDeleteRequest _indexRequest;

        public DeleteWithDeleteRequestAsyncAction(IDeleteRequest indexRequest)
        {
            _indexRequest = indexRequest;
        }

        public override Task<IResponse> Commit(ElasticClient client)
        {
            return Delete(client).Then(r => (IResponse)r);
        }

        public Task<IDeleteResponse> TestWithInMemoryClient(ElasticClient inMemoryClient)
        {
            return Delete(inMemoryClient);
        }

        private Task<IDeleteResponse> Delete(ElasticClient client)
        {
            return client.DeleteAsync(_indexRequest);
        }
    }
}