using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using YuankunHuang.Unity.Core;

namespace YuankunHuang.Unity.CameraCore
{
    public class CameraManager : ICameraManager
    {
        public bool IsInitialized { get; private set; } = false;

        public Camera MainCamera
        {
            get
            {
                if (_mainCam == null)
                {
                    var obj = GameObject.FindGameObjectWithTag(TagNames.MainCamera);
                    if (obj != null) 
                    {
                        _mainCam = obj.GetComponent<Camera>();
                    }
                }
                return _mainCam;
            }
        }
        public Camera UICamera
        {
            get
            {
                if (_uiCam == null)
                {
                    var obj = GameObject.FindGameObjectWithTag(TagNames.UICamera);
                    if (obj != null)
                    {
                        _uiCam = obj.GetComponent<Camera>();
                    }
                }
                return _uiCam;
            }
        }

        private Camera _mainCam;
        private Camera _uiCam;

        private Dictionary<object, List<Camera>> _camOwnerDict = new Dictionary<object, List<Camera>>();

        private static readonly object HomelessCamId = new object();

        public CameraManager()
        {
            IsInitialized = true;
            LogHelper.Log($"CameraManager initialized");
        }

        public void AddToMainStack(Camera cam)
        {
            if (cam == null)
            {
                return;
            }

            if (!_camOwnerDict.TryGetValue(HomelessCamId, out var list))
            {
                list = new List<Camera>();
                _camOwnerDict[HomelessCamId] = list;
            }
            if (!list.Contains(cam))
            {
                list.Add(cam);
            }

            var universalCamData = MainCamera.GetUniversalAdditionalCameraData();
            if (!universalCamData.cameraStack.Contains(cam))
            {
                universalCamData.cameraStack.Add(cam);
            }
        }

        public void RemoveFromMainStack(Camera cam)
        {
            if (cam == null)
            {
                return;
            }

            if (_camOwnerDict.TryGetValue(HomelessCamId, out var list))
            {
                list.Remove(cam);
            }
            var universalCamData = MainCamera.GetUniversalAdditionalCameraData();
            universalCamData.cameraStack.Remove(cam);
        }

        public void AddToMainStackWithOwner(object owner, Camera cam)
        {
            if (owner == null)
            {
                if (cam == null)
                {
                    return;
                }

                LogHelper.LogError($"Homeless camera! {cam.name}");
                _camOwnerDict[HomelessCamId] = new List<Camera>() { cam };
            }
            else
            {
                if (!_camOwnerDict.TryGetValue(owner, out var list))
                {
                    list = new List<Camera>();
                    _camOwnerDict[owner] = list;
                }
                list.Add(cam);
            }

            var universalCamData = MainCamera.GetUniversalAdditionalCameraData();
            universalCamData.cameraStack.Add(cam);
        }

        public void RemoveFromMainStackWithOwner(object owner)
        {
            if (owner == null)
            {
                return;
            }

            if (_camOwnerDict.TryGetValue(owner, out var list))
            {
                var universalCamData = MainCamera.GetUniversalAdditionalCameraData();

                for (var i = 0; i < list.Count; ++i)
                {
                    var cam = list[i];
                    universalCamData.cameraStack.Remove(cam);
                }
                _camOwnerDict.Remove(owner);
            }
        }

        public void Dispose()
        {
            if (MainCamera != null)
            {
                var universalCamData = MainCamera.GetUniversalAdditionalCameraData();
                universalCamData.cameraStack.Clear();
            }

            _camOwnerDict.Clear();
            _mainCam = null;
            _uiCam = null;

            IsInitialized = false;
        }
    }
}