using System.Text.Json;
using NahidaImpact.Common.Security;
using NahidaImpact.Protocol;
using Google.Protobuf;

namespace NahidaImpact.SDK.Handlers;

public static class RegionHandler
{
    private const string CLIENT_CUSTOM_CONFIG = "{\"sdkenv\":\"2\",\"checkdevice\":\"false\",\"loadPatch\":\"false\",\"showexception\":\"false\",\"regionConfig\":\"pm|fk|add\",\"downloadMode\":\"0\"}";
    private static readonly string s_queryRegionListRsp;
    private static readonly string s_queryCurRegionRsp;

    static RegionHandler()
    {
        s_queryRegionListRsp = BuildQueryRegionListResponse();
        s_queryCurRegionRsp = BuildQueryCurrentRegionResponse();
    }

    public static IResult OnQueryRegionList() => TypedResults.Text(s_queryRegionListRsp, "text/plain");
    public static IResult OnQueryCurRegion() => TypedResults.Text(s_queryCurRegionRsp, "application/json");

    private static string BuildQueryCurrentRegionResponse() => "{\"ip\":\"127.0.0.1\"}";

    private static string BuildQueryRegionListResponse()
    {
        byte[] clientCustomConfigEncrypted = MhySecurity.Xor(CLIENT_CUSTOM_CONFIG, MhySecurity.InitialKey);

        QueryRegionListHttpRsp rsp = new()
        {
            ClientCustomConfigEncrypted = ByteString.CopyFrom(clientCustomConfigEncrypted),
            ClientSecretKey = ByteString.CopyFrom(MhySecurity.InitialKeyEc2b),
            EnableLoginPc = true,
            RegionList =
            {
                new RegionSimpleInfo
                {
                    Type = "DEV_PUBLIC",
                    DispatchUrl = "http://127.0.0.1:8888/query_cur_region",
                    Name = "os_russia",
                    Title = "NahidaImpact"
                }
            },
            Retcode = 0
        };

        return Convert.ToBase64String(rsp.ToByteArray());
    }
}
