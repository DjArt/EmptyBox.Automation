using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmptyBox.Automation.Tests
{
    class TestAction : Pipeline<string, string>, IPipelineIO<string, string>
    {
        event EventHandler<string> IPipelineOutput<string>.Output { add => Output += value; remove => Output -= value; }
        private event EventHandler<string> Output;
        private int Delay;

        EventHandler<string> IPipelineInput<string>.Input => (x, y) =>
        {
            Task.Delay(Delay).Wait();
            Output?.Invoke(this, Delay + ": " + y);
        };

        public TestAction(int delay)
        {
            Delay = delay;
        }
        
    }

    class Program
    {
        static void Main(string[] args)
        {
            TestAction a0 = new TestAction(500);
            TestAction a1 = new TestAction(1000);
            Filter<string> f0 = new Filter<string>(x => x.Contains('0'));
            ExternalInput<string> input = new ExternalInput<string>();
            ExternalOutput<string> output = new ExternalOutput<string>((sender, x) => Console.WriteLine(x));
            object k = null;
            k = input >> !a0 >> output;
            k = input >> !a1 >> output;
            for (int i0 = 0; i0 < 4; i0++)
            {
                input.Send(Console.ReadLine());
            }
            k = input > !a0;
            k = input > !a1;
            k = input >> f0 >> a0;
            k = input >> f0 >> a1;
            for (int i0 = 0; i0 < 4; i0++)
            {
                input.Send(Console.ReadLine());
            }
            Console.ReadKey();
        }
    }
}
