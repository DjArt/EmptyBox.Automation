using System;
using System.Collections.Generic;
using System.Text;

namespace EmptyBox.Automation
{
    public class OutputBlock<TOutput, TIndexer> : IPipelineOutput<TOutput, TIndexer>
    {
        OutputDelegate<TOutput, TIndexer> IPipelineOutput<TOutput, TIndexer>.this[TIndexer index]
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
                if (!Events.ContainsKey(index) && value.GetInvocationList().Length > 0)
                {
                    Events.Add(index, null);
                }
                else if (Events.ContainsKey(index))
                {
                    if (value.GetInvocationList().Length == 0)
                    {
                        Events.Remove(index);
                    }
                    else
                    {
                        Events[index] = value;
                    }
                }
            }
        }

        private Dictionary<TIndexer, OutputDelegate<TOutput, TIndexer>> Events;

        public OutputBlock()
        {
            Events = new Dictionary<TIndexer, OutputDelegate<TOutput, TIndexer>>();
        }

        public void Send(TOutput input, TIndexer index)
        {
            (this as IPipelineOutput<TOutput, TIndexer>)[index]?.Invoke(this, input, index);
        }

        public void LinkOutput(TIndexer outputIndex, IPipelineInput<TOutput, TIndexer> pipelineInput, TIndexer inputIndex)
        {
            (this as IPipelineOutput<TOutput, TIndexer>)[outputIndex] += pipelineInput[inputIndex];
        }

        public void UnlinkOutput(TIndexer outputIndex, IPipelineInput<TOutput, TIndexer> pipelineInput, TIndexer inputIndex)
        {
            (this as IPipelineOutput<TOutput, TIndexer>)[outputIndex] -= pipelineInput[inputIndex];
        }
    }
}
