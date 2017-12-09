using System;
using System.Collections.Generic;
using System.Text;

namespace EmptyBox.Automation
{
    public class OutputBlock<TInput, TOutput, TState> : IPipelineOutput<TOutput>, IPipelineInputInformer<TOutput, TState>
    {
        public Func<ulong, TInput, TOutput> SendAction { get; protected set; }
        public Action<IPipelineInput<TOutput>, ulong?, TOutput, TState> InformInputAction { get; protected set; }
        public event OutputHandleDelegate<TOutput> OutputHandle;

        public OutputBlock(Func<ulong, TInput, TOutput> sendaction, Action<IPipelineInput<TOutput>, ulong?, TOutput, TState> informaction = null)
        {
            SendAction = sendaction;
            InformInputAction = informaction;
        }

        public void Send(ulong taskID, TInput input)
        {
            OutputHandle?.Invoke(this, taskID, SendAction.Invoke(taskID, input));
        }

        public void InformInput(IPipelineInput<TOutput> sender, ulong? taskID, TOutput input, TState state)
        {
            InformInputAction?.Invoke(sender, taskID, input, state);
        }

        public void LinkOutput(IPipelineInput<TOutput> input)
        {
            OutputHandle += input.Input;
        }

        public void UnlinkOutput(IPipelineInput<TOutput> input)
        {
            OutputHandle -= input.Input;
        }
    }
}
