using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmptyBox.Automation.Network;
using EmptyBox.Automation;
using EmptyBox.IO.Serializator;

namespace EmptyBox.Automation.ConsoleTests
{
    class Program
    {
        static void Main(string[] args)
        {
            BinarySerializer bs = new BinarySerializer(Encoding.UTF32);
            InputBlock<byte[]> s0 = new InputBlock<byte[]>((x, y) => Console.WriteLine(bs.Deserialize<string>(y)));
            MessageBugger<int> s1 = new MessageBugger<int>(bs, 256);
            MessageBugger<int> s2 = new MessageBugger<int>(bs, 256);
            OutputBlock<byte[], int> s3 = new OutputBlock<byte[], int>();
            s3.LinkOutput(0, s2, MessageBuggerIndexer.Raw, 0);
            s2.LinkOutput(MessageBuggerIndexer.Splitted, 0, s1, MessageBuggerIndexer.Splitted, 0);
            s1.LinkOutput(MessageBuggerIndexer.Raw, 0, s0);
            while (true)
            {
                byte[] str = bs.Serialize(Console.ReadLine());
                Console.WriteLine("BC: {0}", str.Length);
                s3.Send(str, 0);
            }
        }
    }
}
