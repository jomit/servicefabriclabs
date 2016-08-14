using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Net;
using System.Text;

namespace VotingService
{
    internal sealed class VotingService : StatefulService
    {
        public VotingService(StatefulServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new ServiceReplicaListener[] { new ServiceReplicaListener(serviceContext =>
                new HttpCommunicationListener(serviceContext, "ServiceEndpoint", ProcessRequestAsync)) };
        }

        private async Task ProcessRequestAsync(HttpListenerContext context, CancellationToken ct)
        {
            String output = null;
            try
            {
                // Grab the vote item string from a "Vote=" query string parameter
                HttpListenerRequest request = context.Request;
                String voteItem = request.QueryString["Vote"];
                if (voteItem != null)
                {
                    var voteDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("Votes");
                    using (var transaction = this.StateManager.CreateTransaction())
                    {
                        await voteDictionary.AddOrUpdateAsync(transaction, voteItem, 1, (key, value) => ++value);

                        //var q = from kvp in voteEnum
                        //            //orderby kvp.Key // Intentionally commented out
                        //        select $"Item={kvp.Key}, Votes={kvp.Value}";
                        //output = String.Join("<br>", q);

                        var allVotes = await voteDictionary.CreateEnumerableAsync(transaction);
                        var outputList = new List<string>();
                        using (var voteEnumerator = allVotes.GetAsyncEnumerator())
                        {
                            while (await voteEnumerator.MoveNextAsync(ct))
                            {
                                outputList.Add($"Name={voteEnumerator.Current.Key}, Votes={voteEnumerator.Current.Value}");
                            }
                        }
                        output = String.Join("<br/>", outputList);
                        await transaction.CommitAsync();
                    }
                }
            }
            catch (Exception ex) { output = ex.ToString(); }
            // Write response to client:
            using (var response = context.Response)
            {
                if (output != null)
                {
                    Byte[] outBytes = Encoding.UTF8.GetBytes(output);
                    response.OutputStream.Write(outBytes, 0, outBytes.Length);
                }
            }
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        //protected override async Task RunAsync(CancellationToken cancellationToken)
        //{
        //    // TODO: Replace the following sample code with your own logic 
        //    //       or remove this RunAsync override if it's not needed in your service.

        //    var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

        //    while (true)
        //    {
        //        cancellationToken.ThrowIfCancellationRequested();

        //        using (var tx = this.StateManager.CreateTransaction())
        //        {
        //            var result = await myDictionary.TryGetValueAsync(tx, "Counter");

        //            ServiceEventSource.Current.ServiceMessage(this, "Current Counter Value: {0}",
        //                result.HasValue ? result.Value.ToString() : "Value does not exist.");

        //            await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

        //            // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
        //            // discarded, and nothing is saved to the secondary replicas.
        //            await tx.CommitAsync();
        //        }

        //        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        //    }
        //}
    }
}
