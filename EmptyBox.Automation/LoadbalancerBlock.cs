using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmptyBox.Automation
{
    public sealed class LoadbalancerBlock<TInput> : IPipelineInput<TInput, EmptyType>, IPipelineOutput<TInput, uint>, IPipelineInput<RatioUpdate, uint>
    {
        internal sealed class Record
        {
            public uint Ratio;
            public uint CurrentRatio;
            public OutputDelegate<TInput, uint> Event;
        }

        OutputDelegate<TInput, uint> IPipelineOutput<TInput, uint>.this[uint index]
        {
            get
            {
                if (!Events.ContainsKey(index))
                {
                    Events.Add(index, new Record());
                }
                return Events[index].Event;
            }
            set
            {
                if (!Events.ContainsKey(index) && value.GetInvocationList().Length > 0)
                {
                    Events.Add(index, new Record() { Event = value });
                }
                else if (Events.ContainsKey(index))
                {
                    if (Events[index].Ratio == 0 && value.GetInvocationList().Length == 0)
                    {
                        Events.Remove(index);
                    }
                    else
                    {
                        Events[index].Event = value;
                    }
                }
            }
        }

        OutputDelegate<TInput, EmptyType> IPipelineInput<TInput, EmptyType>.this[EmptyType index]
        {
            get
            {
                return Input;
            }
        }

        OutputDelegate<RatioUpdate, uint> IPipelineInput<RatioUpdate, uint>.this[uint index]
        {
            get
            {
                return RatioUpdateInput;
            }
        }

        private Dictionary<uint, Record> Events;
        private Random Random;

        public LoadbalancerBlock()
        {
            Events = new Dictionary<uint, Record>
            {
                { 0, new Record() { Ratio = 1, CurrentRatio = 1 } }
            };
            Random = new Random();
        }

        private void RatioUpdateInput(IPipelineOutput<RatioUpdate, uint> pipeline, RatioUpdate output, uint index)
        {
            for (int i0 = 0; i0 < output.Ratio.Length; i0++)
            {
                if (Events.ContainsKey((uint)i0))
                {
                    Events[(uint)i0].Ratio = output.Ratio[i0];
                    Events[(uint)i0].CurrentRatio = Events[(uint)i0].Ratio;
                }
                else
                {
                    Events.Add((uint)i0, new Record() { Ratio = output.Ratio[i0], CurrentRatio = output.Ratio[i0] });
                }
            }
        }

        private void Input(IPipelineOutput<TInput, EmptyType> pipeline, TInput output, EmptyType index)
        {
            List<uint> keys = Events.Keys.Where(x => Events[x].CurrentRatio > 0).ToList();
            uint key = keys[Random.Next(keys.Count())];
            Events[key].CurrentRatio--;
            if (keys.Count == 1 && Events[key].CurrentRatio == 0)
            {
                foreach (Record r in Events.Values)
                {
                    r.CurrentRatio = r.Ratio;
                }
            }
            Events[key].Event?.Invoke(this, output, key);
        }

        public void LinkInput(EmptyType inputIndex, IPipelineOutput<TInput, EmptyType> pipelineOutput, EmptyType outputIndex)
        {
            pipelineOutput[outputIndex] += (this as IPipelineInput<TInput, EmptyType>)[inputIndex];
        }

        public void LinkOutput(uint outputIndex, IPipelineInput<TInput, uint> pipelineInput, uint inputIndex)
        {
            (this as IPipelineOutput<TInput, uint>)[outputIndex] += pipelineInput[inputIndex];
        }

        public void UnlinkInput(EmptyType inputIndex, IPipelineOutput<TInput, EmptyType> pipelineOutput, EmptyType outputIndex)
        {
            pipelineOutput[outputIndex] -= (this as IPipelineInput<TInput, EmptyType>)[inputIndex];
        }

        public void UnlinkOutput(uint outputIndex, IPipelineInput<TInput, uint> pipelineInput, uint inputIndex)
        {
            (this as IPipelineOutput<TInput, uint>)[outputIndex] -= pipelineInput[inputIndex];
        }

        public void LinkInput(uint inputIndex, IPipelineOutput<RatioUpdate, uint> pipelineOutput, uint outputIndex)
        {
            pipelineOutput[outputIndex] += (this as IPipelineInput<RatioUpdate, uint>)[inputIndex];
        }

        public void UnlinkInput(uint inputIndex, IPipelineOutput<RatioUpdate, uint> pipelineOutput, uint outputIndex)
        {
            pipelineOutput[outputIndex] -= (this as IPipelineInput<RatioUpdate, uint>)[inputIndex];
        }
    }
}
