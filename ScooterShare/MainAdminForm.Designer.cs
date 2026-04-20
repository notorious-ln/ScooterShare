using System.Drawing;
using System.Windows.Forms;


namespace ScooterShare
{
    partial class MainAdminForm
    {
        private System.ComponentModel.IContainer components = null;

        private Panel panelSidebar;
        private Panel panelHeader;
        private Panel panelContent;

        private Label lblTitle;
        private Button btnDashboard, btnScooters, btnUsers, btnRides, btnReports;
        private Button btnReturnApp;

        // Карточки
        private Panel cardUsers, cardScooters, cardRidesToday, cardRevenue;
        private Label lblTotalUsers, lblActiveScooters, lblRidesToday, lblRevenue;
        private Label lblUsersChange, lblScootersChange, lblRidesChange, lblRevenueChange;

        // Графики
        private Panel panelWeekChart;
        private Panel panelRevenueChart;

        // Активность
        private Label lblActivityTitle;
        private FlowLayoutPanel flowActivityPanel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelSidebar = new Panel();
            this.panelHeader = new Panel();
            this.panelContent = new Panel();

            this.lblTitle = new Label();
            this.btnDashboard = new Button();
            this.btnScooters = new Button();
            this.btnUsers = new Button();
            this.btnRides = new Button();
            this.btnReports = new Button();
            this.btnReturnApp = new Button();

            this.lblActivityTitle = new Label();
            this.flowActivityPanel = new FlowLayoutPanel();

            this.panelWeekChart = new Panel();
            this.panelRevenueChart = new Panel();

            this.SuspendLayout();

            // Sidebar — светлый фон, небольшой правый бордер
            this.panelSidebar.BackColor = Color.White;
            this.panelSidebar.Dock = DockStyle.Left;
            this.panelSidebar.Width = 220;
            this.panelSidebar.BorderStyle = BorderStyle.None;
            this.panelSidebar.Padding = new Padding(12);

            this.lblTitle.Text = "⟵  Админ-панель";
            this.lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.FromArgb(34, 34, 34);
            this.lblTitle.Location = new Point(16, 10);
            this.lblTitle.AutoSize = true;

            // Создаём кнопки сайдбара (иконки — простые Unicode-символы для быстрого прототипа)
            CreateSidebarButton(btnDashboard, "📊  Дашборд", 62, true, BtnDashboard_Click);
            CreateSidebarButton(btnScooters, "🛴  Самокаты", 122, false, BtnScooters_Click);
            CreateSidebarButton(btnUsers, "👥  Пользователи", 182, false, BtnUsers_Click);
            CreateSidebarButton(btnRides, "📍  Поездки", 242, false, BtnRides_Click);
            CreateSidebarButton(btnReports, "📄  Отчёты", 302, false, BtnReports_Click);

            // Нижняя кнопка "Вернуться в приложение"
            btnReturnApp.Text = "← Вернуться в приложение";
            btnReturnApp.FlatStyle = FlatStyle.Flat;
            btnReturnApp.FlatAppearance.BorderSize = 0;
            btnReturnApp.BackColor = Color.Transparent;
            btnReturnApp.ForeColor = Color.FromArgb(80, 80, 80);
            btnReturnApp.Font = new Font("Segoe UI", 9F);
            btnReturnApp.Size = new Size(this.panelSidebar.Width - 24, 36);
            btnReturnApp.Location = new Point(12, this.ClientSize.Height - 88);
            btnReturnApp.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnReturnApp.Click += (s, e) => this.Close(); // заглушка — закрывает админку

            this.panelSidebar.Controls.Add(this.lblTitle);
            this.panelSidebar.Controls.Add(this.btnDashboard);
            this.panelSidebar.Controls.Add(this.btnScooters);
            this.panelSidebar.Controls.Add(this.btnUsers);
            this.panelSidebar.Controls.Add(this.btnRides);
            this.panelSidebar.Controls.Add(this.btnReports);
            this.panelSidebar.Controls.Add(this.btnReturnApp);

            // Header — убран контрастный цвет, сделан прозрачным/нейтральным
            this.panelHeader.BackColor = Color.Transparent;
            this.panelHeader.Dock = DockStyle.Top;
            this.panelHeader.Height = 60;

            // Content
            this.panelContent.BackColor = Color.FromArgb(245, 247, 250);
            this.panelContent.Dock = DockStyle.Fill;
            this.panelContent.Padding = new Padding(0);

            // Заголовок панели управления
            Label lblControlPanel = new Label
            {
                Text = "Панель управления",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(34, 34, 34),
                Location = new Point(0, 0),
                AutoSize = true
            };
            this.panelContent.Controls.Add(lblControlPanel);

            // Карточки статистики — создаём через helper (смещены вправо и с отступами)
            CreateModernStatCard(out cardUsers, out lblTotalUsers, out lblUsersChange,
                "Всего пользователей", "2 847", "+12% за месяц", 40, 40);

            CreateModernStatCard(out cardScooters, out lblActiveScooters, out lblScootersChange,
                "Активные самокаты", "156", "+8% за неделю", 360, 40);

            CreateModernStatCard(out cardRidesToday, out lblRidesToday, out lblRidesChange,
                "Поездок сегодня", "1 523", "+15% со вчера", 680, 40);

            CreateModernStatCard(out cardRevenue, out lblRevenue, out lblRevenueChange,
                "Доход (месяц)", "1 245 000 ₽", "+28% за месяц", 1000, 40);

            // Графики — панели с белым фоном и тонкой рамкой
            this.panelWeekChart.Location = new Point(10, 220);
            this.panelWeekChart.Size = new Size(610, 340);
            this.panelWeekChart.BackColor = Color.White;
            this.panelWeekChart.BorderStyle = BorderStyle.None;
            this.panelRevenueChart.Location = new Point(640, 220);
            this.panelRevenueChart.Size = new Size(610, 340);
            this.panelRevenueChart.BackColor = Color.White;
            this.panelRevenueChart.BorderStyle = BorderStyle.None;

            // Активность
            this.lblActivityTitle.Text = "Последняя активность";
            this.lblActivityTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            this.lblActivityTitle.ForeColor = Color.FromArgb(44, 62, 80);
            this.lblActivityTitle.Location = new Point(0, 570);

            this.flowActivityPanel.Location = new Point(0, 610);
            this.flowActivityPanel.Size = new Size(this.panelContent.Width - 20, 260);
            this.flowActivityPanel.BackColor = Color.White;
            this.flowActivityPanel.FlowDirection = FlowDirection.TopDown;
            this.flowActivityPanel.AutoScroll = true;
            this.flowActivityPanel.Padding = new Padding(8);

            // Добавляем элементы в панель контента
            this.panelContent.Controls.Add(cardUsers);
            this.panelContent.Controls.Add(cardScooters);
            this.panelContent.Controls.Add(cardRidesToday);
            this.panelContent.Controls.Add(cardRevenue);
            this.panelContent.Controls.Add(panelWeekChart);
            this.panelContent.Controls.Add(panelRevenueChart);
            this.panelContent.Controls.Add(lblActivityTitle);
            this.panelContent.Controls.Add(flowActivityPanel);

            this.Controls.Add(panelContent);
            this.Controls.Add(panelHeader);
            this.Controls.Add(panelSidebar);

            this.ClientSize = new Size(1350, 820);
            this.Text = "ScooterShare — Админ-панель";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 247, 250);

            // Подпишем Load (в MainAdminForm.cs определим MainAdminForm_Load)
            this.Load += new System.EventHandler(this.MainAdminForm_Load);

            this.ResumeLayout(false);
        }

        private void CreateSidebarButton(Button btn, string text, int y, bool isActive = false, System.EventHandler onClick = null)
        {
            btn.Text = text;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Font = new Font("Segoe UI", 11.5F, FontStyle.Regular);
            btn.ForeColor = isActive ? Color.FromArgb(12, 10, 23) : Color.FromArgb(90, 100, 110);
            btn.BackColor = isActive ? Color.FromArgb(236, 242, 247) : Color.Transparent;
            btn.Location = new Point(8, y);
            btn.Size = new Size(this.panelSidebar.Width - 16, 56);
            btn.Padding = new Padding(16, 0, 0, 0);
            btn.Cursor = Cursors.Hand;
            if (onClick != null) btn.Click += onClick;

            // Hover effect
            btn.MouseEnter += (s, e) => { if (!btn.BackColor.Equals(Color.FromArgb(236, 242, 247))) btn.BackColor = Color.FromArgb(250, 250, 252); };
            btn.MouseLeave += (s, e) => { if (!isActive) btn.BackColor = Color.Transparent; };
        }

        private void CreateModernStatCard(out Panel card, out Label valueLabel, out Label changeLabel,
            string title, string value, string change, int x, int y)
        {
            card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(320, 176),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            Label titleLabel = new Label
            {
                Text = title,
                Location = new Point(22, 20),
                Font = new Font("Segoe UI", 10.5F),
                ForeColor = Color.FromArgb(120, 130, 140),
                AutoSize = true
            };

            valueLabel = new Label
            {
                Text = value,
                Location = new Point(22, 50),
                Font = new Font("Segoe UI", 25F, FontStyle.Bold),
                ForeColor = Color.FromArgb(28, 34, 48),
                AutoSize = true
            };

            changeLabel = new Label
            {
                Text = change,
                Location = new Point(22, 118),
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(46, 204, 113),
                AutoSize = true
            };

            card.Controls.Add(titleLabel);
            card.Controls.Add(valueLabel);
            card.Controls.Add(changeLabel);
        }
    }
}