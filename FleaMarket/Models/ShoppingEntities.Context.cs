﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class ShoppingEntities : DbContext
    {
        public ShoppingEntities()
            : base("name=ShoppingEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Administrators> Administrators { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Favorite> Favorite { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<SRole> SRole { get; set; }
        public DbSet<TradeRecord> TradeRecord { get; set; }
        public DbSet<Users> Users { get; set; }
    }
}
