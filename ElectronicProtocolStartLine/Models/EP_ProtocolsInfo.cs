
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
    
public partial class EP_ProtocolsInfo
{

    public int ID { get; set; }

    public int ProtocolID { get; set; }

    public int TypeVerifID { get; set; }

    public Nullable<System.DateTime> DateCreate { get; set; }

    public Nullable<bool> Signature { get; set; }

    public Nullable<short> UserID { get; set; }

    public string Result { get; set; }

    public string TOPBOT { get; set; }

    public Nullable<bool> Visible { get; set; }

    public string Description { get; set; }

    public Nullable<short> Itter { get; set; }



    public virtual EP_TypeVerification EP_TypeVerification { get; set; }

    public virtual FAS_Users FAS_Users { get; set; }

    public virtual EP_Protocols EP_Protocols { get; set; }

}

}
