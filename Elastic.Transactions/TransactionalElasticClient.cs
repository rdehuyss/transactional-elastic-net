using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Elastic.Transactions.Actions;
using Elastic.Transactions.Infrastructure;
using Elasticsearch.Net;
using Nest;

namespace Elastic.Transactions
{
    public class TransactionalElasticClient : IEnlistmentNotification, IElasticClient
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

        #region Transactional Methods
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
                return new UpdateWithUpdateDescriptorAction<TDocument, TDocument>(documentPath, selector)
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
                return new UpdateWithUpdateRequestAction<TDocument, TDocument>(request)
                    .AddToActions(Actions)
                    .TestWithInMemoryClient(_inMemoryClient);
            }
            return _client.Update<TDocument>(request);
        }

        public IUpdateResponse<TDocument> Update<TDocument, TPartialDocument>(DocumentPath<TDocument> documentPath, Func<UpdateDescriptor<TDocument, TPartialDocument>, IUpdateRequest<TDocument, TPartialDocument>> selector) where TDocument : class where TPartialDocument : class
        {
            if (InTransaction())
            {
                Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                return new UpdateWithUpdateDescriptorAction<TDocument, TPartialDocument>(documentPath, selector)
                    .AddToActions(Actions)
                    .TestWithInMemoryClient(_inMemoryClient);
            }
            return _client.Update(documentPath, selector);
        }

        public IUpdateResponse<TDocument> Update<TDocument, TPartialDocument>(IUpdateRequest<TDocument, TPartialDocument> request) where TDocument : class where TPartialDocument : class
        {
            if (InTransaction())
            {
                Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                return new UpdateWithUpdateRequestAction<TDocument, TPartialDocument>(request)
                    .AddToActions(Actions)
                    .TestWithInMemoryClient(_inMemoryClient);
            }
            return _client.Update(request);
        }

        public Task<IUpdateResponse<TDocument>> UpdateAsync<TDocument>(DocumentPath<TDocument> documentPath, Func<UpdateDescriptor<TDocument, TDocument>, IUpdateRequest<TDocument, TDocument>> selector) where TDocument : class
        {
            if (InTransaction())
            {
                Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                return new UpdateWithUpdateDescriptorAsyncAction<TDocument, TDocument>(documentPath, selector)
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
                return new UpdateWithUpdateRequestAsyncAction<TDocument, TDocument>(request)
                    .AddToActions(AsyncActions)
                    .TestWithInMemoryClient(_inMemoryClient);
            }
            return _client.UpdateAsync<TDocument>(request);
        }

        public Task<IUpdateResponse<TDocument>> UpdateAsync<TDocument, TPartialDocument>(DocumentPath<TDocument> documentPath, Func<UpdateDescriptor<TDocument, TPartialDocument>, IUpdateRequest<TDocument, TPartialDocument>> selector) where TDocument : class where TPartialDocument : class
        {
            if (InTransaction())
            {
                Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                return new UpdateWithUpdateDescriptorAsyncAction<TDocument, TPartialDocument>(documentPath, selector)
                    .AddToActions(AsyncActions)
                    .TestWithInMemoryClient(_inMemoryClient);
            }
            return _client.UpdateAsync(documentPath, selector);
        }

        public Task<IUpdateResponse<TDocument>> UpdateAsync<TDocument, TPartialDocument>(IUpdateRequest<TDocument, TPartialDocument> request) where TDocument : class where TPartialDocument : class
        {
            if (InTransaction())
            {
                Transaction.Current.EnlistVolatile(this, EnlistmentOptions.None);
                return new UpdateWithUpdateRequestAsyncAction<TDocument, TPartialDocument>(request)
                    .AddToActions(AsyncActions)
                    .TestWithInMemoryClient(_inMemoryClient);
            }
            return _client.UpdateAsync(request);
        }
        #endregion

        #region Delegate methods

        public TResponse Dispatch<TRequest, TQueryString, TResponse>(TRequest descriptor, Func<TRequest, PostData<object>, ElasticsearchResponse<TResponse>> dispatch) where TRequest : IRequest<TQueryString> where TQueryString : FluentRequestParameters<TQueryString>, new() where TResponse : ResponseBase
        {
            return ((IHighLevelToLowLevelDispatcher) _client).Dispatch<TRequest, TQueryString, TResponse>(descriptor, dispatch);
        }

        public TResponse Dispatch<TRequest, TQueryString, TResponse>(TRequest descriptor, Func<IApiCallDetails, Stream, TResponse> responseGenerator, Func<TRequest, PostData<object>, ElasticsearchResponse<TResponse>> dispatch) where TRequest : IRequest<TQueryString> where TQueryString : FluentRequestParameters<TQueryString>, new() where TResponse : ResponseBase
        {
            return ((IHighLevelToLowLevelDispatcher) _client).Dispatch<TRequest, TQueryString, TResponse>(descriptor, responseGenerator, dispatch);
        }

        public Task<TResponseInterface> DispatchAsync<TRequest, TQueryString, TResponse, TResponseInterface>(TRequest descriptor, Func<TRequest, PostData<object>, Task<ElasticsearchResponse<TResponse>>> dispatch) where TRequest : IRequest<TQueryString> where TQueryString : FluentRequestParameters<TQueryString>, new() where TResponse : ResponseBase, TResponseInterface where TResponseInterface : IResponse
        {
            return ((IHighLevelToLowLevelDispatcher) _client).DispatchAsync<TRequest, TQueryString, TResponse, TResponseInterface>(descriptor, dispatch);
        }

        public Task<TResponseInterface> DispatchAsync<TRequest, TQueryString, TResponse, TResponseInterface>(TRequest descriptor, Func<IApiCallDetails, Stream, TResponse> responseGenerator,
            Func<TRequest, PostData<object>, Task<ElasticsearchResponse<TResponse>>> dispatch) where TRequest : IRequest<TQueryString> where TQueryString : FluentRequestParameters<TQueryString>, new() where TResponse : ResponseBase, TResponseInterface where TResponseInterface : IResponse
        {
            return ((IHighLevelToLowLevelDispatcher) _client).DispatchAsync<TRequest, TQueryString, TResponse, TResponseInterface>(descriptor, responseGenerator, dispatch);
        }

        public ICatResponse<CatHelpRecord> CatHelp(Func<CatHelpDescriptor, ICatHelpRequest> selector = null)
        {
            return _client.CatHelp(selector);
        }

        public ICatResponse<CatHelpRecord> CatHelp(ICatHelpRequest request)
        {
            return _client.CatHelp(request);
        }

        public Task<ICatResponse<CatHelpRecord>> CatHelpAsync(Func<CatHelpDescriptor, ICatHelpRequest> selector = null)
        {
            return _client.CatHelpAsync(selector);
        }

        public Task<ICatResponse<CatHelpRecord>> CatHelpAsync(ICatHelpRequest request)
        {
            return _client.CatHelpAsync(request);
        }

        public ICatResponse<CatRepositoriesRecord> CatRepositories(Func<CatRepositoriesDescriptor, ICatRepositoriesRequest> selector = null)
        {
            return _client.CatRepositories(selector);
        }

        public ICatResponse<CatRepositoriesRecord> CatRepositories(ICatRepositoriesRequest request)
        {
            return _client.CatRepositories(request);
        }

        public Task<ICatResponse<CatRepositoriesRecord>> CatRepositoriesAsync(Func<CatRepositoriesDescriptor, ICatRepositoriesRequest> selector = null)
        {
            return _client.CatRepositoriesAsync(selector);
        }

        public Task<ICatResponse<CatRepositoriesRecord>> CatRepositoriesAsync(ICatRepositoriesRequest request)
        {
            return _client.CatRepositoriesAsync(request);
        }

        public ICatResponse<CatNodeAttributesRecord> CatNodeAttributes(Func<CatNodeAttributesDescriptor, ICatNodeAttributesRequest> selector = null)
        {
            return _client.CatNodeAttributes(selector);
        }

        public ICatResponse<CatNodeAttributesRecord> CatNodeAttributes(ICatNodeAttributesRequest request)
        {
            return _client.CatNodeAttributes(request);
        }

        public Task<ICatResponse<CatNodeAttributesRecord>> CatNodeAttributesAsync(Func<CatNodeAttributesDescriptor, ICatNodeAttributesRequest> selector = null)
        {
            return _client.CatNodeAttributesAsync(selector);
        }

        public Task<ICatResponse<CatNodeAttributesRecord>> CatNodeAttributesAsync(ICatNodeAttributesRequest request)
        {
            return _client.CatNodeAttributesAsync(request);
        }

        public ICatResponse<CatSnapshotsRecord> CatSnapshots(Names repositories, Func<CatSnapshotsDescriptor, ICatSnapshotsRequest> selector = null)
        {
            return _client.CatSnapshots(repositories, selector);
        }

        public ICatResponse<CatSnapshotsRecord> CatSnapshots(ICatSnapshotsRequest request)
        {
            return _client.CatSnapshots(request);
        }

        public Task<ICatResponse<CatSnapshotsRecord>> CatSnapshotsAsync(Names repositories, Func<CatSnapshotsDescriptor, ICatSnapshotsRequest> selector = null)
        {
            return _client.CatSnapshotsAsync(repositories, selector);
        }

        public Task<ICatResponse<CatSnapshotsRecord>> CatSnapshotsAsync(ICatSnapshotsRequest request)
        {
            return _client.CatSnapshotsAsync(request);
        }

        public ICatResponse<CatAliasesRecord> CatAliases(Func<CatAliasesDescriptor, ICatAliasesRequest> selector = null)
        {
            return _client.CatAliases(selector);
        }

        public ICatResponse<CatAliasesRecord> CatAliases(ICatAliasesRequest request)
        {
            return _client.CatAliases(request);
        }

        public Task<ICatResponse<CatAliasesRecord>> CatAliasesAsync(Func<CatAliasesDescriptor, ICatAliasesRequest> selector = null)
        {
            return _client.CatAliasesAsync(selector);
        }

        public Task<ICatResponse<CatAliasesRecord>> CatAliasesAsync(ICatAliasesRequest request)
        {
            return _client.CatAliasesAsync(request);
        }

        public ICatResponse<CatAllocationRecord> CatAllocation(Func<CatAllocationDescriptor, ICatAllocationRequest> selector = null)
        {
            return _client.CatAllocation(selector);
        }

        public ICatResponse<CatAllocationRecord> CatAllocation(ICatAllocationRequest request)
        {
            return _client.CatAllocation(request);
        }

        public Task<ICatResponse<CatAllocationRecord>> CatAllocationAsync(Func<CatAllocationDescriptor, ICatAllocationRequest> selector = null)
        {
            return _client.CatAllocationAsync(selector);
        }

        public Task<ICatResponse<CatAllocationRecord>> CatAllocationAsync(ICatAllocationRequest request)
        {
            return _client.CatAllocationAsync(request);
        }

        public ICatResponse<CatCountRecord> CatCount(Func<CatCountDescriptor, ICatCountRequest> selector = null)
        {
            return _client.CatCount(selector);
        }

        public ICatResponse<CatCountRecord> CatCount(ICatCountRequest request)
        {
            return _client.CatCount(request);
        }

        public Task<ICatResponse<CatCountRecord>> CatCountAsync(Func<CatCountDescriptor, ICatCountRequest> selector = null)
        {
            return _client.CatCountAsync(selector);
        }

        public Task<ICatResponse<CatCountRecord>> CatCountAsync(ICatCountRequest request)
        {
            return _client.CatCountAsync(request);
        }

        public ICatResponse<CatFielddataRecord> CatFielddata(Func<CatFielddataDescriptor, ICatFielddataRequest> selector = null)
        {
            return _client.CatFielddata(selector);
        }

        public ICatResponse<CatFielddataRecord> CatFielddata(ICatFielddataRequest request)
        {
            return _client.CatFielddata(request);
        }

        public Task<ICatResponse<CatFielddataRecord>> CatFielddataAsync(Func<CatFielddataDescriptor, ICatFielddataRequest> selector = null)
        {
            return _client.CatFielddataAsync(selector);
        }

        public Task<ICatResponse<CatFielddataRecord>> CatFielddataAsync(ICatFielddataRequest request)
        {
            return _client.CatFielddataAsync(request);
        }

        public ICatResponse<CatHealthRecord> CatHealth(Func<CatHealthDescriptor, ICatHealthRequest> selector = null)
        {
            return _client.CatHealth(selector);
        }

        public ICatResponse<CatHealthRecord> CatHealth(ICatHealthRequest request)
        {
            return _client.CatHealth(request);
        }

        public Task<ICatResponse<CatHealthRecord>> CatHealthAsync(Func<CatHealthDescriptor, ICatHealthRequest> selector = null)
        {
            return _client.CatHealthAsync(selector);
        }

        public Task<ICatResponse<CatHealthRecord>> CatHealthAsync(ICatHealthRequest request)
        {
            return _client.CatHealthAsync(request);
        }

        public ICatResponse<CatIndicesRecord> CatIndices(Func<CatIndicesDescriptor, ICatIndicesRequest> selector = null)
        {
            return _client.CatIndices(selector);
        }

        public ICatResponse<CatIndicesRecord> CatIndices(ICatIndicesRequest request)
        {
            return _client.CatIndices(request);
        }

        public Task<ICatResponse<CatIndicesRecord>> CatIndicesAsync(Func<CatIndicesDescriptor, ICatIndicesRequest> selector = null)
        {
            return _client.CatIndicesAsync(selector);
        }

        public Task<ICatResponse<CatIndicesRecord>> CatIndicesAsync(ICatIndicesRequest request)
        {
            return _client.CatIndicesAsync(request);
        }

        public ICatResponse<CatMasterRecord> CatMaster(Func<CatMasterDescriptor, ICatMasterRequest> selector = null)
        {
            return _client.CatMaster(selector);
        }

        public ICatResponse<CatMasterRecord> CatMaster(ICatMasterRequest request)
        {
            return _client.CatMaster(request);
        }

        public Task<ICatResponse<CatMasterRecord>> CatMasterAsync(Func<CatMasterDescriptor, ICatMasterRequest> selector = null)
        {
            return _client.CatMasterAsync(selector);
        }

        public Task<ICatResponse<CatMasterRecord>> CatMasterAsync(ICatMasterRequest request)
        {
            return _client.CatMasterAsync(request);
        }

        public ICatResponse<CatNodesRecord> CatNodes(Func<CatNodesDescriptor, ICatNodesRequest> selector = null)
        {
            return _client.CatNodes(selector);
        }

        public ICatResponse<CatNodesRecord> CatNodes(ICatNodesRequest request)
        {
            return _client.CatNodes(request);
        }

        public Task<ICatResponse<CatNodesRecord>> CatNodesAsync(Func<CatNodesDescriptor, ICatNodesRequest> selector = null)
        {
            return _client.CatNodesAsync(selector);
        }

        public Task<ICatResponse<CatNodesRecord>> CatNodesAsync(ICatNodesRequest request)
        {
            return _client.CatNodesAsync(request);
        }

        public ICatResponse<CatPendingTasksRecord> CatPendingTasks(Func<CatPendingTasksDescriptor, ICatPendingTasksRequest> selector = null)
        {
            return _client.CatPendingTasks(selector);
        }

        public ICatResponse<CatPendingTasksRecord> CatPendingTasks(ICatPendingTasksRequest request)
        {
            return _client.CatPendingTasks(request);
        }

        public Task<ICatResponse<CatPendingTasksRecord>> CatPendingTasksAsync(Func<CatPendingTasksDescriptor, ICatPendingTasksRequest> selector = null)
        {
            return _client.CatPendingTasksAsync(selector);
        }

        public Task<ICatResponse<CatPendingTasksRecord>> CatPendingTasksAsync(ICatPendingTasksRequest request)
        {
            return _client.CatPendingTasksAsync(request);
        }

        public ICatResponse<CatPluginsRecord> CatPlugins(Func<CatPluginsDescriptor, ICatPluginsRequest> selector = null)
        {
            return _client.CatPlugins(selector);
        }

        public ICatResponse<CatPluginsRecord> CatPlugins(ICatPluginsRequest request)
        {
            return _client.CatPlugins(request);
        }

        public Task<ICatResponse<CatPluginsRecord>> CatPluginsAsync(Func<CatPluginsDescriptor, ICatPluginsRequest> selector = null)
        {
            return _client.CatPluginsAsync(selector);
        }

        public Task<ICatResponse<CatPluginsRecord>> CatPluginsAsync(ICatPluginsRequest request)
        {
            return _client.CatPluginsAsync(request);
        }

        public ICatResponse<CatRecoveryRecord> CatRecovery(Func<CatRecoveryDescriptor, ICatRecoveryRequest> selector = null)
        {
            return _client.CatRecovery(selector);
        }

        public ICatResponse<CatRecoveryRecord> CatRecovery(ICatRecoveryRequest request)
        {
            return _client.CatRecovery(request);
        }

        public Task<ICatResponse<CatRecoveryRecord>> CatRecoveryAsync(Func<CatRecoveryDescriptor, ICatRecoveryRequest> selector = null)
        {
            return _client.CatRecoveryAsync(selector);
        }

        public Task<ICatResponse<CatRecoveryRecord>> CatRecoveryAsync(ICatRecoveryRequest request)
        {
            return _client.CatRecoveryAsync(request);
        }

        public ICatResponse<CatSegmentsRecord> CatSegments(Func<CatSegmentsDescriptor, ICatSegmentsRequest> selector = null)
        {
            return _client.CatSegments(selector);
        }

        public ICatResponse<CatSegmentsRecord> CatSegments(ICatSegmentsRequest request)
        {
            return _client.CatSegments(request);
        }

        public Task<ICatResponse<CatSegmentsRecord>> CatSegmentsAsync(Func<CatSegmentsDescriptor, ICatSegmentsRequest> selector = null)
        {
            return _client.CatSegmentsAsync(selector);
        }

        public Task<ICatResponse<CatSegmentsRecord>> CatSegmentsAsync(ICatSegmentsRequest request)
        {
            return _client.CatSegmentsAsync(request);
        }

        public ICatResponse<CatShardsRecord> CatShards(Func<CatShardsDescriptor, ICatShardsRequest> selector = null)
        {
            return _client.CatShards(selector);
        }

        public ICatResponse<CatShardsRecord> CatShards(ICatShardsRequest request)
        {
            return _client.CatShards(request);
        }

        public Task<ICatResponse<CatShardsRecord>> CatShardsAsync(Func<CatShardsDescriptor, ICatShardsRequest> selector = null)
        {
            return _client.CatShardsAsync(selector);
        }

        public Task<ICatResponse<CatShardsRecord>> CatShardsAsync(ICatShardsRequest request)
        {
            return _client.CatShardsAsync(request);
        }

        public ICatResponse<CatThreadPoolRecord> CatThreadPool(Func<CatThreadPoolDescriptor, ICatThreadPoolRequest> selector = null)
        {
            return _client.CatThreadPool(selector);
        }

        public ICatResponse<CatThreadPoolRecord> CatThreadPool(ICatThreadPoolRequest request)
        {
            return _client.CatThreadPool(request);
        }

        public Task<ICatResponse<CatThreadPoolRecord>> CatThreadPoolAsync(Func<CatThreadPoolDescriptor, ICatThreadPoolRequest> selector = null)
        {
            return _client.CatThreadPoolAsync(selector);
        }

        public Task<ICatResponse<CatThreadPoolRecord>> CatThreadPoolAsync(ICatThreadPoolRequest request)
        {
            return _client.CatThreadPoolAsync(request);
        }

        public IClusterHealthResponse ClusterHealth(Func<ClusterHealthDescriptor, IClusterHealthRequest> selector = null)
        {
            return _client.ClusterHealth(selector);
        }

        public IClusterHealthResponse ClusterHealth(IClusterHealthRequest request)
        {
            return _client.ClusterHealth(request);
        }

        public Task<IClusterHealthResponse> ClusterHealthAsync(Func<ClusterHealthDescriptor, IClusterHealthRequest> selector = null)
        {
            return _client.ClusterHealthAsync(selector);
        }

        public Task<IClusterHealthResponse> ClusterHealthAsync(IClusterHealthRequest request)
        {
            return _client.ClusterHealthAsync(request);
        }

        public IClusterPendingTasksResponse ClusterPendingTasks(Func<ClusterPendingTasksDescriptor, IClusterPendingTasksRequest> selector = null)
        {
            return _client.ClusterPendingTasks(selector);
        }

        public Task<IClusterPendingTasksResponse> ClusterPendingTasksAsync(Func<ClusterPendingTasksDescriptor, IClusterPendingTasksRequest> selector = null)
        {
            return _client.ClusterPendingTasksAsync(selector);
        }

        public IClusterPendingTasksResponse ClusterPendingTasks(IClusterPendingTasksRequest request)
        {
            return _client.ClusterPendingTasks(request);
        }

        public Task<IClusterPendingTasksResponse> ClusterPendingTasksAsync(IClusterPendingTasksRequest request)
        {
            return _client.ClusterPendingTasksAsync(request);
        }

        public IClusterRerouteResponse ClusterReroute(Func<ClusterRerouteDescriptor, IClusterRerouteRequest> selector)
        {
            return _client.ClusterReroute(selector);
        }

        public Task<IClusterRerouteResponse> ClusterRerouteAsync(Func<ClusterRerouteDescriptor, IClusterRerouteRequest> selector)
        {
            return _client.ClusterRerouteAsync(selector);
        }

        public IClusterRerouteResponse ClusterReroute(IClusterRerouteRequest request)
        {
            return _client.ClusterReroute(request);
        }

        public Task<IClusterRerouteResponse> ClusterRerouteAsync(IClusterRerouteRequest request)
        {
            return _client.ClusterRerouteAsync(request);
        }

        public IClusterGetSettingsResponse ClusterGetSettings(Func<ClusterGetSettingsDescriptor, IClusterGetSettingsRequest> selector = null)
        {
            return _client.ClusterGetSettings(selector);
        }

        public Task<IClusterGetSettingsResponse> ClusterGetSettingsAsync(Func<ClusterGetSettingsDescriptor, IClusterGetSettingsRequest> selector = null)
        {
            return _client.ClusterGetSettingsAsync(selector);
        }

        public IClusterGetSettingsResponse ClusterGetSettings(IClusterGetSettingsRequest request)
        {
            return _client.ClusterGetSettings(request);
        }

        public Task<IClusterGetSettingsResponse> ClusterGetSettingsAsync(IClusterGetSettingsRequest request)
        {
            return _client.ClusterGetSettingsAsync(request);
        }

        public IClusterPutSettingsResponse ClusterPutSettings(Func<ClusterPutSettingsDescriptor, IClusterPutSettingsRequest> selector)
        {
            return _client.ClusterPutSettings(selector);
        }

        public Task<IClusterPutSettingsResponse> ClusterPutSettingsAsync(Func<ClusterPutSettingsDescriptor, IClusterPutSettingsRequest> selector)
        {
            return _client.ClusterPutSettingsAsync(selector);
        }

        public IClusterPutSettingsResponse ClusterPutSettings(IClusterPutSettingsRequest request)
        {
            return _client.ClusterPutSettings(request);
        }

        public Task<IClusterPutSettingsResponse> ClusterPutSettingsAsync(IClusterPutSettingsRequest request)
        {
            return _client.ClusterPutSettingsAsync(request);
        }

        public IClusterStateResponse ClusterState(Func<ClusterStateDescriptor, IClusterStateRequest> selector = null)
        {
            return _client.ClusterState(selector);
        }

        public IClusterStateResponse ClusterState(IClusterStateRequest request)
        {
            return _client.ClusterState(request);
        }

        public Task<IClusterStateResponse> ClusterStateAsync(Func<ClusterStateDescriptor, IClusterStateRequest> selector = null)
        {
            return _client.ClusterStateAsync(selector);
        }

        public Task<IClusterStateResponse> ClusterStateAsync(IClusterStateRequest request)
        {
            return _client.ClusterStateAsync(request);
        }

        public IClusterStatsResponse ClusterStats(Func<ClusterStatsDescriptor, IClusterStatsRequest> selector = null)
        {
            return _client.ClusterStats(selector);
        }

        public Task<IClusterStatsResponse> ClusterStatsAsync(Func<ClusterStatsDescriptor, IClusterStatsRequest> selector = null)
        {
            return _client.ClusterStatsAsync(selector);
        }

        public IClusterStatsResponse ClusterStats(IClusterStatsRequest request)
        {
            return _client.ClusterStats(request);
        }

        public Task<IClusterStatsResponse> ClusterStatsAsync(IClusterStatsRequest request)
        {
            return _client.ClusterStatsAsync(request);
        }

        public INodesHotThreadsResponse NodesHotThreads(Func<NodesHotThreadsDescriptor, INodesHotThreadsRequest> selector = null)
        {
            return _client.NodesHotThreads(selector);
        }

        public INodesHotThreadsResponse NodesHotThreads(INodesHotThreadsRequest request)
        {
            return _client.NodesHotThreads(request);
        }

        public Task<INodesHotThreadsResponse> NodesHotThreadsAsync(Func<NodesHotThreadsDescriptor, INodesHotThreadsRequest> selector = null)
        {
            return _client.NodesHotThreadsAsync(selector);
        }

        public Task<INodesHotThreadsResponse> NodesHotThreadsAsync(INodesHotThreadsRequest request)
        {
            return _client.NodesHotThreadsAsync(request);
        }

        public INodesInfoResponse NodesInfo(Func<NodesInfoDescriptor, INodesInfoRequest> selector = null)
        {
            return _client.NodesInfo(selector);
        }

        public INodesInfoResponse NodesInfo(INodesInfoRequest request)
        {
            return _client.NodesInfo(request);
        }

        public Task<INodesInfoResponse> NodesInfoAsync(Func<NodesInfoDescriptor, INodesInfoRequest> selector = null)
        {
            return _client.NodesInfoAsync(selector);
        }

        public Task<INodesInfoResponse> NodesInfoAsync(INodesInfoRequest request)
        {
            return _client.NodesInfoAsync(request);
        }

        public INodesStatsResponse NodesStats(Func<NodesStatsDescriptor, INodesStatsRequest> selector = null)
        {
            return _client.NodesStats(selector);
        }

        public INodesStatsResponse NodesStats(INodesStatsRequest request)
        {
            return _client.NodesStats(request);
        }

        public Task<INodesStatsResponse> NodesStatsAsync(Func<NodesStatsDescriptor, INodesStatsRequest> selector = null)
        {
            return _client.NodesStatsAsync(selector);
        }

        public Task<INodesStatsResponse> NodesStatsAsync(INodesStatsRequest request)
        {
            return _client.NodesStatsAsync(request);
        }

        public IPingResponse Ping(Func<PingDescriptor, IPingRequest> selector = null)
        {
            return _client.Ping(selector);
        }

        public Task<IPingResponse> PingAsync(Func<PingDescriptor, IPingRequest> selector = null)
        {
            return _client.PingAsync(selector);
        }

        public IPingResponse Ping(IPingRequest request)
        {
            return _client.Ping(request);
        }

        public Task<IPingResponse> PingAsync(IPingRequest request)
        {
            return _client.PingAsync(request);
        }

        public IRootNodeInfoResponse RootNodeInfo(Func<RootNodeInfoDescriptor, IRootNodeInfoRequest> selector = null)
        {
            return _client.RootNodeInfo(selector);
        }

        public IRootNodeInfoResponse RootNodeInfo(IRootNodeInfoRequest request)
        {
            return _client.RootNodeInfo(request);
        }

        public Task<IRootNodeInfoResponse> RootNodeInfoAsync(Func<RootNodeInfoDescriptor, IRootNodeInfoRequest> selector = null)
        {
            return _client.RootNodeInfoAsync(selector);
        }

        public Task<IRootNodeInfoResponse> RootNodeInfoAsync(IRootNodeInfoRequest request)
        {
            return _client.RootNodeInfoAsync(request);
        }

        public ITasksCancelResponse TasksCancel(Func<TasksCancelDescriptor, ITasksCancelRequest> selector = null)
        {
            return _client.TasksCancel(selector);
        }

        public ITasksCancelResponse TasksCancel(ITasksCancelRequest request)
        {
            return _client.TasksCancel(request);
        }

        public Task<ITasksCancelResponse> TasksCancelAsync(Func<TasksCancelDescriptor, ITasksCancelRequest> selector = null)
        {
            return _client.TasksCancelAsync(selector);
        }

        public Task<ITasksCancelResponse> TasksCancelAsync(ITasksCancelRequest request)
        {
            return _client.TasksCancelAsync(request);
        }

        public ITasksListResponse TasksList(Func<TasksListDescriptor, ITasksListRequest> selector = null)
        {
            return _client.TasksList(selector);
        }

        public ITasksListResponse TasksList(ITasksListRequest request)
        {
            return _client.TasksList(request);
        }

        public Task<ITasksListResponse> TasksListAsync(Func<TasksListDescriptor, ITasksListRequest> selector = null)
        {
            return _client.TasksListAsync(selector);
        }

        public Task<ITasksListResponse> TasksListAsync(ITasksListRequest request)
        {
            return _client.TasksListAsync(request);
        }

        public BulkAllObservable<T> BulkAll<T>(IEnumerable<T> documents, Func<BulkAllDescriptor<T>, IBulkAllRequest<T>> selector,
            CancellationToken cancellationToken = new CancellationToken()) where T : class
        {
            return _client.BulkAll(documents, selector, cancellationToken);
        }

        public BulkAllObservable<T> BulkAll<T>(IBulkAllRequest<T> request, CancellationToken cancellationToken = new CancellationToken()) where T : class
        {
            return _client.BulkAll(request, cancellationToken);
        }

        public IReindexRethrottleResponse Rethrottle(Func<ReindexRethrottleDescriptor, IReindexRethrottleRequest> selector)
        {
            return _client.Rethrottle(selector);
        }

        public IReindexRethrottleResponse Rethrottle(IReindexRethrottleRequest request)
        {
            return _client.Rethrottle(request);
        }

        public Task<IReindexRethrottleResponse> RethrottleAsync(Func<ReindexRethrottleDescriptor, IReindexRethrottleRequest> selector)
        {
            return _client.RethrottleAsync(selector);
        }

        public Task<IReindexRethrottleResponse> RethrottleAsync(IReindexRethrottleRequest request)
        {
            return _client.RethrottleAsync(request);
        }

        public IUpdateByQueryResponse UpdateByQuery<T>(Indices indices, Types types, Func<UpdateByQueryDescriptor<T>, IUpdateByQueryRequest> selector) where T : class
        {
            return _client.UpdateByQuery(indices, types, selector);
        }

        public IUpdateByQueryResponse UpdateByQuery<T>(Func<UpdateByQueryDescriptor<T>, IUpdateByQueryRequest> selector) where T : class
        {
            return _client.UpdateByQuery(selector);
        }

        public IUpdateByQueryResponse UpdateByQuery(IUpdateByQueryRequest request)
        {
            return _client.UpdateByQuery(request);
        }

        public Task<IUpdateByQueryResponse> UpdateByQueryAsync<T>(Indices indices, Types types, Func<UpdateByQueryDescriptor<T>, IUpdateByQueryRequest> selector) where T : class
        {
            return _client.UpdateByQueryAsync(indices, types, selector);
        }

        public Task<IUpdateByQueryResponse> UpdateByQueryAsync<T>(Func<UpdateByQueryDescriptor<T>, IUpdateByQueryRequest> selector) where T : class
        {
            return _client.UpdateByQueryAsync(selector);
        }

        public Task<IUpdateByQueryResponse> UpdateByQueryAsync(IUpdateByQueryRequest request)
        {
            return _client.UpdateByQueryAsync(request);
        }

        public IDeleteByQueryResponse DeleteByQuery<T>(Indices indices, Types types, Func<DeleteByQueryDescriptor<T>, IDeleteByQueryRequest> selector) where T : class
        {
            return _client.DeleteByQuery(indices, types, selector);
        }

        public IDeleteByQueryResponse DeleteByQuery(IDeleteByQueryRequest request)
        {
            return _client.DeleteByQuery(request);
        }

        public Task<IDeleteByQueryResponse> DeleteByQueryAsync<T>(Indices indices, Types types, Func<DeleteByQueryDescriptor<T>, IDeleteByQueryRequest> selector) where T : class
        {
            return _client.DeleteByQueryAsync(indices, types, selector);
        }

        public Task<IDeleteByQueryResponse> DeleteByQueryAsync(IDeleteByQueryRequest request)
        {
            return _client.DeleteByQueryAsync(request);
        }

        public IMultiGetResponse MultiGet(Func<MultiGetDescriptor, IMultiGetRequest> selector = null)
        {
            return _client.MultiGet(selector);
        }

        public IMultiGetResponse MultiGet(IMultiGetRequest request)
        {
            return _client.MultiGet(request);
        }

        public Task<IMultiGetResponse> MultiGetAsync(Func<MultiGetDescriptor, IMultiGetRequest> selector = null)
        {
            return _client.MultiGetAsync(selector);
        }

        public Task<IMultiGetResponse> MultiGetAsync(IMultiGetRequest request)
        {
            return _client.MultiGetAsync(request);
        }

        public IMultiTermVectorsResponse MultiTermVectors(Func<MultiTermVectorsDescriptor, IMultiTermVectorsRequest> selector = null)
        {
            return _client.MultiTermVectors(selector);
        }

        public IMultiTermVectorsResponse MultiTermVectors(IMultiTermVectorsRequest request)
        {
            return _client.MultiTermVectors(request);
        }

        public Task<IMultiTermVectorsResponse> MultiTermVectorsAsync(Func<MultiTermVectorsDescriptor, IMultiTermVectorsRequest> selector = null)
        {
            return _client.MultiTermVectorsAsync(selector);
        }

        public Task<IMultiTermVectorsResponse> MultiTermVectorsAsync(IMultiTermVectorsRequest request)
        {
            return _client.MultiTermVectorsAsync(request);
        }

        public IReindexOnServerResponse ReindexOnServer(Func<ReindexOnServerDescriptor, IReindexOnServerRequest> selector)
        {
            return _client.ReindexOnServer(selector);
        }

        public IReindexOnServerResponse ReindexOnServer(IReindexOnServerRequest request)
        {
            return _client.ReindexOnServer(request);
        }

        public Task<IReindexOnServerResponse> ReindexOnServerAsync(Func<ReindexOnServerDescriptor, IReindexOnServerRequest> selector)
        {
            return _client.ReindexOnServerAsync(selector);
        }

        public Task<IReindexOnServerResponse> ReindexOnServerAsync(IReindexOnServerRequest request)
        {
            return _client.ReindexOnServerAsync(request);
        }

        public IObservable<IReindexResponse<T>> Reindex<T>(IndexName @from, IndexName to, Func<ReindexDescriptor<T>, IReindexRequest> selector = null) where T : class
        {
            return _client.Reindex(@from, to, selector);
        }

        public IObservable<IReindexResponse<T>> Reindex<T>(IReindexRequest request) where T : class
        {
            return _client.Reindex<T>(request);
        }

        public IExistsResponse DocumentExists<T>(DocumentPath<T> document, Func<DocumentExistsDescriptor<T>, IDocumentExistsRequest> selector = null) where T : class
        {
            return _client.DocumentExists(document, selector);
        }

        public IExistsResponse DocumentExists(IDocumentExistsRequest request)
        {
            return _client.DocumentExists(request);
        }

        public Task<IExistsResponse> DocumentExistsAsync<T>(DocumentPath<T> document, Func<DocumentExistsDescriptor<T>, IDocumentExistsRequest> selector = null) where T : class
        {
            return _client.DocumentExistsAsync(document, selector);
        }

        public Task<IExistsResponse> DocumentExistsAsync(IDocumentExistsRequest request)
        {
            return _client.DocumentExistsAsync(request);
        }

        public IGetResponse<T> Get<T>(DocumentPath<T> document, Func<GetDescriptor<T>, IGetRequest> selector = null) where T : class
        {
            return _client.Get(document, selector);
        }

        public IGetResponse<T> Get<T>(IGetRequest request) where T : class
        {
            return _client.Get<T>(request);
        }

        public Task<IGetResponse<T>> GetAsync<T>(DocumentPath<T> document, Func<GetDescriptor<T>, IGetRequest> selector = null) where T : class
        {
            return _client.GetAsync(document, selector);
        }

        public Task<IGetResponse<T>> GetAsync<T>(IGetRequest request) where T : class
        {
            return _client.GetAsync<T>(request);
        }

        public T Source<T>(DocumentPath<T> document, Func<SourceDescriptor<T>, ISourceRequest> selector = null) where T : class
        {
            return _client.Source(document, selector);
        }

        public T Source<T>(ISourceRequest request) where T : class
        {
            return _client.Source<T>(request);
        }

        public Task<T> SourceAsync<T>(DocumentPath<T> document, Func<SourceDescriptor<T>, ISourceRequest> selector = null) where T : class
        {
            return _client.SourceAsync(document, selector);
        }

        public Task<T> SourceAsync<T>(ISourceRequest request) where T : class
        {
            return _client.SourceAsync<T>(request);
        }

        public ITermVectorsResponse TermVectors<T>(Func<TermVectorsDescriptor<T>, ITermVectorsRequest<T>> selector) where T : class
        {
            return _client.TermVectors(selector);
        }

        public ITermVectorsResponse TermVectors<T>(ITermVectorsRequest<T> request) where T : class
        {
            return _client.TermVectors(request);
        }

        public Task<ITermVectorsResponse> TermVectorsAsync<T>(Func<TermVectorsDescriptor<T>, ITermVectorsRequest<T>> selector) where T : class
        {
            return _client.TermVectorsAsync(selector);
        }

        public Task<ITermVectorsResponse> TermVectorsAsync<T>(ITermVectorsRequest<T> request) where T : class
        {
            return _client.TermVectorsAsync(request);
        }

        public IExistsResponse AliasExists(Func<AliasExistsDescriptor, IAliasExistsRequest> selector)
        {
            return _client.AliasExists(selector);
        }

        public IExistsResponse AliasExists(IAliasExistsRequest request)
        {
            return _client.AliasExists(request);
        }

        public Task<IExistsResponse> AliasExistsAsync(Func<AliasExistsDescriptor, IAliasExistsRequest> selector)
        {
            return _client.AliasExistsAsync(selector);
        }

        public Task<IExistsResponse> AliasExistsAsync(IAliasExistsRequest request)
        {
            return _client.AliasExistsAsync(request);
        }

        public IBulkAliasResponse Alias(IBulkAliasRequest request)
        {
            return _client.Alias(request);
        }

        public IBulkAliasResponse Alias(Func<BulkAliasDescriptor, IBulkAliasRequest> selector)
        {
            return _client.Alias(selector);
        }

        public Task<IBulkAliasResponse> AliasAsync(IBulkAliasRequest request)
        {
            return _client.AliasAsync(request);
        }

        public Task<IBulkAliasResponse> AliasAsync(Func<BulkAliasDescriptor, IBulkAliasRequest> selector)
        {
            return _client.AliasAsync(selector);
        }

        public IDeleteAliasResponse DeleteAlias(IDeleteAliasRequest request)
        {
            return _client.DeleteAlias(request);
        }

        public Task<IDeleteAliasResponse> DeleteAliasAsync(IDeleteAliasRequest request)
        {
            return _client.DeleteAliasAsync(request);
        }

        public IDeleteAliasResponse DeleteAlias(Indices indices, Names names, Func<DeleteAliasDescriptor, IDeleteAliasRequest> selector = null)
        {
            return _client.DeleteAlias(indices, names, selector);
        }

        public Task<IDeleteAliasResponse> DeleteAliasAsync(Indices indices, Names names, Func<DeleteAliasDescriptor, IDeleteAliasRequest> selector = null)
        {
            return _client.DeleteAliasAsync(indices, names, selector);
        }

        public IGetAliasesResponse GetAliases(Func<GetAliasesDescriptor, IGetAliasesRequest> selector = null)
        {
            return _client.GetAliases(selector);
        }

        public IGetAliasesResponse GetAliases(IGetAliasesRequest request)
        {
            return _client.GetAliases(request);
        }

        public Task<IGetAliasesResponse> GetAliasesAsync(Func<GetAliasesDescriptor, IGetAliasesRequest> selector = null)
        {
            return _client.GetAliasesAsync(selector);
        }

        public Task<IGetAliasesResponse> GetAliasesAsync(IGetAliasesRequest request)
        {
            return _client.GetAliasesAsync(request);
        }

        public IGetAliasesResponse GetAlias(Func<GetAliasDescriptor, IGetAliasRequest> selector = null)
        {
            return _client.GetAlias(selector);
        }

        public IGetAliasesResponse GetAlias(IGetAliasRequest request)
        {
            return _client.GetAlias(request);
        }

        public Task<IGetAliasesResponse> GetAliasAsync(Func<GetAliasDescriptor, IGetAliasRequest> selector = null)
        {
            return _client.GetAliasAsync(selector);
        }

        public Task<IGetAliasesResponse> GetAliasAsync(IGetAliasRequest request)
        {
            return _client.GetAliasAsync(request);
        }

        public IPutAliasResponse PutAlias(IPutAliasRequest request)
        {
            return _client.PutAlias(request);
        }

        public Task<IPutAliasResponse> PutAliasAsync(IPutAliasRequest request)
        {
            return _client.PutAliasAsync(request);
        }

        public IPutAliasResponse PutAlias(Indices indices, Name alias, Func<PutAliasDescriptor, IPutAliasRequest> selector = null)
        {
            return _client.PutAlias(indices, alias, selector);
        }

        public Task<IPutAliasResponse> PutAliasAsync(Indices indices, Name alias, Func<PutAliasDescriptor, IPutAliasRequest> selector = null)
        {
            return _client.PutAliasAsync(indices, alias, selector);
        }

        public IAnalyzeResponse Analyze(Func<AnalyzeDescriptor, IAnalyzeRequest> selector)
        {
            return _client.Analyze(selector);
        }

        public IAnalyzeResponse Analyze(IAnalyzeRequest request)
        {
            return _client.Analyze(request);
        }

        public Task<IAnalyzeResponse> AnalyzeAsync(Func<AnalyzeDescriptor, IAnalyzeRequest> selector)
        {
            return _client.AnalyzeAsync(selector);
        }

        public Task<IAnalyzeResponse> AnalyzeAsync(IAnalyzeRequest request)
        {
            return _client.AnalyzeAsync(request);
        }

        public ICreateIndexResponse CreateIndex(IndexName index, Func<CreateIndexDescriptor, ICreateIndexRequest> selector = null)
        {
            return _client.CreateIndex(index, selector);
        }

        public ICreateIndexResponse CreateIndex(ICreateIndexRequest request)
        {
            return _client.CreateIndex(request);
        }

        public Task<ICreateIndexResponse> CreateIndexAsync(IndexName index, Func<CreateIndexDescriptor, ICreateIndexRequest> selector = null)
        {
            return _client.CreateIndexAsync(index, selector);
        }

        public Task<ICreateIndexResponse> CreateIndexAsync(ICreateIndexRequest request)
        {
            return _client.CreateIndexAsync(request);
        }

        public IDeleteIndexResponse DeleteIndex(Indices indices, Func<DeleteIndexDescriptor, IDeleteIndexRequest> selector = null)
        {
            return _client.DeleteIndex(indices, selector);
        }

        public IDeleteIndexResponse DeleteIndex(IDeleteIndexRequest request)
        {
            return _client.DeleteIndex(request);
        }

        public Task<IDeleteIndexResponse> DeleteIndexAsync(Indices indices, Func<DeleteIndexDescriptor, IDeleteIndexRequest> selector = null)
        {
            return _client.DeleteIndexAsync(indices, selector);
        }

        public Task<IDeleteIndexResponse> DeleteIndexAsync(IDeleteIndexRequest request)
        {
            return _client.DeleteIndexAsync(request);
        }

        public IGetIndexResponse GetIndex(Indices indices, Func<GetIndexDescriptor, IGetIndexRequest> selector = null)
        {
            return _client.GetIndex(indices, selector);
        }

        public IGetIndexResponse GetIndex(IGetIndexRequest request)
        {
            return _client.GetIndex(request);
        }

        public Task<IGetIndexResponse> GetIndexAsync(Indices indices, Func<GetIndexDescriptor, IGetIndexRequest> selector = null)
        {
            return _client.GetIndexAsync(indices, selector);
        }

        public Task<IGetIndexResponse> GetIndexAsync(IGetIndexRequest request)
        {
            return _client.GetIndexAsync(request);
        }

        public IExistsResponse IndexExists(Indices indices, Func<IndexExistsDescriptor, IIndexExistsRequest> selector = null)
        {
            return _client.IndexExists(indices, selector);
        }

        public IExistsResponse IndexExists(IIndexExistsRequest request)
        {
            return _client.IndexExists(request);
        }

        public Task<IExistsResponse> IndexExistsAsync(Indices indices, Func<IndexExistsDescriptor, IIndexExistsRequest> selector = null)
        {
            return _client.IndexExistsAsync(indices, selector);
        }

        public Task<IExistsResponse> IndexExistsAsync(IIndexExistsRequest request)
        {
            return _client.IndexExistsAsync(request);
        }

        public ICloseIndexResponse CloseIndex(Indices indices, Func<CloseIndexDescriptor, ICloseIndexRequest> selector = null)
        {
            return _client.CloseIndex(indices, selector);
        }

        public ICloseIndexResponse CloseIndex(ICloseIndexRequest request)
        {
            return _client.CloseIndex(request);
        }

        public Task<ICloseIndexResponse> CloseIndexAsync(Indices indices, Func<CloseIndexDescriptor, ICloseIndexRequest> selector = null)
        {
            return _client.CloseIndexAsync(indices, selector);
        }

        public Task<ICloseIndexResponse> CloseIndexAsync(ICloseIndexRequest request)
        {
            return _client.CloseIndexAsync(request);
        }

        public IOpenIndexResponse OpenIndex(Indices indices, Func<OpenIndexDescriptor, IOpenIndexRequest> selector = null)
        {
            return _client.OpenIndex(indices, selector);
        }

        public IOpenIndexResponse OpenIndex(IOpenIndexRequest request)
        {
            return _client.OpenIndex(request);
        }

        public Task<IOpenIndexResponse> OpenIndexAsync(Indices indices, Func<OpenIndexDescriptor, IOpenIndexRequest> selector = null)
        {
            return _client.OpenIndexAsync(indices, selector);
        }

        public Task<IOpenIndexResponse> OpenIndexAsync(IOpenIndexRequest request)
        {
            return _client.OpenIndexAsync(request);
        }

        public IExistsResponse TypeExists(Indices indices, Types types, Func<TypeExistsDescriptor, ITypeExistsRequest> selector = null)
        {
            return _client.TypeExists(indices, types, selector);
        }

        public IExistsResponse TypeExists(ITypeExistsRequest request)
        {
            return _client.TypeExists(request);
        }

        public Task<IExistsResponse> TypeExistsAsync(Indices indices, Types types, Func<TypeExistsDescriptor, ITypeExistsRequest> selector = null)
        {
            return _client.TypeExistsAsync(indices, types, selector);
        }

        public Task<IExistsResponse> TypeExistsAsync(ITypeExistsRequest request)
        {
            return _client.TypeExistsAsync(request);
        }

        public IGetIndexSettingsResponse GetIndexSettings(Func<GetIndexSettingsDescriptor, IGetIndexSettingsRequest> selector)
        {
            return _client.GetIndexSettings(selector);
        }

        public IGetIndexSettingsResponse GetIndexSettings(IGetIndexSettingsRequest request)
        {
            return _client.GetIndexSettings(request);
        }

        public Task<IGetIndexSettingsResponse> GetIndexSettingsAsync(Func<GetIndexSettingsDescriptor, IGetIndexSettingsRequest> selector)
        {
            return _client.GetIndexSettingsAsync(selector);
        }

        public Task<IGetIndexSettingsResponse> GetIndexSettingsAsync(IGetIndexSettingsRequest request)
        {
            return _client.GetIndexSettingsAsync(request);
        }

        public IDeleteIndexTemplateResponse DeleteIndexTemplate(Name name, Func<DeleteIndexTemplateDescriptor, IDeleteIndexTemplateRequest> selector = null)
        {
            return _client.DeleteIndexTemplate(name, selector);
        }

        public IDeleteIndexTemplateResponse DeleteIndexTemplate(IDeleteIndexTemplateRequest request)
        {
            return _client.DeleteIndexTemplate(request);
        }

        public Task<IDeleteIndexTemplateResponse> DeleteIndexTemplateAsync(Name name, Func<DeleteIndexTemplateDescriptor, IDeleteIndexTemplateRequest> selector = null)
        {
            return _client.DeleteIndexTemplateAsync(name, selector);
        }

        public Task<IDeleteIndexTemplateResponse> DeleteIndexTemplateAsync(IDeleteIndexTemplateRequest request)
        {
            return _client.DeleteIndexTemplateAsync(request);
        }

        public IGetIndexTemplateResponse GetIndexTemplate(Func<GetIndexTemplateDescriptor, IGetIndexTemplateRequest> selector = null)
        {
            return _client.GetIndexTemplate(selector);
        }

        public IGetIndexTemplateResponse GetIndexTemplate(IGetIndexTemplateRequest request)
        {
            return _client.GetIndexTemplate(request);
        }

        public Task<IGetIndexTemplateResponse> GetIndexTemplateAsync(Func<GetIndexTemplateDescriptor, IGetIndexTemplateRequest> selector = null)
        {
            return _client.GetIndexTemplateAsync(selector);
        }

        public Task<IGetIndexTemplateResponse> GetIndexTemplateAsync(IGetIndexTemplateRequest request)
        {
            return _client.GetIndexTemplateAsync(request);
        }

        public IExistsResponse IndexTemplateExists(Name template, Func<IndexTemplateExistsDescriptor, IIndexTemplateExistsRequest> selector = null)
        {
            return _client.IndexTemplateExists(template, selector);
        }

        public IExistsResponse IndexTemplateExists(IIndexTemplateExistsRequest request)
        {
            return _client.IndexTemplateExists(request);
        }

        public Task<IExistsResponse> IndexTemplateExistsAsync(Name template, Func<IndexTemplateExistsDescriptor, IIndexTemplateExistsRequest> selector = null)
        {
            return _client.IndexTemplateExistsAsync(template, selector);
        }

        public Task<IExistsResponse> IndexTemplateExistsAsync(IIndexTemplateExistsRequest request)
        {
            return _client.IndexTemplateExistsAsync(request);
        }

        public IPutIndexTemplateResponse PutIndexTemplate(Name name, Func<PutIndexTemplateDescriptor, IPutIndexTemplateRequest> selector)
        {
            return _client.PutIndexTemplate(name, selector);
        }

        public IPutIndexTemplateResponse PutIndexTemplate(IPutIndexTemplateRequest request)
        {
            return _client.PutIndexTemplate(request);
        }

        public Task<IPutIndexTemplateResponse> PutIndexTemplateAsync(Name name, Func<PutIndexTemplateDescriptor, IPutIndexTemplateRequest> selector)
        {
            return _client.PutIndexTemplateAsync(name, selector);
        }

        public Task<IPutIndexTemplateResponse> PutIndexTemplateAsync(IPutIndexTemplateRequest request)
        {
            return _client.PutIndexTemplateAsync(request);
        }

        public IUpdateIndexSettingsResponse UpdateIndexSettings(Indices indices, Func<UpdateIndexSettingsDescriptor, IUpdateIndexSettingsRequest> selector)
        {
            return _client.UpdateIndexSettings(indices, selector);
        }

        public IUpdateIndexSettingsResponse UpdateIndexSettings(IUpdateIndexSettingsRequest request)
        {
            return _client.UpdateIndexSettings(request);
        }

        public Task<IUpdateIndexSettingsResponse> UpdateIndexSettingsAsync(Indices indices, Func<UpdateIndexSettingsDescriptor, IUpdateIndexSettingsRequest> selector)
        {
            return _client.UpdateIndexSettingsAsync(indices, selector);
        }

        public Task<IUpdateIndexSettingsResponse> UpdateIndexSettingsAsync(IUpdateIndexSettingsRequest request)
        {
            return _client.UpdateIndexSettingsAsync(request);
        }

        public IGetFieldMappingResponse GetFieldMapping<T>(Fields fields, Func<GetFieldMappingDescriptor<T>, IGetFieldMappingRequest> selector = null) where T : class
        {
            return _client.GetFieldMapping(fields, selector);
        }

        public IGetFieldMappingResponse GetFieldMapping(IGetFieldMappingRequest request)
        {
            return _client.GetFieldMapping(request);
        }

        public Task<IGetFieldMappingResponse> GetFieldMappingAsync<T>(Fields fields, Func<GetFieldMappingDescriptor<T>, IGetFieldMappingRequest> selector = null) where T : class
        {
            return _client.GetFieldMappingAsync(fields, selector);
        }

        public Task<IGetFieldMappingResponse> GetFieldMappingAsync(IGetFieldMappingRequest request)
        {
            return _client.GetFieldMappingAsync(request);
        }

        public IGetMappingResponse GetMapping<T>(Func<GetMappingDescriptor<T>, IGetMappingRequest> selector = null) where T : class
        {
            return _client.GetMapping(selector);
        }

        public IGetMappingResponse GetMapping(IGetMappingRequest request)
        {
            return _client.GetMapping(request);
        }

        public Task<IGetMappingResponse> GetMappingAsync<T>(Func<GetMappingDescriptor<T>, IGetMappingRequest> selector = null) where T : class
        {
            return _client.GetMappingAsync(selector);
        }

        public Task<IGetMappingResponse> GetMappingAsync(IGetMappingRequest request)
        {
            return _client.GetMappingAsync(request);
        }

        public IPutMappingResponse Map<T>(Func<PutMappingDescriptor<T>, IPutMappingRequest> selector) where T : class
        {
            return _client.Map(selector);
        }

        public IPutMappingResponse Map(IPutMappingRequest request)
        {
            return _client.Map(request);
        }

        public Task<IPutMappingResponse> MapAsync<T>(Func<PutMappingDescriptor<T>, IPutMappingRequest> selector) where T : class
        {
            return _client.MapAsync(selector);
        }

        public Task<IPutMappingResponse> MapAsync(IPutMappingRequest request)
        {
            return _client.MapAsync(request);
        }

        public IRecoveryStatusResponse RecoveryStatus(Indices indices, Func<RecoveryStatusDescriptor, IRecoveryStatusRequest> selector = null)
        {
            return _client.RecoveryStatus(indices, selector);
        }

        public IRecoveryStatusResponse RecoveryStatus(IRecoveryStatusRequest request)
        {
            return _client.RecoveryStatus(request);
        }

        public Task<IRecoveryStatusResponse> RecoveryStatusAsync(Indices indices, Func<RecoveryStatusDescriptor, IRecoveryStatusRequest> selector = null)
        {
            return _client.RecoveryStatusAsync(indices, selector);
        }

        public Task<IRecoveryStatusResponse> RecoveryStatusAsync(IRecoveryStatusRequest request)
        {
            return _client.RecoveryStatusAsync(request);
        }

        public ISegmentsResponse Segments(Indices indices, Func<SegmentsDescriptor, ISegmentsRequest> selector = null)
        {
            return _client.Segments(indices, selector);
        }

        public ISegmentsResponse Segments(ISegmentsRequest request)
        {
            return _client.Segments(request);
        }

        public Task<ISegmentsResponse> SegmentsAsync(Indices indices, Func<SegmentsDescriptor, ISegmentsRequest> selector = null)
        {
            return _client.SegmentsAsync(indices, selector);
        }

        public Task<ISegmentsResponse> SegmentsAsync(ISegmentsRequest request)
        {
            return _client.SegmentsAsync(request);
        }

        public IIndicesShardStoresResponse IndicesShardStores(Func<IndicesShardStoresDescriptor, IIndicesShardStoresRequest> selector = null)
        {
            return _client.IndicesShardStores(selector);
        }

        public IIndicesShardStoresResponse IndicesShardStores(IIndicesShardStoresRequest request)
        {
            return _client.IndicesShardStores(request);
        }

        public Task<IIndicesShardStoresResponse> IndicesShardStoresAsync(Func<IndicesShardStoresDescriptor, IIndicesShardStoresRequest> selector = null)
        {
            return _client.IndicesShardStoresAsync(selector);
        }

        public Task<IIndicesShardStoresResponse> IndicesShardStoresAsync(IIndicesShardStoresRequest request)
        {
            return _client.IndicesShardStoresAsync(request);
        }

        public IIndicesStatsResponse IndicesStats(Indices indices, Func<IndicesStatsDescriptor, IIndicesStatsRequest> selector = null)
        {
            return _client.IndicesStats(indices, selector);
        }

        public IIndicesStatsResponse IndicesStats(IIndicesStatsRequest request)
        {
            return _client.IndicesStats(request);
        }

        public Task<IIndicesStatsResponse> IndicesStatsAsync(Indices indices, Func<IndicesStatsDescriptor, IIndicesStatsRequest> selector = null)
        {
            return _client.IndicesStatsAsync(indices, selector);
        }

        public Task<IIndicesStatsResponse> IndicesStatsAsync(IIndicesStatsRequest request)
        {
            return _client.IndicesStatsAsync(request);
        }

        public IClearCacheResponse ClearCache(Indices indices, Func<ClearCacheDescriptor, IClearCacheRequest> selector = null)
        {
            return _client.ClearCache(indices, selector);
        }

        public IClearCacheResponse ClearCache(IClearCacheRequest request)
        {
            return _client.ClearCache(request);
        }

        public Task<IClearCacheResponse> ClearCacheAsync(Indices indices, Func<ClearCacheDescriptor, IClearCacheRequest> selector = null)
        {
            return _client.ClearCacheAsync(indices, selector);
        }

        public Task<IClearCacheResponse> ClearCacheAsync(IClearCacheRequest request)
        {
            return _client.ClearCacheAsync(request);
        }

        public IFlushResponse Flush(Indices indices, Func<FlushDescriptor, IFlushRequest> selector = null)
        {
            return _client.Flush(indices, selector);
        }

        public IFlushResponse Flush(IFlushRequest request)
        {
            return _client.Flush(request);
        }

        public Task<IFlushResponse> FlushAsync(Indices indices, Func<FlushDescriptor, IFlushRequest> selector = null)
        {
            return _client.FlushAsync(indices, selector);
        }

        public Task<IFlushResponse> FlushAsync(IFlushRequest request)
        {
            return _client.FlushAsync(request);
        }

        public IForceMergeResponse ForceMerge(Indices indices, Func<ForceMergeDescriptor, IForceMergeRequest> selector = null)
        {
            return _client.ForceMerge(indices, selector);
        }

        public IForceMergeResponse ForceMerge(IForceMergeRequest request)
        {
            return _client.ForceMerge(request);
        }

        public Task<IForceMergeResponse> ForceMergeAsync(Indices indices, Func<ForceMergeDescriptor, IForceMergeRequest> selector = null)
        {
            return _client.ForceMergeAsync(indices, selector);
        }

        public Task<IForceMergeResponse> ForceMergeAsync(IForceMergeRequest request)
        {
            return _client.ForceMergeAsync(request);
        }

        public IOptimizeResponse Optimize(Indices indices, Func<OptimizeDescriptor, IOptimizeRequest> selector = null)
        {
            return _client.Optimize(indices, selector);
        }

        public IOptimizeResponse Optimize(IOptimizeRequest request)
        {
            return _client.Optimize(request);
        }

        public Task<IOptimizeResponse> OptimizeAsync(Indices indices, Func<OptimizeDescriptor, IOptimizeRequest> selector = null)
        {
            return _client.OptimizeAsync(indices, selector);
        }

        public Task<IOptimizeResponse> OptimizeAsync(IOptimizeRequest request)
        {
            return _client.OptimizeAsync(request);
        }

        public IRefreshResponse Refresh(Indices indices, Func<RefreshDescriptor, IRefreshRequest> selector = null)
        {
            return _client.Refresh(indices, selector);
        }

        public IRefreshResponse Refresh(IRefreshRequest request)
        {
            return _client.Refresh(request);
        }

        public Task<IRefreshResponse> RefreshAsync(Indices indices, Func<RefreshDescriptor, IRefreshRequest> selector = null)
        {
            return _client.RefreshAsync(indices, selector);
        }

        public Task<IRefreshResponse> RefreshAsync(IRefreshRequest request)
        {
            return _client.RefreshAsync(request);
        }

        public ISyncedFlushResponse SyncedFlush(Indices indices, Func<SyncedFlushDescriptor, ISyncedFlushRequest> selector = null)
        {
            return _client.SyncedFlush(indices, selector);
        }

        public ISyncedFlushResponse SyncedFlush(ISyncedFlushRequest request)
        {
            return _client.SyncedFlush(request);
        }

        public Task<ISyncedFlushResponse> SyncedFlushAsync(Indices indices, Func<SyncedFlushDescriptor, ISyncedFlushRequest> selector = null)
        {
            return _client.SyncedFlushAsync(indices, selector);
        }

        public Task<ISyncedFlushResponse> SyncedFlushAsync(ISyncedFlushRequest request)
        {
            return _client.SyncedFlushAsync(request);
        }

        public IUpgradeResponse Upgrade(IUpgradeRequest request)
        {
            return _client.Upgrade(request);
        }

        public IUpgradeResponse Upgrade(Indices indices, Func<UpgradeDescriptor, IUpgradeRequest> selector = null)
        {
            return _client.Upgrade(indices, selector);
        }

        public Task<IUpgradeResponse> UpgradeAsync(IUpgradeRequest request)
        {
            return _client.UpgradeAsync(request);
        }

        public Task<IUpgradeResponse> UpgradeAsync(Indices indices, Func<UpgradeDescriptor, IUpgradeRequest> selector = null)
        {
            return _client.UpgradeAsync(indices, selector);
        }

        public IUpgradeStatusResponse UpgradeStatus(IUpgradeStatusRequest request)
        {
            return _client.UpgradeStatus(request);
        }

        public IUpgradeStatusResponse UpgradeStatus(Func<UpgradeStatusDescriptor, IUpgradeStatusRequest> selector = null)
        {
            return _client.UpgradeStatus(selector);
        }

        public Task<IUpgradeStatusResponse> UpgradeStatusAsync(IUpgradeStatusRequest request)
        {
            return _client.UpgradeStatusAsync(request);
        }

        public Task<IUpgradeStatusResponse> UpgradeStatusAsync(Func<UpgradeStatusDescriptor, IUpgradeStatusRequest> selector = null)
        {
            return _client.UpgradeStatusAsync(selector);
        }

        public IDeleteWarmerResponse DeleteWarmer(Indices indices, Names names, Func<DeleteWarmerDescriptor, IDeleteWarmerRequest> selector = null)
        {
            return _client.DeleteWarmer(indices, names, selector);
        }

        public IDeleteWarmerResponse DeleteWarmer(IDeleteWarmerRequest request)
        {
            return _client.DeleteWarmer(request);
        }

        public Task<IDeleteWarmerResponse> DeleteWarmerAsync(Indices indices, Names names, Func<DeleteWarmerDescriptor, IDeleteWarmerRequest> selector = null)
        {
            return _client.DeleteWarmerAsync(indices, names, selector);
        }

        public Task<IDeleteWarmerResponse> DeleteWarmerAsync(IDeleteWarmerRequest request)
        {
            return _client.DeleteWarmerAsync(request);
        }

        public IGetWarmerResponse GetWarmer(Func<GetWarmerDescriptor, IGetWarmerRequest> selector = null)
        {
            return _client.GetWarmer(selector);
        }

        public IGetWarmerResponse GetWarmer(IGetWarmerRequest request)
        {
            return _client.GetWarmer(request);
        }

        public Task<IGetWarmerResponse> GetWarmerAsync(Func<GetWarmerDescriptor, IGetWarmerRequest> selector = null)
        {
            return _client.GetWarmerAsync(selector);
        }

        public Task<IGetWarmerResponse> GetWarmerAsync(IGetWarmerRequest request)
        {
            return _client.GetWarmerAsync(request);
        }

        public IPutWarmerResponse PutWarmer(Name name, Func<PutWarmerDescriptor, IPutWarmerRequest> selector)
        {
            return _client.PutWarmer(name, selector);
        }

        public IPutWarmerResponse PutWarmer(IPutWarmerRequest request)
        {
            return _client.PutWarmer(request);
        }

        public Task<IPutWarmerResponse> PutWarmerAsync(Name name, Func<PutWarmerDescriptor, IPutWarmerRequest> selector)
        {
            return _client.PutWarmerAsync(name, selector);
        }

        public Task<IPutWarmerResponse> PutWarmerAsync(IPutWarmerRequest request)
        {
            return _client.PutWarmerAsync(request);
        }

        public IDeleteScriptResponse DeleteScript(IDeleteScriptRequest request)
        {
            return _client.DeleteScript(request);
        }

        public IDeleteScriptResponse DeleteScript(Name language, Id id, Func<DeleteScriptDescriptor, IDeleteScriptRequest> selector = null)
        {
            return _client.DeleteScript(language, id, selector);
        }

        public Task<IDeleteScriptResponse> DeleteScriptAsync(Name language, Id id, Func<DeleteScriptDescriptor, IDeleteScriptRequest> selector = null)
        {
            return _client.DeleteScriptAsync(language, id, selector);
        }

        public Task<IDeleteScriptResponse> DeleteScriptAsync(IDeleteScriptRequest request)
        {
            return _client.DeleteScriptAsync(request);
        }

        public IGetScriptResponse GetScript(Name language, Id id, Func<GetScriptDescriptor, IGetScriptRequest> selector = null)
        {
            return _client.GetScript(language, id, selector);
        }

        public IGetScriptResponse GetScript(IGetScriptRequest request)
        {
            return _client.GetScript(request);
        }

        public Task<IGetScriptResponse> GetScriptAsync(Name language, Id id, Func<GetScriptDescriptor, IGetScriptRequest> selector = null)
        {
            return _client.GetScriptAsync(language, id, selector);
        }

        public Task<IGetScriptResponse> GetScriptAsync(IGetScriptRequest request)
        {
            return _client.GetScriptAsync(request);
        }

        public IPutScriptResponse PutScript(Name language, Id id, Func<PutScriptDescriptor, IPutScriptRequest> selector)
        {
            return _client.PutScript(language, id, selector);
        }

        public IPutScriptResponse PutScript(IPutScriptRequest request)
        {
            return _client.PutScript(request);
        }

        public Task<IPutScriptResponse> PutScriptAsync(Name language, Id id, Func<PutScriptDescriptor, IPutScriptRequest> selector)
        {
            return _client.PutScriptAsync(language, id, selector);
        }

        public Task<IPutScriptResponse> PutScriptAsync(IPutScriptRequest request)
        {
            return _client.PutScriptAsync(request);
        }

        public ICreateRepositoryResponse CreateRepository(Name repository, Func<CreateRepositoryDescriptor, ICreateRepositoryRequest> selector)
        {
            return _client.CreateRepository(repository, selector);
        }

        public ICreateRepositoryResponse CreateRepository(ICreateRepositoryRequest request)
        {
            return _client.CreateRepository(request);
        }

        public Task<ICreateRepositoryResponse> CreateRepositoryAsync(Name repository, Func<CreateRepositoryDescriptor, ICreateRepositoryRequest> selector)
        {
            return _client.CreateRepositoryAsync(repository, selector);
        }

        public Task<ICreateRepositoryResponse> CreateRepositoryAsync(ICreateRepositoryRequest request)
        {
            return _client.CreateRepositoryAsync(request);
        }

        public IDeleteRepositoryResponse DeleteRepository(Names repositories, Func<DeleteRepositoryDescriptor, IDeleteRepositoryRequest> selector = null)
        {
            return _client.DeleteRepository(repositories, selector);
        }

        public IDeleteRepositoryResponse DeleteRepository(IDeleteRepositoryRequest request)
        {
            return _client.DeleteRepository(request);
        }

        public Task<IDeleteRepositoryResponse> DeleteRepositoryAsync(Names repositories, Func<DeleteRepositoryDescriptor, IDeleteRepositoryRequest> selector = null)
        {
            return _client.DeleteRepositoryAsync(repositories, selector);
        }

        public Task<IDeleteRepositoryResponse> DeleteRepositoryAsync(IDeleteRepositoryRequest request)
        {
            return _client.DeleteRepositoryAsync(request);
        }

        public IGetRepositoryResponse GetRepository(Func<GetRepositoryDescriptor, IGetRepositoryRequest> selector = null)
        {
            return _client.GetRepository(selector);
        }

        public IGetRepositoryResponse GetRepository(IGetRepositoryRequest request)
        {
            return _client.GetRepository(request);
        }

        public Task<IGetRepositoryResponse> GetRepositoryAsync(Func<GetRepositoryDescriptor, IGetRepositoryRequest> selector = null)
        {
            return _client.GetRepositoryAsync(selector);
        }

        public Task<IGetRepositoryResponse> GetRepositoryAsync(IGetRepositoryRequest request)
        {
            return _client.GetRepositoryAsync(request);
        }

        public IVerifyRepositoryResponse VerifyRepository(Name repository, Func<VerifyRepositoryDescriptor, IVerifyRepositoryRequest> selector = null)
        {
            return _client.VerifyRepository(repository, selector);
        }

        public IVerifyRepositoryResponse VerifyRepository(IVerifyRepositoryRequest request)
        {
            return _client.VerifyRepository(request);
        }

        public Task<IVerifyRepositoryResponse> VerifyRepositoryAsync(Name repository, Func<VerifyRepositoryDescriptor, IVerifyRepositoryRequest> selector = null)
        {
            return _client.VerifyRepositoryAsync(repository, selector);
        }

        public Task<IVerifyRepositoryResponse> VerifyRepositoryAsync(IVerifyRepositoryRequest request)
        {
            return _client.VerifyRepositoryAsync(request);
        }

        public IRestoreResponse Restore(IRestoreRequest request)
        {
            return _client.Restore(request);
        }

        public IRestoreResponse Restore(Name repository, Name snapshotName, Func<RestoreDescriptor, IRestoreRequest> selector = null)
        {
            return _client.Restore(repository, snapshotName, selector);
        }

        public Task<IRestoreResponse> RestoreAsync(IRestoreRequest request)
        {
            return _client.RestoreAsync(request);
        }

        public Task<IRestoreResponse> RestoreAsync(Name repository, Name snapshotName, Func<RestoreDescriptor, IRestoreRequest> selector = null)
        {
            return _client.RestoreAsync(repository, snapshotName, selector);
        }

        public IObservable<IRecoveryStatusResponse> RestoreObservable(Name repository, Name snapshot, TimeSpan interval, Func<RestoreDescriptor, IRestoreRequest> selector = null)
        {
            return _client.RestoreObservable(repository, snapshot, interval, selector);
        }

        public IObservable<IRecoveryStatusResponse> RestoreObservable(TimeSpan interval, IRestoreRequest request)
        {
            return _client.RestoreObservable(interval, request);
        }

        public IDeleteSnapshotResponse DeleteSnapshot(Name repository, Name snapshotName, Func<DeleteSnapshotDescriptor, IDeleteSnapshotRequest> selector = null)
        {
            return _client.DeleteSnapshot(repository, snapshotName, selector);
        }

        public IDeleteSnapshotResponse DeleteSnapshot(IDeleteSnapshotRequest request)
        {
            return _client.DeleteSnapshot(request);
        }

        public Task<IDeleteSnapshotResponse> DeleteSnapshotAsync(Name repository, Name snapshotName, Func<DeleteSnapshotDescriptor, IDeleteSnapshotRequest> selector = null)
        {
            return _client.DeleteSnapshotAsync(repository, snapshotName, selector);
        }

        public Task<IDeleteSnapshotResponse> DeleteSnapshotAsync(IDeleteSnapshotRequest request)
        {
            return _client.DeleteSnapshotAsync(request);
        }

        public IGetSnapshotResponse GetSnapshot(Name repository, Names snapshots, Func<GetSnapshotDescriptor, IGetSnapshotRequest> selector = null)
        {
            return _client.GetSnapshot(repository, snapshots, selector);
        }

        public IGetSnapshotResponse GetSnapshot(IGetSnapshotRequest request)
        {
            return _client.GetSnapshot(request);
        }

        public Task<IGetSnapshotResponse> GetSnapshotAsync(Name repository, Names snapshots, Func<GetSnapshotDescriptor, IGetSnapshotRequest> selector = null)
        {
            return _client.GetSnapshotAsync(repository, snapshots, selector);
        }

        public Task<IGetSnapshotResponse> GetSnapshotAsync(IGetSnapshotRequest request)
        {
            return _client.GetSnapshotAsync(request);
        }

        public IObservable<ISnapshotStatusResponse> SnapshotObservable(Name repository, Name snapshotName, TimeSpan interval, Func<SnapshotDescriptor, ISnapshotRequest> selector = null)
        {
            return _client.SnapshotObservable(repository, snapshotName, interval, selector);
        }

        public IObservable<ISnapshotStatusResponse> SnapshotObservable(TimeSpan interval, ISnapshotRequest request)
        {
            return _client.SnapshotObservable(interval, request);
        }

        public ISnapshotStatusResponse SnapshotStatus(Func<SnapshotStatusDescriptor, ISnapshotStatusRequest> selector = null)
        {
            return _client.SnapshotStatus(selector);
        }

        public ISnapshotStatusResponse SnapshotStatus(ISnapshotStatusRequest request)
        {
            return _client.SnapshotStatus(request);
        }

        public Task<ISnapshotStatusResponse> SnapshotStatusAsync(Func<SnapshotStatusDescriptor, ISnapshotStatusRequest> selector = null)
        {
            return _client.SnapshotStatusAsync(selector);
        }

        public Task<ISnapshotStatusResponse> SnapshotStatusAsync(ISnapshotStatusRequest request)
        {
            return _client.SnapshotStatusAsync(request);
        }

        public ISnapshotResponse Snapshot(Name repository, Name snapshotName, Func<SnapshotDescriptor, ISnapshotRequest> selector = null)
        {
            return _client.Snapshot(repository, snapshotName, selector);
        }

        public ISnapshotResponse Snapshot(ISnapshotRequest request)
        {
            return _client.Snapshot(request);
        }

        public Task<ISnapshotResponse> SnapshotAsync(Name repository, Name snapshotName, Func<SnapshotDescriptor, ISnapshotRequest> selector = null)
        {
            return _client.SnapshotAsync(repository, snapshotName, selector);
        }

        public Task<ISnapshotResponse> SnapshotAsync(ISnapshotRequest request)
        {
            return _client.SnapshotAsync(request);
        }

        public ICountResponse Count<T>(Func<CountDescriptor<T>, ICountRequest> selector = null) where T : class
        {
            return _client.Count(selector);
        }

        public ICountResponse Count<T>(ICountRequest request) where T : class
        {
            return _client.Count<T>(request);
        }

        public Task<ICountResponse> CountAsync<T>(Func<CountDescriptor<T>, ICountRequest> selector = null) where T : class
        {
            return _client.CountAsync(selector);
        }

        public Task<ICountResponse> CountAsync<T>(ICountRequest request) where T : class
        {
            return _client.CountAsync<T>(request);
        }

        public IExplainResponse<T> Explain<T>(DocumentPath<T> document, Func<ExplainDescriptor<T>, IExplainRequest<T>> selector) where T : class
        {
            return _client.Explain(document, selector);
        }

        public IExplainResponse<T> Explain<T>(IExplainRequest<T> request) where T : class
        {
            return _client.Explain(request);
        }

        public Task<IExplainResponse<T>> ExplainAsync<T>(DocumentPath<T> document, Func<ExplainDescriptor<T>, IExplainRequest<T>> selector) where T : class
        {
            return _client.ExplainAsync(document, selector);
        }

        public Task<IExplainResponse<T>> ExplainAsync<T>(IExplainRequest<T> request) where T : class
        {
            return _client.ExplainAsync(request);
        }

        public IFieldStatsResponse FieldStats(Indices indices, Func<FieldStatsDescriptor, IFieldStatsRequest> selector = null)
        {
            return _client.FieldStats(indices, selector);
        }

        public IFieldStatsResponse FieldStats(IFieldStatsRequest request)
        {
            return _client.FieldStats(request);
        }

        public Task<IFieldStatsResponse> FieldStatsAsync(Indices indices, Func<FieldStatsDescriptor, IFieldStatsRequest> selector = null)
        {
            return _client.FieldStatsAsync(indices, selector);
        }

        public Task<IFieldStatsResponse> FieldStatsAsync(IFieldStatsRequest request)
        {
            return _client.FieldStatsAsync(request);
        }

        public IMultiSearchResponse MultiSearch(Func<MultiSearchDescriptor, IMultiSearchRequest> selector)
        {
            return _client.MultiSearch(selector);
        }

        public IMultiSearchResponse MultiSearch(IMultiSearchRequest request)
        {
            return _client.MultiSearch(request);
        }

        public Task<IMultiSearchResponse> MultiSearchAsync(Func<MultiSearchDescriptor, IMultiSearchRequest> selector)
        {
            return _client.MultiSearchAsync(selector);
        }

        public Task<IMultiSearchResponse> MultiSearchAsync(IMultiSearchRequest request)
        {
            return _client.MultiSearchAsync(request);
        }

        public IMultiPercolateResponse MultiPercolate(Func<MultiPercolateDescriptor, IMultiPercolateRequest> selector)
        {
            return _client.MultiPercolate(selector);
        }

        public IMultiPercolateResponse MultiPercolate(IMultiPercolateRequest request)
        {
            return _client.MultiPercolate(request);
        }

        public Task<IMultiPercolateResponse> MultiPercolateAsync(Func<MultiPercolateDescriptor, IMultiPercolateRequest> selector)
        {
            return _client.MultiPercolateAsync(selector);
        }

        public Task<IMultiPercolateResponse> MultiPercolateAsync(IMultiPercolateRequest request)
        {
            return _client.MultiPercolateAsync(request);
        }

        public IPercolateCountResponse PercolateCount<T>(Func<PercolateCountDescriptor<T>, IPercolateCountRequest<T>> selector = null) where T : class
        {
            return _client.PercolateCount(selector);
        }

        public IPercolateCountResponse PercolateCount<T>(IPercolateCountRequest<T> request) where T : class
        {
            return _client.PercolateCount(request);
        }

        public Task<IPercolateCountResponse> PercolateCountAsync<T>(Func<PercolateCountDescriptor<T>, IPercolateCountRequest<T>> selector = null) where T : class
        {
            return _client.PercolateCountAsync(selector);
        }

        public Task<IPercolateCountResponse> PercolateCountAsync<T>(IPercolateCountRequest<T> request) where T : class
        {
            return _client.PercolateCountAsync(request);
        }

        public IPercolateResponse Percolate<T>(Func<PercolateDescriptor<T>, IPercolateRequest<T>> selector) where T : class
        {
            return _client.Percolate(selector);
        }

        public IPercolateResponse Percolate<T>(IPercolateRequest<T> request) where T : class
        {
            return _client.Percolate(request);
        }

        public Task<IPercolateResponse> PercolateAsync<T>(Func<PercolateDescriptor<T>, IPercolateRequest<T>> selector) where T : class
        {
            return _client.PercolateAsync(selector);
        }

        public Task<IPercolateResponse> PercolateAsync<T>(IPercolateRequest<T> request) where T : class
        {
            return _client.PercolateAsync(request);
        }

        public IRegisterPercolatorResponse RegisterPercolator<T>(Name name, Func<RegisterPercolatorDescriptor<T>, IRegisterPercolatorRequest> selector) where T : class
        {
            return _client.RegisterPercolator(name, selector);
        }

        public IRegisterPercolatorResponse RegisterPercolator(IRegisterPercolatorRequest request)
        {
            return _client.RegisterPercolator(request);
        }

        public Task<IRegisterPercolatorResponse> RegisterPercolatorAsync<T>(Name name, Func<RegisterPercolatorDescriptor<T>, IRegisterPercolatorRequest> selector) where T : class
        {
            return _client.RegisterPercolatorAsync(name, selector);
        }

        public Task<IRegisterPercolatorResponse> RegisterPercolatorAsync(IRegisterPercolatorRequest request)
        {
            return _client.RegisterPercolatorAsync(request);
        }

        public IUnregisterPercolatorResponse UnregisterPercolator<T>(Name name, Func<UnregisterPercolatorDescriptor<T>, IUnregisterPercolatorRequest> selector = null) where T : class
        {
            return _client.UnregisterPercolator(name, selector);
        }

        public IUnregisterPercolatorResponse UnregisterPercolator(IUnregisterPercolatorRequest request)
        {
            return _client.UnregisterPercolator(request);
        }

        public Task<IUnregisterPercolatorResponse> UnregisterPercolatorAsync<T>(Name name, Func<UnregisterPercolatorDescriptor<T>, IUnregisterPercolatorRequest> selector = null) where T : class
        {
            return _client.UnregisterPercolatorAsync(name, selector);
        }

        public Task<IUnregisterPercolatorResponse> UnregisterPercolatorAsync(IUnregisterPercolatorRequest request)
        {
            return _client.UnregisterPercolatorAsync(request);
        }

        public IClearScrollResponse ClearScroll(Func<ClearScrollDescriptor, IClearScrollRequest> selector)
        {
            return _client.ClearScroll(selector);
        }

        public IClearScrollResponse ClearScroll(IClearScrollRequest request)
        {
            return _client.ClearScroll(request);
        }

        public Task<IClearScrollResponse> ClearScrollAsync(Func<ClearScrollDescriptor, IClearScrollRequest> selector)
        {
            return _client.ClearScrollAsync(selector);
        }

        public Task<IClearScrollResponse> ClearScrollAsync(IClearScrollRequest request)
        {
            return _client.ClearScrollAsync(request);
        }

        public ISearchResponse<T> Scroll<T>(IScrollRequest request) where T : class
        {
            return _client.Scroll<T>(request);
        }

        public ISearchResponse<T> Scroll<T>(Time scrollTime, string scrollId, Func<ScrollDescriptor<T>, IScrollRequest> selector = null) where T : class
        {
            return _client.Scroll(scrollTime, scrollId, selector);
        }

        public Task<ISearchResponse<T>> ScrollAsync<T>(IScrollRequest request) where T : class
        {
            return _client.ScrollAsync<T>(request);
        }

        public Task<ISearchResponse<T>> ScrollAsync<T>(Time scrollTime, string scrollId, Func<ScrollDescriptor<T>, IScrollRequest> selector = null) where T : class
        {
            return _client.ScrollAsync(scrollTime, scrollId, selector);
        }

        public IExistsResponse SearchExists<T>(Func<SearchExistsDescriptor<T>, ISearchExistsRequest> selector) where T : class
        {
            return _client.SearchExists(selector);
        }

        public IExistsResponse SearchExists(ISearchExistsRequest request)
        {
            return _client.SearchExists(request);
        }

        public Task<IExistsResponse> SearchExistsAsync<T>(Func<SearchExistsDescriptor<T>, ISearchExistsRequest> selector) where T : class
        {
            return _client.SearchExistsAsync(selector);
        }

        public Task<IExistsResponse> SearchExistsAsync(ISearchExistsRequest request)
        {
            return _client.SearchExistsAsync(request);
        }

        public ISearchShardsResponse SearchShards<T>(Func<SearchShardsDescriptor<T>, ISearchShardsRequest> selector) where T : class
        {
            return _client.SearchShards(selector);
        }

        public ISearchShardsResponse SearchShards(ISearchShardsRequest request)
        {
            return _client.SearchShards(request);
        }

        public Task<ISearchShardsResponse> SearchShardsAsync<T>(Func<SearchShardsDescriptor<T>, ISearchShardsRequest> selector) where T : class
        {
            return _client.SearchShardsAsync(selector);
        }

        public Task<ISearchShardsResponse> SearchShardsAsync(ISearchShardsRequest request)
        {
            return _client.SearchShardsAsync(request);
        }

        public IDeleteSearchTemplateResponse DeleteSearchTemplate(Id id, Func<DeleteSearchTemplateDescriptor, IDeleteSearchTemplateRequest> selector = null)
        {
            return _client.DeleteSearchTemplate(id, selector);
        }

        public IDeleteSearchTemplateResponse DeleteSearchTemplate(IDeleteSearchTemplateRequest request)
        {
            return _client.DeleteSearchTemplate(request);
        }

        public Task<IDeleteSearchTemplateResponse> DeleteSearchTemplateAsync(Id id, Func<DeleteSearchTemplateDescriptor, IDeleteSearchTemplateRequest> selector = null)
        {
            return _client.DeleteSearchTemplateAsync(id, selector);
        }

        public Task<IDeleteSearchTemplateResponse> DeleteSearchTemplateAsync(IDeleteSearchTemplateRequest request)
        {
            return _client.DeleteSearchTemplateAsync(request);
        }

        public ISearchResponse<T> SearchTemplate<T>(Func<SearchTemplateDescriptor<T>, ISearchTemplateRequest> selector) where T : class
        {
            return _client.SearchTemplate(selector);
        }

        public ISearchResponse<TResult> SearchTemplate<T, TResult>(Func<SearchTemplateDescriptor<T>, ISearchTemplateRequest> selector) where T : class where TResult : class
        {
            return _client.SearchTemplate<T, TResult>(selector);
        }

        public ISearchResponse<T> SearchTemplate<T>(ISearchTemplateRequest request) where T : class
        {
            return _client.SearchTemplate<T>(request);
        }

        public ISearchResponse<TResult> SearchTemplate<T, TResult>(ISearchTemplateRequest request) where T : class where TResult : class
        {
            return _client.SearchTemplate<T, TResult>(request);
        }

        public Task<ISearchResponse<T>> SearchTemplateAsync<T>(Func<SearchTemplateDescriptor<T>, ISearchTemplateRequest> selector) where T : class
        {
            return _client.SearchTemplateAsync(selector);
        }

        public Task<ISearchResponse<TResult>> SearchTemplateAsync<T, TResult>(Func<SearchTemplateDescriptor<T>, ISearchTemplateRequest> selector) where T : class where TResult : class
        {
            return _client.SearchTemplateAsync<T, TResult>(selector);
        }

        public Task<ISearchResponse<T>> SearchTemplateAsync<T>(ISearchTemplateRequest request) where T : class
        {
            return _client.SearchTemplateAsync<T>(request);
        }

        public Task<ISearchResponse<TResult>> SearchTemplateAsync<T, TResult>(ISearchTemplateRequest request) where T : class where TResult : class
        {
            return _client.SearchTemplateAsync<T, TResult>(request);
        }

        public IRenderSearchTemplateResponse RenderSearchTemplate(Func<RenderSearchTemplateDescriptor, IRenderSearchTemplateRequest> selector)
        {
            return _client.RenderSearchTemplate(selector);
        }

        public IRenderSearchTemplateResponse RenderSearchTemplate(IRenderSearchTemplateRequest request)
        {
            return _client.RenderSearchTemplate(request);
        }

        public Task<IRenderSearchTemplateResponse> RenderSearchTemplateAsync(Func<RenderSearchTemplateDescriptor, IRenderSearchTemplateRequest> selector)
        {
            return _client.RenderSearchTemplateAsync(selector);
        }

        public Task<IRenderSearchTemplateResponse> RenderSearchTemplateAsync(IRenderSearchTemplateRequest request)
        {
            return _client.RenderSearchTemplateAsync(request);
        }

        public IGetSearchTemplateResponse GetSearchTemplate(Id id, Func<GetSearchTemplateDescriptor, IGetSearchTemplateRequest> selector = null)
        {
            return _client.GetSearchTemplate(id, selector);
        }

        public IGetSearchTemplateResponse GetSearchTemplate(IGetSearchTemplateRequest request)
        {
            return _client.GetSearchTemplate(request);
        }

        public Task<IGetSearchTemplateResponse> GetSearchTemplateAsync(Id id, Func<GetSearchTemplateDescriptor, IGetSearchTemplateRequest> selector = null)
        {
            return _client.GetSearchTemplateAsync(id, selector);
        }

        public Task<IGetSearchTemplateResponse> GetSearchTemplateAsync(IGetSearchTemplateRequest request)
        {
            return _client.GetSearchTemplateAsync(request);
        }

        public IPutSearchTemplateResponse PutSearchTemplate(Id id, Func<PutSearchTemplateDescriptor, IPutSearchTemplateRequest> selector)
        {
            return _client.PutSearchTemplate(id, selector);
        }

        public IPutSearchTemplateResponse PutSearchTemplate(IPutSearchTemplateRequest request)
        {
            return _client.PutSearchTemplate(request);
        }

        public Task<IPutSearchTemplateResponse> PutSearchTemplateAsync(Id id, Func<PutSearchTemplateDescriptor, IPutSearchTemplateRequest> selector)
        {
            return _client.PutSearchTemplateAsync(id, selector);
        }

        public Task<IPutSearchTemplateResponse> PutSearchTemplateAsync(IPutSearchTemplateRequest request)
        {
            return _client.PutSearchTemplateAsync(request);
        }

        public ISearchResponse<T> Search<T>(Func<SearchDescriptor<T>, ISearchRequest> selector = null) where T : class
        {
            return _client.Search(selector);
        }

        public ISearchResponse<TResult> Search<T, TResult>(Func<SearchDescriptor<T>, ISearchRequest> selector = null) where T : class where TResult : class
        {
            return _client.Search<T, TResult>(selector);
        }

        public ISearchResponse<T> Search<T>(ISearchRequest request) where T : class
        {
            return _client.Search<T>(request);
        }

        public ISearchResponse<TResult> Search<T, TResult>(ISearchRequest request) where T : class where TResult : class
        {
            return _client.Search<T, TResult>(request);
        }

        public Task<ISearchResponse<T>> SearchAsync<T>(Func<SearchDescriptor<T>, ISearchRequest> selector = null) where T : class
        {
            return _client.SearchAsync(selector);
        }

        public Task<ISearchResponse<TResult>> SearchAsync<T, TResult>(Func<SearchDescriptor<T>, ISearchRequest> selector = null) where T : class where TResult : class
        {
            return _client.SearchAsync<T, TResult>(selector);
        }

        public Task<ISearchResponse<T>> SearchAsync<T>(ISearchRequest request) where T : class
        {
            return _client.SearchAsync<T>(request);
        }

        public Task<ISearchResponse<TResult>> SearchAsync<T, TResult>(ISearchRequest request) where T : class where TResult : class
        {
            return _client.SearchAsync<T, TResult>(request);
        }

        public ISuggestResponse Suggest<T>(Func<SuggestDescriptor<T>, ISuggestRequest> selector) where T : class
        {
            return _client.Suggest(selector);
        }

        public ISuggestResponse Suggest(ISuggestRequest request)
        {
            return _client.Suggest(request);
        }

        public Task<ISuggestResponse> SuggestAsync<T>(Func<SuggestDescriptor<T>, ISuggestRequest> selector) where T : class
        {
            return _client.SuggestAsync(selector);
        }

        public Task<ISuggestResponse> SuggestAsync(ISuggestRequest request)
        {
            return _client.SuggestAsync(request);
        }

        public IValidateQueryResponse ValidateQuery<T>(Func<ValidateQueryDescriptor<T>, IValidateQueryRequest> selector) where T : class
        {
            return _client.ValidateQuery(selector);
        }

        public IValidateQueryResponse ValidateQuery(IValidateQueryRequest request)
        {
            return _client.ValidateQuery(request);
        }

        public Task<IValidateQueryResponse> ValidateQueryAsync<T>(Func<ValidateQueryDescriptor<T>, IValidateQueryRequest> selector) where T : class
        {
            return _client.ValidateQueryAsync(selector);
        }

        public Task<IValidateQueryResponse> ValidateQueryAsync(IValidateQueryRequest request)
        {
            return _client.ValidateQueryAsync(request);
        }

        public IGraphExploreResponse GraphExplore<T>(Func<GraphExploreDescriptor<T>, IGraphExploreRequest> selector) where T : class
        {
            return _client.GraphExplore(selector);
        }

        public IGraphExploreResponse GraphExplore(IGraphExploreRequest request)
        {
            return _client.GraphExplore(request);
        }

        public Task<IGraphExploreResponse> GraphExploreAsync<T>(Func<GraphExploreDescriptor<T>, IGraphExploreRequest> selector) where T : class
        {
            return _client.GraphExploreAsync(selector);
        }

        public Task<IGraphExploreResponse> GraphExploreAsync(IGraphExploreRequest request)
        {
            return _client.GraphExploreAsync(request);
        }

        public IDeleteLicenseResponse DeleteLicense(Func<DeleteLicenseDescriptor, IDeleteLicenseRequest> selector = null)
        {
            return _client.DeleteLicense(selector);
        }

        public IDeleteLicenseResponse DeleteLicense(IDeleteLicenseRequest request)
        {
            return _client.DeleteLicense(request);
        }

        public Task<IDeleteLicenseResponse> DeleteLicenseAsync(Func<DeleteLicenseDescriptor, IDeleteLicenseRequest> selector = null)
        {
            return _client.DeleteLicenseAsync(selector);
        }

        public Task<IDeleteLicenseResponse> DeleteLicenseAsync(IDeleteLicenseRequest request)
        {
            return _client.DeleteLicenseAsync(request);
        }

        public IGetLicenseResponse GetLicense(Func<GetLicenseDescriptor, IGetLicenseRequest> selector = null)
        {
            return _client.GetLicense(selector);
        }

        public IGetLicenseResponse GetLicense(IGetLicenseRequest request)
        {
            return _client.GetLicense(request);
        }

        public Task<IGetLicenseResponse> GetLicenseAsync(Func<GetLicenseDescriptor, IGetLicenseRequest> selector = null)
        {
            return _client.GetLicenseAsync(selector);
        }

        public Task<IGetLicenseResponse> GetLicenseAsync(IGetLicenseRequest request)
        {
            return _client.GetLicenseAsync(request);
        }

        public IPostLicenseResponse PostLicense(Func<PostLicenseDescriptor, IPostLicenseRequest> selector = null)
        {
            return _client.PostLicense(selector);
        }

        public IPostLicenseResponse PostLicense(IPostLicenseRequest request)
        {
            return _client.PostLicense(request);
        }

        public Task<IPostLicenseResponse> PostLicenseAsync(Func<PostLicenseDescriptor, IPostLicenseRequest> selector = null)
        {
            return _client.PostLicenseAsync(selector);
        }

        public Task<IPostLicenseResponse> PostLicenseAsync(IPostLicenseRequest request)
        {
            return _client.PostLicenseAsync(request);
        }

        public IAuthenticateResponse Authenticate(Func<AuthenticateDescriptor, IAuthenticateRequest> selector = null)
        {
            return _client.Authenticate(selector);
        }

        public IAuthenticateResponse Authenticate(IAuthenticateRequest request)
        {
            return _client.Authenticate(request);
        }

        public Task<IAuthenticateResponse> AuthenticateAsync(Func<AuthenticateDescriptor, IAuthenticateRequest> selector = null)
        {
            return _client.AuthenticateAsync(selector);
        }

        public Task<IAuthenticateResponse> AuthenticateAsync(IAuthenticateRequest request)
        {
            return _client.AuthenticateAsync(request);
        }

        public IClearCachedRealmsResponse ClearCachedRealms(Names realms, Func<ClearCachedRealmsDescriptor, IClearCachedRealmsRequest> selector = null)
        {
            return _client.ClearCachedRealms(realms, selector);
        }

        public IClearCachedRealmsResponse ClearCachedRealms(IClearCachedRealmsRequest request)
        {
            return _client.ClearCachedRealms(request);
        }

        public Task<IClearCachedRealmsResponse> ClearCachedRealmsAsync(Names realms, Func<ClearCachedRealmsDescriptor, IClearCachedRealmsRequest> selector = null)
        {
            return _client.ClearCachedRealmsAsync(realms, selector);
        }

        public Task<IClearCachedRealmsResponse> ClearCachedRealmsAsync(IClearCachedRealmsRequest request)
        {
            return _client.ClearCachedRealmsAsync(request);
        }

        public IClearCachedRolesResponse ClearCachedRoles(Names roles, Func<ClearCachedRolesDescriptor, IClearCachedRolesRequest> selector = null)
        {
            return _client.ClearCachedRoles(roles, selector);
        }

        public IClearCachedRolesResponse ClearCachedRoles(IClearCachedRolesRequest request)
        {
            return _client.ClearCachedRoles(request);
        }

        public Task<IClearCachedRolesResponse> ClearCachedRolesAsync(Names roles, Func<ClearCachedRolesDescriptor, IClearCachedRolesRequest> selector = null)
        {
            return _client.ClearCachedRolesAsync(roles, selector);
        }

        public Task<IClearCachedRolesResponse> ClearCachedRolesAsync(IClearCachedRolesRequest request)
        {
            return _client.ClearCachedRolesAsync(request);
        }

        public IDeleteRoleResponse DeleteRole(Name role, Func<DeleteRoleDescriptor, IDeleteRoleRequest> selector = null)
        {
            return _client.DeleteRole(role, selector);
        }

        public IDeleteRoleResponse DeleteRole(IDeleteRoleRequest request)
        {
            return _client.DeleteRole(request);
        }

        public Task<IDeleteRoleResponse> DeleteRoleAsync(Name role, Func<DeleteRoleDescriptor, IDeleteRoleRequest> selector = null)
        {
            return _client.DeleteRoleAsync(role, selector);
        }

        public Task<IDeleteRoleResponse> DeleteRoleAsync(IDeleteRoleRequest request)
        {
            return _client.DeleteRoleAsync(request);
        }

        public IGetRoleResponse GetRole(Func<GetRoleDescriptor, IGetRoleRequest> selector = null)
        {
            return _client.GetRole(selector);
        }

        public IGetRoleResponse GetRole(IGetRoleRequest request)
        {
            return _client.GetRole(request);
        }

        public Task<IGetRoleResponse> GetRoleAsync(Func<GetRoleDescriptor, IGetRoleRequest> selector = null)
        {
            return _client.GetRoleAsync(selector);
        }

        public Task<IGetRoleResponse> GetRoleAsync(IGetRoleRequest request)
        {
            return _client.GetRoleAsync(request);
        }

        public IPutRoleResponse PutRole(Name role, Func<PutRoleDescriptor, IPutRoleRequest> selector = null)
        {
            return _client.PutRole(role, selector);
        }

        public IPutRoleResponse PutRole(IPutRoleRequest request)
        {
            return _client.PutRole(request);
        }

        public Task<IPutRoleResponse> PutRoleAsync(Name role, Func<PutRoleDescriptor, IPutRoleRequest> selector = null)
        {
            return _client.PutRoleAsync(role, selector);
        }

        public Task<IPutRoleResponse> PutRoleAsync(IPutRoleRequest request)
        {
            return _client.PutRoleAsync(request);
        }

        public IDeleteUserResponse DeleteUser(Name username, Func<DeleteUserDescriptor, IDeleteUserRequest> selector = null)
        {
            return _client.DeleteUser(username, selector);
        }

        public IDeleteUserResponse DeleteUser(IDeleteUserRequest request)
        {
            return _client.DeleteUser(request);
        }

        public Task<IDeleteUserResponse> DeleteUserAsync(Name username, Func<DeleteUserDescriptor, IDeleteUserRequest> selector = null)
        {
            return _client.DeleteUserAsync(username, selector);
        }

        public Task<IDeleteUserResponse> DeleteUserAsync(IDeleteUserRequest request)
        {
            return _client.DeleteUserAsync(request);
        }

        public IGetUserResponse GetUser(Func<GetUserDescriptor, IGetUserRequest> selector = null)
        {
            return _client.GetUser(selector);
        }

        public IGetUserResponse GetUser(IGetUserRequest request)
        {
            return _client.GetUser(request);
        }

        public Task<IGetUserResponse> GetUserAsync(Func<GetUserDescriptor, IGetUserRequest> selector = null)
        {
            return _client.GetUserAsync(selector);
        }

        public Task<IGetUserResponse> GetUserAsync(IGetUserRequest request)
        {
            return _client.GetUserAsync(request);
        }

        public IPutUserResponse PutUser(Name username, Func<PutUserDescriptor, IPutUserRequest> selector = null)
        {
            return _client.PutUser(username, selector);
        }

        public IPutUserResponse PutUser(IPutUserRequest request)
        {
            return _client.PutUser(request);
        }

        public Task<IPutUserResponse> PutUserAsync(Name username, Func<PutUserDescriptor, IPutUserRequest> selector = null)
        {
            return _client.PutUserAsync(username, selector);
        }

        public Task<IPutUserResponse> PutUserAsync(IPutUserRequest request)
        {
            return _client.PutUserAsync(request);
        }

        public IElasticsearchSerializer Serializer
        {
            get { return _client.Serializer; }
        }

        public Inferrer Infer
        {
            get { return _client.Infer; }
        }

        public IConnectionSettingsValues ConnectionSettings
        {
            get { return _client.ConnectionSettings; }
        }

        public IElasticLowLevelClient LowLevel
        {
            get { return _client.LowLevel; }
        }

        #endregion

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