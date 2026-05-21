using System.Data;
using SupermarketMS.BLL;

namespace SupermarketMS.UI;

public class InventoryForm : Form
{
    private DataGridView dgvAll = null!;
    private DataGridView dgvLow = null!;
    private TextBox txtSearch = null!;

    public InventoryForm()
    {
        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        this.BackColor = Color.White;

        var topBar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.WhiteSmoke };
        topBar.Controls.Add(new Label
        {
            Text = "库存查询",
            Font = new Font("微软雅黑", 14, FontStyle.Bold),
            ForeColor = Color.DarkSlateBlue,
            Location = new Point(15, 12), AutoSize = true
        });

        txtSearch = new TextBox { Location = new Point(500, 14), Width = 180, Font = new Font("微软雅黑", 9) };
        var btnSearch = new Button
        {
            Text = "搜索", Location = new Point(685, 12), Size = new Size(60, 28),
            BackColor = Color.DarkSlateBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat
        };
        btnSearch.FlatAppearance.BorderSize = 0;
        btnSearch.Click += (s, e) => LoadData();
        var btnRefresh = new Button
        {
            Text = "刷新", Location = new Point(755, 12), Size = new Size(60, 28),
            BackColor = Color.Gray, ForeColor = Color.White, FlatStyle = FlatStyle.Flat
        };
        btnRefresh.FlatAppearance.BorderSize = 0;
        btnRefresh.Click += (s, e) => LoadData();
        topBar.Controls.AddRange([txtSearch, btnSearch, btnRefresh]);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 300
        };

        // 全部库存
        var lblAll = new Label
        {
            Text = "全部库存：",
            Dock = DockStyle.Top, Height = 25, Font = new Font("微软雅黑", 10, FontStyle.Bold),
            ForeColor = Color.DarkSlateBlue, BackColor = Color.WhiteSmoke, Padding = new Padding(5, 3, 0, 0)
        };
        dgvAll = new DataGridView
        {
            Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
            ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false, BackgroundColor = Color.White, Font = new Font("微软雅黑", 9)
        };
        var allPanel = new Panel { Dock = DockStyle.Fill };
        allPanel.Controls.Add(dgvAll);
        allPanel.Controls.Add(lblAll);

        // 库存不足预警
        var lowPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        var lblLow = new Label
        {
            Text = "库存不足预警（红色标记）：",
            Dock = DockStyle.Top, Height = 25, Font = new Font("微软雅黑", 10, FontStyle.Bold),
            ForeColor = Color.Red, BackColor = Color.WhiteSmoke, Padding = new Padding(5, 3, 0, 0)
        };
        dgvLow = new DataGridView
        {
            Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
            ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false, BackgroundColor = Color.White, Font = new Font("微软雅黑", 9)
        };
        dgvLow.CellFormatting += (s, e) =>
        {
            if (e.RowIndex >= 0 && e.Value != null)
            {
                e.CellStyle.BackColor = Color.MistyRose;
            }
        };
        lowPanel.Controls.Add(dgvLow);
        lowPanel.Controls.Add(lblLow);

        split.Panel1.Controls.Add(allPanel);
        split.Panel2.Controls.Add(lowPanel);

        this.Controls.Add(split);
        this.Controls.Add(topBar);
    }

    private void LoadData()
    {
        var kw = txtSearch.Text.Trim();
        var allDt = string.IsNullOrEmpty(kw) ? ProductBLL.GetAll() : ProductBLL.Search(kw);
        dgvAll.DataSource = allDt;

        foreach (DataGridViewColumn col in dgvAll.Columns)
        {
            col.HeaderText = col.Name switch
            {
                "Code" => "编码", "Name" => "商品名称", "CategoryName" => "分类",
                "Unit" => "单位", "Price" => "售价", "Cost" => "进价",
                "Stock" => "库存", "MinStock" => "最低库存", "SupplierName" => "供应商",
                "Barcode" => "条码", _ => col.Name
            };
            if (col.Name is "Id" or "CategoryId" or "SupplierId" or "CreateTime") col.Visible = false;
        }

        var lowDt = ProductBLL.GetLowStock();
        dgvLow.DataSource = lowDt;
        foreach (DataGridViewColumn col in dgvLow.Columns)
        {
            col.HeaderText = col.Name switch
            {
                "Code" => "编码", "Name" => "商品名称", "CategoryName" => "分类",
                "Unit" => "单位", "Stock" => "库存", "MinStock" => "最低库存",
                "SupplierName" => "供应商", _ => col.Name
            };
            if (col.Name is not ("Code" or "Name" or "CategoryName" or "Unit" or "Stock" or "MinStock" or "SupplierName"))
                col.Visible = false;
        }
    }
}
