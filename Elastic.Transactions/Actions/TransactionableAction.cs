using Nest;

namespace Elastic.Transactions.Actions
{
    public interface ITransactionableAction
    {
        void Prepare(ElasticClient client);

        void Commit(ElasticClient client);

        void Rollback(ElasticClient client);
    }
}