using System;
using System.Collections.Generic;
using System.Text;

namespace EmptyBox.Automation
{
    public class BufferBlock<TInput> : IPipelineInput<TInput>, IPipelineInputInformer<TInput[], BufferBlockStates>, IPipelineOutput<TInput[]>, IPipelineOutputInformer<TInput, BufferBlockStates>
    {
        protected Dictionary<ulong, List<TInput>> _Values;
        public uint ReleasePool { get; set; }
        public event OutputHandleDelegate<TInput[]> OutputHandle;
        public event InformerOutputHandleDelegate<TInput, BufferBlockStates> InformerOutputHandle;

        public BufferBlock(uint pool)
        {
            _Values = new Dictionary<ulong, List<TInput>>();
            ReleasePool = pool;
        }

        public void InformInput(IPipelineInput<TInput[]> sender, ulong? taskID, TInput[] input, BufferBlockStates state)
        {
            if (taskID != null)
            {
                switch (state)
                {
                    case BufferBlockStates.Release:
                        OutputHandle?.Invoke(this, taskID.Value, _Values[taskID.Value].ToArray());
                        _Values[taskID.Value].Clear();
                        break;
                    case BufferBlockStates.Clear:
                        _Values[taskID.Value].Clear();
                        break;
                }
            }
            else
            {
                switch (state)
                {
                    case BufferBlockStates.Release:
                        foreach (ulong id in _Values.Keys)
                        {
                            OutputHandle?.Invoke(this, id, _Values[id].ToArray());
                        }
                        _Values.Clear();
                        break;
                    case BufferBlockStates.Clear:
                        _Values.Clear();
                        break;
                }
            }
        }

        public void Input(IPipelineOutput<TInput> sender, ulong taskID, TInput output)
        {
            if (!_Values.ContainsKey(taskID))
            {
                _Values[taskID] = new List<TInput>();
            }
            _Values[taskID].Add(output);
            if (_Values[taskID].Count >= ReleasePool)
            {
                OutputHandle?.Invoke(this, taskID, _Values[taskID].ToArray());
                _Values[taskID].Clear();
            }
        }

        public void LinkInput(IPipelineOutput<TInput> output)
        {
            output.OutputHandle += Input;
        }

        public void UnlinkInput(IPipelineOutput<TInput> output)
        {
            output.OutputHandle -= Input;
        }

        public void LinkOutput(IPipelineInput<TInput[]> input)
        {
            OutputHandle += input.Input;
        }

        public void UnlinkOutput(IPipelineInput<TInput[]> input)
        {
            OutputHandle -= input.Input;
        }
    }
}
