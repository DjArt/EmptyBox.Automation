using System;
using System.Collections.Generic;
using System.Text;
using EmptyBox.Automation;
using EmptyBox.IO.Serializator;

namespace EmptyBox.IO.Automation
{
    public class PipelineSelector<T> : IPipelineInput<byte[]>, IPipelineOutput<byte[]>, IPipelineOutputInformer<byte[], bool> where T : struct
    {
        protected BinarySerializer Serializer;
        protected Dictionary<T, OutputHandleDelegate<byte[]>> _UnboxRules;
        protected Dictionary<IPipelineOutput<byte[]>, T> _BoxRules;

        public event OutputHandleDelegate<byte[]> OutputHandle;
        public event InformerOutputHandleDelegate<byte[], bool> InformerOutputHandle;

        public PipelineSelector()
        {
            _UnboxRules = new Dictionary<T, OutputHandleDelegate<byte[]>>();
            _BoxRules = new Dictionary<IPipelineOutput<byte[]>, T>();
            Serializer = new BinarySerializer(Encoding.UTF32);
        }

        public void Input(IPipelineOutput<byte[]> sender, ulong taskID, byte[] output)
        {
            SelectorMessage<T> selectorMessage = Serializer.Deserialize<SelectorMessage<T>>(output);
            if (_UnboxRules.ContainsKey(selectorMessage.Data))
            {
                _UnboxRules[selectorMessage.Data]?.Invoke(this, taskID, selectorMessage.Message);
            }
            else
            {
                InformerOutputHandle?.Invoke(this, taskID, output, false);
            }
        }

        protected void SelectorInput(IPipelineOutput<byte[]> sender, ulong taskID, byte[] output)
        {
            if (_BoxRules.ContainsKey(sender))
            {
                SelectorMessage<T> selectorMessage = new SelectorMessage<T>();
                selectorMessage.Data = _BoxRules[sender];
                selectorMessage.Message = output;
                OutputHandle?.Invoke(this, taskID, Serializer.Serialize(selectorMessage));
            }
            else
            {
                InformerOutputHandle?.Invoke(this, taskID, output, false);
            }
        }


        public void LinkLeftInput(IPipelineOutput<byte[]> pipeline)
        {
            pipeline.OutputHandle += Input;
        }

        public void UnlinkLeftInput(IPipelineOutput<byte[]> pipeline)
        {
            pipeline.OutputHandle -= Input;
        }

        public void LinkLeftOutput(IPipelineInput<byte[]> pipeline)
        {
            OutputHandle += pipeline.Input;
        }

        public void UnlinkLeftOutput(IPipelineInput<byte[]> pipeline)
        {
            OutputHandle -= pipeline.Input;
        }
        /// <summary>
        /// Устанавливает соответсвие заголовка сообщения к модулю конвеера.
        /// </summary>
        /// <param name="value">Заголовок сообщения</param>
        /// <param name="pipeline">Модуль конвеера</param>
        public void LinkRightInput(T value, IPipelineOutput<byte[]> pipeline)
        {
            _BoxRules[pipeline] = value;
            pipeline.OutputHandle += SelectorInput;
        }

        /// <summary>
        /// Устанавливает соответсвие модуля конвеера к заголовку сообщения.
        /// </summary>
        /// <param name="value">Заголовок сообщения</param>
        /// <param name="pipeline">Модуль конвеера</param>
        public void LinkRightOutput(T value, IPipelineInput<byte[]> pipeline)
        {
            if (_UnboxRules.ContainsKey(value))
            {
                _UnboxRules[value] += pipeline.Input;
            }
            else
            {
                _UnboxRules[value] = pipeline.Input;
            }
        }
        /// <summary>
        /// Удаляет соответсвие модуля конвеера к заголовку сообщения.
        /// </summary>
        /// <param name="pipeline">Модуль конвеера</param>
        public void UnlinkRightInput(IPipelineOutput<byte[]> pipeline)
        {
            if (_BoxRules.ContainsKey(pipeline))
            {
                _BoxRules.Remove(pipeline);
                pipeline.OutputHandle -= SelectorInput;
            }
        }
        /// <summary>
        /// Удаляет соответсвие заголовка сообщения к модулю конвеера.
        /// </summary>
        /// <param name="value">Заголовок сообщения</param>
        /// <param name="pipeline">Модуль конвеера</param>
        public void UnlinkRightOutput(T value, IPipelineInput<byte[]> pipeline)
        {
            if (_UnboxRules.ContainsKey(value))
            {
                _UnboxRules[value] -= pipeline.Input;
                if (_UnboxRules[value] == null)
                {
                    _UnboxRules.Remove(value);
                }
            }
        }
    }
}
