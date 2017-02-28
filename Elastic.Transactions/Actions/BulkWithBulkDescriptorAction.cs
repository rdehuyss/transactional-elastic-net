using System;
using System.Threading.Tasks;
using Elastic.Transactions.Infrastructure;
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

    public class BulkWithBulkDescriptorAsyncAction : AbstractTransactionableAsyncAction<BulkWithBulkDescriptorAsyncAction>
    {
        private readonly Func<BulkDescriptor, IBulkRequest> _selector;

        public BulkWithBulkDescriptorAsyncAction(Func<BulkDescriptor, IBulkRequest> selector)
        {
            _selector = selector;
        }

        public override Task<IResponse> Commit(ElasticClient client)
        {
            return Bulk(client).Then(r => (IResponse)r);
        }

        public Task<IBulkResponse> TestWithInMemoryClient(ElasticClient inMemoryClient)
        {
            return Bulk(inMemoryClient);
        }

        private Task<IBulkResponse> Bulk(ElasticClient client)
        {
            return client.BulkAsync(_selector);
        }
    }
}