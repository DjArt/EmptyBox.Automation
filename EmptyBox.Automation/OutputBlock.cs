using System;
using System.Collections.Generic;
using System.Text;

namespace EmptyBox.Automation
{
    public class OutputBlock<TOutput, TIndexer> : IPipelineOutput<TOutput, TIndexer>
    {
        EventHandler<TOutput> IPipelineOutput<TOutput, TIndexer>.this[TIndexer index]
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

        private Dictionary<TIndexer, EventHandler<TOutput>> Events;

        public OutputBlock()
        {
            Events = new Dictionary<TIndexer, EventHandler<TOutput>>();
        }

        public void Send(TOutput input, TIndexer index)
        {
            (this as IPipelineOutput<TOutput, TIndexer>)[index]?.Invoke(this, input);
        }
    }
}
