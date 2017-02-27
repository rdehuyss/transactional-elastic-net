using System;
using System.Collections.Generic;
using System.Transactions;
using Elastic.Transactions.Actions;
using Elasticsearch.Net;
using Nest;

namespace Elastic.Transactions
{
    public class TransactionalElasticClient : IEnlistmentNotification
    {
        private ElasticClient _client;
        private readonly WaitForStatus _minimumStatus;
        private ElasticClient _inMemoryClient;

        [ThreadStatic]
        protected static List<ITransactionableAction> Actions;

        public TransactionalElasticClient(ElasticClient client) : this(client, WaitForStatus.Green)
        {
        }


        public TransactionalElasticClient(ElasticClient client, WaitForStatus minimumStatus)
        {
            _client = client;
            _minimumStatus = minimumStatus;

            var inMemoryConnectionSettings = new ConnectionSettings(client.ConnectionSettings.ConnectionPool, new InMemoryConnection())
                .DefaultIndex(client.ConnectionSettings.DefaultIndex);
            _inMemoryClient = new ElasticClient(inMemoryConnectionSettings);
            Actions = new List<ITransactionableAction>();
        }

        public IIndexResponse Index<T>(T theObject, Func<IndexDescriptor<T>, IIndexRequest> selector = null) where T : class
        {
            if (InTransaction())
            {
                Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                return new IndexObjectAction<T>(theObject, selector)
                    .AddToActions(Actions)
                    .TestWithInMemoryClient(_inMemoryClient);
            }
            return _client.Index(theObject, selector);
        }

        public IIndexResponse Index(IIndexRequest request)
        {
            if (InTransaction())
            {
                Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                return new IndexWithIndexRequestAction(request)
                    .AddToActions(Actions)
                    .TestWithInMemoryClient(_inMemoryClient);
            }
            return _client.Index(request);
        }

        public IDeleteResponse Delete(IDeleteRequest request)
        {
            if (InTransaction())
            {
                Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                return new DeleteWithDeleteRequestAction(request)
                    .AddToActions(Actions)
                    .TestWithInMemoryClient(_inMemoryClient);
            }
            return _client.Delete(request);
        }

        public IDeleteResponse Delete<T>(DocumentPath<T> document, Func<DeleteDescriptor<T>, IDeleteRequest> selector = null) where T : class
        {
            if (InTransaction())
            {
                Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                return new DeleteObjectAction<T>(document, selector)
                    .AddToActions(Actions)
                    .TestWithInMemoryClient(_inMemoryClient);
            }
            return _client.Delete(document, selector);
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            if (!IsMinimumClusterHealthStatusAchieved())
            {
                preparingEnlistment.ForceRollback(new InvalidOperationException("Minimum cluster health '" + _minimumStatus + "' is not achieved..."));
                return;
            }

            Actions.ForEach(action => action.Prepare(_client));

            preparingEnlistment.Prepared();
        }

        public void Commit(Enlistment enlistment)
        {
            Actions.ForEach(action => action.Commit(_client));
            Actions.Clear();
        }

        public void Rollback(Enlistment enlistment)
        {
            Actions.Reverse();
            Actions.ForEach(action => action.Rollback(_client));
            Actions.Clear();
        }

        public void InDoubt(Enlistment enlistment)
        {
            throw new NotImplementedException();
        }

        private bool InTransaction()
        {
            return Transaction.Current != null;
        }

        private bool IsMinimumClusterHealthStatusAchieved()
        {
            var clusterHealthResponse = _client.ClusterHealth(g => g
                .WaitForStatus(_minimumStatus)
                .Timeout(TimeSpan.FromSeconds(3)));

            if (_minimumStatus == WaitForStatus.Green)
            {
                return clusterHealthResponse.Status.Equals(WaitForStatus.Green.ToString(), StringComparison.InvariantCultureIgnoreCase);
            }

            if (_minimumStatus == WaitForStatus.Yellow)
            {
                return clusterHealthResponse.Status.Equals(WaitForStatus.Green.ToString(), StringComparison.InvariantCultureIgnoreCase)
                    || clusterHealthResponse.Status.Equals(WaitForStatus.Yellow.ToString(), StringComparison.InvariantCultureIgnoreCase);
            }
            return true;
        }
    }
}