using System.Data;
using SupermarketMS.BLL;

namespace SupermarketMS.UI;

public class ReportForm : Form
{
    private DateTimePicker dtpStart = null!;
    private DateTimePicker dtpEnd = null!;
    private DataGridView dgvReport = null!;
    private ComboBox cmbReportType = null!;
    private Label lblSummary = null!;

    public ReportForm()
    {
        InitializeComponent();
        LoadReport();
    }

    private void InitializeComponent()
    {
        this.BackColor = Color.White;

        var topBar = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = Color.WhiteSmoke };
        topBar.Controls.Add(new Label
        {
            Text = "销售报表",
            Font = new Font("微软雅黑", 14, FontStyle.Bold),
            ForeColor = Color.DarkSlateBlue,
            Location = new Point(15, 14), AutoSize = true
        });

        var filterPanel = new Panel
        {
            Dock = DockStyle.Top, Height = 40, BackColor = Color.WhiteSmoke,
            Padding = new Padding(5, 5, 5, 5)
        };

        var lblType = new Label { Text = "报表类型：", Location = new Point(10, 10), AutoSize = true, Font = new Font("微软雅黑", 9) };
        cmbReportType = new ComboBox
        {
            Location = new Point(90, 7), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("微软雅黑", 9)
        };
        cmbReportType.Items.AddRange(["每日销售汇总", "月度销售统计", "热销商品排行", "分类销售统计"]);
        cmbReportType.SelectedIndex = 0;

        var lblStart = new Label { Text = "开始：", Location = new Point(260, 10), AutoSize = true, Font = new Font("微软雅黑", 9) };
        dtpStart = new DateTimePicker
        {
            Location = new Point(305, 7), Width = 130, Font = new Font("微软雅黑", 9),
            Format = DateTimePickerFormat.Short, Value = DateTime.Now.AddDays(-30)
        };
        var lblEnd = new Label { Text = "结束：", Location = new Point(445, 10), AutoSize = true, Font = new Font("微软雅黑", 9) };
        dtpEnd = new DateTimePicker
        {
            Location = new Point(490, 7), Width = 130, Font = new Font("微软雅黑", 9),
            Format = DateTimePickerFormat.Short, Value = DateTime.Now
        };
        var btnQuery = new Button
        {
            Text = "查询", Location = new Point(630, 5), Size = new Size(60, 28),
            BackColor = Color.DarkSlateBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat
        };
        btnQuery.FlatAppearance.BorderSize = 0;
        btnQuery.Click += (s, e) => LoadReport();

        filterPanel.Controls.AddRange([lblType, cmbReportType, lblStart, dtpStart, lblEnd, dtpEnd, btnQuery]);

        lblSummary = new Label
        {
            Dock = DockStyle.Top, Height = 35, Font = new Font("微软雅黑", 11, FontStyle.Bold),
            ForeColor = Color.DarkSlateBlue, BackColor = Color.WhiteSmoke,
            Padding = new Padding(10, 8, 0, 0)
        };

        dgvReport = new DataGridView
        {
            Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
            ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false, BackgroundColor = Color.White, Font = new Font("微软雅黑", 9)
        };

        var chartPanel = new Panel
        {
            Dock = DockStyle.Right, Width = 350, BackColor = Color.White,
            Padding = new Padding(5)
        };
        var lblChart = new Label
        {
            Text = "图表视图",
            Dock = DockStyle.Top, Height = 25, Font = new Font("微软雅黑", 10, FontStyle.Bold),
            ForeColor = Color.DarkSlateBlue
        };
        var picChart = new PictureBox
        {
            Dock = DockStyle.Fill, BackColor = Color.WhiteSmoke,
            SizeMode = PictureBoxSizeMode.Zoom
        };
        picChart.Paint += PicChart_Paint;
        chartPanel.Controls.Add(picChart);
        chartPanel.Controls.Add(lblChart);

        this.Controls.Add(dgvReport);
        this.Controls.Add(chartPanel);
        this.Controls.Add(lblSummary);
        this.Controls.Add(filterPanel);
        this.Controls.Add(topBar);
    }

    private void LoadReport()
    {
        var start = dtpStart.Value.Date;
        var end = dtpEnd.Value.Date;

        DataTable dt;
        switch (cmbReportType.SelectedIndex)
        {
            case 0: // 每日销售汇总
                dt = SalesBLL.GetMonthlyReport(start.Year, start.Month);
                break;
            case 1: // 月度销售统计
                dt = SalesBLL.GetMonthlyReport(start.Year, start.Month);
                break;
            case 2: // 热销商品排行
                dt = SalesBLL.GetTopProducts(start, end);
                break;
            case 3: // 分类销售统计
                dt = SalesBLL.GetCategorySales(start, end);
                break;
            default:
                dt = new DataTable();
                break;
        }

        dgvReport.DataSource = dt;

        decimal totalAmount = 0;
        int totalCount = 0;
        if (dt.Columns.Contains("TotalAmount"))
        {
            foreach (DataRow row in dt.Rows)
                totalAmount += row["TotalAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TotalAmount"]);
        }
        if (dt.Columns.Contains("OrderCount"))
        {
            foreach (DataRow row in dt.Rows)
                totalCount += row["OrderCount"] == DBNull.Value ? 0 : Convert.ToInt32(row["OrderCount"]);
        }

        lblSummary.Text = cmbReportType.SelectedIndex switch
        {
            0 or 1 => $"  销售总额：¥ {totalAmount:0.00}    订单数：{totalCount}    日期：{start:yyyy-MM-dd} ~ {end:yyyy-MM-dd}",
            2 => $"  热销商品排行    日期：{start:yyyy-MM-dd} ~ {end:yyyy-MM-dd}",
            3 => $"  分类销售统计    日期：{start:yyyy-MM-dd} ~ {end:yyyy-MM-dd}",
            _ => ""
        };

        if (dgvReport.Columns["TotalAmount"] != null)
            dgvReport.Columns["TotalAmount"].DefaultCellStyle.Format = "0.00";
        if (dgvReport.Columns["Amount"] != null)
            dgvReport.Columns["Amount"].DefaultCellStyle.Format = "0.00";
    }

    private void PicChart_Paint(object? sender, PaintEventArgs e)
    {
        if (dgvReport.Rows.Count == 0) return;

        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        var rect = e.ClipRectangle;
        var barWidth = Math.Min(40, (rect.Width - 80) / Math.Max(1, dgvReport.Rows.Count));

        double maxVal = 1;
        foreach (DataGridViewRow row in dgvReport.Rows)
        {
            if (row.Cells.Count > 1 && double.TryParse(row.Cells[1].Value?.ToString(), out var v))
                maxVal = Math.Max(maxVal, v);
        }

        var colors = new[] { Color.DarkSlateBlue, Color.Teal, Color.MediumSeaGreen,
                             Color.Orange, Color.Tomato, Color.MediumPurple,
                             Color.SteelBlue, Color.Goldenrod, Color.Crimson };

        int x = 50;
        int i = 0;
        foreach (DataGridViewRow row in dgvReport.Rows)
        {
            if (row.Cells.Count < 2) continue;
            var label = row.Cells[0].Value?.ToString() ?? "";
            if (label.Length > 6) label = label[..6] + "..";

            if (double.TryParse(row.Cells[1].Value?.ToString(), out var val))
            {
                var barH = (int)((val / maxVal) * (rect.Height - 80));
                var y = rect.Height - 40 - barH;
                var color = colors[i % colors.Length];

                using var brush = new SolidBrush(color);
                g.FillRectangle(brush, x, y, barWidth - 4, barH);
                g.DrawString($"¥{val:0}", new Font("微软雅黑", 7), Brushes.Black, x, y - 14);
                g.DrawString(label, new Font("微软雅黑", 7), Brushes.Black, x, rect.Height - 36);
            }
            x += barWidth;
            i++;
        }
    }
}
