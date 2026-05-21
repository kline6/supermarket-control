namespace SupermarketMS.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string RealName { get; set; } = string.Empty;
    public string Role { get; set; } = "user";
    public DateTime CreateTime { get; set; } = DateTime.Now;
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; } = DateTime.Now;
}

public class Supplier
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; } = DateTime.Now;
}

public class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Unit { get; set; } = "个";
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
    public int Stock { get; set; }
    public int MinStock { get; set; } = 10;
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; } = DateTime.Now;
}

public class PurchaseOrder
{
    public int Id { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Operator { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; } = DateTime.Now;
    public List<PurchaseDetail> Details { get; set; } = new();
}

public class PurchaseDetail
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal Price { get; set; }
    public decimal Amount { get; set; }
}

public class SalesOrder
{
    public int Id { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Operator { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; } = DateTime.Now;
    public List<SalesDetail> Details { get; set; } = new();
}

public class SalesDetail
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal Price { get; set; }
    public decimal Amount { get; set; }
}

public class ReportData
{
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public int Count { get; set; }
}
