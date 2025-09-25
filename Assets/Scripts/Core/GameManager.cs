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
using YuankunHuang.Unity.AudioCore;
using YuankunHuang.Unity.GraphicCore;

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
                    LogHelper.Log("[GameManager] Starting GameDataManager initialization...");
#if UNITY_WEBGL && !UNITY_EDITOR
                    await YuankunHuang.Unity.GameDataConfig.GameDataManager.InitializeWebGLAsync();
                    LogHelper.Log("[GameManager] GameDataManager initialized successfully (WebGL async)");
#else
                    YuankunHuang.Unity.GameDataConfig.GameDataManager.Initialize();
                    LogHelper.Log("[GameManager] GameDataManager initialized successfully");
#endif

                    LogHelper.Log("[GameManager] Starting MonoManager initialization...");
                    MonoManager.Initialize(_monoManager);
                    LogHelper.Log("[GameManager] MonoManager initialized successfully");

                    LogHelper.Log("[GameManager] Starting InputBlocker initialization...");
                    InputBlocker.Initialize(_inputBlocker);
                    LogHelper.Log("[GameManager] InputBlocker initialized successfully");

                    LogHelper.Log("[GameManager] Starting AssetManager initialization...");
                    var assetManager = new AssetManager();
                    assetManager.Initialize(_assetManagerConfig);
                    ModuleRegistry.Register<IAssetManager>(assetManager);
                    LogHelper.Log("[GameManager] AssetManager initialized successfully");

                    LogHelper.Log("[GameManager] Starting UIManager initialization...");
                    ModuleRegistry.Register<IUIManager>(new UIManager());
                    LogHelper.Log("[GameManager] UIManager initialized successfully");

                    LogHelper.Log("[GameManager] Starting NetworkManager initialization...");
                    ModuleRegistry.Register<INetworkManager>(new NetworkManager());
                    LogHelper.Log("[GameManager] NetworkManager initialized successfully");

                    LogHelper.Log("[GameManager] Starting CameraManager initialization...");
                    ModuleRegistry.Register<ICameraManager>(new CameraManager());
                    LogHelper.Log("[GameManager] CameraManager initialized successfully");

                    LogHelper.Log("[GameManager] Starting AccountManager initialization...");
                    ModuleRegistry.Register<IAccountManager>(new AccountManager());
                    LogHelper.Log("[GameManager] AccountManager initialized successfully");

                    LogHelper.Log("[GameManager] Starting CommandManager initialization...");
                    ModuleRegistry.Register<ICommandManager>(new CommandManager());
                    LogHelper.Log("[GameManager] CommandManager initialized successfully");

                    LogHelper.Log("[GameManager] Starting AudioManager initialization...");
                    ModuleRegistry.Register<IAudioManager>(new AudioManager());
                    LogHelper.Log("[GameManager] AudioManager initialized successfully");

                    LogHelper.Log("[GameManager] Starting GraphicManager initialization...");
                    ModuleRegistry.Register<IGraphicManager>(new GraphicManager());
                    LogHelper.Log("[GameManager] GraphicManager initialized successfully");

                    LogHelper.Log("[GameManager] Starting LocalizationManager initialization...");
                    var localizationManager = new LocalizationManager();
                    await localizationManager.InitializeAsync().WithLogging();
                    ModuleRegistry.Register<ILocalizationManager>(localizationManager);
                    LogHelper.Log("[GameManager] LocalizationManager initialized successfully");

                    LogHelper.Log("[GameManager] Starting FirebaseManager initialization...");
                    var firebaseManager = new UnifiedFirebaseManager();
                    try
                    {
                        await firebaseManager.InitializeAsync().WithLogging();
                        ModuleRegistry.Register<IFirebaseManager>(firebaseManager);
                        LogHelper.Log($"[GameManager] Firebase initialized: {firebaseManager.IsInitialized}");
                    }
                    catch (Exception firebaseEx)
                    {
                        LogHelper.LogWarning($"[GameManager] Firebase initialization failed, continuing without Firebase: {firebaseEx.Message}");
                        ModuleRegistry.Register<IFirebaseManager>(firebaseManager);
                    }

                    LogHelper.Log("[GameManager] Starting Camera setup...");
                    var camManager = ModuleRegistry.Get<ICameraManager>();
                    camManager.AddToMainStack(camManager.UICamera);
                    LogHelper.Log("[GameManager] Camera setup completed successfully");

                    LogHelper.Log("[GameManager] Starting UI window show...");
                    ModuleRegistry.Get<IUIManager>().Show(WindowNames.LoginWindow);
                    LogHelper.Log("[GameManager] All initialization completed successfully");
                }
                catch (Exception ex)
                {
                    LogHelper.LogError($"[GameManager] Initialization failed: {ex.Message}");
                    LogHelper.LogError($"StackTrace: {ex.StackTrace}");
                    LogHelper.LogError($"InnerException: {ex.InnerException?.Message}");
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
            ModuleRegistry.Get<IAudioManager>().Dispose();
            ModuleRegistry.Get<IGraphicManager>().Dispose();
            ModuleRegistry.Get<IFirebaseManager>().Dispose();

            ModuleRegistry.Unregister<IAssetManager>();
            ModuleRegistry.Unregister<IUIManager>();
            ModuleRegistry.Unregister<ICameraManager>();
            ModuleRegistry.Unregister<ICommandManager>();
            ModuleRegistry.Unregister<ILocalizationManager>();
            ModuleRegistry.Unregister<INetworkManager>();
            ModuleRegistry.Unregister<IAccountManager>();
            ModuleRegistry.Unregister<IAudioManager>();
            ModuleRegistry.Unregister<IGraphicManager>();
            ModuleRegistry.Unregister<IFirebaseManager>();

            MonoManager.Dispose();
            InputBlocker.Dispose();

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