using System;
using System.Threading.Tasks;
using Elastic.Transactions.Infrastructure;
using Nest;

namespace Elastic.Transactions.Actions
{
    public class IndexWithIndexDescriptorAction<T> : AbstractTransactionableAction<IndexWithIndexDescriptorAction<T>> where T : class
    {
        private readonly T _theObject;
        private readonly Func<IndexDescriptor<T>, IIndexRequest> _selector;

        public IndexWithIndexDescriptorAction(T theObject, Func<IndexDescriptor<T>, IIndexRequest> selector = null)
        {
            _theObject = theObject;
            _selector = selector;
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
            return client.Index(_theObject, _selector);
        }
    }

    public class IndexObjectAsyncAction<T> : AbstractTransactionableAsyncAction<IndexObjectAsyncAction<T>> where T : class
    {
        private readonly T _theObject;
        private readonly Func<IndexDescriptor<T>, IIndexRequest> _selector;

        public IndexObjectAsyncAction(T theObject, Func<IndexDescriptor<T>, IIndexRequest> selector = null)
        {
            _theObject = theObject;
            _selector = selector;
        }

        public override Task<IResponse> Commit(ElasticClient client)
        {
            return Index(client).Then(r => (IResponse) r);
        }

        public Task<IIndexResponse> TestWithInMemoryClient(ElasticClient inMemoryClient)
        {
            return Index(inMemoryClient);
        }

        private Task<IIndexResponse> Index(ElasticClient client)
        {
            return client.IndexAsync(_theObject, _selector);
        }
    }
}