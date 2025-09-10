namespace YuankunHuang.Unity.GameDataConfig
{
    /// <summary>
    /// Field types for binary data reading
    /// </summary>
    public enum FieldType
    {
        String,
        Int,
        Long,
        Float,
        Bool,
        DateTime,
        Enum
    }

    public enum AudioGroupType
    {
        BGM = 1,
        SFX = 2,
        UI = 3,
    }

    public enum AudioIdType
    {
        TestButtonClick = 1,
        TestBGM = 2,
    }

    public enum SampleType
    {
        Warrior = 1,
        Mage = 2,
    }

}
