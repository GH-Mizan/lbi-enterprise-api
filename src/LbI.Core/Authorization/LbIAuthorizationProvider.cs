using Abp.Authorization;
using Abp.Localization;
using Abp.MultiTenancy;

namespace LbI.Authorization;

public class LbIAuthorizationProvider : AuthorizationProvider
{
    public override void SetPermissions(IPermissionDefinitionContext context)
    {
        context.CreatePermission(PermissionNames.Pages_Users, L("Users"));
        context.CreatePermission(PermissionNames.Pages_Users_Activation, L("UsersActivation"));
        context.CreatePermission(PermissionNames.Pages_Roles, L("Roles"));
        context.CreatePermission(PermissionNames.Pages_Tenants, L("Tenants"), multiTenancySides: MultiTenancySides.Host);
        context.CreatePermission(PermissionNames.Pages_Purchase_Edit, L("PurchaseEdit"));
        context.CreatePermission(PermissionNames.Pages_Purchase_Delete, L("PurchaseDelete"));
        context.CreatePermission(PermissionNames.Pages_Sale_Edit, L("SaleEdit"));
        context.CreatePermission(PermissionNames.Pages_Sale_Delete, L("SaleDelete"));
        context.CreatePermission(PermissionNames.Pages_DailyCash, L("DailyCash"));
        context.CreatePermission(PermissionNames.Pages_Product_Edit, L("ProductEdit"));
        context.CreatePermission(PermissionNames.Pages_Client_Edit, L("ClientEdit"));
        context.CreatePermission(PermissionNames.Pages_Supplier_Edit, L("SupplierEdit"));
        context.CreatePermission(PermissionNames.Pages_Employee_Edit, L("EmployeeEdit"));
        context.CreatePermission(PermissionNames.Pages_Department_Edit, L("DepartmentEdit"));
        context.CreatePermission(PermissionNames.Pages_Designation_Edit, L("DesignationEdit"));
        context.CreatePermission(PermissionNames.Pages_StockPoint_Edit, L("StockPointEdit"));
        context.CreatePermission(PermissionNames.Pages_Salary, L("Salary"));
        context.CreatePermission(PermissionNames.Pages_SalaryAdvance, L("SalaryAdvance"));

        context.CreatePermission(PermissionNames.Reports_SaleCollectionDueReport, L("SaleCollectionDueReport"));
        context.CreatePermission(PermissionNames.Reports_DailyPurchaseReport, L("DailyPurchaseReport"));
        context.CreatePermission(PermissionNames.Reports_DailySalesReport, L("DailySalesReport"));
        context.CreatePermission(PermissionNames.Reports_ClientsLedgerReport, L("ClientsLedgerReport"));
        context.CreatePermission(PermissionNames.Reports_ClientsDueReport, L("ClientsDueReport"));
        context.CreatePermission(PermissionNames.Reports_ClientsBalanceReport, L("ClientsBalanceReport"));
        context.CreatePermission(PermissionNames.Reports_MonthlySalesRankingReport, L("MonthlySalesRankingReport"));
        context.CreatePermission(PermissionNames.Reports_SalesInvoiceReport, L("SalesInvoiceReport"));
        context.CreatePermission(PermissionNames.Reports_BuySaleDiff, L("BuySaleDiff"));
    }

    private static ILocalizableString L(string name)
    {
        return new LocalizableString(name, LbIConsts.LocalizationSourceName);
    }
}
