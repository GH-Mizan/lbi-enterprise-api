using Abp.Zero.EntityFrameworkCore;
using AutoMapper.Execution;
using LbI.Authorization.Roles;
using LbI.Authorization.Users;
using LbI.Entities;
using LbI.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace LbI.EntityFrameworkCore;

public class LbIDbContext : AbpZeroDbContext<Tenant, Role, User, LbIDbContext>
{
    /* Define a DbSet for each entity of the application */

    public LbIDbContext(DbContextOptions<LbIDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductHistory> ProductHistories { get; set; }
    public DbSet<StockPoint> StockPoints { get; set; }
    public DbSet<DailyCash> DailyCashes { get; set; }
    public DbSet<Purchase> Purchases { get; set; }
    public DbSet<PurchaseDetail> PurchaseDetails { get; set; }
    public DbSet<DuePaymentHistory> DuePaymentHistories { get; set; }
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleDetail> SalesDetials { get; set; }
    public DbSet<DueReceivedHistory> DueReceivedHistories { get; set; }
    public DbSet<LbiSetting> LbiSettings { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Designation> Designations { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<ProductTransfer> ProductTransfers { get; set; }
    public DbSet<CustomerPrice> CustomerPrices { get; set; }

    public DbSet<VirtualItem> VirtualItems { get; set; }
    public DbSet<VirtualInventory> VirtualInventories { get; set; }
    public DbSet<VirtualStock> VirtualStocks { get; set; }
    public DbSet<VirtualStockDetail> VirtualStockDetails { get; set; }

    public DbSet<SalesOrder> SalesOrders { get; set; }

    public DbSet<Salary> Salarys { get; set; }
    public DbSet<SalaryAdvance> SalaryAdvances { get; set; }
    public DbSet<SalaryAdvanceHistory> SalaryAdvanceHistories { get; set; }

}
