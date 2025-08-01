using UnityEngine;

namespace YuankunHuang.SynthMind.Core
{
    public class InputManager
    {
        public static bool GetKey(KeyCode keyCode)
        {
            return Input.GetKey(keyCode);
        }

        public static bool GetKeyDown(KeyCode keyCode)
        {
            return Input.GetKeyDown(keyCode);
        }

        public static bool GetKeyUp(KeyCode keyCode)
        {
            return Input.GetKeyUp(keyCode);
        }
    }
}