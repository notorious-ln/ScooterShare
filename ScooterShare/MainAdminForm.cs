using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using System.Windows.Forms.Integration; // for ElementHost
using WpfMaterialControls.ViewModels; // WPF Material view
using WpfMaterialControls; // WPF windows
using System.Windows.Interop;
using System.Globalization;



namespace ScooterShare
{
    public partial class MainAdminForm : Form
    {
        // hovered cell for action icons (for hover effect)
        private int dgvHoverRow = -1;
        private int dgvHoverCol = -1;
        private TableLayoutPanel dashboardLayout;
        private TableLayoutPanel dashboardCardsLayout;
        private TableLayoutPanel dashboardChartsLayout;
        private Panel dashboardActivityContainer;

        public MainAdminForm()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            // subscribe to DB activity events so UI can refresh
            DatabaseHelper.ActivityLogged += OnActivityLogged;
            this.FormClosed += (s, e) => { try { DatabaseHelper.ActivityLogged -= OnActivityLogged; } catch { } };
        }

        private string GetRelativeTime(DateTime dt)
        {
            var span = DateTime.Now - dt;
            if (span.TotalSeconds < 60) return "только что";
            if (span.TotalMinutes < 60) return (int)span.TotalMinutes + " минут назад";
            if (span.TotalHours < 24) return (int)span.TotalHours + " часов назад";
            if (span.TotalDays < 7) return (int)span.TotalDays + " дней назад";
            return dt.ToString("dd.MM.yyyy HH:mm");
        }

        private void OnActivityLogged()
        {
            try
            {
                if (this.IsHandleCreated)
                {
                    this.BeginInvoke((Action)(() => {
                        try { LoadRecentActivity(); } catch { }
                        try { LoadDashboardData(); } catch { }
                    }));
                }
            }
            catch { }
        }

        private void MainAdminForm_Load(object sender, EventArgs e)
        {
            // Подгружаем данные и визуальные графики
            ShowDashboard();

            // Установим активную кнопку (дашборд)
            SetActiveSidebar(btnDashboard);
            // diagnostic: button to check activity log
            var diag = new Button { Text = "Проверить лог", Size = new Size(120, 28), Location = new Point(300, 10) };
            diag.Click += (s, ev) => {
                try
                {
                    DatabaseHelper.EnsureActivityTableExists();
                    var dt = DatabaseHelper.ExecuteQuery("SELECT COUNT(*) cnt, MAX(activity_time) last FROM ActivityLog");
                    int cnt = dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0]["cnt"]) : 0;
                    var last = dt.Rows.Count > 0 ? dt.Rows[0]["last"] : null;
                    MessageBox.Show($"ActivityLog count={cnt}\nlast={last}\nlastError={DatabaseHelper.ActivityLastError}", "Диагностика", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };
            panelHeader.Controls.Add(diag);
        }

        private void SetActiveSidebar(Button active)
        {
            // сбрасываем все
            foreach (Control c in panelSidebar.Controls)
            {
                if (c is Button b && b != btnReturnApp)
                {
                    b.BackColor = Color.Transparent;
                    b.ForeColor = Color.FromArgb(90, 100, 110);
                    b.Font = new Font("Segoe UI", 11.5F, FontStyle.Regular);
                }
            }

            if (active != null)
            {
                active.BackColor = Color.FromArgb(236, 242, 247);
                active.ForeColor = Color.FromArgb(12, 10, 23);
                active.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);
            }
        }

        private void SetRoundedRegion(Control ctrl, int radius)
        {
            if (ctrl == null || ctrl.Width == 0 || ctrl.Height == 0) return;
            var path = new GraphicsPath();
            var rect = new Rectangle(0, 0, ctrl.Width, ctrl.Height);
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            ctrl.Region = new Region(path);

            
            ctrl.Padding = new Padding(8);
        }

        private void RefreshDashboardRoundedRegions()
        {
            SetRoundedRegion(cardUsers, 12);
            SetRoundedRegion(cardScooters, 12);
            SetRoundedRegion(cardRidesToday, 12);
            SetRoundedRegion(cardRevenue, 12);
            SetRoundedRegion(panelWeekChart, 10);
            SetRoundedRegion(panelRevenueChart, 10);
            SetRoundedRegion(flowActivityPanel, 10);
        }

        private void PanelContent_ResizeDashboard(object sender, EventArgs e)
        {
            if (dashboardLayout == null || !dashboardLayout.Visible)
            {
                return;
            }

            RefreshDashboardRoundedRegions();
        }

        private void ReplacePanelContent(Control control)
        {
            panelContent.SuspendLayout();
            panelContent.Controls.Clear();
            control.Dock = DockStyle.Fill;
            panelContent.Controls.Add(control);
            panelContent.ResumeLayout(true);
        }

        private void LoadDashboardData()
        {
            try
            {
                // Users total
                var usersTotalObj = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Clients");
                int usersTotal = usersTotalObj == null || usersTotalObj == DBNull.Value ? 0 : Convert.ToInt32(usersTotalObj);
                lblTotalUsers.Text = FormatNumber(usersTotal);

                // Available scooters based on current operational status
                var activeObj = DatabaseHelper.ExecuteScalar(@"
IF COL_LENGTH('Scooters', 'operational_status') IS NOT NULL
    SELECT COUNT(*) FROM Scooters WHERE operational_status = N'Доступен';
ELSE
    SELECT COUNT(*) FROM Scooters WHERE condition_id = 1;");
                int activeScooters = activeObj == null || activeObj == DBNull.Value ? 0 : Convert.ToInt32(activeObj);
                lblActiveScooters.Text = activeScooters.ToString();

                // Rides today
                var ridesTodayObj = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Active_rentals WHERE CAST(start_time AS DATE) = CAST(GETDATE() AS DATE)");
                int ridesToday = ridesTodayObj == null || ridesTodayObj == DBNull.Value ? 0 : Convert.ToInt32(ridesTodayObj);
                lblRidesToday.Text = ridesToday.ToString();

                // Revenue this month based on rentals and current tariffs
                var revObj = DatabaseHelper.ExecuteScalar(@"
SELECT ISNULL(SUM(
    CASE
        WHEN ar.start_time IS NULL THEN 0
        ELSE DATEDIFF(MINUTE, ar.start_time, ISNULL(ar.plannedfFinishTime, GETDATE())) * ISNULL(r.costPerMinute, 0)
    END
), 0)
FROM Active_rentals ar
LEFT JOIN Rates r ON r.rate_id = ar.rate_id
WHERE MONTH(ar.start_time) = MONTH(GETDATE()) AND YEAR(ar.start_time) = YEAR(GETDATE());");
                decimal revenue = revObj == null || revObj == DBNull.Value ? 0 : Convert.ToDecimal(revObj);
                lblRevenue.Text = $"₽{revenue:N0}";

                // Compute change metrics
                // Users: this month vs previous month (if possible)
                try
                {
                    string dateCol = DatabaseHelper.FindDateColumn("Clients");
                    if (!string.IsNullOrEmpty(dateCol))
                    {
                        var cur = DatabaseHelper.ExecuteScalar($"SELECT COUNT(*) FROM Clients WHERE MONTH([{dateCol}]) = MONTH(GETDATE()) AND YEAR([{dateCol}]) = YEAR(GETDATE())");
                        var prev = DatabaseHelper.ExecuteScalar($"SELECT COUNT(*) FROM Clients WHERE MONTH([{dateCol}]) = MONTH(DATEADD(month,-1,GETDATE())) AND YEAR([{dateCol}]) = YEAR(DATEADD(month,-1,GETDATE()))");
                        int curI = cur == null || cur == DBNull.Value ? 0 : Convert.ToInt32(cur);
                        int prevI = prev == null || prev == DBNull.Value ? 0 : Convert.ToInt32(prev);
                        if (prevI == 0) lblUsersChange.Text = curI > 0 ? $"+{curI * 100}% за месяц" : "0%";
                        else
                        {
                            double p = (curI - prevI) / (double)prevI * 100.0;
                            lblUsersChange.Text = (p >= 0 ? "+" : "") + $"{p:N0}% за месяц";
                        }
                    }
                    else lblUsersChange.Text = "";
                }
                catch { lblUsersChange.Text = ""; }

                // Scooters: percent of available among total
                try
                {
                    var totObj = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Scooters");
                    int tot = totObj == null || totObj == DBNull.Value ? 0 : Convert.ToInt32(totObj);
                    if (tot > 0)
                    {
                        double perc = activeScooters / (double)tot * 100.0;
                        lblScootersChange.Text = $"{perc:N0}% доступных";
                    }
                    else lblScootersChange.Text = "0%";
                }
                catch { lblScootersChange.Text = ""; }

                // Rides: today vs yesterday
                try
                {
                    var yesterdayObj = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Active_rentals WHERE CAST(start_time AS DATE) = CAST(DATEADD(day, -1, GETDATE()) AS DATE)");
                    int yest = yesterdayObj == null || yesterdayObj == DBNull.Value ? 0 : Convert.ToInt32(yesterdayObj);
                    if (yest == 0) lblRidesChange.Text = ridesToday > 0 ? $"+{ridesToday * 100}% со вчера" : "0%";
                    else
                    {
                        double p = (ridesToday - yest) / (double)yest * 100.0;
                        lblRidesChange.Text = (p >= 0 ? "+" : "") + $"{p:N0}% со вчера";
                    }
                }
                catch { lblRidesChange.Text = ""; }

                // Revenue: this month vs previous month
                try
                {
                    var prevRevObj = DatabaseHelper.ExecuteScalar(@"
SELECT ISNULL(SUM(
    CASE
        WHEN ar.start_time IS NULL THEN 0
        ELSE DATEDIFF(MINUTE, ar.start_time, ISNULL(ar.plannedfFinishTime, GETDATE())) * ISNULL(r.costPerMinute, 0)
    END
), 0)
FROM Active_rentals ar
LEFT JOIN Rates r ON r.rate_id = ar.rate_id
WHERE MONTH(ar.start_time) = MONTH(DATEADD(month,-1,GETDATE()))
  AND YEAR(ar.start_time) = YEAR(DATEADD(month,-1,GETDATE()));");
                    decimal prevRev = prevRevObj == null || prevRevObj == DBNull.Value ? 0 : Convert.ToDecimal(prevRevObj);
                    if (prevRev == 0) lblRevenueChange.Text = revenue > 0 ? $"+{(revenue * 100):N0}% за месяц" : "0%";
                    else
                    {
                        double p = (double)((revenue - prevRev) / prevRev * 100M);
                        lblRevenueChange.Text = (p >= 0 ? "+" : "") + $"{p:N0}% за месяц";
                    }
                }
                catch { lblRevenueChange.Text = ""; }
            }
            catch
            {
                lblTotalUsers.Text = "2 847";
                lblActiveScooters.Text = "156";
                lblRidesToday.Text = "1 523";
                lblRevenue.Text = "1 245 000 ₽";
            }
        }

        private string FormatNumber(int number) => number.ToString("N0").Replace(",", " ");

        private void LoadWeekChart()
        {
            panelWeekChart.Controls.Clear();
            var chartData = GetWeeklyRideChartData();
            Panel chart = CreateBarChart(chartData.Values, chartData.Labels, "Поездки за последние 7 дней");
            panelWeekChart.Controls.Add(chart);
        }

        private void LoadRevenueChart()
        {
            panelRevenueChart.Controls.Clear();
            var chartData = GetRevenueTrendChartData();
            Panel chart = CreateLineChart(chartData.Values, chartData.Labels, "Доход по месяцам");
            panelRevenueChart.Controls.Add(chart);
        }

        private (int[] Values, string[] Labels) GetWeeklyRideChartData()
        {
            var today = DateTime.Today;
            var firstDay = today.AddDays(-6);
            var labels = Enumerable.Range(0, 7)
                .Select(offset => firstDay.AddDays(offset).ToString("dd.MM"))
                .ToArray();

            var valuesByDate = new Dictionary<DateTime, int>();
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery(@"
SELECT CAST(start_time AS DATE) AS ride_date, COUNT(*) AS ride_count
FROM Active_rentals
WHERE start_time >= @startDate AND start_time < DATEADD(day, 1, @endDate)
GROUP BY CAST(start_time AS DATE);",
                    new[]
                    {
                        new SqlParameter("@startDate", firstDay),
                        new SqlParameter("@endDate", today)
                    });

                foreach (DataRow row in dt.Rows)
                {
                    DateTime date = Convert.ToDateTime(row["ride_date"]).Date;
                    valuesByDate[date] = Convert.ToInt32(row["ride_count"]);
                }
            }
            catch
            {
                // Fall back to zeros if DB data is temporarily unavailable.
            }

            int[] values = Enumerable.Range(0, 7)
                .Select(offset =>
                {
                    DateTime date = firstDay.AddDays(offset);
                    return valuesByDate.TryGetValue(date, out int count) ? count : 0;
                })
                .ToArray();

            return (values, labels);
        }

        private (decimal[] Values, string[] Labels) GetRevenueTrendChartData()
        {
            var monthStarts = Enumerable.Range(0, 6)
                .Select(offset => new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-5 + offset))
                .ToArray();
            var labels = monthStarts.Select(month => month.ToString("MMM")).ToArray();
            var revenueByMonth = monthStarts.ToDictionary(month => month, _ => 0m);

            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery(@"
SELECT
    DATEFROMPARTS(YEAR(ar.start_time), MONTH(ar.start_time), 1) AS month_start,
    ISNULL(SUM(
        CASE
            WHEN ar.start_time IS NULL THEN 0
            ELSE DATEDIFF(MINUTE, ar.start_time, ISNULL(ar.plannedfFinishTime, GETDATE())) * ISNULL(r.costPerMinute, 0)
        END
    ), 0) AS revenue_sum
FROM Active_rentals ar
LEFT JOIN Rates r ON r.rate_id = ar.rate_id
WHERE ar.start_time >= @fromDate
GROUP BY DATEFROMPARTS(YEAR(ar.start_time), MONTH(ar.start_time), 1)
ORDER BY month_start;",
                    new[] { new SqlParameter("@fromDate", monthStarts.First()) });

                foreach (DataRow row in dt.Rows)
                {
                    DateTime monthStart = Convert.ToDateTime(row["month_start"]).Date;
                    revenueByMonth[monthStart] = Convert.ToDecimal(row["revenue_sum"]);
                }
            }
            catch
            {
                // Fall back to zeros if DB data is temporarily unavailable.
            }

            decimal[] values = monthStarts.Select(month => revenueByMonth[month]).ToArray();
            return (values, labels);
        }

        private Panel CreateBarChart(int[] values, string[] labels, string title)
        {
            // Создаём панель с собственным рендером, приближённым к макету
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            // Включаем двойную буферизацию через рефлексию (DoubleBuffered - защищённый член)
            typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(panel, true, null);
            int paddingLeft = 40;
            int paddingBottom = 40;
            int paddingTop = 30;
            int hoveredIndex = -1;
            Point hoveredPoint = Point.Empty;

            Rectangle GetPlotRectangle()
            {
                int w = panel.ClientSize.Width;
                int h = panel.ClientSize.Height;
                return new Rectangle(paddingLeft, paddingTop + 20, w - paddingLeft - 20, h - paddingTop - paddingBottom - 20);
            }

            RectangleF[] GetBarRectangles()
            {
                Rectangle plot = GetPlotRectangle();
                if (values == null || values.Length == 0 || plot.Width <= 0 || plot.Height <= 0)
                {
                    return Array.Empty<RectangleF>();
                }

                int count = values.Length;
                int max = Math.Max(1, values.DefaultIfEmpty(0).Max());
                int spacing = Math.Max(20, plot.Width / Math.Max(1, count * 2));
                int barWidth = Math.Min(60, Math.Max(18, (plot.Width / count) - spacing / 2));
                var result = new RectangleF[count];

                for (int i = 0; i < count; i++)
                {
                    float x = plot.Left + i * (plot.Width / (float)count) + (plot.Width / (float)count - barWidth) / 2f;
                    float barHeight = (values[i] / (float)max) * (plot.Height - 20);
                    float y = plot.Bottom - barHeight;
                    result[i] = new RectangleF(x, y, barWidth, barHeight);
                }

                return result;
            }

            int HitTestBar(Point location)
            {
                var bars = GetBarRectangles();
                for (int i = 0; i < bars.Length; i++)
                {
                    RectangleF hitRect = bars[i];
                    hitRect.Inflate(8, 8);
                    if (hitRect.Contains(location))
                    {
                        return i;
                    }
                }

                return -1;
            }

            panel.MouseMove += (s, e) =>
            {
                hoveredPoint = e.Location;
                int newHovered = HitTestBar(e.Location);
                if (newHovered != hoveredIndex)
                {
                    hoveredIndex = newHovered;
                    panel.Invalidate();
                }
            };

            panel.MouseLeave += (s, e) =>
            {
                hoveredIndex = -1;
                panel.Invalidate();
            };

            panel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                int w = panel.ClientSize.Width;
                int h = panel.ClientSize.Height;

                // Title
                using (var titleFont = new Font("Segoe UI", 12F, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.FromArgb(44, 62, 80)))
                {
                    g.DrawString(title, titleFont, brush, new PointF(10, 8));
                }

                // plot area
                Rectangle plot = GetPlotRectangle();
                int max = Math.Max(1, values.DefaultIfEmpty(0).Max());
                RectangleF[] barRectangles = GetBarRectangles();

                // grid lines
                using (var penGrid = new Pen(Color.FromArgb(230, 230, 230)))
                {
                    int rows = 4;
                    for (int r = 0; r <= rows; r++)
                    {
                        int yy = plot.Top + r * (plot.Height) / rows;
                        g.DrawLine(penGrid, plot.Left, yy, plot.Right, yy);
                    }
                }

                int count = values.Length;
                int spacing = Math.Max(20, plot.Width / (count * 2));
                int barWidth = Math.Min(60, (plot.Width / count) - spacing / 2);

                for (int i = 0; i < count; i++)
                {
                    float x = plot.Left + i * (plot.Width / count) + (plot.Width / count - barWidth) / 2;
                    float barHeight = (values[i] / (float)max) * (plot.Height - 20);
                    float y = plot.Bottom - barHeight;

                    // rounded bar
                    var rect = barRectangles[i];
                    Color barColor = i == hoveredIndex
                        ? Color.FromArgb(198, 198, 203)
                        : Color.FromArgb(12, 10, 23);
                    using (var p = new SolidBrush(barColor))
                    using (var path = CreateRoundRectPath(rect, 8f))
                    {
                        g.FillPath(p, path);
                    }

                    // day label
                    using (var f = new Font("Segoe UI", 9F))
                    using (var br = new SolidBrush(Color.Gray))
                    {
                        var sz = g.MeasureString(labels[i], f);
                        g.DrawString(labels[i], f, br, x + (barWidth - sz.Width) / 2, plot.Bottom + 6);
                    }
                }

                if (hoveredIndex >= 0 && hoveredIndex < values.Length)
                {
                    RectangleF hoveredBar = barRectangles[hoveredIndex];
                    Point anchor = new Point((int)(hoveredBar.Left + hoveredBar.Width / 2), (int)hoveredBar.Top);
                    DrawChartTooltip(
                        g,
                        panel.ClientRectangle,
                        anchor,
                        labels[hoveredIndex],
                        $"rides : {values[hoveredIndex]}",
                        Color.FromArgb(12, 10, 23),
                        hoveredPoint);
                }
            };

            return panel;
        }

        private Panel CreateLineChart(decimal[] values, string[] labels, string title)
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            // Включаем двойную буферизацию через рефлексию
            typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(panel, true, null);
            int hoveredIndex = -1;
            Point hoveredPoint = Point.Empty;

            Rectangle GetPlotRectangle()
            {
                int w = panel.ClientSize.Width;
                int h = panel.ClientSize.Height;
                return new Rectangle(40, 40, w - 60, h - 80);
            }

            PointF[] GetChartPoints()
            {
                Rectangle plot = GetPlotRectangle();
                if (values == null || values.Length == 0 || plot.Width <= 0 || plot.Height <= 0)
                {
                    return Array.Empty<PointF>();
                }

                decimal max = Math.Max(1M, values.DefaultIfEmpty(0M).Max());
                var points = new PointF[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    float x = values.Length == 1
                        ? plot.Left + plot.Width / 2f
                        : plot.Left + i * (plot.Width / (float)(values.Length - 1));
                    float y = plot.Bottom - ((float)(values[i] / max)) * plot.Height;
                    points[i] = new PointF(x, y);
                }

                return points;
            }

            int HitTestPoint(Point location)
            {
                PointF[] points = GetChartPoints();
                for (int i = 0; i < points.Length; i++)
                {
                    RectangleF hitRect = new RectangleF(points[i].X - 10, points[i].Y - 10, 20, 20);
                    if (hitRect.Contains(location))
                    {
                        return i;
                    }
                }

                return -1;
            }

            panel.MouseMove += (s, e) =>
            {
                hoveredPoint = e.Location;
                int newHovered = HitTestPoint(e.Location);
                if (newHovered != hoveredIndex)
                {
                    hoveredIndex = newHovered;
                    panel.Invalidate();
                }
            };

            panel.MouseLeave += (s, e) =>
            {
                hoveredIndex = -1;
                panel.Invalidate();
            };

            panel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                int w = panel.ClientSize.Width;
                int h = panel.ClientSize.Height;

                using (var titleFont = new Font("Segoe UI", 12F, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.FromArgb(44, 62, 80)))
                {
                    g.DrawString(title, titleFont, brush, new PointF(10, 8));
                }

                Rectangle plot = GetPlotRectangle();

                // grid horizontal
                using (var penGrid = new Pen(Color.FromArgb(235, 235, 235)))
                {
                    int rows = 4;
                    for (int r = 0; r <= rows; r++)
                    {
                        int yy = plot.Top + r * (plot.Height) / rows;
                        g.DrawLine(penGrid, plot.Left, yy, plot.Right, yy);
                    }
                }

                // map values
                decimal max = Math.Max(1M, values.DefaultIfEmpty(0M).Max());
                PointF[] pts = GetChartPoints();

                using (var pen = new Pen(Color.FromArgb(230, 115, 41), 2.5f))
                {
                    if (pts.Length >= 2)
                    {
                        g.DrawLines(pen, pts);
                    }
                }

                // draw points
                using (var br = new SolidBrush(Color.White))
                using (var penPoint = new Pen(Color.FromArgb(230, 115, 41), 2f))
                {
                    for (int i = 0; i < pts.Length; i++)
                    {
                        var pnt = pts[i];
                        float radius = i == hoveredIndex ? 5f : 4f;
                        g.FillEllipse(br, pnt.X - radius, pnt.Y - radius, radius * 2, radius * 2);
                        g.DrawEllipse(penPoint, pnt.X - radius, pnt.Y - radius, radius * 2, radius * 2);
                    }
                }

                // labels
                using (var f = new Font("Segoe UI", 9F))
                using (var br = new SolidBrush(Color.Gray))
                {
                    for (int i = 0; i < labels.Length; i++)
                    {
                        var lbl = labels[i];
                        var x = plot.Left + i * (plot.Width / (float)(labels.Length - 1));
                        g.DrawString(lbl, f, br, x - 12, plot.Bottom + 6);
                    }
                }

                if (hoveredIndex >= 0 && hoveredIndex < pts.Length)
                {
                    using (var guidePen = new Pen(Color.FromArgb(214, 214, 214), 1f))
                    {
                        g.DrawLine(guidePen, pts[hoveredIndex].X, plot.Top, pts[hoveredIndex].X, plot.Bottom);
                    }

                    DrawChartTooltip(
                        g,
                        panel.ClientRectangle,
                        Point.Round(pts[hoveredIndex]),
                        labels[hoveredIndex],
                        $"revenue : {(int)values[hoveredIndex]}",
                        Color.FromArgb(230, 115, 41),
                        hoveredPoint);
                }
            };
            return panel;
        }

        private void DrawChartTooltip(Graphics g, Rectangle bounds, Point anchor, string title, string valueText, Color valueColor, Point mousePoint)
        {
            using (var titleFont = new Font("Segoe UI", 11F, FontStyle.Regular))
            using (var valueFont = new Font("Segoe UI", 10.5F, FontStyle.Regular))
            using (var textBrush = new SolidBrush(Color.FromArgb(34, 34, 34)))
            using (var valueBrush = new SolidBrush(valueColor))
            using (var borderPen = new Pen(Color.FromArgb(214, 214, 214)))
            using (var backBrush = new SolidBrush(Color.White))
            {
                SizeF titleSize = g.MeasureString(title, titleFont);
                SizeF valueSize = g.MeasureString(valueText, valueFont);
                int tooltipWidth = (int)Math.Ceiling(Math.Max(titleSize.Width, valueSize.Width)) + 32;
                int tooltipHeight = (int)Math.Ceiling(titleSize.Height + valueSize.Height) + 28;

                int tooltipX = anchor.X + 18;
                int tooltipY = anchor.Y - tooltipHeight - 12;

                if (tooltipX + tooltipWidth > bounds.Right - 10)
                {
                    tooltipX = anchor.X - tooltipWidth - 18;
                }

                if (tooltipY < bounds.Top + 10)
                {
                    tooltipY = anchor.Y + 18;
                }

                tooltipX = Math.Max(bounds.Left + 10, Math.Min(tooltipX, bounds.Right - tooltipWidth - 10));
                tooltipY = Math.Max(bounds.Top + 10, Math.Min(tooltipY, bounds.Bottom - tooltipHeight - 10));

                var rect = new Rectangle(tooltipX, tooltipY, tooltipWidth, tooltipHeight);
                g.FillRectangle(backBrush, rect);
                g.DrawRectangle(borderPen, rect);

                g.DrawString(title, titleFont, textBrush, rect.Left + 16, rect.Top + 14);
                g.DrawString(valueText, valueFont, valueBrush, rect.Left + 16, rect.Top + 14 + titleSize.Height + 8);
            }
        }

        private GraphicsPath CreateRoundRectPath(RectangleF rect, float radius)
        {
            var path = new GraphicsPath();
            float d = radius * 2f;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void LoadRecentActivity()
        {
            flowActivityPanel.Controls.Clear();

            try
            {
                DatabaseHelper.EnsureActivityTableExists();
                DataTable dt = DatabaseHelper.ExecuteQuery("SELECT TOP 20 message, activity_time FROM ActivityLog WHERE activity_time >= DATEADD(day, -7, GETDATE()) ORDER BY activity_time DESC");
                if (dt.Rows.Count == 0)
                {
                    // fallback example items
                    AddActivityItem("Новый пользователь зарегистрирован: maria.garcia@example.com", "2 минуты назад", Color.FromArgb(46, 204, 113));
                    AddActivityItem("Самокат #47 прошел обслуживание", "15 минут назад", Color.FromArgb(52, 152, 219));
                    AddActivityItem("Высокий спрос в центре города", "32 минуты назад", Color.FromArgb(241, 196, 15));
                    AddActivityItem("Самокат #23 требует зарядки — низкий заряд", "1 час назад", Color.FromArgb(231, 76, 60));
                }
                else
                {
                    foreach (DataRow r in dt.Rows)
                    {
                        DateTime at = Convert.ToDateTime(r["activity_time"]);
                        string rel = GetRelativeTime(at);
                        AddActivityItem(r["message"].ToString(), rel, Color.FromArgb(120, 120, 120));
                    }
                }
            }
            catch
            {
                // fallback
                AddActivityItem("Новый пользователь зарегистрирован: maria.garcia@example.com", "2 минуты назад", Color.FromArgb(46, 204, 113));
                AddActivityItem("Самокат #47 прошел обслуживание", "15 минут назад", Color.FromArgb(52, 152, 219));
                AddActivityItem("Высокий спрос в центре города", "32 минуты назад", Color.FromArgb(241, 196, 15));
                AddActivityItem("Самокат #23 требует зарядки — низкий заряд", "1 час назад", Color.FromArgb(231, 76, 60));
            }
        }
        private void AddActivityItem(string message, string time, Color color)
        {
            // Создаём элемент, адаптированный под FlowLayoutPanel (вертикальный список)
            int cardWidth = Math.Max(100, flowActivityPanel.ClientSize.Width - 24);
            Panel item = new Panel { BackColor = Color.White, Height = 88, Width = cardWidth };
            item.Margin = new Padding(6);

            var sep = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(240, 240, 240) };

            Label msg = new Label { Text = message, AutoSize = false, Height = 48, Font = new Font("Segoe UI", 11F), Left = 12, Top = 8, Width = item.Width - 24 };
            Label t = new Label { Text = time, AutoSize = false, Height = 24, Font = new Font("Segoe UI", 9F), ForeColor = Color.Gray, Left = 12, Top = 56, Width = item.Width - 24 };

            item.Controls.Add(sep);
            item.Controls.Add(msg);
            item.Controls.Add(t);

            flowActivityPanel.Controls.Add(item);
        }

        // Переходы
        private void BtnScooters_Click(object sender, EventArgs e)
        {
            SetActiveSidebar(btnScooters);
            ShowScooters();
        }

        private void BtnUsers_Click(object sender, EventArgs e)
        {
            SetActiveSidebar(btnUsers);
            ShowUsers();
        }

        private void BtnRides_Click(object sender, EventArgs e)
        {
            SetActiveSidebar(btnRides);
            ShowRides();
        }

        private void BtnReports_Click(object sender, EventArgs e)
        {
            SetActiveSidebar(btnReports);
            try
            {
                var win = new ReportsWindow();
                try { new WindowInteropHelper(win).Owner = this.Handle; } catch { }
                win.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть отчёты.\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDashboard_Click(object sender, EventArgs e)
        {
            SetActiveSidebar(btnDashboard);
            // можно добавить переинициализацию дашборда
            ShowDashboard();
            // ensure recent activity is up-to-date when user goes to dashboard
            LoadRecentActivity();
        }

        // Показывает дашборд в правой части (восстановление содержимого panelContent)
        private void ShowDashboard()
        {
            panelContent.SuspendLayout();
            panelContent.Controls.Clear();
            panelContent.Resize -= PanelContent_ResizeDashboard;

            dashboardLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(24, 22, 24, 22),
                ColumnCount = 1,
                RowCount = 3
            };
            dashboardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 184F));
            dashboardLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 48F));
            dashboardLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 52F));

            dashboardCardsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 4,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 20),
                Padding = new Padding(0, 0, 0, 8)
            };
            for (int i = 0; i < 4; i++)
            {
                dashboardCardsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            }

            ConfigureDashboardCard(cardUsers);
            ConfigureDashboardCard(cardScooters);
            ConfigureDashboardCard(cardRidesToday);
            ConfigureDashboardCard(cardRevenue);
            dashboardCardsLayout.Controls.Add(cardUsers, 0, 0);
            dashboardCardsLayout.Controls.Add(cardScooters, 1, 0);
            dashboardCardsLayout.Controls.Add(cardRidesToday, 2, 0);
            dashboardCardsLayout.Controls.Add(cardRevenue, 3, 0);

            dashboardChartsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 18)
            };
            dashboardChartsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            dashboardChartsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            ConfigureDashboardSurface(panelWeekChart, new Padding(0, 0, 12, 0));
            ConfigureDashboardSurface(panelRevenueChart, new Padding(12, 0, 0, 0));
            dashboardChartsLayout.Controls.Add(panelWeekChart, 0, 0);
            dashboardChartsLayout.Controls.Add(panelRevenueChart, 1, 0);

            dashboardActivityContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };

            lblActivityTitle.Dock = DockStyle.Top;
            lblActivityTitle.Height = 38;
            lblActivityTitle.Margin = new Padding(0, 0, 0, 12);

            flowActivityPanel.Dock = DockStyle.Fill;
            flowActivityPanel.BackColor = Color.White;
            flowActivityPanel.FlowDirection = FlowDirection.TopDown;
            flowActivityPanel.WrapContents = false;
            flowActivityPanel.AutoScroll = true;
            flowActivityPanel.Padding = new Padding(8);
            flowActivityPanel.Margin = new Padding(0);
            flowActivityPanel.Resize -= FlowActivityPanel_Resize;
            flowActivityPanel.Resize += FlowActivityPanel_Resize;

            dashboardActivityContainer.Controls.Add(flowActivityPanel);
            dashboardActivityContainer.Controls.Add(lblActivityTitle);

            dashboardLayout.Controls.Add(dashboardCardsLayout, 0, 0);
            dashboardLayout.Controls.Add(dashboardChartsLayout, 0, 1);
            dashboardLayout.Controls.Add(dashboardActivityContainer, 0, 2);
            panelContent.Controls.Add(dashboardLayout);
            panelContent.ResumeLayout(true);

            // Обновляем данные
            LoadDashboardData();
            LoadWeekChart();
            LoadRevenueChart();
            LoadRecentActivity();

            panelContent.Resize += PanelContent_ResizeDashboard;
            RefreshDashboardRoundedRegions();
        }

        private void FlowActivityPanel_Resize(object sender, EventArgs e)
        {
            foreach (Control c in flowActivityPanel.Controls)
            {
                c.Width = Math.Max(100, flowActivityPanel.ClientSize.Width - 24);
            }
        }

        private void ConfigureDashboardCard(Panel card)
        {
            card.Dock = DockStyle.Fill;
            card.Margin = new Padding(0, 0, 16, 0);
        }

        private void ConfigureDashboardSurface(Panel panel, Padding margin)
        {
            panel.Dock = DockStyle.Fill;
            panel.Margin = margin;
            panel.BackColor = Color.White;
        }

        // Показывает список самокатов в panelContent (вместо отдельной формы)
        private void ShowScooters()
        {
            // Use WPF MaterialDesign view hosted inside WinForms ElementHost for pixel-perfect Material UI
            try
            {
                var host = new ElementHost { Dock = DockStyle.Fill, BackColor = Color.Transparent };
                var scootersView = new WpfMaterialControls.ScootersView();
                host.Child = scootersView;
                ReplacePanelContent(host);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить Material WPF view: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowEditScooterDialog(int scooterId)
        {
            // simple edit dialog: load current values, allow edit model, condition and year
            DataTable conds = null;
            try { conds = DatabaseHelper.ExecuteQuery("SELECT condition_id, condition_name FROM Conditions"); }
            catch { conds = new DataTable(); conds.Columns.Add("condition_id"); conds.Columns.Add("condition_name"); }

            DataTable dt = null;
            try { dt = DatabaseHelper.ExecuteQuery("SELECT scooter_id, model, condition_id, yearOfRelease FROM Scooters WHERE scooter_id=@id", new SqlParameter[] { new SqlParameter("@id", scooterId) }); }
            catch { }

            string curModel = dt != null && dt.Rows.Count > 0 ? dt.Rows[0]["model"].ToString() : "";
            int curCond = dt != null && dt.Rows.Count > 0 ? (dt.Rows[0]["condition_id"] == DBNull.Value ? 1 : Convert.ToInt32(dt.Rows[0]["condition_id"]) ) : 1;
            DateTime curYear = dt != null && dt.Rows.Count > 0 && dt.Rows[0]["yearOfRelease"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["yearOfRelease"]) : DateTime.Today;

            // use Guna2 form for edit
            using (Form frm = new Form())
            {
                frm.Text = "Редактировать самокат";
                frm.Width = 520; frm.Height = 260; frm.FormBorderStyle = FormBorderStyle.FixedDialog; frm.StartPosition = FormStartPosition.CenterParent;

                var panel = new Guna2Panel { Dock = DockStyle.Fill, FillColor = Color.WhiteSmoke };
                frm.Controls.Add(panel);

                Label l1 = new Label { Left = 16, Top = 16, Text = "Модель", AutoSize = true };
                Guna2TextBox tbModel = new Guna2TextBox { Left = 16, Top = 40, Width = 480, Text = curModel, BorderRadius = 6 };

                Label l2 = new Label { Left = 16, Top = 84, Text = "Состояние", AutoSize = true };
                Guna2ComboBox cbCond = new Guna2ComboBox { Left = 16, Top = 108, Width = 240, DropDownStyle = ComboBoxStyle.DropDownList, BorderRadius = 6 };
                foreach (DataRow r in conds.Rows) cbCond.Items.Add(new ComboBoxItem(Convert.ToInt32(r["condition_id"]), r["condition_name"].ToString()));
                if (cbCond.Items.Count > 0)
                {
                    for (int i = 0; i < cbCond.Items.Count; i++) if (((ComboBoxItem)cbCond.Items[i]).Id == curCond) { cbCond.SelectedIndex = i; break; }
                    if (cbCond.SelectedIndex < 0) cbCond.SelectedIndex = 0;
                }

                Label l3 = new Label { Left = 280, Top = 84, Text = "Год", AutoSize = true };
                Guna2DateTimePicker dtp = new Guna2DateTimePicker { Left = 280, Top = 108, Width = 216, Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd", Value = curYear, BorderRadius = 6 };

                Guna2Button ok = new Guna2Button { Text = "Сохранить", Left = 320, Width = 80, Top = 168, BorderRadius = 6 };
                Guna2Button cancel = new Guna2Button { Text = "Отмена", Left = 416, Width = 80, Top = 168, BorderRadius = 6 };

                ok.Click += (s, e) => { frm.DialogResult = DialogResult.OK; frm.Close(); };
                cancel.Click += (s, e) => { frm.DialogResult = DialogResult.Cancel; frm.Close(); };

                panel.Controls.Add(l1); panel.Controls.Add(tbModel); panel.Controls.Add(l2); panel.Controls.Add(cbCond); panel.Controls.Add(l3); panel.Controls.Add(dtp); panel.Controls.Add(ok); panel.Controls.Add(cancel);

                if (frm.ShowDialog() == DialogResult.OK)
                {
                    string newModel = tbModel.Text;
                    int newCond = cbCond.SelectedItem is ComboBoxItem ci ? ci.Id : curCond;
                    DateTime newYear = dtp.Value;
                    DatabaseHelper.ExecuteNonQuery("UPDATE Scooters SET model=@m, condition_id=@c, yearOfRelease=@y WHERE scooter_id=@id",
                        new SqlParameter[] { new SqlParameter("@m", newModel), new SqlParameter("@c", newCond), new SqlParameter("@y", newYear), new SqlParameter("@id", scooterId) });
                    try { DatabaseHelper.LogActivity($"Изменён самокат #{scooterId}: модель->{newModel}"); } catch { }
                    ShowScooters();
                }
            }
        }

        private class ComboBoxItem
        {
            public int Id; public string Text;
            public ComboBoxItem(int id, string text) { Id = id; Text = text; }
            public override string ToString() => Text;
        }

        private void ShowUsers()
        {
            try
            {
                var host = new ElementHost { Dock = DockStyle.Fill, BackColor = Color.Transparent };
                var usersView = new WpfMaterialControls.UsersView();
                host.Child = usersView;
                ReplacePanelContent(host);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить Users view: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowRides()
        {
            try
            {
                var host = new ElementHost { Dock = DockStyle.Fill, BackColor = Color.Transparent };
                var ridesView = new WpfMaterialControls.RidesView();
                host.Child = ridesView;
                ReplacePanelContent(host);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить Rides view: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string ShowInputBox(string prompt, string title, string defaultValue = "")
        {
            // Use shared friendly input dialog with inline hints/validation.
            // We keep this wrapper to avoid touching older code paths.
            return UserInputDialog.Prompt(
                null,
                title,
                prompt,
                defaultValue,
                "Введите значение и нажмите OK. Если не уверены — нажмите Отмена.",
                _ => null,
                lettersOnly: false,
                trimResult: true);
        }
    }
}