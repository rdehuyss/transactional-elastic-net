using System;
using Nest;

namespace Elastic.Transactions.Actions
{
    public class BulkWithBulkDescriptorAction : AbstractTransactionableAction<BulkWithBulkDescriptorAction>
    {
        private readonly Func<BulkDescriptor, IBulkRequest> _selector;

        public BulkWithBulkDescriptorAction(Func<BulkDescriptor, IBulkRequest> selector)
        {
            _selector = selector;
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
            return client.Bulk(_selector);
        }
    }
}