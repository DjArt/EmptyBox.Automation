using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmptyBox.Automation
{
    public sealed class Loadbalancer<TInput, TMarker> : IPipelineInput<TInput, TMarker>, IPipelineOutput<TInput, TMarker, uint>, IPipelineInput<LoadbalancerControlPacket<TMarker>>, IPipelineOutput<LoadbalancerStatePacket<TMarker>>
    {
        #region Private structs
        private sealed class Record
        {
            public uint Ratio;
            public uint CurrentRatio;
            public EventHandler<TInput> Event;
        }
        #endregion

        #region IPipelineOutput<TInput, TMarker, uint> interface properties
        EventHandler<TInput> IPipelineOutput<TInput, TMarker, uint>.this[TMarker index0, uint index1]
        {
            get
            {
                return Events[index0][index1].Event;
            }
            set
            {
                Events[index0][index1].Event = value;
            }
        }

        event Action<IPipelineOutput<TInput, TMarker, uint>, TInput, TMarker, uint> IPipelineOutput<TInput, TMarker, uint>.Output { add => Output += value; remove => Output -= value; }
        #endregion

        #region IPipelineInput<TInput, TMarker> interface properties
        EventHandler<TInput> IPipelineInput<TInput, TMarker>.this[TMarker index]
        {
            get
            {
                return (x, y) => (this as IPipelineInput<TInput, TMarker>).Input(x as IPipelineOutput<TInput, TMarker>, y, index);
            }
        }
        #endregion

        #region IPipelineOutput<LoadbalancerStatePacket<TMarker>> interface properties
        event EventHandler<LoadbalancerStatePacket<TMarker>> IPipelineOutput<LoadbalancerStatePacket<TMarker>>.Output { add => StateOutput += value; remove => StateOutput -= value; }
        #endregion

        #region Private objects
        private Dictionary<TMarker, Dictionary<uint, Record>> Events;
        private Random Random;
        private event EventHandler<LoadbalancerStatePacket<TMarker>> StateOutput;
        private event Action<IPipelineOutput<TInput, TMarker, uint>, TInput, TMarker, uint> Output;
        #endregion

        #region Constructors
        public Loadbalancer()
        {
            Events = new Dictionary<TMarker, Dictionary<uint, Record>>();
            Random = new Random();
        }
        #endregion

        #region IPipelineInput<LoadbalancerControlPacket<TMarker>> interface functions
        void IPipelineInput<LoadbalancerControlPacket<TMarker>>.Input(object sender, LoadbalancerControlPacket<TMarker> output)
        {
            switch(output.Action)
            {
                case LoadbalancerControlAction.NewRatio:
                    if (!Events.ContainsKey(output.Marker))
                    {
                        Events.Add(output.Marker, new Dictionary<uint, Record>());
                    }
                    for (uint i0 = 0; i0 < output.Ratio.Length; i0++)
                    {
                        //Здесь нужно подумать о событиях, которые не будут использоватся
                        //Возможно, их  стоит удалить
                        if (!Events[output.Marker].ContainsKey(i0))
                        {
                            Events[output.Marker].Add(i0, new Record());
                        }
                        Events[output.Marker][i0].Ratio = output.Ratio[i0];
                        if (Events[output.Marker][i0].CurrentRatio > Events[output.Marker][i0].Ratio)
                        {
                            Events[output.Marker][i0].CurrentRatio = Events[output.Marker][i0].Ratio;
                        }
                    }
                    break;
                case LoadbalancerControlAction.RemoveAllMarkers:
                    Events.Clear();
                    break;
                case LoadbalancerControlAction.RemoveMarker:
                    if (Events.ContainsKey(output.Marker))
                    {
                        Events.Remove(output.Marker);
                    }
                    else
                    {
                        StateOutput?.Invoke(this, new LoadbalancerStatePacket<TMarker>() { Action = output.Action, State = LoadbalancerState.MarkerNotExist, Marker = output.Marker });
                    }
                    break;
                case LoadbalancerControlAction.ResetAllRatio:
                    foreach(Dictionary<uint, Record> dict in Events.Values)
                    {
                        foreach(Record rec in dict.Values)
                        {
                            rec.CurrentRatio = rec.Ratio;
                        }
                    }
                    break;
                case LoadbalancerControlAction.ResetRatio:
                    if (Events.ContainsKey(output.Marker))
                    {
                        foreach (Record rec in Events[output.Marker].Values)
                        {
                            rec.CurrentRatio = rec.Ratio;
                        }
                    }
                    else
                    {
                        StateOutput?.Invoke(this, new LoadbalancerStatePacket<TMarker>() { Action = output.Action, State = LoadbalancerState.MarkerNotExist, Marker = output.Marker });
                    }
                    break;
                default:

                    break;
            }
        }
        #endregion

        #region IPipelineInput<TInput, TMarker> interface functions
        void IPipelineInput<TInput, TMarker>.Input(IPipelineOutput<TInput, TMarker> sender, TInput input, TMarker marker)
        {
            List<uint> keys = Events[marker].Keys.Where(x => Events[marker][x].CurrentRatio > 0).ToList();
            uint key = keys[Random.Next(keys.Count())];
            Events[marker][key].CurrentRatio--;
            if (keys.Count == 1 && Events[marker][key].CurrentRatio == 0)
            {
                foreach (Record r in Events[marker].Values)
                {
                    r.CurrentRatio = r.Ratio;
                }
            }
        }
        #endregion

        #region Private functions
        private void EventCaller(TMarker marker, uint key, TInput input)
        {
            Events[marker][key].Event?.Invoke(this, input);
            Output?.Invoke(this, input, marker, key);
        }
        #endregion
    }
}
