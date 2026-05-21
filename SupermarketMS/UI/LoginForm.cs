using SupermarketMS.BLL;

namespace SupermarketMS.UI;

public class LoginForm : Form
{
    private TextBox txtUsername = null!;
    private TextBox txtPassword = null!;
    private Button btnLogin = null!;
    private Label lblTitle = null!;

    public LoginForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "超市商品管理系统 - 登录";
        this.Size = new Size(420, 320);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.BackColor = Color.WhiteSmoke;

        lblTitle = new Label
        {
            Text = "超市商品管理系统",
            Font = new Font("微软雅黑", 18, FontStyle.Bold),
            ForeColor = Color.DarkSlateBlue,
            AutoSize = true,
            Location = new Point(80, 30)
        };

        var lblUser = new Label
        {
            Text = "用户名：",
            Font = new Font("微软雅黑", 10),
            Location = new Point(80, 90),
            AutoSize = true
        };

        txtUsername = new TextBox
        {
            Font = new Font("微软雅黑", 10),
            Location = new Point(160, 87),
            Width = 160,
            Text = "admin"
        };

        var lblPwd = new Label
        {
            Text = "密  码：",
            Font = new Font("微软雅黑", 10),
            Location = new Point(80, 130),
            AutoSize = true
        };

        txtPassword = new TextBox
        {
            Font = new Font("微软雅黑", 10),
            Location = new Point(160, 127),
            Width = 160,
            PasswordChar = '*',
            Text = "123456"
        };

        btnLogin = new Button
        {
            Text = "登 录",
            Font = new Font("微软雅黑", 10, FontStyle.Bold),
            Location = new Point(140, 180),
            Size = new Size(120, 36),
            BackColor = Color.DarkSlateBlue,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnLogin.FlatAppearance.BorderSize = 0;
        btnLogin.Click += BtnLogin_Click;

        var lblHint = new Label
        {
            Text = "默认账号：admin  密码：123456",
            Font = new Font("微软雅黑", 8),
            ForeColor = Color.Gray,
            AutoSize = true,
            Location = new Point(110, 230)
        };

        this.Controls.AddRange(new Control[] {
            lblTitle, lblUser, txtUsername, lblPwd, txtPassword, btnLogin, lblHint
        });

        this.AcceptButton = btnLogin;
    }

    private void BtnLogin_Click(object? sender, EventArgs e)
    {
        var user = UserBLL.Login(txtUsername.Text.Trim(), txtPassword.Text.Trim());
        if (user == null)
        {
            MessageBox.Show("用户名或密码错误！", "登录失败",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        this.Hide();
        var mainForm = new MainForm(user);
        mainForm.FormClosed += (s, args) => this.Close();
        mainForm.Show();
    }
}
