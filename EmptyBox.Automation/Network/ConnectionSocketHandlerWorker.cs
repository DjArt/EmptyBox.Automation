using System;
using System.Collections.Generic;
using System.Text;
using EmptyBox.IO.Network;

namespace EmptyBox.Automation.Network
{
    public sealed class ConnectionSocketHandlerWorker : IPipelineInput<IConnectionListener>, IPipelineOutput<IConnection>
    {
        event EventHandler<IConnection> IPipelineOutput<IConnection>.Output { add => Output += value; remove => Output -= value; }

        private List<IConnectionListener> Handlers;
        private event EventHandler<IConnection> Output;

        public ConnectionSocketHandlerWorker()
        {
            Handlers = new List<IConnectionListener>();
        }


        private async void Input(object sender, IConnectionListener output)
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

        void IPipelineInput<IConnectionListener>.Input(object sender, IConnectionListener output)
        {
            throw new NotImplementedException();
        }

        private void Output_ConnectionSocketReceived(IConnectionListener handler, IConnection socket)
        {
            Output?.Invoke(this, socket);
        }
    }
}