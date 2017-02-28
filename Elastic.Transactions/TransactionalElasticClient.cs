using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Elastic.Transactions.Actions;
using Elastic.Transactions.Infrastructure;
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
        [ThreadStatic]
        protected static List<ITransactionableAsyncAction> AsyncActions;

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
            AsyncActions = new List<ITransactionableAsyncAction>();
        }

        public IIndexResponse Index<T>(T theObject, Func<IndexDescriptor<T>, IIndexRequest> selector = null) where T : class
        {
            if (InTransaction())
            {
                Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                return new IndexWithIndexDescriptorAction<T>(theObject, selector)
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

        public Task<IIndexResponse> IndexAsync<T>(T theObject, Func<IndexDescriptor<T>, IIndexRequest> selector = null) where T : class
        {
            if (InTransaction())
            {
                Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                return new IndexObjectAsyncAction<T>(theObject, selector)
                    .AddToActions(AsyncActions)
                    .TestWithInMemoryClient(_inMemoryClient);
            }
            return _client.IndexAsync(theObject, selector);
        }

        public Task<IIndexResponse> IndexAsync(IIndexRequest request)
        {
            if (InTransaction())
            {
                Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                return new IndexWithIndexRequestAsyncAction(request)
                    .AddToActions(AsyncActions)
                    .TestWithInMemoryClient(_inMemoryClient);
            }
            return _client.IndexAsync(request);
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
                return new DeleteWithDeleteDescriptorAction<T>(document, selector)
                    .AddToActions(Actions)
                    .TestWithInMemoryClient(_inMemoryClient);
            }
            return _client.Delete(document, selector);
        }

        public Task<IDeleteResponse> DeleteAsync(IDeleteRequest request)
        {
            if (InTransaction())
            {
                Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                return new DeleteWithDeleteRequestAsyncAction(request)
                    .AddToActions(AsyncActions)
                    .TestWithInMemoryClient(_inMemoryClient);
            }
            return _client.DeleteAsync(request);
        }

        public Task<IDeleteResponse> DeleteAsync<T>(DocumentPath<T> document, Func<DeleteDescriptor<T>, IDeleteRequest> selector = null) where T : class
        {
            if (InTransaction())
            {
                Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                return new DeleteObjectAsyncAction<T>(document, selector)
                    .AddToActions(AsyncActions)
                    .TestWithInMemoryClient(_inMemoryClient);
            }
            return _client.DeleteAsync(document, selector);
        }

        public IBulkResponse Bulk(IBulkRequest request)
        {
            if (InTransaction())
            {
                Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                return new BulkWithBulkRequestAction(request)
                    .AddToActions(Actions)
                    .TestWithInMemoryClient(_inMemoryClient);
            }
            return _client.Bulk(request);
        }

        public IBulkResponse Bulk(Func<BulkDescriptor, IBulkRequest> selector = null)
        {
            if (InTransaction())
            {
                Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                return new BulkWithBulkDescriptorAction(selector)
                    .AddToActions(Actions)
                    .TestWithInMemoryClient(_inMemoryClient);
            }
            return _client.Bulk(selector);
        }

        public Task<IBulkResponse> BulkAsync(IBulkRequest request)
        {
            if (InTransaction())
            {
                Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                return new BulkWithBulkRequestAsyncAction(request)
                    .AddToActions(AsyncActions)
                    .TestWithInMemoryClient(_inMemoryClient);
            }
            return _client.BulkAsync(request);
        }

        public Task<IBulkResponse> BulkAsync(Func<BulkDescriptor, IBulkRequest> selector = null)
        {
            if (InTransaction())
            {
                Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                return new BulkWithBulkDescriptorAsyncAction(selector)
                    .AddToActions(AsyncActions)
                    .TestWithInMemoryClient(_inMemoryClient);
            }
            return _client.BulkAsync(selector);
        }

        public IUpdateResponse<TDocument> Update<TDocument>(DocumentPath<TDocument> documentPath, Func<UpdateDescriptor<TDocument, TDocument>, IUpdateRequest<TDocument, TDocument>> selector) where TDocument : class
        {
            if (InTransaction())
            {
                Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                return new UpdateWithUpdateDescriptorAction<TDocument>(documentPath, selector)
                    .AddToActions(Actions)
                    .TestWithInMemoryClient(_inMemoryClient);
            }
            return _client.Update<TDocument>(documentPath, selector);
        }

        public IUpdateResponse<TDocument> Update<TDocument>(IUpdateRequest<TDocument, TDocument> request) where TDocument : class
        {
            if (InTransaction())
            {
                Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                return new UpdateWithUpdateRequestAction<TDocument>(request)
                    .AddToActions(Actions)
                    .TestWithInMemoryClient(_inMemoryClient);
            }
            return _client.Update<TDocument>(request);
        }

        public Task<IUpdateResponse<TDocument>> UpdateAsync<TDocument>(DocumentPath<TDocument> documentPath, Func<UpdateDescriptor<TDocument, TDocument>, IUpdateRequest<TDocument, TDocument>> selector) where TDocument : class
        {
            if (InTransaction())
            {
                Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                return new UpdateWithUpdateDescriptorAsyncAction<TDocument>(documentPath, selector)
                    .AddToActions(AsyncActions)
                    .TestWithInMemoryClient(_inMemoryClient);
            }
            return _client.UpdateAsync<TDocument>(documentPath, selector);
        }

        public Task<IUpdateResponse<TDocument>> UpdateAsync<TDocument>(IUpdateRequest<TDocument, TDocument> request) where TDocument : class
        {
            if (InTransaction())
            {
                Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                return new UpdateWithUpdateRequestAsyncAction<TDocument>(request)
                    .AddToActions(AsyncActions)
                    .TestWithInMemoryClient(_inMemoryClient);
            }
            return _client.UpdateAsync<TDocument>(request);
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            if (!IsMinimumClusterHealthStatusAchieved())
            {
                preparingEnlistment.ForceRollback(new InvalidOperationException("Minimum cluster health '" + _minimumStatus + "' is not achieved..."));
                return;
            }

            Actions.ForEach(action => action.Prepare(_client));
            AsyncActions.ForEach(action => action.Prepare(_client));

            preparingEnlistment.Prepared();
        }

        public void Commit(Enlistment enlistment)
        {
            var allSyncActionsSucceeded = Actions
                .Select(action => action.Commit(_client).IsValid)
                .FirstOrDefault(result => result == false);
            var allAsyncTasks = AsyncActions
                .Select(action => action.Commit(_client));
            AsyncPump.Run(() => Task.WhenAll(allAsyncTasks)); //TODO review me: Is this ok? See https://github.com/danielmarbach/AsyncTransactions/blob/master/AsyncTransactions/AsynchronousBlockingResourceManager.cs
            var allAsyncActionsSucceeded = allAsyncTasks
                .Select(action => action.Result.IsValid)
                .FirstOrDefault(result => result == false);

            if (!allSyncActionsSucceeded || !allAsyncActionsSucceeded)
            {
                //todo? Log? Throw exception
            }
            Actions.Clear();
            AsyncActions.Clear();
            enlistment.Done();
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

            if (clusterHealthResponse.IsValid)
            {
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
            return false;
        }
    }
}