namespace YuankunHuang.SynthMind.Core
{
    public class EventKeys
    {
        private static int EventId = 0;

        public static int GetUniqueEventKey()
        {
            return ++EventId;
        }
    }
}