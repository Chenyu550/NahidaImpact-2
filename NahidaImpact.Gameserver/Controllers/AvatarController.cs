using NahidaImpact.Gameserver.Controllers.Attributes;
using NahidaImpact.Gameserver.Controllers.Result;
using NahidaImpact.Gameserver.Game.Scene;
using NahidaImpact.Gameserver.Network.Session;
using NahidaImpact.Protocol;

namespace NahidaImpact.Gameserver.Controllers;

[NetController]
internal class AvatarController : ControllerBase
{
    [NetCommand(CmdType.SetUpAvatarTeamReq)]
    public async ValueTask<IResult> OnSetUpAvatarTeamReq(NetSession session, SceneManager sceneManager)
    {
        SetUpAvatarTeamReq request = Packet!.DecodeBody<SetUpAvatarTeamReq>();

        AvatarTeam newTeam = new();
        newTeam.AvatarGuidList.AddRange(request.AvatarTeamGuidList);
        await session.NotifyAsync(CmdType.AvatarTeamUpdateNotify, new AvatarTeamUpdateNotify
        {
            AvatarTeamMap = { { request.TeamId, newTeam } }
        });

        await sceneManager.ChangeTeamAvatarsAsync(request.AvatarTeamGuidList.ToArray());

        SetUpAvatarTeamRsp response = new()
        {
            CurAvatarGuid = request.CurAvatarGuid,
            TeamId = request.TeamId,
        };
        response.AvatarTeamGuidList.AddRange(request.AvatarTeamGuidList);

        return Response(CmdType.SetUpAvatarTeamRsp, response);
    }
}
