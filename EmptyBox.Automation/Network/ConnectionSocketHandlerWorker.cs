using System;
using System.Collections.Generic;
using System.Text;
using EmptyBox.IO.Network;

namespace EmptyBox.Automation.Network
{
    public sealed class ConnectionSocketHandlerWorker : IPipelineInput<IConnectionSocketHandler>, IPipelineOutput<IConnectionSocket>
    {
        event EventHandler<IConnectionSocket> IPipelineOutput<IConnectionSocket>.Output { add => Output += value; remove => Output -= value; }

        private List<IConnectionSocketHandler> Handlers;
        private event EventHandler<IConnectionSocket> Output;

        public ConnectionSocketHandlerWorker()
        {
            Handlers = new List<IConnectionSocketHandler>();
        }


        private async void Input(object sender, IConnectionSocketHandler output)
        {
            Handlers.Add(output);
            output.ConnectionSocketReceived += Output_ConnectionSocketReceived;
            SocketOperationStatus status = await output.Start();
            switch (status)
            {
                case SocketOperationStatus.Success:
                    break;
                default:
                    Handlers.Remove(output);
                    output.ConnectionSocketReceived -= Output_ConnectionSocketReceived;
                    break;
            }
        }

        void IPipelineInput<IConnectionSocketHandler>.Input(object sender, IConnectionSocketHandler output)
        {
            throw new NotImplementedException();
        }

        private void Output_ConnectionSocketReceived(IConnectionSocketHandler handler, IConnectionSocket socket)
        {
            Output?.Invoke(this, socket);
        }
    }
}