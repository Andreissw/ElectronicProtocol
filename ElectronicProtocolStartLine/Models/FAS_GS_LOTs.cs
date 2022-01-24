
//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------


namespace ElectronicProtocolStartLine.Models
{

using System;
    using System.Collections.Generic;
    
public partial class FAS_GS_LOTs
{

    public short LOTID { get; set; }

    public short LOTCode { get; set; }

    public string FULL_LOT_Code { get; set; }

    public string Specification { get; set; }

    public short ModelID { get; set; }

    public bool IsActive { get; set; }

    public Nullable<bool> IsHDCPUpload { get; set; }

    public Nullable<bool> IsCertUpload { get; set; }

    public Nullable<bool> IsMACUpload { get; set; }

    public Nullable<byte> WorkingScenarioID { get; set; }

    public Nullable<byte> LabelScenarioID { get; set; }

    public Nullable<int> LiterIndex { get; set; }

    public Nullable<int> BoxCapacity { get; set; }

    public Nullable<int> PalletCapacity { get; set; }

    public string Manufacturer { get; set; }

    public string Operator { get; set; }

    public string MarketID { get; set; }

    public string PTID { get; set; }

    public Nullable<bool> ModelCheck { get; set; }

    public Nullable<bool> SWRead { get; set; }

    public Nullable<bool> SWGS1Read { get; set; }

    public System.DateTime CreateDate { get; set; }

    public short CreateByID { get; set; }

    public Nullable<System.DateTime> CloseDate { get; set; }

    public Nullable<short> CloseByID { get; set; }

    public Nullable<bool> Fixed_Range { get; set; }

    public Nullable<System.DateTime> Fixed_Range_Date { get; set; }

    public Nullable<bool> GetWeight { get; set; }

    public Nullable<int> RangeStart { get; set; }

    public Nullable<int> RangeEnd { get; set; }

    public Nullable<bool> FixedRG { get; set; }

    public Nullable<System.DateTime> StartDate { get; set; }

    public string SWVersion { get; set; }

    public Nullable<bool> IsBunch { get; set; }

    public Nullable<bool> IsWeighingPackage { get; set; }

    public Nullable<bool> FakePackage { get; set; }

    public Nullable<int> FakeBoxNum { get; set; }

    public Nullable<int> FakePalletNum { get; set; }

    public Nullable<int> CountinLot { get; set; }

    public Nullable<System.DateTime> DateManufacter { get; set; }



    public virtual FAS_Users FAS_Users { get; set; }

    public virtual FAS_Users FAS_Users1 { get; set; }

    public virtual FAS_Models FAS_Models { get; set; }

}

}
