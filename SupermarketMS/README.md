# 超市商品管理系统

基于 .NET 8 + Windows Forms + SQLite 的超市商品管理系统。

## 开发环境

- .NET 8 SDK
- Visual Studio 2022 / VS Code / Rider
- 无需额外安装数据库（SQLite 文件型数据库）

## 运行方式

```bash
cd SupermarketMS
dotnet restore
dotnet run
```

或在 Visual Studio 中打开 SupermarketMS.csproj 直接运行。

## 项目结构

```
SupermarketMS/
├── SupermarketMS.csproj    # 项目文件
├── GlobalUsings.cs          # 全局引用
├── Program.cs               # 程序入口
├── Models/
│   └── Models.cs            # 数据模型（User, Category, Product, Supplier, Order等）
├── DAL/
│   └── DBHelper.cs          # 数据库访问层（SQLite初始化、CRUD封装、事务支持）
├── BLL/
│   └── BLL.cs               # 业务逻辑层（用户、商品、分类、供应商、进货、销售）
└── UI/
    ├── LoginForm.cs          # 登录窗体
    ├── MainForm.cs           # 主窗体（菜单导航 + 状态栏）
    ├── ProductForm.cs        # 商品管理（增删改查、分类/供应商关联）
    ├── CategoryForm.cs       # 商品分类管理
    ├── SupplierForm.cs       # 供应商管理
    ├── PurchaseForm.cs       # 进货管理（进货单创建、明细查看）
    ├── SalesForm.cs          # 销售收银（扫码结算、购物车、库存校验）
    ├── InventoryForm.cs      # 库存查询（全部库存 + 低库存预警）
    ├── ReportForm.cs         # 销售报表（日汇总、热销排行、分类统计、柱状图）
    └── UserForm.cs           # 用户管理
```

## 功能模块

### 1. 登录系统
- 默认管理员：admin / 123456
- 默认普通用户：user / 123456
- 支持多用户登录

### 2. 基础资料管理
- **商品管理**：商品编码、名称、分类、单位、售价、进价、库存、最低库存、供应商、条码
- **商品分类**：分类名称和描述
- **供应商管理**：供应商名称、联系人、电话、地址

### 3. 业务管理
- **进货管理**：选择供应商 → 录入商品（支持条码/编码扫描）→ 自动更新库存
- **销售收银**：扫码添加商品 → 自动校验库存 → 结算收款 → 扣减库存

### 4. 查询统计
- **库存查询**：全部库存查看 + 低库存预警（红色标记）
- **销售报表**：每日汇总、热销商品排行、分类销售统计、柱状图可视化

### 5. 系统管理
- **用户管理**：新增/删除系统用户
- 状态栏显示当前用户和实时时钟

## 数据库表结构

| 表名 | 说明 |
|------|------|
| Users | 系统用户表 |
| Categories | 商品分类表 |
| Suppliers | 供应商表 |
| Products | 商品表 |
| PurchaseOrders | 进货单主表 |
| PurchaseDetails | 进货单明细表 |
| SalesOrders | 销售单主表 |
| SalesDetails | 销售单明细表 |

## 技术特点

- 三层架构：UI → BLL → DAL
- SQLite 本地数据库，首次运行自动创建并初始化示例数据
- 事务保证进货/销售数据一致性（库存同步更新）
- 商品条码扫描支持（模拟 POS 收银流程）
- 低库存预警（库存 <= 最低库存时红色标记）
- GDI+ 绘制销售统计柱状图
- 菜单式导航，窗体嵌入主面板展示
