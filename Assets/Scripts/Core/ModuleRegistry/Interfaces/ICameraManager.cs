using UnityEngine;
using YuankunHuang.Unity.ModuleCore;

namespace YuankunHuang.Unity.CameraCore
{
    public interface ICameraManager : IModule
    {
        Camera MainCamera { get; }
        Camera UICamera { get; }

        void AddToMainStack(Camera cam);
        void RemoveFromMainStack(Camera cam);
        void AddToMainStackWithOwner(object owner, Camera cam);
        void RemoveFromMainStackWithOwner(object owner);
    }
}