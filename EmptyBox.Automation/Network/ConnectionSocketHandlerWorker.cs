using System;
using System.Collections.Generic;
using System.Text;
using EmptyBox.IO.Network;

namespace EmptyBox.Automation.Network
{
    public class ConnectionSocketHandlerWorker : IPipelineInput<IConnectionSocketHandler>, IPipelineOutput<IConnectionSocket>
    {
        private List<IConnectionSocketHandler> _Handlers;

        public event OutputHandleDelegate<IConnectionSocket> OutputHandle;

        public ConnectionSocketHandlerWorker()
        {
            _Handlers = new List<IConnectionSocketHandler>();
        }

        public async void Input(IPipelineOutput<IConnectionSocketHandler> sender, ulong taskID, IConnectionSocketHandler output)
        {
            _Handlers.Add(output);
            output.ConnectionSocketReceived += Output_ConnectionSocketReceived;
            SocketOperationStatus status = await output.Start();
            switch (status)
            {
                case SocketOperationStatus.Success:
                    break;
                default:
                    _Handlers.Remove(output);
                    output.ConnectionSocketReceived -= Output_ConnectionSocketReceived;
                    break;
            }
        }

        private void Output_ConnectionSocketReceived(IConnectionSocketHandler handler, IConnectionSocket socket)
        {
            OutputHandle?.Invoke(this, 0, socket);
        }
    }
}