using System.Data;
using SupermarketMS.BLL;

namespace SupermarketMS.UI;

public class CategoryForm : Form
{
    private DataGridView dgv = null!;
    private Button btnAdd = null!, btnEdit = null!, btnDelete = null!;

    public CategoryForm()
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
            Text = "商品分类管理",
            Font = new Font("微软雅黑", 14, FontStyle.Bold),
            ForeColor = Color.DarkSlateBlue,
            Location = new Point(15, 12),
            AutoSize = true
        });

        var toolBar = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.WhiteSmoke };
        btnAdd = CreateBtn("新增分类", 10);
        btnEdit = CreateBtn("修改分类", 90);
        btnDelete = CreateBtn("删除分类", 170);
        btnAdd.Click += (s, e) => ShowEditDialog(null);
        btnEdit.Click += (s, e) =>
        {
            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("请先选择分类"); return; }
            ShowEditDialog(dgv.SelectedRows[0]);
        };
        btnDelete.Click += (s, e) =>
        {
            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("请先选择分类"); return; }
            var name = dgv.SelectedRows[0].Cells["Name"].Value?.ToString();
            if (MessageBox.Show($"确定删除分类「{name}」？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                CategoryBLL.Delete(Convert.ToInt32(dgv.SelectedRows[0].Cells["Id"].Value));
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
        Text = text, Location = new Point(x, 6), Size = new Size(75, 28),
        BackColor = Color.DarkSlateBlue, ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat, Font = new Font("微软雅黑", 9)
    };

    private void LoadData() => dgv.DataSource = CategoryBLL.GetAll();

    private void ShowEditDialog(DataGridViewRow? row)
    {
        var name = row?.Cells["Name"].Value?.ToString() ?? "";
        var desc = row?.Cells["Description"].Value?.ToString() ?? "";
        var id = row != null ? Convert.ToInt32(row.Cells["Id"].Value) : 0;

        var form = new Form
        {
            Text = id == 0 ? "新增分类" : "修改分类",
            Size = new Size(350, 220),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false
        };

        var lblName = new Label { Text = "分类名称：", Location = new Point(30, 25), AutoSize = true };
        var txtName = new TextBox { Location = new Point(110, 22), Width = 180, Text = name };
        var lblDesc = new Label { Text = "描    述：", Location = new Point(30, 65), AutoSize = true };
        var txtDesc = new TextBox { Location = new Point(110, 62), Width = 180, Text = desc };

        var btnOk = new Button
        {
            Text = "保存", Location = new Point(110, 110), Size = new Size(75, 30),
            BackColor = Color.DarkSlateBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat
        };
        btnOk.FlatAppearance.BorderSize = 0;
        btnOk.Click += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("名称不能为空"); return; }
            if (id == 0) CategoryBLL.Add(new Models.Category { Name = txtName.Text, Description = txtDesc.Text });
            else CategoryBLL.Update(new Models.Category { Id = id, Name = txtName.Text, Description = txtDesc.Text });
            form.DialogResult = DialogResult.OK;
            form.Close();
        };

        var btnCancel = new Button
        {
            Text = "取消", Location = new Point(200, 110), Size = new Size(75, 30),
            FlatStyle = FlatStyle.Flat
        };
        btnCancel.Click += (s, e) => { form.DialogResult = DialogResult.Cancel; form.Close(); };

        form.Controls.AddRange([lblName, txtName, lblDesc, txtDesc, btnOk, btnCancel]);
        if (form.ShowDialog() == DialogResult.OK) LoadData();
    }
}
