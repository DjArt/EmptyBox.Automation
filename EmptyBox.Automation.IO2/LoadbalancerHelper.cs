using System;
using System.Collections.Generic;
using System.Text;

namespace EmptyBox.Automation
{
    public enum LoadbalancerControlAction
    {
        NewRatio,
        ResetRatio,
        ResetAllRatio,
        RemoveMarker,
        RemoveAllMarkers
    }

    public enum LoadbalancerState
    {
        MarkerNotExist
    }

    public struct LoadbalancerControlPacket<TMarker>
    {
        public LoadbalancerControlAction Action;
        public uint[] Ratio;
        public TMarker Marker;
    }

    public struct LoadbalancerStatePacket<TMarker>
    {
        public LoadbalancerControlAction Action;
        public LoadbalancerState State;
        public TMarker Marker;
    }
}
