using System.Data;
using SupermarketMS.BLL;
using SupermarketMS.Models;

namespace SupermarketMS.UI;

public class ProductForm : Form
{
    private DataGridView dgv = null!;
    private TextBox txtSearch = null!;
    private Button btnSearch = null!;
    private Button btnAdd = null!;
    private Button btnEdit = null!;
    private Button btnDelete = null!;

    public ProductForm()
    {
        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        this.BackColor = Color.White;

        var topBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = Color.WhiteSmoke,
            Padding = new Padding(10, 10, 10, 5)
        };

        var lbl = new Label
        {
            Text = "商品管理",
            Font = new Font("微软雅黑", 14, FontStyle.Bold),
            ForeColor = Color.DarkSlateBlue,
            Location = new Point(10, 12),
            AutoSize = true
        };

        txtSearch = new TextBox
        {
            Location = new Point(680, 14),
            Width = 180,
            Font = new Font("微软雅黑", 9)
        };

        btnSearch = new Button
        {
            Text = "搜索",
            Location = new Point(865, 12),
            Size = new Size(60, 28),
            BackColor = Color.DarkSlateBlue,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnSearch.FlatAppearance.BorderSize = 0;
        btnSearch.Click += (s, e) => LoadData();

        topBar.Controls.AddRange([lbl, txtSearch, btnSearch]);

        var toolBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 40,
            BackColor = Color.WhiteSmoke,
            Padding = new Padding(10, 0, 10, 5)
        };

        btnAdd = CreateToolButton("新增商品", 0);
        btnEdit = CreateToolButton("修改商品", 80);
        btnDelete = CreateToolButton("删除商品", 160);

        btnAdd.Click += BtnAdd_Click;
        btnEdit.Click += BtnEdit_Click;
        btnDelete.Click += BtnDelete_Click;

        toolBar.Controls.AddRange([btnAdd, btnEdit, btnDelete]);

        dgv = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false,
            BackgroundColor = Color.White,
            Font = new Font("微软雅黑", 9)
        };

        this.Controls.Add(dgv);
        this.Controls.Add(toolBar);
        this.Controls.Add(topBar);
    }

    private Button CreateToolButton(string text, int x)
    {
        return new Button
        {
            Text = text,
            Location = new Point(10 + x, 6),
            Size = new Size(75, 28),
            BackColor = Color.DarkSlateBlue,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("微软雅黑", 9)
        };
    }

    private void LoadData()
    {
        var keyword = txtSearch.Text.Trim();
        var dt = string.IsNullOrEmpty(keyword) ? ProductBLL.GetAll() : ProductBLL.Search(keyword);
        dgv.DataSource = dt;
        SetColumnHeaders();
    }

    private void SetColumnHeaders()
    {
        var headers = new Dictionary<string, string>
        {
            ["Id"] = "ID", ["Code"] = "商品编码", ["Name"] = "商品名称",
            ["CategoryName"] = "分类", ["Unit"] = "单位", ["Price"] = "售价",
            ["Cost"] = "进价", ["Stock"] = "库存", ["MinStock"] = "最低库存",
            ["SupplierName"] = "供应商", ["Barcode"] = "条码", ["CreateTime"] = "创建时间",
            ["CategoryId"] = "分类ID", ["SupplierId"] = "供应商ID"
        };

        foreach (DataGridViewColumn col in dgv.Columns)
        {
            if (headers.TryGetValue(col.Name, out var header))
                col.HeaderText = header;
            else
                col.Visible = false;
        }

        if (dgv.Columns["Price"] != null) dgv.Columns["Price"].DefaultCellStyle.Format = "0.00";
        if (dgv.Columns["Cost"] != null) dgv.Columns["Cost"].DefaultCellStyle.Format = "0.00";
    }

    private void BtnAdd_Click(object? sender, EventArgs e)
    {
        var dialog = new ProductEditForm();
        if (dialog.ShowDialog() == DialogResult.OK)
            LoadData();
    }

    private void BtnEdit_Click(object? sender, EventArgs e)
    {
        if (dgv.SelectedRows.Count == 0) { MessageBox.Show("请先选择要修改的商品"); return; }
        var id = Convert.ToInt32(dgv.SelectedRows[0].Cells["Id"].Value);
        var product = ProductBLL.GetById(id);
        if (product == null) return;

        var dialog = new ProductEditForm(product);
        if (dialog.ShowDialog() == DialogResult.OK)
            LoadData();
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (dgv.SelectedRows.Count == 0) { MessageBox.Show("请先选择要删除的商品"); return; }
        var name = dgv.SelectedRows[0].Cells["Name"].Value?.ToString();
        if (MessageBox.Show($"确定要删除商品「{name}」吗？", "确认删除",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        var id = Convert.ToInt32(dgv.SelectedRows[0].Cells["Id"].Value);
        ProductBLL.Delete(id);
        LoadData();
    }
}

/// <summary>
/// 商品编辑窗体（新增/修改）
/// </summary>
public class ProductEditForm : Form
{
    private TextBox txtCode = null!, txtName = null!, txtUnit = null!;
    private TextBox txtPrice = null!, txtCost = null!, txtStock = null!;
    private TextBox txtMinStock = null!, txtBarcode = null!;
    private ComboBox cmbCategory = null!, cmbSupplier = null!;
    private Button btnSave = null!, btnCancel = null!;
    private readonly Product? _product;

    public ProductEditForm(Product? product = null)
    {
        _product = product;
        InitializeComponent();
        LoadCombos();
        if (product != null) BindProduct(product);
    }

    private void InitializeComponent()
    {
        this.Text = _product == null ? "新增商品" : "修改商品";
        this.Size = new Size(450, 480);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.BackColor = Color.White;

        int y = 20;
        AddLabel("商品编码：", 30, y + 4);
        txtCode = AddTextBox(130, y); y += 35;
        txtCode.CharacterCasing = CharacterCasing.Upper;

        AddLabel("商品名称：", 30, y + 4);
        txtName = AddTextBox(130, y); y += 35;

        AddLabel("商品分类：", 30, y + 4);
        cmbCategory = new ComboBox { Location = new Point(130, y), Width = 250, Font = new Font("微软雅黑", 9), DropDownStyle = ComboBoxStyle.DropDownList };
        this.Controls.Add(cmbCategory); y += 35;

        AddLabel("单    位：", 30, y + 4);
        txtUnit = AddTextBox(130, y);
        txtUnit.Text = "个"; y += 35;

        AddLabel("售    价：", 30, y + 4);
        txtPrice = AddTextBox(130, y); y += 35;

        AddLabel("进    价：", 30, y + 4);
        txtCost = AddTextBox(130, y); y += 35;

        AddLabel("当前库存：", 30, y + 4);
        txtStock = AddTextBox(130, y);
        txtStock.Text = "0"; y += 35;

        AddLabel("最低库存：", 30, y + 4);
        txtMinStock = AddTextBox(130, y);
        txtMinStock.Text = "10"; y += 35;

        AddLabel("供 应 商：", 30, y + 4);
        cmbSupplier = new ComboBox { Location = new Point(130, y), Width = 250, Font = new Font("微软雅黑", 9), DropDownStyle = ComboBoxStyle.DropDownList };
        this.Controls.Add(cmbSupplier); y += 35;

        AddLabel("条    码：", 30, y + 4);
        txtBarcode = AddTextBox(130, y); y += 45;

        btnSave = new Button
        {
            Text = "保存", Location = new Point(130, y), Size = new Size(90, 32),
            BackColor = Color.DarkSlateBlue, ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat, Font = new Font("微软雅黑", 9)
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += BtnSave_Click;

        btnCancel = new Button
        {
            Text = "取消", Location = new Point(240, y), Size = new Size(90, 32),
            BackColor = Color.Gray, ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat, Font = new Font("微软雅黑", 9)
        };
        btnCancel.FlatAppearance.BorderSize = 0;
        btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

        this.Controls.AddRange([btnSave, btnCancel]);
        this.AcceptButton = btnSave;
    }

    private void AddLabel(string text, int x, int y)
    {
        this.Controls.Add(new Label
        {
            Text = text, Location = new Point(x, y), AutoSize = true, Font = new Font("微软雅黑", 9)
        });
    }

    private TextBox AddTextBox(int x, int y)
    {
        var tb = new TextBox { Location = new Point(x, y), Width = 250, Font = new Font("微软雅黑", 9) };
        this.Controls.Add(tb);
        return tb;
    }

    private void LoadCombos()
    {
        var catDt = CategoryBLL.GetAll();
        cmbCategory.DataSource = catDt;
        cmbCategory.DisplayMember = "Name";
        cmbCategory.ValueMember = "Id";

        var supDt = SupplierBLL.GetAll();
        cmbSupplier.DataSource = supDt;
        cmbSupplier.DisplayMember = "Name";
        cmbSupplier.ValueMember = "Id";
    }

    private void BindProduct(Product p)
    {
        txtCode.Text = p.Code;
        txtName.Text = p.Name;
        cmbCategory.SelectedValue = p.CategoryId;
        txtUnit.Text = p.Unit;
        txtPrice.Text = p.Price.ToString("0.00");
        txtCost.Text = p.Cost.ToString("0.00");
        txtStock.Text = p.Stock.ToString();
        txtMinStock.Text = p.MinStock.ToString();
        cmbSupplier.SelectedValue = p.SupplierId;
        txtBarcode.Text = p.Barcode;
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtCode.Text) || string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("商品编码和名称不能为空！"); return;
        }

        var p = new Product
        {
            Code = txtCode.Text.Trim(),
            Name = txtName.Text.Trim(),
            CategoryId = Convert.ToInt32(cmbCategory.SelectedValue),
            Unit = txtUnit.Text.Trim(),
            Price = decimal.TryParse(txtPrice.Text, out var pr) ? pr : 0,
            Cost = decimal.TryParse(txtCost.Text, out var co) ? co : 0,
            Stock = int.TryParse(txtStock.Text, out var st) ? st : 0,
            MinStock = int.TryParse(txtMinStock.Text, out var ms) ? ms : 10,
            SupplierId = Convert.ToInt32(cmbSupplier.SelectedValue),
            Barcode = txtBarcode.Text.Trim()
        };

        try
        {
            if (_product == null)
                ProductBLL.Add(p);
            else
            {
                p.Id = _product.Id;
                ProductBLL.Update(p);
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"保存失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
