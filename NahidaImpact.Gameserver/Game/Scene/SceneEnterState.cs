namespace NahidaImpact.Gameserver.Game.Scene;
internal enum SceneEnterState
{
    None = -1,
    EnterRequested,
    ReadyToEnter,
    InitFinished,
    EnterDone,
    PostEnter,

    Complete
}
