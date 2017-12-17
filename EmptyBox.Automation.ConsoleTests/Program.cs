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
            InputBlock<byte[], MessageBuggerIndexer> s0 = new InputBlock<byte[], MessageBuggerIndexer>((x, y, z) => Console.WriteLine(bs.Deserialize<string>(y)));
            MessageBugger s1 = new MessageBugger(bs, 256);
            MessageBugger s2 = new MessageBugger(bs, 256);
            OutputBlock<byte[], MessageBuggerIndexer> s3 = new OutputBlock<byte[], MessageBuggerIndexer>();
            s3.LinkOutput(MessageBuggerIndexer.Raw, s2, MessageBuggerIndexer.Raw);
            s2.LinkOutput(MessageBuggerIndexer.Splitted, s1, MessageBuggerIndexer.Splitted);
            s1.LinkOutput(MessageBuggerIndexer.Raw, s0, MessageBuggerIndexer.Raw);
            while (true)
            {
                byte[] str = bs.Serialize(Console.ReadLine());
                Console.WriteLine("BC: {0}", str.Length);
                s3.Send(str, MessageBuggerIndexer.Raw);
            }
        }
    }
}
