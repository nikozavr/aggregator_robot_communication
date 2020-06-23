using Grpc.Core;
using Grpc.Core.Utils;
using System;
using System.Threading.Tasks;

namespace RobotSemantics
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Channel channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);

            var client = new SemanticsProvider.SemanticsProviderClient(channel);

            AsyncServerStreamingCall<SemanticsEvent> reply = client.GetSemantics(new Empty());

            while (await reply.ResponseStream.MoveNext())
            {
                Console.WriteLine($"Get event: {reply.ResponseStream.Current.Name}");
            }

            channel.ShutdownAsync().Wait();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
