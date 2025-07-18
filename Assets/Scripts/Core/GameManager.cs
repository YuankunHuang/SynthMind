using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using YuankunHuang.Unity.GameDataConfig;

namespace YuankunHuang.Unity.Core
{
    public class GameManager : MonoBehaviour
    {
        private void OnEnable()
        {
            Init();
        }

        private void OnDisable()
        {
            Dispose(null);
        }

        private static void Init()
        {
            LogHelper.Log($"[GameManager]::Init");

            ModuleRegistry.Register<IUIManager>(new UIManager());
            ModuleRegistry.Register<ICameraManager>(new CameraManager());
            ModuleRegistry.Register<IAccountManager>(new AccountManager());
            ModuleRegistry.Register<INetworkManager>(new NetworkManager());

            GameDataManager.Initialize();

            SceneManager.LoadSceneAsync(SceneKeys.UIScene, onFinished: () =>
            {
                var camManager = ModuleRegistry.Get<ICameraManager>();
                camManager.AddToMainStack(camManager.UICamera);

                ModuleRegistry.Get<IUIManager>().ShowStackableWindow(WindowNames.LoginWindow);
            });
        }

        private static void Dispose(Action onFinished)
        {
            LogHelper.Log($"[GameManager]::Dispose");

            ModuleRegistry.Get<IUIManager>().Dispose();
            ModuleRegistry.Get<ICameraManager>().Dispose();
            ModuleRegistry.Get<IAccountManager>().Dispose();
            ModuleRegistry.Get<INetworkManager>().Dispose();

            ModuleRegistry.Unregister<IUIManager>();
            ModuleRegistry.Unregister<ICameraManager>();
            ModuleRegistry.Unregister<IAccountManager>();
            ModuleRegistry.Unregister<INetworkManager>();

            SceneManager.UnloadAll(onFinished);
        }

        public static void Restart()
        {
            LogHelper.Log($"[GameManager]::Restart");

            Dispose(() =>
            {
                Init();
            });
        }
    }
}