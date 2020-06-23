using Grpc.Core;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Aggregator
{
    class SemanticsProviderImpl : SemanticsProvider.SemanticsProviderBase
    {
        private readonly ChannelReader<SemanticsEvent> _semanticsEventChannel;

        public SemanticsProviderImpl(ChannelReader<SemanticsEvent> semanticsEventChannel)
        {
            _semanticsEventChannel = semanticsEventChannel;
        }

        public override async Task GetSemantics(Empty request, IServerStreamWriter<SemanticsEvent> responseStream, ServerCallContext context)
        {
            await foreach (var semanticsEvent in _semanticsEventChannel.ReadAllAsync())
            {
                await responseStream.WriteAsync(semanticsEvent);
            }
        }
    }
}
