using System.ComponentModel;

namespace WalletPayment.Domain.Enums;
/// <summary>
/// Snapshot types
/// </summary>
public enum SnapshotType
{
    [Description("روزانه")]
    Daily = 1,
    [Description("هفتگی")]
    Weekly = 2,
    [Description("ماهانه")]
    Monthly = 3,
    [Description("دستی")]
    Manual = 4,
   [Description("قبل از عملیات")]
    PreOperation = 5
}