//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace FleaMarket.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class TradeRecord
    {
        public int TraID { get; set; }
        public int TraPID { get; set; }
        public int UserBuyID { get; set; }
        public int UserSellID { get; set; }
        public string TraComment { get; set; }
        public Nullable<System.DateTime> TraBuyTime { get; set; }
    
        public virtual Product Product { get; set; }
        public virtual Users Users { get; set; }
        public virtual Users Users1 { get; set; }
    }
}
