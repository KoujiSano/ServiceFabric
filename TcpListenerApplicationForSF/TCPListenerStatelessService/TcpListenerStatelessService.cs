using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Net.Sockets;
using System.IO;

namespace TCPListenerStatelessService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class TcpListenerService : StatelessService
    {
        public TcpListenerService(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[] { new ServiceInstanceListener(context => this.CreateTcpCommunicationListener(context)) };
        }

        private ICommunicationListener CreateTcpCommunicationListener(StatelessServiceContext context)
        {
            return new TcpCommunicationListener(context, this.ProcessRequest);
        }

        private async Task ProcessRequest(TcpListener listener, CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                //System.Threading.Thread.Sleep(100);
                TcpClient client = await listener.AcceptTcpClientAsync();
                if (token.IsCancellationRequested)
                {
                    try
                    {
                        client.Close();
                    }
                    finally
                    {
                    }
                    return;
                }

                using (System.Net.Sockets.NetworkStream netStream = client.GetStream())
                {
                    using (StreamReader reader = new StreamReader(netStream))
                    {
                        using (StreamWriter writer = new StreamWriter(netStream))
                        {
                            writer.AutoFlush = true;

                            // Show application
                            string input = string.Empty;
                            while (!input.Equals("9"))
                            {
                                // Show menu
                                writer.WriteLine("Menu:");
                                writer.WriteLine("-----");
                                writer.WriteLine("  1) Display date");
                                writer.WriteLine("  2) Display time");
                                writer.WriteLine("  9) Quit");
                                writer.WriteLine();
                                writer.Write("Your choice: ");

                                input = reader.ReadLine();
                                writer.WriteLine();

                                switch (input)
                                {
                                    case "1":
                                        {
                                            writer.WriteLine(String.Format("Current date: {0}", DateTime.Now.ToShortDateString()));
                                        }
                                        break;
                                    case "2":
                                        {
                                            writer.WriteLine(String.Format("Current time: {0}", DateTime.Now.ToShortTimeString()));
                                        }
                                        break;
                                    case "9":
                                        {
                                            writer.WriteLine("Thank you! ");
                                        }
                                        continue;
                                }
                                writer.WriteLine();
                            }

                            // Done!
                            client.Close();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                ServiceEventSource.Current.ServiceMessage(this, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
