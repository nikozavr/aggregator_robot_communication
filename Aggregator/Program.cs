using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Aggregator
{
    class Program
    {
        const int SemanticsPort = 50051;

        static readonly List<string> _events = new List<string>() { "face appeared", "face disappeared", "object appeared", "object desappeared" };

        static async Task Main(string[] args)
        {
            var semanticsEventsChannel = System.Threading.Channels.Channel.CreateUnbounded<SemanticsEvent>();
            var positionsEventsChannel = System.Threading.Channels.Channel.CreateUnbounded<ObjectPosition>();
            using var cancellationTokenSource = new CancellationTokenSource();

            var generateSemanticsTask = Task.Run(async () => {
                var random = new Random();
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    await Task.Delay(1000, cancellationTokenSource.Token);
                    int index = random.Next(_events.Count);
                    string createdEvent = _events [index];
                    Console.WriteLine($"Event created: {createdEvent}");
                    await semanticsEventsChannel.Writer.WriteAsync(new SemanticsEvent()
                    {
                        EventType = (SemanticsEvent.Types.EventType)random.Next(Enum.GetNames(typeof(SemanticsEvent.Types.EventType)).Length),//SemanticsEvent.Types.EventType.Moving,
                        EventProperties = { new Property()
                        {
                            PropertyName = "test1",
                            Value = random.Next(1200)
                        },
                        new Property(){
                            PropertyName = "test2",
                            Value = random.Next(1200)
                        }
                        },
                        EventParticipants = { new Participant()
                        {
                            Id = Convert.ToUInt16(random.Next(100)),
                            Role = (Participant.Types.Role)random.Next(Enum.GetNames(typeof(Participant.Types.Role)).Length),
                            Type = (Participant.Types.Type)random.Next(Enum.GetNames(typeof(Participant.Types.Type)).Length),
                            ParticipantProperties = { new Property()
                                {
                                    PropertyName = "test1",
                                    Value = random.Next(1200)
                                },
                                new Property(){
                                    PropertyName = "test2",
                                    Value = random.Next(1200)
                                }
                            },
                        }}
                    }, cancellationTokenSource.Token);
                }
            }, cancellationTokenSource.Token);

            var generatePositionsTask = Task.Run(async () => {
                var random = new Random();
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    await Task.Delay(100, cancellationTokenSource.Token);
                    var newObjectPosition = new ObjectPosition()
                    {
                        Name = $"object {random.Next()}",
                        X = random.NextDouble(),
                        Y = random.NextDouble(),
                        Z = random.NextDouble(),
                    };
                    await positionsEventsChannel.Writer.WriteAsync(newObjectPosition, cancellationTokenSource.Token);
                }
            }, cancellationTokenSource.Token);

            Server semanticsServer = new Server
            {
                Services = { 
                    SemanticsProvider.BindService(new SemanticsProviderImpl(semanticsEventsChannel.Reader)),
                    ObjectPositionProvider.BindService(new ObjectsPositionsProviderImp(positionsEventsChannel.Reader))
                },
                Ports = { new ServerPort("localhost", SemanticsPort, ServerCredentials.Insecure) }
            };
            semanticsServer.Start();

            Console.WriteLine("Semantics server listening on port " + SemanticsPort);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            cancellationTokenSource.Cancel();

            semanticsEventsChannel.Writer.Complete();
            positionsEventsChannel.Writer.Complete();

            await semanticsServer.ShutdownAsync();
        }
    }
}
