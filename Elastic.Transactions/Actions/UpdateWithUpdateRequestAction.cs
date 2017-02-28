using System;
using System.Threading.Tasks;
using Elastic.Transactions.Infrastructure;
using Nest;

namespace Elastic.Transactions.Actions
{
    public class UpdateWithUpdateRequestAction<TDocument, TPartialDocument> : AbstractTransactionableAction<UpdateWithUpdateRequestAction<TDocument, TPartialDocument>>
        where TDocument : class
        where TPartialDocument : class
    {
        private readonly IUpdateRequest<TDocument, TPartialDocument> _request;

        public UpdateWithUpdateRequestAction(IUpdateRequest<TDocument, TPartialDocument> request)
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
            return client.Update(_request);
        }
    }

    public class UpdateWithUpdateRequestAsyncAction<TDocument, TPartialDocument> : AbstractTransactionableAsyncAction<UpdateWithUpdateRequestAsyncAction<TDocument, TPartialDocument>>
        where TDocument : class
        where TPartialDocument : class
    {
        private readonly IUpdateRequest<TDocument, TPartialDocument> _request;

        public UpdateWithUpdateRequestAsyncAction(IUpdateRequest<TDocument, TPartialDocument> request)
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
            return client.UpdateAsync(_request);
        }
    }
}