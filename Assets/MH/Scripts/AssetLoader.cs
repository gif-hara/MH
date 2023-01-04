using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace MH
{
    /// <summary>
    /// 
    /// </summary>
    public static class AssetLoader
    {
        public static async UniTask<T> LoadAsync<T>(string path)
        {
            return await Addressables.LoadAssetAsync<T>(path);
        }
    }
}
