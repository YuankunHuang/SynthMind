using System;
using System.Threading.Tasks;
using UnityEngine;
using YuankunHuang.Unity.AssetCore;
using YuankunHuang.Unity.GameDataConfig;
using YuankunHuang.Unity.ModuleCore;
using YuankunHuang.Unity.UICore;
using YuankunHuang.Unity.CameraCore;
using YuankunHuang.Unity.AccountCore;
using YuankunHuang.Unity.NetworkCore;
using YuankunHuang.Unity.SandboxCore;
using YuankunHuang.Unity.LocalizationCore;
using YuankunHuang.Unity.FirebaseCore;
using YuankunHuang.Unity.Core.Debug;

namespace YuankunHuang.Unity.Core
{
    public class GameManager : MonoBehaviour
    {
        public AssetManagerConfig AssetManagerConfig => _assetManagerConfig;

        [Header("Modules")]
        [SerializeField] private AssetManagerConfig _assetManagerConfig;
        [SerializeField] private MonoManager _monoManager;
        [SerializeField] private InputBlocker _inputBlocker;

        private float _restartTimer = 0f;

        private void OnEnable()
        {
            Init();
        }

        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            LogHelper.LogError($"[AsyncException] Unobserved task exception:");
            foreach (var ex in e.Exception.InnerExceptions)
            {
                LogHelper.LogError($"Exception: {ex.GetType().Name} - {ex.Message}");
                LogHelper.LogError($"StackTrace: {ex.StackTrace}");
            }
            e.SetObserved();
        }

        private void OnDisable()
        {
            Dispose(null);
        }

        private void Update()
        {
            if (InputManager.GetKey(KeyCode.Escape))
            {
                if (_restartTimer > -0.1f) // only trigger once per press
                {
                    _restartTimer += Time.deltaTime;
                    if (_restartTimer >= 1f)
                    {
                        Restart();
                        _restartTimer = -1f;
                    }
                }
            }
            else
            {
                _restartTimer = 0;
            }
        }

        private void Init()
        {
            LogHelper.Log($"[GameManager]::Init");

            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            SceneManager.LoadSceneAsync(SceneKeys.UIScene, onFinished: OnUISceneReady);

            async void OnUISceneReady()
            {
                try
                {
                    GameDataManager.Initialize();
                    MonoManager.Initialize(_monoManager);
                    InputBlocker.Initialize(_inputBlocker);

                    var assetManager = new AssetManager();
                    assetManager.Initialize(_assetManagerConfig);
                    ModuleRegistry.Register<IAssetManager>(assetManager);
                    ModuleRegistry.Register<IUIManager>(new UIManager());
                    ModuleRegistry.Register<INetworkManager>(new NetworkManager());
                    ModuleRegistry.Register<ICameraManager>(new CameraManager());
                    ModuleRegistry.Register<IAccountManager>(new AccountManager());
                    ModuleRegistry.Register<ICommandManager>(new CommandManager());

                    var localizationManager = new LocalizationManager();
                    await localizationManager.InitializeAsync().WithLogging();
                    ModuleRegistry.Register<ILocalizationManager>(localizationManager);

                    var camManager = ModuleRegistry.Get<ICameraManager>();
                    camManager.AddToMainStack(camManager.UICamera);

                    ModuleRegistry.Get<IUIManager>().Show(WindowNames.LoginWindow);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError($"[GameManager] Initialization failed: {ex.Message}");
                    LogHelper.LogError($"StackTrace: {ex.StackTrace}");
                }
            }
        }

        private void Dispose(Action onFinished)
        {
            LogHelper.Log($"[GameManager]::Dispose");

            ModuleRegistry.Get<IAssetManager>().Dispose();
            ModuleRegistry.Get<IUIManager>().Dispose();
            ModuleRegistry.Get<ICameraManager>().Dispose();
            ModuleRegistry.Get<ICommandManager>().Dispose();
            ModuleRegistry.Get<ILocalizationManager>().Dispose();
            ModuleRegistry.Get<INetworkManager>().Dispose();
            ModuleRegistry.Get<IAccountManager>().Dispose();

            ModuleRegistry.Unregister<IAssetManager>();
            ModuleRegistry.Unregister<IUIManager>();
            ModuleRegistry.Unregister<ICameraManager>();
            ModuleRegistry.Unregister<ICommandManager>();
            ModuleRegistry.Unregister<ILocalizationManager>();
            ModuleRegistry.Unregister<INetworkManager>();
            ModuleRegistry.Unregister<IAccountManager>();

            MonoManager.Dispose();
            InputBlocker.Dispose();

            FirebaseManager.Dispose();

            SceneManager.UnloadAll(onFinished);

            TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
        }

        public void Restart()
        {
            LogHelper.Log($"[GameManager]::Restart");

            Dispose(() =>
            {
                Init();
            });
        }
    }
}