using System;
using System.Collections.Generic;
using EmptyBox.IO.Network;

namespace EmptyBox.Automation.Network
{
    public class ConnectionSocketWorker : IPipelineInput<IConnection>, IPipelineOutput<byte[], IAccessPoint>, IPipelineInput<byte[], IAccessPoint>
    {
        EventHandler<byte[]> IPipelineOutput<byte[], IAccessPoint>.this[IAccessPoint index]
        {
            get
            {
                if (!Events.ContainsKey(index))
                {
                    Events.Add(index, null);
                }
                return Events[index];
            }
            set
            {
                Events[index] = value;
            }
        }
        EventHandler<byte[]> IPipelineInput<byte[], IAccessPoint>.this[IAccessPoint index] => (object sender, byte[] message) => Input(sender, message, index);

        private List<IConnection> Sockets;
        private Dictionary<IAccessPoint, EventHandler<byte[]>> Events;

        public ConnectionSocketWorker()
        {
            Sockets = new List<IConnection>();
            Events = new Dictionary<IAccessPoint, EventHandler<byte[]>>();
        }

        async void IPipelineInput<IConnection>.Input(object sender, IConnection output)
        {
            Sockets.Add(output);
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

        private async void Input(object sender, byte[] message, IAccessPoint accessPoint)
        {
            IConnection connection = Sockets.Find(x => x.RemoteHost == accessPoint);
            if (connection != null)
            {
                await connection.Send(message);
            }
        }

        private void Output_ConnectionInterrupt(IConnection connection)
        {
            Sockets.Remove(connection);
            connection.MessageReceived -= Output_MessageReceived;
            connection.ConnectionInterrupt -= Output_ConnectionInterrupt;
        }

        private void Output_MessageReceived(IConnection connection, byte[] message)
        {
            Events[connection.RemoteHost]?.Invoke(this, message);
        }
    }
}
