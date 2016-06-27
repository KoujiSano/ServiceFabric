using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCPListenerStatelessService
{
    //HttpCommunicationListener　からTcpCommunicationListener に変更
    internal class TcpCommunicationListener : ICommunicationListener
    {
        private StatelessServiceContext context = null;
        private TcpListener listener = null;
        private string publishUri = string.Empty;
        private readonly Func<TcpListener, CancellationToken, Task> processRequest;
        private readonly CancellationTokenSource processRequestsCancellation = new CancellationTokenSource();

        public TcpCommunicationListener(StatelessServiceContext context, Func<TcpListener, CancellationToken, Task> processRequest)
        {
            this.context = context;
            this.processRequest = processRequest;
        }

        public void Abort()
        {
            this.processRequestsCancellation.Cancel();
            this.listener.Stop();
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            this.processRequestsCancellation.Cancel();

            if (this.listener != null)
                this.listener.Stop();

            return Task.FromResult("CloseAsync Completed!!");
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            EndpointResourceDescription endpoint =
                this.context.CodePackageActivationContext.GetEndpoint("WebEndpointForTcpListener");

            string uriPrefix = $"{endpoint.Protocol}://+:{endpoint.Port}/myapp/";

            this.listener = new TcpListener(IPAddress.Any, endpoint.Port);
            this.listener.Start();

            string uriPublished = uriPrefix.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);
            this.publishUri = uriPublished;

            Task openTask = this.processRequest(this.listener, this.processRequestsCancellation.Token);

            return Task.FromResult(this.publishUri);
        }
    }

}
