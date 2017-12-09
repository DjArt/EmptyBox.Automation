using System;
using System.Collections.Generic;
using System.Text;
using EmptyBox.IO.Network;

namespace EmptyBox.Automation.Network
{
    public class ConnectionSocketWorker : IPipelineInput<IConnectionSocket>, IPipelineOutput<(IAccessPoint, byte[])>, IPipelineInput<(IAccessPoint, byte[])>
    {
        private List<IConnectionSocket> _Sockets;
        public event OutputHandleDelegate<(IAccessPoint, byte[])> OutputHandle;

        public ConnectionSocketWorker()
        {
            _Sockets = new List<IConnectionSocket>();
        }

        public async void Input(IPipelineOutput<IConnectionSocket> sender, ulong taskID, IConnectionSocket output)
        {
            _Sockets.Add(output);
            output.MessageReceived += Output_MessageReceived;
            output.ConnectionInterrupt += Output_ConnectionInterrupt;
            SocketOperationStatus status = await output.Open();
            switch (status)
            {
                case SocketOperationStatus.Success:
                    break;
                default:
                    Output_ConnectionInterrupt(output);
                    break;
            }
        }

        private void Output_ConnectionInterrupt(IConnectionSocket connection)
        {
            _Sockets.Remove(connection);
            connection.MessageReceived -= Output_MessageReceived;
            connection.ConnectionInterrupt -= Output_ConnectionInterrupt;
        }

        private void Output_MessageReceived(IConnectionSocket connection, byte[] message)
        {
            OutputHandle?.Invoke(this, 0, (connection.RemoteHost, message));
        }

        public void Input(IPipelineOutput<(IAccessPoint, byte[])> sender, ulong taskID, (IAccessPoint, byte[]) output)
        {
            IConnectionSocket connection = _Sockets.Find(x => x == output.Item1);
            if (connection != null)
            {
                connection.Send(output.Item2);
            }
        }
    }
}
