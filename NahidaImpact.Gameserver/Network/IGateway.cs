namespace NahidaImpact.Gameserver.Network;
internal interface IGateway
{
    Task Start();
    Task Stop();
}
