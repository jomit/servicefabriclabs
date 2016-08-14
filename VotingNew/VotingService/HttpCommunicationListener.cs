using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Fabric;
using System.Fabric.Description;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace VotingService
{
    public sealed class HttpCommunicationListener : ICommunicationListener
    {
        private readonly String m_uriPublished;
        private readonly HttpListener m_httpListener = new HttpListener();
        private readonly Func<HttpListenerContext, CancellationToken, Task> m_processRequest;
        private readonly CancellationTokenSource m_processRequestsCancellation = new
        CancellationTokenSource();
        public HttpCommunicationListener(ServiceContext context, string
        endpointName, Func<HttpListenerContext, CancellationToken, Task> processRequest)
        {
            m_processRequest = processRequest;

            // Partition replica's URL is the node's IP, desired port, PartitionId, ReplicaId, Guid
            StatefulServiceContext statefulServiceContext = context as StatefulServiceContext;

            EndpointResourceDescription internalEndpoint = statefulServiceContext.CodePackageActivationContext.GetEndpoint(endpointName);
            var uriPrefix = $"{internalEndpoint.Protocol}://+:{internalEndpoint.Port}/"
            + $"{statefulServiceContext.PartitionId}/{statefulServiceContext.ReplicaId}"
            + $"-{Guid.NewGuid()}/"; // Uniqueness
            m_httpListener.Prefixes.Add(uriPrefix);
            m_uriPublished = uriPrefix.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);
        }

        public void Abort()
        {
            m_processRequestsCancellation.Cancel(); m_httpListener.Abort();
        }
        public Task CloseAsync(CancellationToken cancellationToken)
        {
            m_processRequestsCancellation.Cancel();
            m_httpListener.Close(); return Task.FromResult(true);
        }
        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            m_httpListener.Start();
            var noWarning = ProcessRequestsAsync(m_processRequestsCancellation.Token);
            return Task.FromResult(m_uriPublished);
        }
        private async Task ProcessRequestsAsync(CancellationToken processRequests)
        {
            while (!processRequests.IsCancellationRequested)
            {
                HttpListenerContext request = await m_httpListener.GetContextAsync();
                // The ContinueWith forces rethrowing the exception if the task fails.
                var noWarning = m_processRequest(request,
                m_processRequestsCancellation.Token)
                .ContinueWith(async t => await t /* Rethrow unhandled exception */,
                TaskContinuationOptions.OnlyOnFaulted);
            }
        }
    }
}