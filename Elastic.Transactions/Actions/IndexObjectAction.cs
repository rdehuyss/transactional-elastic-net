using System;
using System.Collections.Generic;
using Nest;

namespace Elastic.Transactions.Actions
{
    public class IndexObjectAction<T> : AbstractTransactionableAction<IndexObjectAction<T>>
        where T : class
    {
        private readonly T _theObject;
        private readonly Func<IndexDescriptor<T>, IIndexRequest> _selector;

        public IndexObjectAction(T theObject, Func<IndexDescriptor<T>, IIndexRequest> selector = null)
        {
            _theObject = theObject;
            _selector = selector;
        }

        public override void Commit(ElasticClient client)
        {
            Index(client);
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
}