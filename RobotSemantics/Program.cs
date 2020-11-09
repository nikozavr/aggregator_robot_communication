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
                Console.WriteLine($"Get event: {reply.ResponseStream.Current.EventType.ToString()}");
                Console.WriteLine("  Properties:");
                foreach (var eventProperty in reply.ResponseStream.Current.EventProperties)
                {
                    Console.WriteLine($"    {eventProperty.Id} - {eventProperty.Value}");
                }
                Console.WriteLine("  Participants:");
                foreach (var eventParticipant in reply.ResponseStream.Current.EventParticipants)
                {
                    Console.WriteLine($"    Id - {eventParticipant.Id}");
                    Console.WriteLine($"    Role - {eventParticipant.Role.ToString()}");
                    Console.WriteLine($"    Type - {eventParticipant.Type.ToString()}");
                    foreach (var participantProperty in eventParticipant.ParticipantProperties)
                    {
                        Console.WriteLine($"       {participantProperty.Id} - {participantProperty.Value}");
                    }
                    Console.WriteLine("    ----");
                }
                Console.WriteLine();
            }

            channel.ShutdownAsync().Wait();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
