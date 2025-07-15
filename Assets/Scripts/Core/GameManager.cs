using UnityEngine;
using UnityEngine.AddressableAssets;

namespace YuankunHuang.Unity.Core
{
    public class GameManager : MonoBehaviour
    {
        private void OnEnable()
        {
            ModuleRegistry.Register<IUIManager>(new UIManager());
            ModuleRegistry.Register<ICameraManager>(new CameraManager());

            SceneManager.LoadSceneAsync(SceneKeys.UIScene, onFinished: () =>
            {
                var camManager = ModuleRegistry.Get<ICameraManager>();
                camManager.AddToMainStack(camManager.UICamera);

                ModuleRegistry.Get<IUIManager>().ShowStackableWindow(WindowNames.MainMenu);
            });
        }

        private void OnDisable()
        {
            ModuleRegistry.Unregister<IUIManager>();
            ModuleRegistry.Unregister<ICameraManager>();
        }
    }
}