namespace NewLife.JT1078.Models;

/// <summary>存储器类型</summary>
public enum StorageKinds : Byte
{
    /// <summary>所有存储器</summary>
    All,

    /// <summary>主存储器</summary>
    Main,

    /// <summary>灾备存储器</summary>
    Backup,
}