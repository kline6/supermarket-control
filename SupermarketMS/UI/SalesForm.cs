using System.Data;
using Microsoft.Data.Sqlite;
using SupermarketMS.BLL;
using SupermarketMS.DAL;
using SupermarketMS.Models;

namespace SupermarketMS.UI;

public class SalesForm : Form
{
    private readonly string _operator;
    private DataGridView dgvCart = null!;
    private DataGridView dgvOrders = null!;
    private Label lblTotal = null!;
    private TextBox txtBarcode = null!;
    private TextBox txtQuantity = null!;
    private readonly List<SalesDetail> _cart = new();

    public SalesForm(string operatorName)
    {
        _operator = operatorName;
        InitializeComponent();
        LoadRecentOrders();
    }

    private void InitializeComponent()
    {
        this.BackColor = Color.White;

        var topBar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.WhiteSmoke };
        topBar.Controls.Add(new Label
        {
            Text = "销售收银",
            Font = new Font("微软雅黑", 14, FontStyle.Bold),
            ForeColor = Color.DarkSlateBlue,
            Location = new Point(15, 12), AutoSize = true
        });

        var mainSplit = new SplitContainer
        {
            Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 520
        };

        // 左侧：收银区域
        var leftPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

        var inputPanel = new Panel
        {
            Dock = DockStyle.Top, Height = 35, BackColor = Color.WhiteSmoke,
            Padding = new Padding(10, 5, 10, 5)
        };
        var lblCode = new Label { Text = "条码/编码：", Location = new Point(5, 8), AutoSize = true, Font = new Font("微软雅黑", 9) };
        txtBarcode = new TextBox { Location = new Point(95, 5), Width = 150, Font = new Font("微软雅黑", 9) };
        var lblQty = new Label { Text = "数量：", Location = new Point(255, 8), AutoSize = true, Font = new Font("微软雅黑", 9) };
        txtQuantity = new TextBox { Location = new Point(300, 5), Width = 60, Font = new Font("微软雅黑", 9), Text = "1" };
        var btnAdd = new Button
        {
            Text = "添加", Location = new Point(370, 3), Size = new Size(55, 26),
            BackColor = Color.DarkSlateBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat
        };
        btnAdd.FlatAppearance.BorderSize = 0;
        btnAdd.Click += BtnAdd_Click;
        inputPanel.Controls.AddRange([lblCode, txtBarcode, lblQty, txtQuantity, btnAdd]);

        dgvCart = new DataGridView
        {
            Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
            ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false, BackgroundColor = Color.White, Font = new Font("微软雅黑", 10)
        };

        var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.WhiteSmoke };
        lblTotal = new Label
        {
            Text = "合计：¥ 0.00",
            Location = new Point(300, 18), AutoSize = true,
            Font = new Font("微软雅黑", 16, FontStyle.Bold), ForeColor = Color.Red
        };
        var btnCheckout = new Button
        {
            Text = "结算收款",
            Location = new Point(50, 12), Size = new Size(100, 36),
            BackColor = Color.DarkSlateBlue, ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat, Font = new Font("微软雅黑", 10, FontStyle.Bold)
        };
        btnCheckout.FlatAppearance.BorderSize = 0;
        btnCheckout.Click += BtnCheckout_Click;

        var btnClear = new Button
        {
            Text = "清空", Location = new Point(165, 12), Size = new Size(80, 36),
            FlatStyle = FlatStyle.Flat, Font = new Font("微软雅黑", 10)
        };
        btnClear.Click += (s, e) => { _cart.Clear(); RefreshCart(); };
        bottomPanel.Controls.AddRange([lblTotal, btnCheckout, btnClear]);

        leftPanel.Controls.Add(dgvCart);
        leftPanel.Controls.Add(inputPanel);
        leftPanel.Controls.Add(bottomPanel);

        // 右侧：近期订单
        var rightPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        var lblRecent = new Label
        {
            Text = "近期销售记录",
            Dock = DockStyle.Top, Height = 30,
            Font = new Font("微软雅黑", 10, FontStyle.Bold),
            ForeColor = Color.DarkSlateBlue,
            BackColor = Color.WhiteSmoke,
            Padding = new Padding(5, 5, 0, 0)
        };
        dgvOrders = new DataGridView
        {
            Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
            ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false, BackgroundColor = Color.White, Font = new Font("微软雅黑", 9)
        };
        dgvOrders.SelectionChanged += (s, e) =>
        {
            if (dgvOrders.SelectedRows.Count == 0) return;
            var oid = Convert.ToInt32(dgvOrders.SelectedRows[0].Cells["Id"].Value);
        };
        rightPanel.Controls.Add(dgvOrders);
        rightPanel.Controls.Add(lblRecent);

        mainSplit.Panel1.Controls.Add(leftPanel);
        mainSplit.Panel2.Controls.Add(rightPanel);

        this.Controls.Add(mainSplit);
        this.Controls.Add(topBar);

        this.AcceptButton = btnAdd;
    }

    private void BtnAdd_Click(object? sender, EventArgs e)
    {
        var code = txtBarcode.Text.Trim();
        if (string.IsNullOrEmpty(code)) return;

        var product = ProductBLL.GetByBarcode(code);
        if (product == null)
        {
            var dt = ProductBLL.Search(code);
            if (dt.Rows.Count == 0) { MessageBox.Show("未找到该商品"); return; }
            product = new Product
            {
                Id = Convert.ToInt32(dt.Rows[0]["Id"]),
                Name = dt.Rows[0]["Name"].ToString()!,
                Price = Convert.ToDecimal(dt.Rows[0]["Price"]),
                Stock = Convert.ToInt32(dt.Rows[0]["Stock"])
            };
        }

        var qty = int.TryParse(txtQuantity.Text, out var q) && q > 0 ? q : 1;

        if (product.Stock < qty)
        {
            MessageBox.Show($"库存不足！当前库存：{product.Stock}", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var existing = _cart.FirstOrDefault(d => d.ProductId == product.Id);
        if (existing != null)
        {
            if (product.Stock < existing.Quantity + qty)
            {
                MessageBox.Show($"库存不足！当前库存：{product.Stock}，购物车已有：{existing.Quantity}");
                return;
            }
            existing.Quantity += qty;
            existing.Amount = existing.Quantity * existing.Price;
        }
        else
        {
            _cart.Add(new SalesDetail
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = qty,
                Price = product.Price,
                Amount = qty * product.Price
            });
        }

        RefreshCart();
        txtBarcode.Clear();
        txtQuantity.Text = "1";
        txtBarcode.Focus();
    }

    private void RefreshCart()
    {
        dgvCart.DataSource = null;
        dgvCart.DataSource = _cart.Select((d, i) => new
        {
            序号 = i + 1,
            商品名称 = d.ProductName,
            单价 = d.Price,
            数量 = d.Quantity,
            金额 = d.Amount
        }).ToList();

        if (dgvCart.Columns["ProductId"] != null) dgvCart.Columns["ProductId"].Visible = false;
        if (dgvCart.Columns["单价"] != null) dgvCart.Columns["单价"].DefaultCellStyle.Format = "0.00";
        if (dgvCart.Columns["金额"] != null) dgvCart.Columns["金额"].DefaultCellStyle.Format = "0.00";

        var total = _cart.Sum(d => d.Amount);
        lblTotal.Text = $"合计：¥ {total:0.00}";
    }

    private void BtnCheckout_Click(object? sender, EventArgs e)
    {
        if (_cart.Count == 0) { MessageBox.Show("购物车为空"); return; }

        var total = _cart.Sum(d => d.Amount);
        var result = MessageBox.Show(
            $"应收金额：¥ {total:0.00}\n\n确认结算？",
            "确认收款", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (result != DialogResult.Yes) return;

        var order = new SalesOrder
        {
            OrderNo = DBHelper.GenerateOrderNo("XS"),
            TotalAmount = total,
            Operator = _operator,
            Details = _cart.ToList()
        };

        try
        {
            SalesBLL.CreateOrder(order);
            MessageBox.Show($"收款成功！\n找零请当面确认。\n单号：{order.OrderNo}",
                "交易完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _cart.Clear();
            RefreshCart();
            LoadRecentOrders();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"结算失败：{ex.Message}", "错误",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadRecentOrders()
    {
        var dt = SalesBLL.GetOrders();
        dgvOrders.DataSource = dt;
        foreach (DataGridViewColumn col in dgvOrders.Columns)
        {
            col.HeaderText = col.Name switch
            {
                "Id" => "ID", "OrderNo" => "销售单号", "TotalAmount" => "金额",
                "Operator" => "收银员", "CreateTime" => "时间", _ => col.Name
            };
        }
    }
}
