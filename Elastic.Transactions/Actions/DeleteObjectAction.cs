using System;
using Nest;

namespace Elastic.Transactions.Actions
{
    public class DeleteObjectAction<T> : AbstractTransactionableAction<DeleteObjectAction<T>> where T : class
    {
        private readonly DocumentPath<T> _document;
        private readonly Func<DeleteDescriptor<T>, IDeleteRequest> _selector;

        public DeleteObjectAction(DocumentPath<T> document, Func<DeleteDescriptor<T>, IDeleteRequest> selector = null)
        {
            _document = document;
            _selector = selector;
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
            return client.Delete<T>(_document, _selector);
        }
    }
}