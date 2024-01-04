using System.Text.Json;

namespace NahidaImpact.Common.Data.Provider;
public interface IAssetProvider
{
    JsonDocument GetExcelTableJson(string assetName);
    IEnumerable<string> EnumerateAvatarConfigFiles();
    JsonDocument GetFileAsJsonDocument(string fullPath);
}
