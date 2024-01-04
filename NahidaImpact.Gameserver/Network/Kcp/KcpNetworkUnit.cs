using System.Net;
using NahidaImpact.Kcp;

namespace NahidaImpact.Gameserver.Network.Kcp;
internal class KcpNetworkUnit(KcpConversation conversation, IPEndPoint remoteEndPoint) : INetworkUnit
{
    public IPEndPoint RemoteEndPoint { get; } = remoteEndPoint;

    private readonly KcpConversation _conversation = conversation;

    public async ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        KcpConversationReceiveResult result = await _conversation.ReceiveAsync(buffer, cancellationToken);
        if (result.TransportClosed)
            return -1;

        return result.BytesReceived;
    }

    public async ValueTask SendAsync(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        await _conversation.SendAsync(buffer, cancellationToken);
    }

    public void Dispose()
    {
        _conversation.Dispose();
    }
}
