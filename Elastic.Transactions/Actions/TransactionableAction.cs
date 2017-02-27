using Nest;

namespace Elastic.Transactions.Actions
{
    public interface ITransactionableAction
    {
        void Prepare(ElasticClient client);

        IResponse Commit(ElasticClient client);

        void Rollback(ElasticClient client);
    }
}