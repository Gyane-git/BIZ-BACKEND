namespace BIZ.Infrastructure.Authorization;

public static class SystemPermissionCatalog
{
    public static readonly IReadOnlyList<(string Code, string Name, string Description)> All =
    [
        ("PERM_DASHBOARD", "Dashboard", "View dashboard"),
        ("PERM_COMPANY_MASTER", "Company Master", "Manage companies and tenant databases"),
        ("PERM_USER_MANAGEMENT", "User Management", "Create and manage users"),
        ("PERM_ROLE_MANAGEMENT", "Role Management", "Create and manage roles"),
        ("PERM_PERMISSION_MANAGEMENT", "Permission Management", "Create and assign permissions"),
        ("PERM_BRANCH_MASTER", "Branch Master", "Manage branches"),
        ("PERM_COMPANY_UNIT_MASTER", "Company Unit Master", "Manage company units"),
        ("PERM_FISCAL_YEAR_MASTER", "Fiscal Year Master", "Manage fiscal years"),
        ("PERM_FISCAL_PERIOD_MASTER", "Fiscal Period Master", "Manage fiscal year periods"),
        ("PERM_WAREHOUSE_MASTER", "Warehouse Master", "Manage warehouses and locations"),
        ("PERM_PRODUCT_MASTER", "Product Master", "Manage products and classifications"),
        ("PERM_PRODUCT_OPENING", "Product Opening", "Manage opening stock"),
        ("PERM_CUSTOMER_MASTER", "Customer Master", "Manage customers"),
        ("PERM_SUPPLIER_MASTER", "Supplier Master", "Manage suppliers"),
        ("PERM_CHART_OF_ACCOUNTS", "Chart of Accounts", "Manage ledger accounts"),
        ("PERM_JOURNAL_VOUCHER", "Journal Voucher", "Create and manage journal vouchers"),
        ("PERM_SALES_INVOICE", "Sales Invoice", "Create and manage sales invoices"),
        ("PERM_PURCHASE_INVOICE", "Purchase Invoice", "Create and manage purchase invoices"),
        ("PERM_STOCK_TRANSFER", "Stock Transfer", "Create and manage stock transfers"),
        ("PERM_REPORTS", "Reports", "View business reports"),
        ("PERM_FINANCE", "Finance", "Access finance module"),
        ("PERM_SALES", "Sales", "Access sales module"),
        ("PERM_PURCHASE", "Purchase", "Access purchase module"),
        ("PERM_INVENTORY", "Inventory", "Access inventory module"),
        ("PERM_FIXED_ASSET", "Fixed Asset", "Access fixed asset module"),
        ("PERM_SMART_SALES", "Smart Sales", "Access smart sales module"),
        ("PERM_AUTO_MOBILE", "Auto Mobile", "Access auto mobile module"),
        ("PERM_DOC_MANAGEMENT", "Document Management", "Access document management module"),
        ("PERM_UTILITY", "Utility", "Access utility module"),
        ("PERM_USER_MENU", "User", "Access user module"),
        ("PERM_TASK", "Task", "Access task module"),
        ("PERM_HELP", "Help", "Access help module")
    ];
}
