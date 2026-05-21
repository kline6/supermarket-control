using System.Data;
using Microsoft.Data.Sqlite;
using SupermarketMS.BLL;
using SupermarketMS.DAL;
using SupermarketMS.Models;

namespace SupermarketMS.UI;

public class PurchaseForm : Form
{
    private readonly string _operator;
    private DataGridView dgvOrders = null!;
    private DataGridView dgvDetails = null!;
    private ComboBox cmbSupplier = null!;
    private Button btnNewOrder = null!;
    private SplitContainer split = null!;

    public PurchaseForm(string operatorName)
    {
        _operator = operatorName;
        InitializeComponent();
        LoadOrders();
    }

    private void InitializeComponent()
    {
        this.BackColor = Color.White;

        var topBar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.WhiteSmoke };
        topBar.Controls.Add(new Label
        {
            Text = "进货管理",
            Font = new Font("微软雅黑", 14, FontStyle.Bold),
            ForeColor = Color.DarkSlateBlue,
            Location = new Point(15, 12), AutoSize = true
        });

        var toolBar = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.WhiteSmoke };
        btnNewOrder = new Button
        {
            Text = "新建进货单", Location = new Point(10, 6), Size = new Size(90, 28),
            BackColor = Color.DarkSlateBlue, ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat, Font = new Font("微软雅黑", 9)
        };
        btnNewOrder.FlatAppearance.BorderSize = 0;
        btnNewOrder.Click += BtnNewOrder_Click;
        toolBar.Controls.Add(btnNewOrder);

        split = new SplitContainer
        {
            Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 250
        };

        dgvOrders = new DataGridView
        {
            Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
            ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false, BackgroundColor = Color.White, Font = new Font("微软雅黑", 9)
        };
        dgvOrders.SelectionChanged += DgvOrders_SelectionChanged;

        var detailLabel = new Label
        {
            Text = "进货明细：",
            Dock = DockStyle.Top, Height = 25,
            Font = new Font("微软雅黑", 10, FontStyle.Bold),
            ForeColor = Color.DarkSlateBlue,
            BackColor = Color.WhiteSmoke,
            Padding = new Padding(5, 3, 0, 0)
        };

        dgvDetails = new DataGridView
        {
            Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
            ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false, BackgroundColor = Color.White, Font = new Font("微软雅黑", 9)
        };

        var detailPanel = new Panel { Dock = DockStyle.Fill };
        detailPanel.Controls.Add(dgvDetails);
        detailPanel.Controls.Add(detailLabel);

        split.Panel1.Controls.Add(dgvOrders);
        split.Panel2.Controls.Add(detailPanel);

        this.Controls.Add(split);
        this.Controls.Add(toolBar);
        this.Controls.Add(topBar);
    }

    private void LoadOrders()
    {
        var dt = PurchaseBLL.GetOrders();
        dgvOrders.DataSource = dt;
        foreach (DataGridViewColumn col in dgvOrders.Columns)
        {
            col.HeaderText = col.Name switch
            {
                "Id" => "ID", "OrderNo" => "进货单号", "SupplierName" => "供应商",
                "TotalAmount" => "总金额", "Operator" => "操作员", "CreateTime" => "进货时间",
                _ => col.Name
            };
        }
    }

    private void DgvOrders_SelectionChanged(object? sender, EventArgs e)
    {
        if (dgvOrders.SelectedRows.Count == 0) return;
        var orderId = Convert.ToInt32(dgvOrders.SelectedRows[0].Cells["Id"].Value);
        var dt = PurchaseBLL.GetOrderDetails(orderId);
        dgvDetails.DataSource = dt;
        foreach (DataGridViewColumn col in dgvDetails.Columns)
        {
            col.HeaderText = col.Name switch
            {
                "ProductName" => "商品名称", "Quantity" => "数量",
                "Price" => "进货单价", "Amount" => "金额", _ => col.Name
            };
            if (col.Name is "Id" or "OrderId" or "ProductId") col.Visible = false;
        }
    }

    private void BtnNewOrder_Click(object? sender, EventArgs e)
    {
        var form = new PurchaseEditForm(_operator);
        if (form.ShowDialog() == DialogResult.OK)
            LoadOrders();
    }
}

public class PurchaseEditForm : Form
{
    private readonly string _operator;
    private ComboBox cmbSupplier = null!;
    private DataGridView dgvItems = null!;
    private Label lblTotal = null!;
    private readonly List<PurchaseDetail> _items = new();
    private TextBox txtProductCode = null!;
    private TextBox txtQuantity = null!;
    private TextBox txtPrice = null!;

    public PurchaseEditForm(string operatorName)
    {
        _operator = operatorName;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "新建进货单";
        this.Size = new Size(700, 550);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.White;

        int y = 15;
        var lblSup = new Label { Text = "供应商：", Location = new Point(20, y + 4), AutoSize = true, Font = new Font("微软雅黑", 9) };
        cmbSupplier = new ComboBox
        {
            Location = new Point(90, y), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("微软雅黑", 9)
        };
        var supDt = SupplierBLL.GetAll();
        cmbSupplier.DataSource = supDt;
        cmbSupplier.DisplayMember = "Name";
        cmbSupplier.ValueMember = "Id";

        y += 30;
        var lblCode = new Label { Text = "商品编码/条码：", Location = new Point(20, y + 4), AutoSize = true, Font = new Font("微软雅黑", 9) };
        txtProductCode = new TextBox { Location = new Point(140, y), Width = 150, Font = new Font("微软雅黑", 9) };
        var lblQty = new Label { Text = "数量：", Location = new Point(300, y + 4), AutoSize = true, Font = new Font("微软雅黑", 9) };
        txtQuantity = new TextBox { Location = new Point(350, y), Width = 80, Font = new Font("微软雅黑", 9), Text = "1" };
        var lblPrice = new Label { Text = "单价：", Location = new Point(440, y + 4), AutoSize = true, Font = new Font("微软雅黑", 9) };
        txtPrice = new TextBox { Location = new Point(490, y), Width = 80, Font = new Font("微软雅黑", 9) };
        var btnAddItem = new Button
        {
            Text = "添加", Location = new Point(580, y - 2), Size = new Size(60, 28),
            BackColor = Color.DarkSlateBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat
        };
        btnAddItem.FlatAppearance.BorderSize = 0;
        btnAddItem.Click += BtnAddItem_Click;

        y += 35;
        dgvItems = new DataGridView
        {
            Location = new Point(15, y), Width = 650, Height = 280,
            AllowUserToAddRows = false, AllowUserToDeleteRows = false,
            ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false, BackgroundColor = Color.White, Font = new Font("微软雅黑", 9)
        };

        y += 290;
        lblTotal = new Label
        {
            Text = "合计金额：¥ 0.00",
            Location = new Point(450, y), AutoSize = true,
            Font = new Font("微软雅黑", 12, FontStyle.Bold), ForeColor = Color.Red
        };

        var btnSave = new Button
        {
            Text = "保存进货单", Location = new Point(200, y + 5), Size = new Size(100, 32),
            BackColor = Color.DarkSlateBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += BtnSave_Click;

        var btnCancel = new Button
        {
            Text = "取消", Location = new Point(320, y + 5), Size = new Size(100, 32),
            FlatStyle = FlatStyle.Flat
        };
        btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

        this.Controls.AddRange([lblSup, cmbSupplier,
            lblCode, txtProductCode, lblQty, txtQuantity, lblPrice, txtPrice, btnAddItem,
            dgvItems, lblTotal, btnSave, btnCancel]);
    }

    private void BtnAddItem_Click(object? sender, EventArgs e)
    {
        var code = txtProductCode.Text.Trim();
        if (string.IsNullOrEmpty(code)) { MessageBox.Show("请输入商品编码或条码"); return; }

        var product = ProductBLL.GetByBarcode(code)
                      ?? (int.TryParse(code, out _) ? ProductBLL.GetById(Convert.ToInt32(code)) : null);

        if (product == null)
        {
            var dt = ProductBLL.Search(code);
            if (dt.Rows.Count == 0) { MessageBox.Show("未找到该商品"); return; }
            product = new Product
            {
                Id = Convert.ToInt32(dt.Rows[0]["Id"]),
                Name = dt.Rows[0]["Name"].ToString()!,
                Price = Convert.ToDecimal(dt.Rows[0]["Price"]),
                Cost = Convert.ToDecimal(dt.Rows[0]["Cost"])
            };
        }

        var qty = int.TryParse(txtQuantity.Text, out var q) && q > 0 ? q : 1;
        var price = decimal.TryParse(txtPrice.Text, out var p) && p > 0 ? p : product.Cost;

        _items.Add(new PurchaseDetail
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Quantity = qty,
            Price = price,
            Amount = qty * price
        });

        RefreshGrid();
        txtProductCode.Clear();
        txtQuantity.Text = "1";
        txtPrice.Clear();
        txtProductCode.Focus();
    }

    private void RefreshGrid()
    {
        dgvItems.DataSource = null;
        dgvItems.DataSource = _items.Select((d, i) => new
        {
            序号 = i + 1,
            商品名称 = d.ProductName,
            数量 = d.Quantity,
            单价 = d.Price,
            金额 = d.Amount
        }).ToList();

        foreach (DataGridViewColumn col in dgvItems.Columns)
            if (col.Name is "ProductId") col.Visible = false;

        if (dgvItems.Columns["单价"] != null) dgvItems.Columns["单价"].DefaultCellStyle.Format = "0.00";
        if (dgvItems.Columns["金额"] != null) dgvItems.Columns["金额"].DefaultCellStyle.Format = "0.00";

        var total = _items.Sum(d => d.Amount);
        lblTotal.Text = $"合计金额：¥ {total:0.00}";
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (_items.Count == 0) { MessageBox.Show("请至少添加一个商品"); return; }
        if (cmbSupplier.SelectedValue == null) { MessageBox.Show("请选择供应商"); return; }

        var order = new PurchaseOrder
        {
            OrderNo = DBHelper.GenerateOrderNo("JH"),
            SupplierId = Convert.ToInt32(cmbSupplier.SelectedValue),
            TotalAmount = _items.Sum(d => d.Amount),
            Operator = _operator,
            Details = _items
        };

        try
        {
            PurchaseBLL.CreateOrder(order);
            MessageBox.Show("进货单保存成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"保存失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
