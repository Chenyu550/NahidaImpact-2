using System.Net;
using NahidaImpact.Common.Security;
using NahidaImpact.Gameserver.Controllers.Dispatching;
using NahidaImpact.Gameserver.Controllers.Result;
using NahidaImpact.Protocol;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace NahidaImpact.Gameserver.Network.Session;
internal abstract class NetSession(ILogger<NetSession> logger, NetSessionManager sessionManager, NetCommandDispatcher commandDispatcher) : IDisposable
{
    public IPEndPoint EndPoint => _networkUnit!.RemoteEndPoint;

    public long SessionId { get; private set; }
    private INetworkUnit? _networkUnit;

    private readonly ILogger _logger = logger;
    private readonly NetSessionManager _sessionManager = sessionManager;
    private readonly NetCommandDispatcher _commandDispatcher = commandDispatcher;

    protected byte[] EncryptionKey { get; private set; } = MhySecurity.InitialKey;

    public abstract ValueTask RunAsync();
    public abstract ValueTask SendAsync(NetPacket packet);

    public void Establish(long sessionId, INetworkUnit networkUnit)
    {
        SessionId = sessionId;
        _networkUnit = networkUnit;

        _sessionManager.Add(this);
    }

    public async Task NotifyAsync<TNotify>(CmdType cmdType, TNotify notify) where TNotify : IMessage<TNotify>
    {
        await SendAsync(new()
        {
            CmdType = cmdType,
            Head = Memory<byte>.Empty,
            Body = notify.ToByteArray()
        });
    }

    public async ValueTask HandlePacket(NetPacket packet)
    {
        IResult? result = await _commandDispatcher.InvokeHandler(packet);
        if (result != null)
        {
            while (result.NextPacket(out NetPacket? serverPacket))
            {
                await SendAsync(serverPacket);

                if (serverPacket.CmdType == CmdType.GetPlayerTokenRsp)
                {
                    InitializeEncryption(1337); // hardcoded MT seed with patch
                }
            }

            Debug.WriteLine("Successfully handled command of type {0}", packet.CmdType);
        }
    }

    protected async ValueTask<int> ConsumePacketsAsync(Memory<byte> buffer)
    {
        if (buffer.Length < 12)
            return 0;

        int consumed = 0;
        do
        {
            (NetPacket? packet, int bytesConsumed) = NetPacket.DecodeFrom(buffer[consumed..]);
            consumed += bytesConsumed;

            if (packet == null)
                return consumed;

            if (packet != null) await HandlePacket(packet);
        } while (buffer.Length - consumed >= 12);

        return consumed;
    }

    private void InitializeEncryption(ulong seed)
    {
        EncryptionKey = MhySecurity.GenerateSecretKey(seed);
    }

    protected async ValueTask<int> ReadWithTimeoutAsync(Memory<byte> buffer, int timeoutSeconds)
    {
        using CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromSeconds(timeoutSeconds));
        return await _networkUnit!.ReceiveAsync(buffer, cancellationTokenSource.Token);
    }

    protected async ValueTask WriteWithTimeoutAsync(Memory<byte> buffer, int timeoutSeconds)
    {
        using CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromSeconds(timeoutSeconds));
        await _networkUnit!.SendAsync(buffer, cancellationTokenSource.Token);
    }

    public virtual void Dispose()
    {
        _networkUnit?.Dispose();
        _ = _sessionManager.TryRemove(this);
    }
}
