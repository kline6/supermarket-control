using SupermarketMS.Models;

namespace SupermarketMS.UI;

public class MainForm : Form
{
    private readonly User _currentUser;
    private MenuStrip menuStrip = null!;
    private StatusStrip statusStrip = null!;
    private ToolStripStatusLabel lblUser = null!;
    private Panel contentPanel = null!;

    public MainForm(User user)
    {
        _currentUser = user;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "超市商品管理系统";
        this.Size = new Size(1100, 700);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.WhiteSmoke;

        // 菜单栏
        menuStrip = new MenuStrip
        {
            Font = new Font("微软雅黑", 10),
            BackColor = Color.DarkSlateBlue,
            ForeColor = Color.White
        };

        var menuBasic = new ToolStripMenuItem("基础资料");
        menuBasic.DropDownItems.AddRange([
            CreateMenuItem("商品管理", MenuItem_Click),
            CreateMenuItem("商品分类", MenuItem_Click),
            CreateMenuItem("供应商管理", MenuItem_Click)
        ]);

        var menuBiz = new ToolStripMenuItem("业务管理");
        menuBiz.DropDownItems.AddRange([
            CreateMenuItem("进货管理", MenuItem_Click),
            CreateMenuItem("销售收银", MenuItem_Click)
        ]);

        var menuReport = new ToolStripMenuItem("查询统计");
        menuReport.DropDownItems.AddRange([
            CreateMenuItem("库存查询", MenuItem_Click),
            CreateMenuItem("销售报表", MenuItem_Click)
        ]);

        var menuSys = new ToolStripMenuItem("系统管理");
        menuSys.DropDownItems.AddRange([
            CreateMenuItem("用户管理", MenuItem_Click),
            new ToolStripSeparator(),
            CreateMenuItem("退出系统", (s, e) => Application.Exit())
        ]);

        menuStrip.Items.AddRange([menuBasic, menuBiz, menuReport, menuSys]);
        this.MainMenuStrip = menuStrip;

        // 内容面板
        contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };

        // 状态栏
        statusStrip = new StatusStrip();
        lblUser = new ToolStripStatusLabel($"当前用户：{_currentUser.RealName}({_currentUser.Role})");
        var lblTime = new ToolStripStatusLabel(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        statusStrip.Items.AddRange([lblUser, new ToolStripStatusLabel("  |  "), lblTime]);

        this.Controls.Add(contentPanel);
        this.Controls.Add(menuStrip);
        this.Controls.Add(statusStrip);

        // 定时器更新时钟
        var timer = new System.Windows.Forms.Timer { Interval = 1000 };
        timer.Tick += (s, e) => lblTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        timer.Start();

        // 默认显示商品管理
        ShowFormInPanel(new ProductForm());
    }

    private ToolStripMenuItem CreateMenuItem(string text, EventHandler handler)
    {
        var item = new ToolStripMenuItem(text)
        {
            ForeColor = Color.DarkSlateBlue,
            BackColor = Color.White
        };
        item.Click += handler;
        return item;
    }

    private void MenuItem_Click(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem item) return;

        Form form = item.Text switch
        {
            "商品管理" => new ProductForm(),
            "商品分类" => new CategoryForm(),
            "供应商管理" => new SupplierForm(),
            "进货管理" => new PurchaseForm(_currentUser.RealName),
            "销售收银" => new SalesForm(_currentUser.RealName),
            "库存查询" => new InventoryForm(),
            "销售报表" => new ReportForm(),
            "用户管理" => new UserForm(),
            _ => null!
        };

        if (form != null) ShowFormInPanel(form);
    }

    private void ShowFormInPanel(Form form)
    {
        contentPanel.Controls.Clear();
        form.TopLevel = false;
        form.FormBorderStyle = FormBorderStyle.None;
        form.Dock = DockStyle.Fill;
        contentPanel.Controls.Add(form);
        form.Show();
    }
}
