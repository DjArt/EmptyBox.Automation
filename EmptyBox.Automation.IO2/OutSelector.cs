using System;
using System.Collections.Generic;
using System.Text;

namespace EmptyBox.Automation
{
    public class OutSelector<TIn, TIndex> : IPipelineInput<TIn>, IPipelineOutput<TIn, TIndex>
    {
        public event OutputEvent<TIn, TIndex> Output;
        public Func<TIn, TIndex> Selector { get; } 

        public OutSelector(Func<TIn, TIndex> selector)
        {
            Selector = selector;
        }

        public void Input(object source, TIn output)
        {
            Output?.Invoke(source, output, Selector(output));
        }
    }

    public class OutSelector<TIn, TIndex0, TIndex1> : IPipelineInput<TIn>, IPipelineOutput<TIn, TIndex0, TIndex1>
    {
        public event OutputEvent<TIn, TIndex0, TIndex1> Output;
        public Func<TIn, TIndex0> Selector0 { get; }
        public Func<TIn, TIndex1> Selector1 { get; }

        public OutSelector(Func<TIn, TIndex0> selector0, Func<TIn, TIndex1> selector1)
        {
            Selector0 = selector0;
            Selector1 = selector1;
        }

        public void Input(object source, TIn output)
        {
            Output?.Invoke(source, output, Selector0(output), Selector1(output));
        }
    }

    public class OutSelector<TIn, TIndex0, TIndex1, TIndex2> : IPipelineInput<TIn, TIndex0>, IPipelineOutput<TIn, TIndex1, TIndex2>
    {
        public event OutputEvent<TIn, TIndex1, TIndex2> Output;
        public Func<TIn, TIndex0, TIndex1> Selector0 { get; }
        public Func<TIn, TIndex0, TIndex2> Selector1 { get; }

        public OutSelector(Func<TIn, TIndex0, TIndex1> selector0, Func<TIn, TIndex0, TIndex2> selector1)
        {
            Selector0 = selector0;
            Selector1 = selector1;
        }

        public void Input(object source, TIn output, TIndex0 index)
        {
            Output?.Invoke(source, output, Selector0(output, index), Selector1(output, index));
        }
    }
}
