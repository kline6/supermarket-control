using System.Data;
using Microsoft.Data.Sqlite;

namespace SupermarketMS.DAL;

public static class DBHelper
{
    private static readonly string DbPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "supermarket.db");
    private static readonly string ConnStr = $"Data Source={DbPath}";

    static DBHelper()
    {
        InitDatabase();
    }

    public static string ConnectionString => ConnStr;

    private static void InitDatabase()
    {
        if (File.Exists(DbPath)) return;

        using var conn = new SqliteConnection(ConnStr);
        conn.Open();

        var sql = @"
            CREATE TABLE Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username VARCHAR(50) NOT NULL UNIQUE,
                Password VARCHAR(50) NOT NULL,
                RealName VARCHAR(50),
                Role VARCHAR(20) DEFAULT 'user',
                CreateTime DATETIME DEFAULT (datetime('now','localtime'))
            );

            CREATE TABLE Categories (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name VARCHAR(50) NOT NULL,
                Description VARCHAR(200),
                CreateTime DATETIME DEFAULT (datetime('now','localtime'))
            );

            CREATE TABLE Suppliers (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name VARCHAR(100) NOT NULL,
                ContactPerson VARCHAR(50),
                Phone VARCHAR(20),
                Address VARCHAR(200),
                CreateTime DATETIME DEFAULT (datetime('now','localtime'))
            );

            CREATE TABLE Products (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Code VARCHAR(50) NOT NULL UNIQUE,
                Name VARCHAR(100) NOT NULL,
                CategoryId INTEGER,
                Unit VARCHAR(20) DEFAULT '个',
                Price DECIMAL(10,2) DEFAULT 0,
                Cost DECIMAL(10,2) DEFAULT 0,
                Stock INTEGER DEFAULT 0,
                MinStock INTEGER DEFAULT 10,
                SupplierId INTEGER,
                Barcode VARCHAR(50),
                CreateTime DATETIME DEFAULT (datetime('now','localtime')),
                FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
                FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id)
            );

            CREATE TABLE PurchaseOrders (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                OrderNo VARCHAR(50) NOT NULL UNIQUE,
                SupplierId INTEGER,
                TotalAmount DECIMAL(10,2) DEFAULT 0,
                Operator VARCHAR(50),
                CreateTime DATETIME DEFAULT (datetime('now','localtime')),
                FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id)
            );

            CREATE TABLE PurchaseDetails (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                OrderId INTEGER,
                ProductId INTEGER,
                Quantity INTEGER DEFAULT 1,
                Price DECIMAL(10,2) DEFAULT 0,
                Amount DECIMAL(10,2) DEFAULT 0,
                FOREIGN KEY (OrderId) REFERENCES PurchaseOrders(Id),
                FOREIGN KEY (ProductId) REFERENCES Products(Id)
            );

            CREATE TABLE SalesOrders (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                OrderNo VARCHAR(50) NOT NULL UNIQUE,
                TotalAmount DECIMAL(10,2) DEFAULT 0,
                Operator VARCHAR(50),
                CreateTime DATETIME DEFAULT (datetime('now','localtime'))
            );

            CREATE TABLE SalesDetails (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                OrderId INTEGER,
                ProductId INTEGER,
                Quantity INTEGER DEFAULT 1,
                Price DECIMAL(10,2) DEFAULT 0,
                Amount DECIMAL(10,2) DEFAULT 0,
                FOREIGN KEY (OrderId) REFERENCES SalesOrders(Id),
                FOREIGN KEY (ProductId) REFERENCES Products(Id)
            );

            INSERT INTO Users (Username, Password, RealName, Role) VALUES ('admin', '123456', '系统管理员', 'admin');
            INSERT INTO Users (Username, Password, RealName, Role) VALUES ('user', '123456', '普通用户', 'user');

            INSERT INTO Categories (Name, Description) VALUES ('食品饮料', '食品和饮料类商品');
            INSERT INTO Categories (Name, Description) VALUES ('日用百货', '日常生活用品');
            INSERT INTO Categories (Name, Description) VALUES ('烟酒茶糖', '烟酒茶糖类商品');
            INSERT INTO Categories (Name, Description) VALUES ('粮油调味', '粮油和调味品');
            INSERT INTO Categories (Name, Description) VALUES ('洗护用品', '个人洗护清洁用品');

            INSERT INTO Suppliers (Name, ContactPerson, Phone, Address) VALUES ('宏发商贸有限公司', '张经理', '13800001001', '本市中山路88号');
            INSERT INTO Suppliers (Name, ContactPerson, Phone, Address) VALUES ('万通食品批发', '李经理', '13800001002', '本市人民路156号');
            INSERT INTO Suppliers (Name, ContactPerson, Phone, Address) VALUES ('日新日化供应', '王经理', '13800001003', '本市解放路233号');
        ";

        using var cmd = new SqliteCommand(sql, conn);
        cmd.ExecuteNonQuery();
    }

    public static int ExecuteNonQuery(string sql, params SqliteParameter[] parameters)
    {
        using var conn = new SqliteConnection(ConnStr);
        conn.Open();
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddRange(parameters);
        return cmd.ExecuteNonQuery();
    }

    public static object? ExecuteScalar(string sql, params SqliteParameter[] parameters)
    {
        using var conn = new SqliteConnection(ConnStr);
        conn.Open();
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddRange(parameters);
        return cmd.ExecuteScalar();
    }

    public static DataTable ExecuteQuery(string sql, params SqliteParameter[] parameters)
    {
        var dt = new DataTable();
        using var conn = new SqliteConnection(ConnStr);
        conn.Open();
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddRange(parameters);
        using var reader = cmd.ExecuteReader();
        dt.Load(reader);
        return dt;
    }

    public static int ExecuteTransaction(Action<SqliteConnection, SqliteTransaction> action)
    {
        using var conn = new SqliteConnection(ConnStr);
        conn.Open();
        using var tran = conn.BeginTransaction();
        try
        {
            action(conn, tran);
            tran.Commit();
            return 1;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    public static string GenerateOrderNo(string prefix)
    {
        var now = DateTime.Now;
        return $"{prefix}{now:yyyyMMddHHmmss}{new Random().Next(100, 999)}";
    }
}
