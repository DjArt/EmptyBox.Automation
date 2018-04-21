using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ClassLibrary1;

namespace EmptyBox.Automation
{
    public class f
    {
        static void y()
        {
            Class1 b = new Class1();
            b = b << b;
        }
    }

    public class Wrapper<TInput, TOutput> : IPipelineInput<TInput>, IPipelineOutput<TOutput>
    {
        public event OutputEvent<TOutput> Output;
        public Func<TInput, TOutput> Wrap { get; }

        public Wrapper(Func<TInput, TOutput> wrap)
        {
            Wrap = wrap;
        }

        public void Input(object source, TInput output)
        {
            Output?.Invoke(source, Wrap(output));
        }
    }

    public class Wrapper<TIn, TOut, TInIndex, TOutIndex> : IPipelineInput<TIn, TInIndex>, IPipelineOutput<TOut, TOutIndex>
    {
        public event OutputEvent<TOut, TOutIndex> Output;

        public Func<TIn, TOut> DataWrap { get; }
        public Func<TInIndex, TOutIndex> IndexWrap { get; }

        public Wrapper(Func<TIn, TOut> data, Func<TInIndex, TOutIndex> index)
        {
            DataWrap = data;
            IndexWrap = index;
        }

        public void Input(object source, TIn input, TInIndex index)
        {
            Output?.Invoke(source, DataWrap(input), IndexWrap(index));
        }
    }

    public class Wrapper<TIn, TOut, TInIndex0, TInIndex1, TOutIndex0, TOutIndex1> : IPipelineInput<TIn, TInIndex0, TInIndex1>, IPipelineOutput<TOut, TOutIndex0, TOutIndex1>
    {
        public event OutputEvent<TOut, TOutIndex0, TOutIndex1> Output;

        public Func<TIn, TOut> DataWrap { get; }
        public Func<TInIndex0, TOutIndex0> Index0Wrap { get; }
        public Func<TInIndex1, TOutIndex1> Index1Wrap { get; }

        public Wrapper(Func<TIn, TOut> data, Func<TInIndex0, TOutIndex0> index0, Func<TInIndex1, TOutIndex1> index1)
        {
            DataWrap = data;
            Index0Wrap = index0;
            Index1Wrap = index1;
        }

        public void Input(object source, TIn input, TInIndex0 index0, TInIndex1 index1)
        {
            Output?.Invoke(source, DataWrap(input), Index0Wrap(index0), Index1Wrap(index1));
        }
    }
}
