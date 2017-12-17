using System;
using System.Collections.Generic;
using System.Text;
using EmptyBox.IO.Network;

namespace EmptyBox.Automation.Network
{
    public class ConnectionSocketHandlerWorker : IPipelineInput<IConnectionSocketHandler, EmptyType>,
                                                 IPipelineOutput<IConnectionSocket, EmptyType>
    {
        OutputDelegate<IConnectionSocket, EmptyType> IPipelineOutput<IConnectionSocket, EmptyType>.this[EmptyType index]
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

        OutputDelegate<IConnectionSocketHandler, EmptyType> IPipelineInput<IConnectionSocketHandler, EmptyType>.this[EmptyType index]
        {
            get
            {
                return Input;
            }
        }

        private List<IConnectionSocketHandler> Handlers;
        private event OutputDelegate<IConnectionSocket, EmptyType> Output;

        public ConnectionSocketHandlerWorker()
        {
            Handlers = new List<IConnectionSocketHandler>();
        }

        private async void Input(IPipelineOutput<IConnectionSocketHandler, EmptyType> sender, IConnectionSocketHandler output, EmptyType indexer)
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

        private void Output_ConnectionSocketReceived(IConnectionSocketHandler handler, IConnectionSocket socket)
        {
            Output?.Invoke(this, socket, EmptyType.Empty);
        }

        public void LinkInput(EmptyType inputIndex, IPipelineOutput<IConnectionSocketHandler, EmptyType> pipelineOutput, EmptyType outputIndex)
        {
            pipelineOutput[inputIndex] += (this as IPipelineInput<IConnectionSocketHandler, EmptyType>)[inputIndex];
        }

        public void UnlinkInput(EmptyType inputIndex, IPipelineOutput<IConnectionSocketHandler, EmptyType> pipelineOutput, EmptyType outputIndex)
        {
            pipelineOutput[inputIndex] -= (this as IPipelineInput<IConnectionSocketHandler, EmptyType>)[inputIndex];
        }

        public void LinkOutput(EmptyType outputIndex, IPipelineInput<IConnectionSocket, EmptyType> pipelineInput, EmptyType inputIndex)
        {
            (this as IPipelineOutput<IConnectionSocket, EmptyType>)[outputIndex] += pipelineInput[inputIndex];
        }

        public void UnlinkOutput(EmptyType outputIndex, IPipelineInput<IConnectionSocket, EmptyType> pipelineInput, EmptyType inputIndex)
        {
            (this as IPipelineOutput<IConnectionSocket, EmptyType>)[outputIndex] -= pipelineInput[inputIndex];
        }
    }
}