using System;
using System.Collections.Generic;
using System.Text;

namespace EmptyBox.Automation
{
    public class InputBlock<TInput, TIndexer> : IPipelineInput<TInput, TIndexer>
    {
        OutputDelegate<TInput, TIndexer> IPipelineInput<TInput, TIndexer>.this[TIndexer index]
        {
            get
            {
                return Input;
            }
        }

        public Action<IPipelineOutput<TInput, TIndexer>, TInput, TIndexer> EventHandler { get; set; }

        public InputBlock(Action<IPipelineOutput<TInput, TIndexer>, TInput, TIndexer> action)
        {
            EventHandler = action;
        }

        private void Input(IPipelineOutput<TInput, TIndexer> pipeline, TInput output, TIndexer index)
        {
            EventHandler?.Invoke(pipeline, output, index);
        }

        public void LinkInput(TIndexer inputIndex, IPipelineOutput<TInput, TIndexer> pipelineOutput, TIndexer outputIndex)
        {
            pipelineOutput[outputIndex] += (this as IPipelineInput<TInput, TIndexer>)[inputIndex];
        }

        public void UnlinkInput(TIndexer inputIndex, IPipelineOutput<TInput, TIndexer> pipelineOutput, TIndexer outputIndex)
        {
            pipelineOutput[outputIndex] -= (this as IPipelineInput<TInput, TIndexer>)[inputIndex];
        }
    }
}
