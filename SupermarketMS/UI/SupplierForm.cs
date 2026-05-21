using System.Data;
using SupermarketMS.BLL;

namespace SupermarketMS.UI;

public class SupplierForm : Form
{
    private DataGridView dgv = null!;
    private TextBox txtSearch = null!;

    public SupplierForm()
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
            Text = "供应商管理",
            Font = new Font("微软雅黑", 14, FontStyle.Bold),
            ForeColor = Color.DarkSlateBlue,
            Location = new Point(15, 12), AutoSize = true
        });

        txtSearch = new TextBox { Location = new Point(680, 14), Width = 180, Font = new Font("微软雅黑", 9) };
        var btnSearch = new Button
        {
            Text = "搜索", Location = new Point(865, 12), Size = new Size(60, 28),
            BackColor = Color.DarkSlateBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat
        };
        btnSearch.FlatAppearance.BorderSize = 0;
        btnSearch.Click += (s, e) => LoadData();
        topBar.Controls.AddRange([txtSearch, btnSearch]);

        var toolBar = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.WhiteSmoke };
        var btnAdd = CreateBtn("新增供应商", 10);
        var btnEdit = CreateBtn("修改供应商", 90);
        var btnDelete = CreateBtn("删除供应商", 170);

        btnAdd.Click += (s, e) => ShowEditDialog(null);
        btnEdit.Click += (s, e) =>
        {
            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("请先选择供应商"); return; }
            ShowEditDialog(dgv.SelectedRows[0]);
        };
        btnDelete.Click += (s, e) =>
        {
            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("请先选择供应商"); return; }
            var name = dgv.SelectedRows[0].Cells["Name"].Value?.ToString();
            if (MessageBox.Show($"确定删除供应商「{name}」？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                SupplierBLL.Delete(Convert.ToInt32(dgv.SelectedRows[0].Cells["Id"].Value));
                LoadData();
            }
        };
        toolBar.Controls.AddRange([btnAdd, btnEdit, btnDelete]);

        dgv = new DataGridView
        {
            Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
            ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false, BackgroundColor = Color.White, Font = new Font("微软雅黑", 9)
        };

        this.Controls.Add(dgv);
        this.Controls.Add(toolBar);
        this.Controls.Add(topBar);
    }

    private Button CreateBtn(string text, int x) => new()
    {
        Text = text, Location = new Point(x, 6), Size = new Size(80, 28),
        BackColor = Color.DarkSlateBlue, ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat, Font = new Font("微软雅黑", 9)
    };

    private void LoadData()
    {
        var kw = txtSearch.Text.Trim();
        dgv.DataSource = string.IsNullOrEmpty(kw) ? SupplierBLL.GetAll() : SupplierBLL.Search(kw);
    }

    private void ShowEditDialog(DataGridViewRow? row)
    {
        var form = new Form
        {
            Text = row == null ? "新增供应商" : "修改供应商",
            Size = new Size(400, 300),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false
        };

        var lblName = new Label { Text = "供应商名称：", Location = new Point(30, 25), AutoSize = true };
        var txtName = new TextBox { Location = new Point(130, 22), Width = 220, Text = row?.Cells["Name"].Value?.ToString() ?? "" };
        var lblContact = new Label { Text = "联 系 人：", Location = new Point(30, 65), AutoSize = true };
        var txtContact = new TextBox { Location = new Point(130, 62), Width = 220, Text = row?.Cells["ContactPerson"].Value?.ToString() ?? "" };
        var lblPhone = new Label { Text = "联系电话：", Location = new Point(30, 105), AutoSize = true };
        var txtPhone = new TextBox { Location = new Point(130, 102), Width = 220, Text = row?.Cells["Phone"].Value?.ToString() ?? "" };
        var lblAddr = new Label { Text = "地    址：", Location = new Point(30, 145), AutoSize = true };
        var txtAddr = new TextBox { Location = new Point(130, 142), Width = 220, Text = row?.Cells["Address"].Value?.ToString() ?? "" };

        var btnOk = new Button { Text = "保存", Location = new Point(130, 190), Size = new Size(75, 30), BackColor = Color.DarkSlateBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btnOk.FlatAppearance.BorderSize = 0;
        btnOk.Click += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("名称不能为空"); return; }
            var sup = new Models.Supplier { Name = txtName.Text, ContactPerson = txtContact.Text, Phone = txtPhone.Text, Address = txtAddr.Text };
            if (row == null) SupplierBLL.Add(sup);
            else { sup.Id = Convert.ToInt32(row.Cells["Id"].Value); SupplierBLL.Update(sup); }
            form.DialogResult = DialogResult.OK; form.Close();
        };
        var btnCancel = new Button { Text = "取消", Location = new Point(220, 190), Size = new Size(75, 30), FlatStyle = FlatStyle.Flat };
        btnCancel.Click += (s, e) => { form.DialogResult = DialogResult.Cancel; form.Close(); };

        form.Controls.AddRange([lblName, txtName, lblContact, txtContact, lblPhone, txtPhone, lblAddr, txtAddr, btnOk, btnCancel]);
        if (form.ShowDialog() == DialogResult.OK) LoadData();
    }
}
