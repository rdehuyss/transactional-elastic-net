using System;
using System.Threading.Tasks;
using Elastic.Transactions.Infrastructure;
using Nest;

namespace Elastic.Transactions.Actions
{
    public class UpdateWithUpdateDescriptorAction<TDocument> : AbstractTransactionableAction<UpdateWithUpdateDescriptorAction<TDocument>> where TDocument : class
    {
        private readonly DocumentPath<TDocument> _documentPath;
        private readonly Func<UpdateDescriptor<TDocument, TDocument>, IUpdateRequest<TDocument, TDocument>> _selector;

        public UpdateWithUpdateDescriptorAction(DocumentPath<TDocument> documentPath, Func<UpdateDescriptor<TDocument, TDocument>, IUpdateRequest<TDocument, TDocument>> selector)
        {
            _documentPath = documentPath;
            _selector = selector;
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
            return client.Update<TDocument>(_documentPath, _selector);
        }
    }

    public class UpdateWithUpdateDescriptorAsyncAction<TDocument> : AbstractTransactionableAsyncAction<UpdateWithUpdateDescriptorAsyncAction<TDocument>> where TDocument : class
    {
        private readonly DocumentPath<TDocument> _documentPath;
        private readonly Func<UpdateDescriptor<TDocument, TDocument>, IUpdateRequest<TDocument, TDocument>> _selector;

        public UpdateWithUpdateDescriptorAsyncAction(DocumentPath<TDocument> documentPath, Func<UpdateDescriptor<TDocument, TDocument>, IUpdateRequest<TDocument, TDocument>> selector)
        {
            _documentPath = documentPath;
            _selector = selector;
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
            return client.UpdateAsync<TDocument>(_documentPath, _selector);
        }
    }
}