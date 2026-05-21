using System.Data;
using SupermarketMS.BLL;

namespace SupermarketMS.UI;

public class UserForm : Form
{
    private DataGridView dgv = null!;

    public UserForm()
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
            Text = "用户管理",
            Font = new Font("微软雅黑", 14, FontStyle.Bold),
            ForeColor = Color.DarkSlateBlue,
            Location = new Point(15, 12), AutoSize = true
        });

        var toolBar = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.WhiteSmoke };
        var btnAdd = CreateBtn("新增用户", 10);
        var btnDelete = CreateBtn("删除用户", 90);
        btnAdd.Click += (s, e) => ShowAddDialog();
        btnDelete.Click += (s, e) =>
        {
            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("请先选择用户"); return; }
            var name = dgv.SelectedRows[0].Cells["Username"].Value?.ToString();
            if (name == "admin") { MessageBox.Show("不能删除admin管理员！"); return; }
            if (MessageBox.Show($"确定删除用户「{name}」？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                UserBLL.DeleteUser(Convert.ToInt32(dgv.SelectedRows[0].Cells["Id"].Value));
                LoadData();
            }
        };
        toolBar.Controls.AddRange([btnAdd, btnDelete]);

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

    private void LoadData() => dgv.DataSource = UserBLL.GetAllUsers();

    private void ShowAddDialog()
    {
        var form = new Form
        {
            Text = "新增用户", Size = new Size(350, 250),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false
        };

        var lblUser = new Label { Text = "用户名：", Location = new Point(30, 25), AutoSize = true };
        var txtUser = new TextBox { Location = new Point(110, 22), Width = 180 };
        var lblPwd = new Label { Text = "密  码：", Location = new Point(30, 65), AutoSize = true };
        var txtPwd = new TextBox { Location = new Point(110, 62), Width = 180 };
        var lblName = new Label { Text = "真实姓名：", Location = new Point(30, 105), AutoSize = true };
        var txtName = new TextBox { Location = new Point(110, 102), Width = 180 };
        var lblRole = new Label { Text = "角  色：", Location = new Point(30, 145), AutoSize = true };
        var cmbRole = new ComboBox
        {
            Location = new Point(110, 142), Width = 180,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbRole.Items.AddRange(["user", "admin"]);
        cmbRole.SelectedIndex = 0;

        var btnOk = new Button
        {
            Text = "保存", Location = new Point(110, 185), Size = new Size(75, 30),
            BackColor = Color.DarkSlateBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat
        };
        btnOk.FlatAppearance.BorderSize = 0;
        btnOk.Click += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(txtUser.Text) || string.IsNullOrWhiteSpace(txtPwd.Text))
            { MessageBox.Show("用户名和密码不能为空"); return; }
            UserBLL.AddUser(new Models.User
            {
                Username = txtUser.Text,
                Password = txtPwd.Text,
                RealName = txtName.Text,
                Role = cmbRole.SelectedItem?.ToString() ?? "user"
            });
            form.DialogResult = DialogResult.OK; form.Close();
        };

        var btnCancel = new Button
        {
            Text = "取消", Location = new Point(200, 185), Size = new Size(75, 30),
            FlatStyle = FlatStyle.Flat
        };
        btnCancel.Click += (s, e) => { form.DialogResult = DialogResult.Cancel; form.Close(); };

        form.Controls.AddRange([lblUser, txtUser, lblPwd, txtPwd, lblName, txtName, lblRole, cmbRole, btnOk, btnCancel]);
        if (form.ShowDialog() == DialogResult.OK) LoadData();
    }
}
