using System;
using System.Threading.Tasks;
using Elastic.Transactions.Infrastructure;
using Nest;

namespace Elastic.Transactions.Actions
{
    public class UpdateWithUpdateRequestAction<TDocument> : AbstractTransactionableAction<UpdateWithUpdateRequestAction<TDocument>> where TDocument : class
    {
        private readonly IUpdateRequest<TDocument, TDocument> _request;

        public UpdateWithUpdateRequestAction(IUpdateRequest<TDocument, TDocument> request)
        {
            _request = request;
        }

        public override IResponse Commit(ElasticClient client)
        {
            return Update(client);
        }

        public IUpdateResponse<TDocument> TestWithInMemoryClient(ElasticClient inMemoryClient)
        {
            return Update(inMemoryClient);
        }

        private IUpdateResponse<TDocument> Update(ElasticClient client)
        {
            return client.Update<TDocument>(_request);
        }
    }

    public class UpdateWithUpdateRequestAsyncAction<TDocument> : AbstractTransactionableAsyncAction<UpdateWithUpdateRequestAsyncAction<TDocument>> where TDocument : class
    {
        private readonly IUpdateRequest<TDocument, TDocument> _request;

        public UpdateWithUpdateRequestAsyncAction(IUpdateRequest<TDocument, TDocument> request)
        {
            _request = request;
        }

        public override Task<IResponse> Commit(ElasticClient client)
        {
            return Update(client).Then(r => (IResponse) r);
        }

        public Task<IUpdateResponse<TDocument>> TestWithInMemoryClient(ElasticClient inMemoryClient)
        {
            return Update(inMemoryClient);
        }

        private Task<IUpdateResponse<TDocument>> Update(ElasticClient client)
        {
            return client.UpdateAsync<TDocument>(_request);
        }
    }
}