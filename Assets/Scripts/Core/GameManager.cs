using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using YuankunHuang.SynthMind.GameDataConfig;

namespace YuankunHuang.SynthMind.Core
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

            GameDataManager.Initialize();

            ModuleRegistry.Register<IUIManager>(new UIManager());
            ModuleRegistry.Register<ICameraManager>(new CameraManager());
            ModuleRegistry.Register<IAccountManager>(new AccountManager());
            ModuleRegistry.Register<INetworkManager>(new NetworkManager());
            ModuleRegistry.Register<ICommandManager>(new CommandManager());

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
            ModuleRegistry.Get<INetworkManager>().Dispose();
            ModuleRegistry.Get<ICommandManager>().Dispose();
            ModuleRegistry.Get<IAccountManager>().Dispose();

            ModuleRegistry.Unregister<IUIManager>();
            ModuleRegistry.Unregister<ICameraManager>();
            ModuleRegistry.Unregister<INetworkManager>();
            ModuleRegistry.Unregister<ICommandManager>();
            ModuleRegistry.Unregister<IAccountManager>();

            SceneManager.UnloadAll(onFinished);
            FirebaseManager.Dispose();
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