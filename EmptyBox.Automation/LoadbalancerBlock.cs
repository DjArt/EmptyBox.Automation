using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmptyBox.Automation
{
    public class LoadbalancerBlock<TInput> : IPipelineInput<TInput>, IPipelineInputInformer<TInput, LoadbalancerBlock<TInput>.ControlPacket>, IPipelineOutput<(int outnum, TInput output)>
    {
        public enum ControlState
        {
            SetRatio
        }

        public struct ControlPacket
        {
            public ControlState State;
            public byte[] Ratio;
        }

        protected Dictionary<ulong, byte[]> _SettedRatio;
        protected Dictionary<ulong, byte[]> _CurrentRatio;
        protected Random _Random;

        public event OutputHandleDelegate<(int outnum, TInput output)> OutputHandle;

        public LoadbalancerBlock()
        {

            _SettedRatio = new Dictionary<ulong, byte[]>();
            _CurrentRatio = new Dictionary<ulong, byte[]>();
            _Random = new Random();
        }

        public void Input(IPipelineOutput<TInput> sender, ulong taskID, TInput output)
        {
            if (_SettedRatio.ContainsKey(taskID))
            {
                if (!_CurrentRatio.ContainsKey(taskID) || _CurrentRatio[taskID].Count(x => x == 0) == _CurrentRatio[taskID].Length)
                {
                    _CurrentRatio[taskID] = new byte[_SettedRatio[taskID].Length];
                    Array.Copy(_SettedRatio[taskID], _CurrentRatio[taskID], _SettedRatio[taskID].Length);
                }
                int count = _CurrentRatio[taskID].Count(x => x > 0);
                int select = _Random.Next(count);
                bool done = false;
                int abspos = -1;
                int relpos = -1;
                do
                {
                    if (relpos == select)
                    {
                        done = true;
                    }
                    else
                    {
                        abspos++;
                        if (_CurrentRatio[taskID][abspos] > 0)
                        {
                            relpos++;
                        }
                    }
                }
                while (!done);
                _CurrentRatio[taskID][abspos]--;
                OutputHandle?.Invoke(this, taskID, (abspos, output));
            }
            else
            {
                throw new Exception("wow, we not found this taskID");
            }
        }

        public void InformInput(IPipelineInput<TInput> sender, ulong? taskID, TInput input, ControlPacket state)
        {
            switch(state.State)
            {
                case ControlState.SetRatio:
                    if (state.Ratio == null)
                    {
                        if (!_SettedRatio.ContainsKey(taskID.Value))
                        {
                            _SettedRatio.Remove(taskID.Value);
                            _CurrentRatio.Remove(taskID.Value);
                        }
                    }
                    else
                    {
                        //Не стоит забывать, что массивы - это ссылочные типы.
                        _SettedRatio[taskID.Value] = new byte[state.Ratio.Length];
                        _CurrentRatio[taskID.Value] = new byte[state.Ratio.Length];
                        Array.Copy(state.Ratio, _CurrentRatio[taskID.Value], state.Ratio.Length);
                        Array.Copy(state.Ratio, _SettedRatio[taskID.Value], state.Ratio.Length);
                    }
                    break;
            }
        }

        public void LinkBalancerInput(IPipelineOutput<TInput> output)
        {
            output.OutputHandle += Input;
        }

        public void UninkBalancerInput(IPipelineOutput<TInput> output)
        {
            output.OutputHandle -= Input;
        }

        public void LinkBalancerOutput(IPipelineInput<(int, TInput)> input)
        {
            OutputHandle += input.Input;
        }

        public void UninkBalancerOutput(IPipelineInput<(int, TInput)> input)
        {
            OutputHandle -= input.Input;
        }
    }
}
