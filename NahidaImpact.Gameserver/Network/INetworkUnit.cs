using System.Net;

namespace NahidaImpact.Gameserver.Network;
internal interface INetworkUnit : IDisposable
{
    IPEndPoint RemoteEndPoint { get; }

    ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken);
    ValueTask SendAsync(Memory<byte> buffer, CancellationToken cancellationToken);
}
