using Grpc.Core;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Aggregator
{
    class ObjectsPositionsProviderImp : ObjectPositionProvider.ObjectPositionProviderBase
    {
        private readonly ChannelReader<ObjectPosition> _objectPositionChannel;

        public ObjectsPositionsProviderImp(ChannelReader<ObjectPosition> objectPositionChannel)
        {
            _objectPositionChannel = objectPositionChannel;
        }

        public override async Task GetObjectsPositions(EmptyMessage request, IServerStreamWriter<ObjectPosition> responseStream, ServerCallContext context)
        {
            await foreach (var objectPosition in _objectPositionChannel.ReadAllAsync())
            {
                await responseStream.WriteAsync(objectPosition);
            }
        }
    }
}
