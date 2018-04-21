//using System;
//using System.IO;
//using System.Linq;
//using System.Text;

//namespace EmptyBox.Automation
//{
//    public class EqualizerBlock<TOutput> : IPipelineInput<TOutput[]>, IPipelineOutput<TOutput>
//    {
//        public event OutputDelegate<TOutput> Output;

//        public EqualizerBlock()
//        {

//        }

//        public void Input(IPipelineOutput<TOutput[]> sender, ulong taskID, TOutput[] output)
//        {
//            foreach (TOutput elem in output)
//            {
//                Output?.Invoke(this, taskID, elem);
//            }
//        }

//        public void LinkInput(IPipelineOutput<TOutput[]> outputPipe)
//        {
//            outputPipe.Output += Input;
//        }

//        public void LinkOutput(IPipelineInput<TOutput> inputPipe)
//        {
//            Output += inputPipe.Input;
//        }

//        public void UnlinkInput(IPipelineOutput<TOutput[]> outputPipe)
//        {
//            outputPipe.Output -= Input;
//        }

//        public void UnlinkOutput(IPipelineInput<TOutput> inputPipe)
//        {
//            Output -= inputPipe.Input;
//        }

//        public void LinkInput<TIndexer>(IPipelineMultiOutput<TOutput[], TIndexer> outputPipe, TIndexer inputIndex)
//        {
//            outputPipe[inputIndex] += Input;
//        }

//        public void UnlinkInput<TIndexer>(IPipelineMultiOutput<TOutput[], TIndexer> outputPipe, TIndexer inputIndex)
//        {
//            outputPipe[inputIndex] -= Input;
//        }

//        public void LinkOutput<TIndexer>(IPipelineMultiInput<TOutput, TIndexer> inputPipe, TIndexer outputIndex)
//        {
//            Output += inputPipe[outputIndex];
//        }

//        public void UnlinkOutput<TIndexer>(IPipelineMultiInput<TOutput, TIndexer> inputPipe, TIndexer outputIndex)
//        {
//            Output -= inputPipe[outputIndex];
//        }
//    }
//}