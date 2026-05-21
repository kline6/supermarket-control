using System.Data;
using Microsoft.Data.Sqlite;
using SupermarketMS.DAL;
using SupermarketMS.Models;

namespace SupermarketMS.BLL;

public class UserBLL
{
    public static User? Login(string username, string password)
    {
        var sql = "SELECT * FROM Users WHERE Username = @u AND Password = @p";
        var dt = DBHelper.ExecuteQuery(sql,
            new SqliteParameter("@u", username),
            new SqliteParameter("@p", password));
        if (dt.Rows.Count == 0) return null;
        var row = dt.Rows[0];
        return new User
        {
            Id = Convert.ToInt32(row["Id"]),
            Username = row["Username"].ToString()!,
            RealName = row["RealName"].ToString()!,
            Role = row["Role"].ToString()!
        };
    }

    public static DataTable GetAllUsers() =>
        DBHelper.ExecuteQuery("SELECT Id, Username, RealName, Role, CreateTime FROM Users");

    public static int AddUser(User u) =>
        DBHelper.ExecuteNonQuery(
            "INSERT INTO Users (Username, Password, RealName, Role) VALUES (@u,@p,@r,@o)",
            new SqliteParameter("@u", u.Username),
            new SqliteParameter("@p", u.Password),
            new SqliteParameter("@r", u.RealName),
            new SqliteParameter("@o", u.Role));

    public static int DeleteUser(int id) =>
        DBHelper.ExecuteNonQuery("DELETE FROM Users WHERE Id = @id AND Username != 'admin'",
            new SqliteParameter("@id", id));
}

public class CategoryBLL
{
    public static DataTable GetAll() =>
        DBHelper.ExecuteQuery("SELECT * FROM Categories ORDER BY Id");

    public static int Add(Category c) =>
        DBHelper.ExecuteNonQuery(
            "INSERT INTO Categories (Name, Description) VALUES (@n, @d)",
            new SqliteParameter("@n", c.Name),
            new SqliteParameter("@d", c.Description));

    public static int Update(Category c) =>
        DBHelper.ExecuteNonQuery(
            "UPDATE Categories SET Name = @n, Description = @d WHERE Id = @id",
            new SqliteParameter("@n", c.Name),
            new SqliteParameter("@d", c.Description),
            new SqliteParameter("@id", c.Id));

    public static int Delete(int id) =>
        DBHelper.ExecuteNonQuery("DELETE FROM Categories WHERE Id = @id",
            new SqliteParameter("@id", id));
}

public class SupplierBLL
{
    public static DataTable GetAll() =>
        DBHelper.ExecuteQuery("SELECT * FROM Suppliers ORDER BY Id");

    public static DataTable Search(string keyword) =>
        DBHelper.ExecuteQuery(
            "SELECT * FROM Suppliers WHERE Name LIKE @k OR ContactPerson LIKE @k OR Phone LIKE @k ORDER BY Id",
            new SqliteParameter("@k", $"%{keyword}%"));

    public static int Add(Supplier s) =>
        DBHelper.ExecuteNonQuery(
            "INSERT INTO Suppliers (Name, ContactPerson, Phone, Address) VALUES (@n,@c,@p,@a)",
            new SqliteParameter("@n", s.Name),
            new SqliteParameter("@c", s.ContactPerson),
            new SqliteParameter("@p", s.Phone),
            new SqliteParameter("@a", s.Address));

    public static int Update(Supplier s) =>
        DBHelper.ExecuteNonQuery(
            "UPDATE Suppliers SET Name=@n, ContactPerson=@c, Phone=@p, Address=@a WHERE Id=@id",
            new SqliteParameter("@n", s.Name),
            new SqliteParameter("@c", s.ContactPerson),
            new SqliteParameter("@p", s.Phone),
            new SqliteParameter("@a", s.Address),
            new SqliteParameter("@id", s.Id));

    public static int Delete(int id) =>
        DBHelper.ExecuteNonQuery("DELETE FROM Suppliers WHERE Id = @id",
            new SqliteParameter("@id", id));
}

public class ProductBLL
{
    public static DataTable GetAll() =>
        DBHelper.ExecuteQuery(@"
            SELECT p.*, c.Name AS CategoryName, s.Name AS SupplierName
            FROM Products p
            LEFT JOIN Categories c ON p.CategoryId = c.Id
            LEFT JOIN Suppliers s ON p.SupplierId = s.Id
            ORDER BY p.Id");

    public static DataTable Search(string keyword) =>
        DBHelper.ExecuteQuery(@"
            SELECT p.*, c.Name AS CategoryName, s.Name AS SupplierName
            FROM Products p
            LEFT JOIN Categories c ON p.CategoryId = c.Id
            LEFT JOIN Suppliers s ON p.SupplierId = s.Id
            WHERE p.Name LIKE @k OR p.Code LIKE @k OR p.Barcode LIKE @k
            ORDER BY p.Id",
            new SqliteParameter("@k", $"%{keyword}%"));

    public static DataTable GetLowStock() =>
        DBHelper.ExecuteQuery(@"
            SELECT p.*, c.Name AS CategoryName, s.Name AS SupplierName
            FROM Products p
            LEFT JOIN Categories c ON p.CategoryId = c.Id
            LEFT JOIN Suppliers s ON p.SupplierId = s.Id
            WHERE p.Stock <= p.MinStock
            ORDER BY p.Stock");

    public static Product? GetById(int id)
    {
        var dt = DBHelper.ExecuteQuery(
            "SELECT p.*, c.Name AS CategoryName, s.Name AS SupplierName FROM Products p LEFT JOIN Categories c ON p.CategoryId=c.Id LEFT JOIN Suppliers s ON p.SupplierId=s.Id WHERE p.Id=@id",
            new SqliteParameter("@id", id));
        if (dt.Rows.Count == 0) return null;
        return RowToProduct(dt.Rows[0]);
    }

    public static Product? GetByBarcode(string barcode)
    {
        var dt = DBHelper.ExecuteQuery(
            "SELECT p.*, c.Name AS CategoryName, s.Name AS SupplierName FROM Products p LEFT JOIN Categories c ON p.CategoryId=c.Id LEFT JOIN Suppliers s ON p.SupplierId=s.Id WHERE p.Barcode=@b",
            new SqliteParameter("@b", barcode));
        if (dt.Rows.Count == 0) return null;
        return RowToProduct(dt.Rows[0]);
    }

    private static Product RowToProduct(DataRow r) => new()
    {
        Id = Convert.ToInt32(r["Id"]),
        Code = r["Code"].ToString()!,
        Name = r["Name"].ToString()!,
        CategoryId = r["CategoryId"] == DBNull.Value ? 0 : Convert.ToInt32(r["CategoryId"]),
        CategoryName = r["CategoryName"].ToString()!,
        Unit = r["Unit"].ToString()!,
        Price = Convert.ToDecimal(r["Price"]),
        Cost = Convert.ToDecimal(r["Cost"]),
        Stock = Convert.ToInt32(r["Stock"]),
        MinStock = Convert.ToInt32(r["MinStock"]),
        SupplierId = r["SupplierId"] == DBNull.Value ? 0 : Convert.ToInt32(r["SupplierId"]),
        SupplierName = r["SupplierName"].ToString()!,
        Barcode = r["Barcode"].ToString()!
    };

    public static int Add(Product p) =>
        DBHelper.ExecuteNonQuery(
            "INSERT INTO Products (Code, Name, CategoryId, Unit, Price, Cost, Stock, MinStock, SupplierId, Barcode) VALUES (@c,@n,@ci,@u,@p,@co,@s,@ms,@si,@b)",
            new SqliteParameter("@c", p.Code),
            new SqliteParameter("@n", p.Name),
            new SqliteParameter("@ci", p.CategoryId),
            new SqliteParameter("@u", p.Unit),
            new SqliteParameter("@p", p.Price),
            new SqliteParameter("@co", p.Cost),
            new SqliteParameter("@s", p.Stock),
            new SqliteParameter("@ms", p.MinStock),
            new SqliteParameter("@si", p.SupplierId),
            new SqliteParameter("@b", p.Barcode));

    public static int Update(Product p) =>
        DBHelper.ExecuteNonQuery(
            "UPDATE Products SET Code=@c,Name=@n,CategoryId=@ci,Unit=@u,Price=@p,Cost=@co,MinStock=@ms,SupplierId=@si,Barcode=@b WHERE Id=@id",
            new SqliteParameter("@c", p.Code),
            new SqliteParameter("@n", p.Name),
            new SqliteParameter("@ci", p.CategoryId),
            new SqliteParameter("@u", p.Unit),
            new SqliteParameter("@p", p.Price),
            new SqliteParameter("@co", p.Cost),
            new SqliteParameter("@ms", p.MinStock),
            new SqliteParameter("@si", p.SupplierId),
            new SqliteParameter("@b", p.Barcode),
            new SqliteParameter("@id", p.Id));

    public static int Delete(int id) =>
        DBHelper.ExecuteNonQuery("DELETE FROM Products WHERE Id=@id", new SqliteParameter("@id", id));

    public static int UpdateStock(int productId, int quantity)
    {
        return DBHelper.ExecuteNonQuery(
            "UPDATE Products SET Stock = Stock + @q WHERE Id = @id",
            new SqliteParameter("@q", quantity),
            new SqliteParameter("@id", productId));
    }
}

public class PurchaseBLL
{
    public static DataTable GetOrders() =>
        DBHelper.ExecuteQuery(@"
            SELECT po.*, s.Name AS SupplierName
            FROM PurchaseOrders po
            LEFT JOIN Suppliers s ON po.SupplierId = s.Id
            ORDER BY po.Id DESC");

    public static DataTable GetOrderDetails(int orderId) =>
        DBHelper.ExecuteQuery(@"
            SELECT pd.*, p.Name AS ProductName
            FROM PurchaseDetails pd
            LEFT JOIN Products p ON pd.ProductId = p.Id
            WHERE pd.OrderId = @oid",
            new SqliteParameter("@oid", orderId));

    public static int CreateOrder(PurchaseOrder order)
    {
        return DBHelper.ExecuteTransaction((conn, tran) =>
        {
            var sql = "INSERT INTO PurchaseOrders (OrderNo, SupplierId, TotalAmount, Operator) VALUES (@on, @si, @ta, @op)";
            using var cmd = new SqliteCommand(sql, conn, tran);
            cmd.Parameters.AddWithValue("@on", order.OrderNo);
            cmd.Parameters.AddWithValue("@si", order.SupplierId);
            cmd.Parameters.AddWithValue("@ta", order.TotalAmount);
            cmd.Parameters.AddWithValue("@op", order.Operator);
            cmd.ExecuteNonQuery();

            cmd.CommandText = "SELECT last_insert_rowid()";
            var orderId = Convert.ToInt32(cmd.ExecuteScalar());

            foreach (var d in order.Details)
            {
                cmd.CommandText = "INSERT INTO PurchaseDetails (OrderId, ProductId, Quantity, Price, Amount) VALUES (@oid, @pid, @q, @p, @a)";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@oid", orderId);
                cmd.Parameters.AddWithValue("@pid", d.ProductId);
                cmd.Parameters.AddWithValue("@q", d.Quantity);
                cmd.Parameters.AddWithValue("@p", d.Price);
                cmd.Parameters.AddWithValue("@a", d.Amount);

                cmd.ExecuteNonQuery();

                cmd.CommandText = "UPDATE Products SET Stock = Stock + @q WHERE Id = @pid";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@q", d.Quantity);
                cmd.Parameters.AddWithValue("@pid", d.ProductId);
                cmd.ExecuteNonQuery();
            }
        });
    }
}

public class SalesBLL
{
    public static DataTable GetOrders() =>
        DBHelper.ExecuteQuery("SELECT * FROM SalesOrders ORDER BY Id DESC");

    public static DataTable GetOrderDetails(int orderId) =>
        DBHelper.ExecuteQuery(@"
            SELECT sd.*, p.Name AS ProductName
            FROM SalesDetails sd
            LEFT JOIN Products p ON sd.ProductId = p.Id
            WHERE sd.OrderId = @oid",
            new SqliteParameter("@oid", orderId));

    public static int CreateOrder(SalesOrder order)
    {
        return DBHelper.ExecuteTransaction((conn, tran) =>
        {
            var cmd = new SqliteCommand(
                "INSERT INTO SalesOrders (OrderNo, TotalAmount, Operator) VALUES (@on, @ta, @op)",
                conn, tran);
            cmd.Parameters.AddWithValue("@on", order.OrderNo);
            cmd.Parameters.AddWithValue("@ta", order.TotalAmount);
            cmd.Parameters.AddWithValue("@op", order.Operator);
            cmd.ExecuteNonQuery();

            cmd.CommandText = "SELECT last_insert_rowid()";
            var orderId = Convert.ToInt32(cmd.ExecuteScalar());

            foreach (var d in order.Details)
            {
                cmd.CommandText = "INSERT INTO SalesDetails (OrderId, ProductId, Quantity, Price, Amount) VALUES (@oid, @pid, @q, @p, @a)";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@oid", orderId);
                cmd.Parameters.AddWithValue("@pid", d.ProductId);
                cmd.Parameters.AddWithValue("@q", d.Quantity);
                cmd.Parameters.AddWithValue("@p", d.Price);
                cmd.Parameters.AddWithValue("@a", d.Amount);
                cmd.ExecuteNonQuery();

                cmd.CommandText = "UPDATE Products SET Stock = Stock - @q WHERE Id = @pid";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@q", d.Quantity);
                cmd.Parameters.AddWithValue("@pid", d.ProductId);
                cmd.ExecuteNonQuery();
            }
        });
    }

    public static DataTable GetDailyReport(DateTime date)
    {
        var d = date.ToString("yyyy-MM-dd");
        return DBHelper.ExecuteQuery(@"
            SELECT COUNT(*) AS OrderCount, COALESCE(SUM(TotalAmount),0) AS TotalAmount
            FROM SalesOrders
            WHERE date(CreateTime) = @d",
            new SqliteParameter("@d", d));
    }

    public static DataTable GetMonthlyReport(int year, int month)
    {
        return DBHelper.ExecuteQuery(@"
            SELECT date(CreateTime) AS Day, COUNT(*) AS OrderCount, COALESCE(SUM(TotalAmount),0) AS TotalAmount
            FROM SalesOrders
            WHERE strftime('%Y', CreateTime) = @y AND strftime('%m', CreateTime) = @m
            GROUP BY date(CreateTime)
            ORDER BY Day",
            new SqliteParameter("@y", year.ToString()),
            new SqliteParameter("@m", month.ToString("D2")));
    }

    public static DataTable GetTopProducts(DateTime start, DateTime end)
    {
        return DBHelper.ExecuteQuery(@"
            SELECT p.Name, SUM(sd.Quantity) AS TotalQty, SUM(sd.Amount) AS TotalAmount
            FROM SalesDetails sd
            LEFT JOIN Products p ON sd.ProductId = p.Id
            LEFT JOIN SalesOrders so ON sd.OrderId = so.Id
            WHERE so.CreateTime BETWEEN @s AND @e
            GROUP BY p.Name
            ORDER BY TotalAmount DESC
            LIMIT 20",
            new SqliteParameter("@s", start.ToString("yyyy-MM-dd")),
            new SqliteParameter("@e", end.ToString("yyyy-MM-dd 23:59:59")));
    }

    public static DataTable GetCategorySales(DateTime start, DateTime end)
    {
        return DBHelper.ExecuteQuery(@"
            SELECT c.Name, COALESCE(SUM(sd.Amount),0) AS TotalAmount, COALESCE(SUM(sd.Quantity),0) AS TotalQty
            FROM SalesDetails sd
            LEFT JOIN Products p ON sd.ProductId = p.Id
            LEFT JOIN Categories c ON p.CategoryId = c.Id
            LEFT JOIN SalesOrders so ON sd.OrderId = so.Id
            WHERE so.CreateTime BETWEEN @s AND @e
            GROUP BY c.Name
            ORDER BY TotalAmount DESC",
            new SqliteParameter("@s", start.ToString("yyyy-MM-dd")),
            new SqliteParameter("@e", end.ToString("yyyy-MM-dd 23:59:59")));
    }
}
