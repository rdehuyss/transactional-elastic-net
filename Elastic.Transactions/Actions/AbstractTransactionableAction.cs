using System.Collections.Generic;
using System.Threading.Tasks;
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

        public abstract IResponse Commit(ElasticClient client);

        public void Rollback(ElasticClient client)
        {
            //nothing to do
        }
    }

    public abstract class AbstractTransactionableAsyncAction<T> : ITransactionableAsyncAction where T:AbstractTransactionableAsyncAction<T>
    {

        public T AddToActions(List<ITransactionableAsyncAction> actions)
        {
            actions.Add(this);
            return (T)this;
        }

        public void Prepare(ElasticClient client)
        {
            //nothing to do
        }

        public abstract Task<IResponse> Commit(ElasticClient client);

        public void Rollback(ElasticClient client)
        {
            //nothing to do
        }
    }
}