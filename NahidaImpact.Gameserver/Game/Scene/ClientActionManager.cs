using Google.Protobuf;

namespace NahidaImpact.Gameserver.Game.Scene;
internal class ClientActionManager(SceneManager sceneManager)
{
    public async ValueTask InvokeAsync(CombatTypeArgument combatTypeArgument, IMessage data)
    {
        switch (combatTypeArgument)
        {
            case CombatTypeArgument.EntityMove:
                await PerformEntityMovement((EntityMoveInfo)data);
                break;
        }
    }

    public async ValueTask PerformEntityMovement(EntityMoveInfo info)
    {
        await sceneManager.MotionChanged(info.EntityId, info.MotionInfo);
    }
}
