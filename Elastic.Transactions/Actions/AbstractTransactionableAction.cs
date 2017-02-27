using System.Collections.Generic;
using Nest;

namespace Elastic.Transactions.Actions
{
    public abstract class AbstractTransactionableAction<T> : ITransactionableAction where T:AbstractTransactionableAction<T>
    {

        public T AddToActions(List<ITransactionableAction> actions)
        {
            actions.Add(this);
            return (T)this;
        }

        public void Prepare(ElasticClient client)
        {
            //nothing to do
        }

        public abstract void Commit(ElasticClient client);

        public void Rollback(ElasticClient client)
        {
            //nothing to do
        }
    }
}