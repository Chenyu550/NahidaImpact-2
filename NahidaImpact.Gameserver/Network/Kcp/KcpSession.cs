using NahidaImpact.Gameserver.Controllers.Dispatching;
using NahidaImpact.Gameserver.Network.Session;
using Microsoft.Extensions.Logging;

namespace NahidaImpact.Gameserver.Network.Kcp;
internal class KcpSession(ILogger<NetSession> logger, NetSessionManager sessionManager, NetCommandDispatcher commandDispatcher) : NetSession(logger, sessionManager, commandDispatcher)
{
    private const int MaxPacketSize = 32768;
    private const int ReadTimeout = 30;
    private const int WriteTimeout = 30;

    private readonly byte[] _recvBuffer = GC.AllocateUninitializedArray<byte>(MaxPacketSize);
    private readonly byte[] _sendBuffer = GC.AllocateUninitializedArray<byte>(MaxPacketSize);

    public override async ValueTask RunAsync()
    {
        Memory<byte> buffer = _recvBuffer.AsMemory();

        while (true)
        {
            int readAmount = await ReadWithTimeoutAsync(buffer, ReadTimeout);
            if (readAmount <= 0)
                break;

            //MhySecurity.Xor(buffer[..readAmount].Span, EncryptionKey);
            int consumedBytes = await ConsumePacketsAsync(buffer[..readAmount]);
            if (consumedBytes == -1)
                break;
        }
    }

    public override async ValueTask SendAsync(NetPacket packet)
    {
        Memory<byte> buffer = _sendBuffer.AsMemory();

        int length = packet.EncodeTo(buffer);
        //MhySecurity.Xor(buffer[..length].Span, EncryptionKey);

        await WriteWithTimeoutAsync(buffer[..length], WriteTimeout);
    }
}
