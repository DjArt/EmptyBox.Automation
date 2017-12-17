using System;
using System.Collections.Generic;
using System.Text;
using EmptyBox.IO.Network;

namespace EmptyBox.Automation.Network
{
    public class ConnectionSocketWorker : IPipelineInput<IConnectionSocket, EmptyType>,
                                          IPipelineOutput<(IAccessPoint, byte[]), EmptyType>,
                                          IPipelineInput<(IAccessPoint, byte[]), EmptyType>
    {
        OutputDelegate<IConnectionSocket, EmptyType> IPipelineInput<IConnectionSocket, EmptyType>.this[EmptyType index]
        {
            get
            {
                return Input;
            }
        }

        OutputDelegate<(IAccessPoint, byte[]), EmptyType> IPipelineInput<(IAccessPoint, byte[]), EmptyType>.this[EmptyType index]
        {
            get
            {
                return Input;
            }
        }

        OutputDelegate<(IAccessPoint, byte[]), EmptyType> IPipelineOutput<(IAccessPoint, byte[]), EmptyType>.this[EmptyType index]
        {
            get
            {
                return Output;
            }
            set
            {
                Output = value;
            }
        }

        private List<IConnectionSocket> Sockets;
        private event OutputDelegate<(IAccessPoint, byte[]), EmptyType> Output;

        public ConnectionSocketWorker()
        {
            Sockets = new List<IConnectionSocket>();
        }

        private async void Input(IPipelineOutput<IConnectionSocket, EmptyType> sender, IConnectionSocket output, EmptyType index)
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

        private void Input(IPipelineOutput<(IAccessPoint, byte[]), EmptyType> sender, (IAccessPoint, byte[]) output, EmptyType index)
        {
            IConnectionSocket connection = Sockets.Find(x => x == output.Item1);
            if (connection != null)
            {
                connection.Send(output.Item2);
            }
        }

        private void Output_ConnectionInterrupt(IConnectionSocket connection)
        {
            Sockets.Remove(connection);
            connection.MessageReceived -= Output_MessageReceived;
            connection.ConnectionInterrupt -= Output_ConnectionInterrupt;
        }

        private void Output_MessageReceived(IConnectionSocket connection, byte[] message)
        {
            Output?.Invoke(this, (connection.RemoteHost, message), EmptyType.Empty);
        }

        public void LinkInput(EmptyType inputIndex, IPipelineOutput<IConnectionSocket, EmptyType> pipelineOutput, EmptyType outputIndex)
        {
            pipelineOutput[outputIndex] += (this as IPipelineInput<IConnectionSocket, EmptyType>)[inputIndex];
        }

        public void UnlinkInput(EmptyType inputIndex, IPipelineOutput<IConnectionSocket, EmptyType> pipelineOutput, EmptyType outputIndex)
        {
            pipelineOutput[outputIndex] += (this as IPipelineInput<IConnectionSocket, EmptyType>)[inputIndex];
        }

        public void LinkOutput(EmptyType outputIndex, IPipelineInput<(IAccessPoint, byte[]), EmptyType> pipelineInput, EmptyType inputIndex)
        {
            (this as IPipelineOutput<(IAccessPoint, byte[]), EmptyType>)[inputIndex] += pipelineInput[outputIndex];
        }

        public void UnlinkOutput(EmptyType outputIndex, IPipelineInput<(IAccessPoint, byte[]), EmptyType> pipelineInput, EmptyType inputIndex)
        {
            (this as IPipelineOutput<(IAccessPoint, byte[]), EmptyType>)[inputIndex] -= pipelineInput[outputIndex];
        }

        public void LinkInput(EmptyType inputIndex, IPipelineOutput<(IAccessPoint, byte[]), EmptyType> pipelineOutput, EmptyType outputIndex)
        {
            pipelineOutput[outputIndex] += (this as IPipelineInput<(IAccessPoint, byte[]), EmptyType>)[inputIndex];
        }

        public void UnlinkInput(EmptyType inputIndex, IPipelineOutput<(IAccessPoint, byte[]), EmptyType> pipelineOutput, EmptyType outputIndex)
        {
            pipelineOutput[outputIndex] -= (this as IPipelineInput<(IAccessPoint, byte[]), EmptyType>)[inputIndex];
        }
    }
}
