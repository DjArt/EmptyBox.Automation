using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using EmptyBox.Automation;
using EmptyBox.ScriptRuntime.Extensions;

namespace EmptyBox.IO.Automation
{
    //"Как назовёшь корабль, так он и поплывёт." (Капитан Врунгель)
    //TODO: Ну что это за сериализация значений! Следует заменить на BinarySerializer после его переработки.
    //Интерфейс IPipelineOutputInformer<byte[], bool> должен реализовывать более информативный тип, чем bool, скорее всего ещё не существующий.

    public class MessageBugger : IPipelineInput<(ulong SequenceID, byte[] Message)>, IPipelineOutput<byte[][]>, IPipelineInput<byte[]>, IPipelineOutput<(ulong SequenceID, byte[] Message)>, IPipelineOutputInformer<byte[], bool>
    {
        //Ну хрен его знает, стоит пересмотреть этот подход.
        public const byte HEADER_ID = 99;
        public const byte PART_ID = 48;

        public struct Header
        {
            public ulong SequenceID;
            public int PartsCount;
            public int MessageLength;
            public int PartLength;
            public int LastPartLength;
        }

        public event InformerOutputHandleDelegate<byte[], bool> InformerOutputHandle;
        protected OutputHandleDelegate<byte[][]> _OH0;
        protected OutputHandleDelegate<(ulong SequenceID, byte[] Message)> _OH1;
        protected MessageEnumerator _Enumerator;
        //Очевидно, здесь не всё так просто. Разные источники могут прислать нам один и тот же SequenceID, что вызовет пропажу пакетов. Нужна привязка к taskID, но это позже.
        protected Dictionary<Header, byte[][]> _MessagePool;
        event OutputHandleDelegate<byte[][]> IPipelineOutput<byte[][]>.OutputHandle
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
        event OutputHandleDelegate<(ulong SequenceID, byte[] Message)> IPipelineOutput<(ulong SequenceID, byte[] Message)>.OutputHandle
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
        public int PacketSize { get; set; }

        public MessageBugger(int packetsize)
        {
            PacketSize = packetsize;
            _Enumerator = new MessageEnumerator();
            _MessagePool = new Dictionary<Header, byte[][]>();
        }

        public void Input(IPipelineOutput<(ulong SequenceID, byte[] Message)> sender, ulong taskID, (ulong SequenceID, byte[] Message) output)
        {
            //Первый пакет - заголовок, в котором хранится информация о расфасовке
            //-1 - это ещё один байт на записть ID пакета, либо это Header, либо Part
            int partlength = PacketSize - MessageEnumerator.AdditionalSize - 1;
            int count = (int)Math.Ceiling(output.Message.Length / (double)(partlength));
            int lastpartlength = output.Message.Length - partlength * (count - 1);
            byte[][] packets = new byte[count + 1][];
            packets[0] = new byte[25];
            packets[0][0] = HEADER_ID;
            Array.Copy(BitConverter.GetBytes(output.SequenceID), 0, packets[0], 1, sizeof(ulong));
            Array.Copy(BitConverter.GetBytes(count), 0, packets[0], 9, sizeof(int));
            Array.Copy(BitConverter.GetBytes(output.Message.Length), 0, packets[0], 13, sizeof(int));
            Array.Copy(BitConverter.GetBytes(partlength), 0, packets[0], 17, sizeof(int));
            Array.Copy(BitConverter.GetBytes(lastpartlength), 0, packets[0], 21, sizeof(int));
            for (int i0 = 0; i0 < count; i0++)
            {
                packets[i0 + 1] = new byte[i0 + 1 == count ? lastpartlength + 1 + 8 + 4 : partlength + 1 + 8 + 4];
                packets[i0 + 1][0] = PART_ID;
                Array.Copy(BitConverter.GetBytes(output.SequenceID), 0, packets[i0 + 1], 1, sizeof(ulong));
                Array.Copy(BitConverter.GetBytes(i0), 0, packets[i0 + 1], 9, sizeof(int));
                Array.Copy(output.Message, i0 * partlength, packets[i0 + 1], 13, i0 + 1 == count ? lastpartlength : partlength);
            }
            _OH0?.Invoke(this, taskID, packets);
        }

        public void Input(IPipelineOutput<byte[]> sender, ulong taskID, byte[] output)
        {
            switch (output[0])
            {
                case PART_ID:
                    //Такая проверка нужна, но это быдлокод. Можно заменить на try/catch
                    if (output.Length > 13)
                    {
                        ulong SequenceID = BitConverter.ToUInt64(output, 1);
                        int ID = BitConverter.ToInt32(output, 9);
                        int index = _MessagePool.Keys.ToList().FindIndex(x => x.SequenceID == SequenceID);
                        if (index > -1)
                        {
                            Header header = _MessagePool.Keys.ElementAt(index);
                            //Тут тоже быдлокод и потенциальная ошибка. ОНА НЕ ПОТЕНЦИАЛЬНА!!!
                            if (_MessagePool[header][ID] == null/* && (ID < header.PartsCount - 1 && output.Length - 13 == header.PartLength || ID == header.PartsCount - 1 && output.Length - 13 == header.LastPartLength)*/)
                            {
                                _MessagePool[header][ID] = new byte[output.Length - 13];
                                Array.Copy(output, 13, _MessagePool[header][ID], 0, _MessagePool[header][ID].Length);
                                //Уведомляем конвеер об удачной обработке пакета
                                InformerOutputHandle?.Invoke(this, taskID, output, true);
                                if (_MessagePool[header].All(x => x != null))
                                {
                                    byte[] message = ArrayExtensions.Combine(_MessagePool[header]);
                                    _MessagePool.Remove(header);
                                    _OH1?.Invoke(this, taskID, (SequenceID, message));
                                }
                            }
                            else
                            {
                                //Уведомляем конвеер о "неправильном" пакете
                                InformerOutputHandle?.Invoke(this, taskID, output, false);
                            }
                        }
                        else
                        {
                            //Уведомляем конвеер о "неправильном" пакете
                            InformerOutputHandle?.Invoke(this, taskID, output, false);
                        }
                    }
                    break;
                case HEADER_ID:
                    //Такая проверка нужна, но это быдлокод. Можно заменить на try/catch
                    if (output.Length == 25)
                    {
                        Header readed = new Header
                        {
                            SequenceID = BitConverter.ToUInt64(output, 1),
                            PartsCount = BitConverter.ToInt32(output, 9),
                            MessageLength = BitConverter.ToInt32(output, 13),
                            PartLength = BitConverter.ToInt32(output, 17),
                            LastPartLength = BitConverter.ToInt32(output, 21)
                        };
                        if (!_MessagePool.Keys.Any(x => x.SequenceID == readed.SequenceID))
                        {
                            _MessagePool[readed] = new byte[readed.PartsCount][];
                            //Уведомляем конвеер об удачной обработке пакета
                            InformerOutputHandle?.Invoke(this, taskID, output, true);
                        }
                        else
                        {
                            //Уведомляем конвеер о "неправильном" пакете
                            InformerOutputHandle?.Invoke(this, taskID, output, false);
                        }
                    }
                    else
                    {
                        //Уведомляем конвеер о "неправильном" пакете
                        InformerOutputHandle?.Invoke(this, taskID, output, false);
                    }
                    break;
            }
        }

        #region IPipelineInput<(ulong SequenceID, byte[] Message)>

        public void LinkSeparatorInput(IPipelineOutput<(ulong SequenceID, byte[] Message)> output)
        {
            output.OutputHandle += Input;
        }

        public void UnlinkSeparatorInput(IPipelineOutput<(ulong SequenceID, byte[] Message)> output)
        {
            output.OutputHandle -= Input;
        }

        #endregion

        #region IPipelineOutput<byte[][]>

        public void LinkSeparatorOutput(IPipelineInput<byte[][]> input)
        {
            _OH0 += input.Input;
        }

        public void UnlinkSeparatorOutput(IPipelineInput<byte[][]> input)
        {
            _OH0 -= input.Input;
        }

        #endregion

        #region IPipelineInput<byte[]>

        public void LinkCombinerInput(IPipelineOutput<byte[]> output)
        {
            output.OutputHandle += Input;
        }


        public void UnlinkCombinerInput(IPipelineOutput<byte[]> output)
        {
            output.OutputHandle -= Input;
        }

        #endregion

        #region IPipelineOutput<(ulong SequenceID, byte[] Message)>

        public void LinkCombinerOutput(IPipelineInput<(ulong SequenceID, byte[] Message)> input)
        {
            _OH1 += input.Input;
        }

        public void UnlinkCombinerOutput(IPipelineInput<(ulong SequenceID, byte[] Message)> input)
        {
            _OH1 -= input.Input;
        }

        #endregion
    }
}
