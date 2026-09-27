using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Threading.Tasks;

namespace aki_lua87.AssetsListGenerator
{
    internal static class QRCodeCache
    {
        private const string CacheDir = "Assets/aki_lua87/AssetsListGenerator/img_cache";

        private static string GetCachePath(string url)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(url));
            return $"{CacheDir}/{System.BitConverter.ToString(hash).Replace("-", "").ToLower()}.png";
        }

        private static void EnsureCacheDir()
        {
            var fullPath = System.IO.Path.GetFullPath(CacheDir);
            if (!System.IO.Directory.Exists(fullPath))
            {
                System.IO.Directory.CreateDirectory(fullPath);
                AssetDatabase.Refresh();
            }
        }

        public static async Task<Sprite> GetOrCreateQRSprite(string url)
        {
            var cachePath = GetCachePath(url);

            var cachedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(cachePath);
            if (cachedSprite != null)
            {
                Debug.Log($"QR cache hit: {cachePath}");
                return cachedSprite;
            }

            string apiUrl = $"https://api.akakitune87.net/qrcode/generate?content={url}";
            var client = new System.Net.Http.HttpClient();
            var res = await client.GetAsync(apiUrl);
            var base64 = await res.Content.ReadAsStringAsync();

            EnsureCacheDir();
            System.IO.File.WriteAllBytes(System.IO.Path.GetFullPath(cachePath), System.Convert.FromBase64String(base64));
            AssetDatabase.ImportAsset(cachePath);

            var importer = AssetImporter.GetAtPath(cachePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.maxTextureSize = 512;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(cachePath);
        }
    }
}
