using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using YuankunHuang.Unity.Core;

namespace YuankunHuang.Unity.GraphicCore
{
    public class GraphicManager : IGraphicManager
    {
        public bool IsInitialized { get; private set; }

        public GraphicManager()
        {
            Initialize();
        }

        public void Initialize()
        {
            SetResolution(GraphicPreferences.Resolution);
            SetFullScreenMode(GraphicPreferences.FullScreenMode);
            SetQuality(GraphicPreferences.Quality);
            SetFPSLimit(GraphicPreferences.FPSLimit);
            SetVSync(GraphicPreferences.VSync);

            IsInitialized = true;
        }

        public void Dispose()
        {
            IsInitialized = false;
        }

        #region Interfaces
        public void SetResolution(Vector2Int resolution)
        {
            GraphicPreferences.Resolution = resolution;
            Screen.SetResolution(resolution.x, resolution.y, GraphicPreferences.FullScreenMode);
            LogHelper.Log($"[GraphicManager] SetResolution - {resolution}");
        }
        public void SetFullScreenMode(FullScreenMode mode)
        {
            GraphicPreferences.FullScreenMode = mode;
            Screen.fullScreenMode = mode;
            LogHelper.Log($"[GraphicManager] SetFullScreenMode - {mode}");
        }
        public void SetQuality(GraphicQuality quality)
        {
            GraphicPreferences.Quality = quality;
            QualitySettings.SetQualityLevel((int)quality);
            LogHelper.Log($"[GraphicManager] SetQuality - {quality}");
        }
        public void SetFPSLimit(GraphicFPSLimit fpsLimit)
        {
            GraphicPreferences.FPSLimit = fpsLimit;

            switch (fpsLimit)
            {
                case GraphicFPSLimit.FPS_30:
                    Application.targetFrameRate = 30;
                    break;
                case GraphicFPSLimit.FPS_60:
                    Application.targetFrameRate = 60;
                    break;
                case GraphicFPSLimit.FPS_Default:
                    Application.targetFrameRate = -1;
                    break;
                default:
                    LogHelper.LogError($"Undefined fps limit type: {fpsLimit}");
                    break;
            }

            LogHelper.Log($"[GraphicManager] SetFPSLimit - {fpsLimit}");
        }
        public void SetVSync(GraphicVSync vsync)
        {
            GraphicPreferences.VSync = vsync;
            QualitySettings.vSyncCount = (int)vsync;

            LogHelper.Log($"[GraphicManager] SetVSync - {vsync}");
        }
        #endregion
    }
}