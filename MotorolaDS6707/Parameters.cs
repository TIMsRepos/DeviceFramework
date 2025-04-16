namespace TIM.Devices.MotorolaDS6707
{
    public enum Parameters
    {
        TriggerMode = 0x8a,
        BeepAfterGoodDecode = 0x38
    }

    public enum TriggerModes
    {
        Level = 0x00,
        Blink = 0x07,
        Host = 0x08,
        AutoAim = 0x09
    }
}