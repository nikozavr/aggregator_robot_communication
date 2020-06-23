using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace RobotExecution
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Channel channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);

            var client = new ObjectPositionProvider.ObjectPositionProviderClient(channel);

            AsyncServerStreamingCall<ObjectPosition> reply = client.GetObjectsPositions(new EmptyMessage());

            while (await reply.ResponseStream.MoveNext())
            {
                Console.WriteLine($"Get position: {reply.ResponseStream.Current.Name} - ({reply.ResponseStream.Current.X},{reply.ResponseStream.Current.Y},{reply.ResponseStream.Current.Z})");
            }

            channel.ShutdownAsync().Wait();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
