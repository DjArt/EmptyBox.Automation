using System;
using System.IO;
using System.Linq;
using System.Text;

namespace EmptyBox.Automation
{
    public class EqualizerBlock<TOutput> : IPipelineInput<TOutput[]>, IPipelineOutput<TOutput>
    {
        public event OutputHandleDelegate<TOutput> OutputHandle;

        public EqualizerBlock()
        {

        }

        public void Input(IPipelineOutput<TOutput[]> sender, ulong taskID, TOutput[] output)
        {
            foreach (TOutput elem in output)
            {
                OutputHandle?.Invoke(this, taskID, elem);
            }
        }

        public void LinkInput(IPipelineOutput<TOutput[]> output)
        {
            output.OutputHandle += Input;
        }

        public void LinkOutput(IPipelineInput<TOutput> input)
        {
            OutputHandle += input.Input;
        }

        public void UnlinkInput(IPipelineOutput<TOutput[]> output)
        {
            output.OutputHandle -= Input;
        }

        public void UnlinkOutput(IPipelineInput<TOutput> input)
        {
            OutputHandle -= input.Input;
        }
    }
}