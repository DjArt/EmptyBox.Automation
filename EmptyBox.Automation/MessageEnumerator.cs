using System;
using System.Collections.Generic;
using System.Text;
using EmptyBox.Automation;

namespace EmptyBox.IO.Automation
{
    //TODO
    //Заменить в интерфейсе IPipelineOutputInformer<byte[], bool> тип bool на перечисляемое значение, соответсвующее состоянию данного конвеера
    public class MessageEnumerator : IPipelineInput<(int Number, byte[] Message)>, IPipelineOutput<byte[]>, IPipelineInput<byte[]>, IPipelineOutput<(int Number, byte[] Message)>, IPipelineOutputInformer<byte[], bool>
    {
        public event InformerOutputHandleDelegate<byte[], bool> InformerOutputHandle;
        protected event OutputHandleDelegate<byte[]> _OH0;
        protected event OutputHandleDelegate<(int, byte[])> _OH1;
        public static byte AdditionalSize => sizeof(int);

        event OutputHandleDelegate<byte[]> IPipelineOutput<byte[]>.OutputHandle
        {
            add
            {
                _OH0 += value;
            }
            remove
            {
                _OH0 -= value;
            }
        }
        event OutputHandleDelegate<(int Number, byte[] Message)> IPipelineOutput<(int Number, byte[] Message)>.OutputHandle
        {
            add
            {
                _OH1 += value;
            }
            remove
            {
                _OH1 -= value;
            }
        }

        public void Input(IPipelineOutput<byte[]> sender, ulong taskID, byte[] output)
        {
            if (output.Length < AdditionalSize)
            {
                InformerOutputHandle?.Invoke(this, taskID, output, false);
            }
        }

        public void Input(IPipelineOutput<(int Number, byte[] Message)> sender, ulong taskID, (int Number, byte[] Message) output)
        {
            byte[] number = BitConverter.GetBytes(output.Number);
            byte[] message = new byte[number.Length + output.Message.Length];
            Array.Copy(number, message, number.Length);
            Array.Copy(output.Message, 0, message, number.Length, output.Message.Length);
            _OH0?.Invoke(this, taskID, message);
        }

        public void LinkInput(IPipelineOutput<(int Number, byte[] Message)> output)
        {
            output.OutputHandle += Input;
        }

        public void LinkInput(IPipelineOutput<byte[]> output)
        {
            output.OutputHandle += Input;
        }

        public void LinkOutput(IPipelineInput<byte[]> input)
        {
            _OH0 += input.Input;
        }

        public void LinkOutput(IPipelineInput<(int Number, byte[] Message)> input)
        {
            _OH1 += input.Input;
        }

        public void UnlinkInput(IPipelineOutput<(int Number, byte[] Message)> output)
        {
            output.OutputHandle -= Input;
        }

        public void UnlinkInput(IPipelineOutput<byte[]> output)
        {
            output.OutputHandle -= Input;
        }

        public void UnlinkOutput(IPipelineInput<byte[]> input)
        {
            _OH0 -= input.Input;
        }

        public void UnlinkOutput(IPipelineInput<(int Number, byte[] Message)> input)
        {
            _OH1 -= input.Input;
        }
    }
}
