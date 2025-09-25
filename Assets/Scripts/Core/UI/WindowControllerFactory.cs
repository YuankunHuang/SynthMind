using System;
using System.Collections.Generic;
using YuankunHuang.Unity.Core.Debug;
using YuankunHuang.Unity.HotUpdate;
using YuankunHuang.Unity.Core;

namespace YuankunHuang.Unity.UICore
{
    /// <summary>
    /// WebGL-compatible window controller factory that avoids reflection
    /// </summary>
    public static class WindowControllerFactory
    {
        private static readonly Dictionary<string, Func<WindowControllerBase>> _controllerFactories =
            new Dictionary<string, Func<WindowControllerBase>>
            {
                { "LoginWindow", () => new LoginWindowController() },
                { "MainMenu", () => new MainMenuController() },
                { "ProfileWindow", () => new ProfileWindowController() },
                { "InfoWindow", () => new InfoWindowController() },
                { "ConfirmWindow", () => new ConfirmWindowController() },
            };

        public static WindowControllerBase CreateController(string windowName)
        {
            LogHelper.Log($"[WindowControllerFactory] Creating controller for {windowName}");

            if (_controllerFactories.TryGetValue(windowName, out var factory))
            {
                try
                {
                    var controller = factory();
                    LogHelper.Log($"[WindowControllerFactory] Successfully created {windowName}Controller");
                    return controller;
                }
                catch (Exception ex)
                {
                    LogHelper.LogError($"[WindowControllerFactory] Failed to create {windowName}Controller: {ex.Message}");
                    throw;
                }
            }

            LogHelper.LogError($"[WindowControllerFactory] Controller not found for window: {windowName}");
            throw new Exception($"Controller not found: {windowName}Controller");
        }
    }
}