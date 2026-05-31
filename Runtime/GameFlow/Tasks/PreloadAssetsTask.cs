using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBase.Asset.Core;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProjectBase.GameFlow.Tasks
{
    public class PreloadAssetsTask : IBootstrapTask
    {
        private readonly IReadOnlyList<AssetReference> _assets;

        public string Name => "Loading Assets";
        public float Weight { get; }

        public PreloadAssetsTask(IReadOnlyList<AssetReference> assets, float weight = 0.5f)
        {
            _assets = assets;
            Weight = weight;
        }

        public async UniTask Execute(IProgress<float> progress, CancellationToken ct)
        {
            if (_assets == null || _assets.Count == 0)
            {
                progress?.Report(1f);
                return;
            }

            for (int i = 0; i < _assets.Count; i++)
            {
                await AssetLoader.Instance.LoadAssetAsync<UnityEngine.Object>(_assets[i], ct);
                progress?.Report((float)(i + 1) / _assets.Count);
            }
        }
    }
}
