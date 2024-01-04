namespace NahidaImpact.Kcp
{
    internal interface IKcpConversationUpdateNotificationSource
    {
        ReadOnlyMemory<byte> Packet { get; }
        void Release();
    }
}
