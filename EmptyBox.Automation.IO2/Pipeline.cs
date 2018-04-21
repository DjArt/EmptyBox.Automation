using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EmptyBox.Automation
{
    public static class Pipeline
    {
        #region IPipeline<,,>
        public static void LinkOutput<TElement, TOutputIndex0, TOutputIndex1, TInputIndex0, TInputIndex1>(this IPipelineOutput<TElement, TOutputIndex0, TOutputIndex1> pipelineOutput, TOutputIndex0 outputIndex0, TOutputIndex1 outputIndex1, IPipelineInput<TElement, TInputIndex0, TInputIndex1> pipelineInput, TInputIndex0 inputIndex0, TInputIndex1 inputIndex1)
        {
            pipelineOutput[outputIndex0, outputIndex1] += pipelineInput[inputIndex0, inputIndex1];
        }
        public static void UnlinkOutput<TElement, TOutputIndex0, TOutputIndex1, TInputIndex0, TInputIndex1>(this IPipelineOutput<TElement, TOutputIndex0, TOutputIndex1> pipelineOutput, TOutputIndex0 outputIndex0, TOutputIndex1 outputIndex1, IPipelineInput<TElement, TInputIndex0, TInputIndex1> pipelineInput, TInputIndex0 inputIndex0, TInputIndex1 inputIndex1)
        {
            pipelineOutput[outputIndex0, outputIndex1] -= pipelineInput[inputIndex0, inputIndex1];
        }
        public static void LinkInput<TElement, TInputIndex0, TInputIndex1, TOutputIndex0, TOutputIndex1>(this IPipelineInput<TElement, TInputIndex0, TInputIndex1> pipelineInput, TInputIndex0 inputIndex0, TInputIndex1 inputIndex1, IPipelineOutput<TElement, TOutputIndex0, TOutputIndex1> pipelineOutput, TOutputIndex0 outputIndex0, TOutputIndex1 outputIndex1)
        {
            pipelineOutput[outputIndex0, outputIndex1] += pipelineInput[inputIndex0, inputIndex1];
        }
        public static void UnlinkInput<TElement, TInputIndex0, TInputIndex1, TOutputIndex0, TOutputIndex1>(this IPipelineInput<TElement, TInputIndex0, TInputIndex1> pipelineInput, TInputIndex0 inputIndex0, TInputIndex1 inputIndex1, IPipelineOutput<TElement, TOutputIndex0, TOutputIndex1> pipelineOutput, TOutputIndex0 outputIndex0, TOutputIndex1 outputIndex1)
        {
            pipelineOutput[outputIndex0, outputIndex1] -= pipelineInput[inputIndex0, inputIndex1];
        }
        #endregion

        #region IPipeline<,>
        public static void LinkOutput<TElement, TOutputIndex, TInputIndex>(this IPipelineOutput<TElement, TOutputIndex> pipelineOutput, TOutputIndex outputIndex, IPipelineInput<TElement, TInputIndex> pipelineInput, TInputIndex inputIndex)
        {
            pipelineOutput[outputIndex] += pipelineInput[inputIndex];
        }
        public static void UnlinkOutput<TElement, TOutputIndex, TInputIndex>(this IPipelineOutput<TElement, TOutputIndex> pipelineOutput, TOutputIndex outputIndex, IPipelineInput<TElement, TInputIndex> pipelineInput, TInputIndex inputIndex)
        {
            pipelineOutput[outputIndex] -= pipelineInput[inputIndex];
        }
        public static void LinkInput<TElement, TInputIndex, TOutputIndex>(this IPipelineInput<TElement, TInputIndex> pipelineInput, TInputIndex inputIndex, IPipelineOutput<TElement, TOutputIndex> pipelineOutput, TOutputIndex outputIndex)
        {
            pipelineOutput[outputIndex] += pipelineInput[inputIndex];
        }
        public static void UnlinkInput<TElement, TInputIndex, TOutputIndex>(this IPipelineInput<TElement, TInputIndex> pipelineInput, TInputIndex inputIndex, IPipelineOutput<TElement, TOutputIndex> pipelineOutput, TOutputIndex outputIndex)
        {
            pipelineOutput[outputIndex] -= pipelineInput[inputIndex];
        }
        public static void LinkAllOutput<TElement, TIndex>(this IPipelineOutput<TElement, TIndex> pipelineOutput, IPipelineInput<TElement, TIndex> pipelineInput)
        {
            pipelineOutput.Output += pipelineInput.Input;
        }
        public static void UnlinkAllOutput<TElement, TIndex>(this IPipelineOutput<TElement, TIndex> pipelineOutput, IPipelineInput<TElement, TIndex> pipelineInput)
        {
            pipelineOutput.Output -= pipelineInput.Input;
        }
        public static void LinkAllInput<TElement, TIndex>(this IPipelineInput<TElement, TIndex> pipelineInput, IPipelineOutput<TElement, TIndex> pipelineOutput)
        {
            pipelineOutput.Output += pipelineInput.Input;
        }
        public static void UnlinkAllInput<TElement, TIndex>(this IPipelineInput<TElement, TIndex> pipelineInput, IPipelineOutput<TElement, TIndex> pipelineOutput)
        {
            pipelineOutput.Output -= pipelineInput.Input;
        }
        #endregion

        #region IPipeline<>
        public static void LinkOutput<TElement>(this IPipelineOutput<TElement> pipelineOutput, IPipelineInput<TElement> pipelineInput)
        {
            pipelineOutput.Output += pipelineInput.Input;
        }
        public static void UnlinkOutput<TElement>(this IPipelineOutput<TElement> pipelineOutput, IPipelineInput<TElement> pipelineInput)
        {
            pipelineOutput.Output -= pipelineInput.Input;
        }
        public static void LinkInput<TElement>(this IPipelineInput<TElement> pipelineInput, IPipelineOutput<TElement> pipelineOutput)
        {
            pipelineOutput.Output += pipelineInput.Input;
        }
        public static void UnlinkInput<TElement>(this IPipelineInput<TElement> pipelineInput, IPipelineOutput<TElement> pipelineOutput)
        {
            pipelineOutput.Output -= pipelineInput.Input;
        }
        #endregion

        #region IPipeline<,,> & IPipeline<,>
        public static void LinkOutput<TElement, TOutputIndex0, TOutputIndex1, TInputIndex>(this IPipelineOutput<TElement, TOutputIndex0, TOutputIndex1> pipelineOutput, TOutputIndex0 outputIndex0, TOutputIndex1 outputIndex1, IPipelineInput<TElement, TInputIndex> pipelineInput, TInputIndex inputIndex)
        {
            pipelineOutput[outputIndex0, outputIndex1] += pipelineInput[inputIndex];
        }
        public static void UnlinkOutput<TElement, TOutputIndex0, TOutputIndex1, TInputIndex>(this IPipelineOutput<TElement, TOutputIndex0, TOutputIndex1> pipelineOutput, TOutputIndex0 outputIndex0, TOutputIndex1 outputIndex1, IPipelineInput<TElement, TInputIndex> pipelineInput, TInputIndex inputIndex)
        {
            pipelineOutput[outputIndex0, outputIndex1] -= pipelineInput[inputIndex];
        }
        public static void LinkOutput<TElement, TOutputIndex, TInputIndex0, TInputIndex1>(this IPipelineOutput<TElement, TOutputIndex> pipelineOutput, TOutputIndex outputIndex, IPipelineInput<TElement, TInputIndex0, TInputIndex1> pipelineInput, TInputIndex0 inputIndex0, TInputIndex1 inputIndex1)
        {
            pipelineOutput[outputIndex] += pipelineInput[inputIndex0, inputIndex1];
        }
        public static void UnlinkOutput<TElement, TOutputIndex, TInputIndex0, TInputIndex1>(this IPipelineOutput<TElement, TOutputIndex> pipelineOutput, TOutputIndex outputIndex, IPipelineInput<TElement, TInputIndex0, TInputIndex1> pipelineInput, TInputIndex0 inputIndex0, TInputIndex1 inputIndex1)
        {
            pipelineOutput[outputIndex] -= pipelineInput[inputIndex0, inputIndex1];
        }
        public static void LinkInput<TElement, TInputIndex0, TInputIndex1, TOutputIndex>(this IPipelineInput<TElement, TInputIndex0, TInputIndex1> pipelineInput, TInputIndex0 inputIndex0, TInputIndex1 inputIndex1, IPipelineOutput<TElement, TOutputIndex> pipelineOutput, TOutputIndex outputIndex)
        {
            pipelineOutput[outputIndex] += pipelineInput[inputIndex0, inputIndex1];
        }
        public static void UnlinkInput<TElement, TInputIndex0, TInputIndex1, TOutputIndex>(this IPipelineInput<TElement, TInputIndex0, TInputIndex1> pipelineInput, TInputIndex0 inputIndex0, TInputIndex1 inputIndex1, IPipelineOutput<TElement, TOutputIndex> pipelineOutput, TOutputIndex outputIndex)
        {
            pipelineOutput[outputIndex] -= pipelineInput[inputIndex0, inputIndex1];
        }
        public static void LinkInput<TElement, TInputIndex, TOutputIndex0, TOutputIndex1>(this IPipelineInput<TElement, TInputIndex> pipelineInput, TInputIndex inputIndex, IPipelineOutput<TElement, TOutputIndex0, TOutputIndex1> pipelineOutput, TOutputIndex0 outputIndex0, TOutputIndex1 outputIndex1)
        {
            pipelineOutput[outputIndex0, outputIndex1] += pipelineInput[inputIndex];
        }
        public static void UnlinkInput<TElement, TInputIndex, TOutputIndex0, TOutputIndex1>(this IPipelineInput<TElement, TInputIndex> pipelineInput, TInputIndex inputIndex, IPipelineOutput<TElement, TOutputIndex0, TOutputIndex1> pipelineOutput, TOutputIndex0 outputIndex0, TOutputIndex1 outputIndex1)
        {
            pipelineOutput[outputIndex0, outputIndex1] -= pipelineInput[inputIndex];
        }
        #endregion

        #region IPipeline<,,> & IPipeline<>
        public static void LinkOutput<TElement, TOutputIndex0, TOutputIndex1>(this IPipelineOutput<TElement, TOutputIndex0, TOutputIndex1> pipelineOutput, TOutputIndex0 outputIndex0, TOutputIndex1 outputIndex1, IPipelineInput<TElement> pipelineInput)
        {
            pipelineOutput[outputIndex0, outputIndex1] += pipelineInput.Input;
        }
        public static void UnlinkOutput<TElement, TOutputIndex0, TOutputIndex1>(this IPipelineOutput<TElement, TOutputIndex0, TOutputIndex1> pipelineOutput, TOutputIndex0 outputIndex0, TOutputIndex1 outputIndex1, IPipelineInput<TElement> pipelineInput)
        {
            pipelineOutput[outputIndex0, outputIndex1] -= pipelineInput.Input;
        }
        public static void LinkOutput<TElement, TInputIndex0, TInputIndex1>(this IPipelineOutput<TElement> pipelineOutput, IPipelineInput<TElement, TInputIndex0, TInputIndex1> pipelineInput, TInputIndex0 inputIndex0, TInputIndex1 inputIndex1)
        {
            pipelineOutput.Output += pipelineInput[inputIndex0, inputIndex1];
        }
        public static void UnlinkOutput<TElement, TInputIndex0, TInputIndex1>(this IPipelineOutput<TElement> pipelineOutput, IPipelineInput<TElement, TInputIndex0, TInputIndex1> pipelineInput, TInputIndex0 inputIndex0, TInputIndex1 inputIndex1)
        {
            pipelineOutput.Output -= pipelineInput[inputIndex0, inputIndex1];
        }
        public static void LinkInput<TElement, TInputIndex0, TInputIndex1>(this IPipelineInput<TElement, TInputIndex0, TInputIndex1> pipelineInput, TInputIndex0 inputIndex0, TInputIndex1 inputIndex1, IPipelineOutput<TElement> pipelineOutput)
        {
            pipelineOutput.Output += pipelineInput[inputIndex0, inputIndex1];
        }
        public static void UnlinkInput<TElement, TInputIndex0, TInputIndex1>(this IPipelineInput<TElement, TInputIndex0, TInputIndex1> pipelineInput, TInputIndex0 inputIndex0, TInputIndex1 inputIndex1, IPipelineOutput<TElement> pipelineOutput)
        {
            pipelineOutput.Output -= pipelineInput[inputIndex0, inputIndex1];
        }
        public static void LinkInput<TElement, TOutputIndex0, TOutputIndex1>(this IPipelineInput<TElement> pipelineInput, IPipelineOutput<TElement, TOutputIndex0, TOutputIndex1> pipelineOutput, TOutputIndex0 outputIndex0, TOutputIndex1 outputIndex1)
        {
            pipelineOutput[outputIndex0, outputIndex1] += pipelineInput.Input;
        }
        public static void UnlinkInput<TElement, TOutputIndex0, TOutputIndex1>(this IPipelineInput<TElement> pipelineInput, IPipelineOutput<TElement, TOutputIndex0, TOutputIndex1> pipelineOutput, TOutputIndex0 outputIndex0, TOutputIndex1 outputIndex1)
        {
            pipelineOutput[outputIndex0, outputIndex1] -= pipelineInput.Input;
        }
        #endregion

        #region IPipeline<,> & IPipeline<>
        public static void LinkOutput<TElement, TOutputIndex>(this IPipelineOutput<TElement, TOutputIndex> pipelineOutput, TOutputIndex outputIndex, IPipelineInput<TElement> pipelineInput)
        {
            pipelineOutput[outputIndex] += pipelineInput.Input;
        }
        public static void UnlinkOutput<TElement, TOutputIndex>(this IPipelineOutput<TElement, TOutputIndex> pipelineOutput, TOutputIndex outputIndex, IPipelineInput<TElement> pipelineInput)
        {
            pipelineOutput[outputIndex] -= pipelineInput.Input;
        }
        public static void LinkOutput<TElement, TInputIndex>(this IPipelineOutput<TElement> pipelineOutput, IPipelineInput<TElement, TInputIndex> pipelineInput, TInputIndex inputIndex)
        {
            pipelineOutput.Output += pipelineInput[inputIndex];
        }
        public static void UnlinkOutput<TElement, TInputIndex>(this IPipelineOutput<TElement> pipelineOutput, IPipelineInput<TElement, TInputIndex> pipelineInput, TInputIndex inputIndex)
        {
            pipelineOutput.Output -= pipelineInput[inputIndex];
        }
        public static void LinkInput<TElement, TInputIndex>(this IPipelineInput<TElement, TInputIndex> pipelineInput, TInputIndex inputIndex, IPipelineOutput<TElement> pipelineOutput)
        {
            pipelineOutput.Output += pipelineInput[inputIndex];
        }
        public static void UnlinkInput<TElement, TInputIndex>(this IPipelineInput<TElement, TInputIndex> pipelineInput, TInputIndex inputIndex, IPipelineOutput<TElement> pipelineOutput)
        {
            pipelineOutput.Output -= pipelineInput[inputIndex];
        }
        public static void LinkInput<TElement, TOutputIndex>(this IPipelineInput<TElement> pipelineInput, IPipelineOutput<TElement, TOutputIndex> pipelineOutput, TOutputIndex outputIndex)
        {
            pipelineOutput[outputIndex] += pipelineInput.Input;
        }
        public static void UnlinkInput<TElement, TOutputIndex>(this IPipelineInput<TElement> pipelineInput, IPipelineOutput<TElement, TOutputIndex> pipelineOutput, TOutputIndex outputIndex)
        {
            pipelineOutput[outputIndex] -= pipelineInput.Input;
        }
        #endregion

        public static void Warp<TElement, TOutputIndex, TInputIndex>(this IPipelineOutput<TElement, TOutputIndex> pipelineOutput, TOutputIndex outputIndex, IPipelineInput<TElement, TInputIndex> pipelineInput, TInputIndex inputIndex)
        {
            pipelineOutput[outputIndex] += pipelineInput[inputIndex];
        }
    }
}