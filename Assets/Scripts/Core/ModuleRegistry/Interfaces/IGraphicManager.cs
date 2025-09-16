using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YuankunHuang.Unity.ModuleCore;

namespace YuankunHuang.Unity.GraphicCore
{
    public enum GraphicFPSLimit
    {
        FPS_Default = 0,
        FPS_30 = 1,
        FPS_60 = 2,
    }

    public enum GraphicQuality
    {
        VeryLow = 0,
        Low = 1,
        Medium = 2,
        High = 3,
        VeryHigh = 4,
        Ultra = 5,
    }

    public enum GraphicVSync
    {
        Off = 0,
        EveryFrame = 1,
        EveryTwoFrames = 2,
    }

    public interface IGraphicManager : IModule
    {
        void SetResolution(Vector2Int resolution);
        void SetFullScreenMode(FullScreenMode mode);
        void SetQuality(GraphicQuality quality);
        void SetFPSLimit(GraphicFPSLimit limit);
        void SetVSync(GraphicVSync vsync);
    }
}