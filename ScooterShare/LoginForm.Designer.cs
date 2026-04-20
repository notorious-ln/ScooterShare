using System.Windows.Forms;


namespace ScooterShare
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;

        private Panel pnlBackground;
        private Panel cardPanel;
        private PictureBox pbLogo;
        private Label lblTitle;
        private Label lblSubtitle;

        private Label lblEmailCaption;
        private Panel pnlEmail;
        private PictureBox pbEmailIcon;
        private TextBox txtEmail;

        private Label lblPasswordCaption;
        private Panel pnlPassword;
        private PictureBox pbPasswordIcon;
        private TextBox txtPassword;
        private Button btnTogglePassword;

        private CheckBox chkRemember;
        private LinkLabel lblForgotPassword;

        private Button btnLogin;
        private Label sepLineLeft;
        private Label sepLineRight;
        private Label lblOr;
        private Button btnGuest;

        private LinkLabel lblRegister;
        private LinkLabel lblAdminLink;
        private Label lblTerms;

        // Новые кнопки управления окном
        private Button btnMinimize;
        private Button btnClose;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.pnlBackground = new System.Windows.Forms.Panel();
            this.cardPanel = new System.Windows.Forms.Panel();
            this.pbLogo = new System.Windows.Forms.PictureBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();

            this.lblEmailCaption = new System.Windows.Forms.Label();
            this.pnlEmail = new System.Windows.Forms.Panel();
            this.pbEmailIcon = new System.Windows.Forms.PictureBox();
            this.txtEmail = new System.Windows.Forms.TextBox();

            this.lblPasswordCaption = new System.Windows.Forms.Label();
            this.pnlPassword = new System.Windows.Forms.Panel();
            this.pbPasswordIcon = new System.Windows.Forms.PictureBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.btnTogglePassword = new System.Windows.Forms.Button();

            this.chkRemember = new System.Windows.Forms.CheckBox();
            this.lblForgotPassword = new System.Windows.Forms.LinkLabel();

            this.btnLogin = new System.Windows.Forms.Button();
            this.sepLineLeft = new System.Windows.Forms.Label();
            this.sepLineRight = new System.Windows.Forms.Label();
            this.lblOr = new System.Windows.Forms.Label();
            this.btnGuest = new System.Windows.Forms.Button();

            this.lblRegister = new System.Windows.Forms.LinkLabel();
            this.lblAdminLink = new System.Windows.Forms.LinkLabel();
            this.lblTerms = new System.Windows.Forms.Label();

            this.btnMinimize = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();

            // Form
            this.SuspendLayout();
            // Делаем окно равным карточке, без внешнего фона
            this.ClientSize = new System.Drawing.Size(420, 600);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            // Прозрачные углы зададим через TransparencyKey в коде формы
            this.BackColor = System.Drawing.Color.Fuchsia;
            this.Text = "ScooterShare - Вход";

            // cardPanel (сделан чуть меньше снизу, скругление в коде)
            this.cardPanel.Size = new System.Drawing.Size(420, 600);
            this.cardPanel.BackColor = System.Drawing.Color.White;
            this.cardPanel.Location = new System.Drawing.Point(0, 0);
            this.cardPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            this.cardPanel.Padding = new Padding(24);
            // allow dragging by mouse on cardPanel
            this.cardPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.CardPanel_MouseDown);

            // btnMinimize (в правом верхнем углу карточки)
            this.btnMinimize.Size = new System.Drawing.Size(28, 24);
            this.btnMinimize.Location = new System.Drawing.Point(this.cardPanel.Width - 24 - 56, 12);
            this.btnMinimize.FlatStyle = FlatStyle.Flat;
            this.btnMinimize.FlatAppearance.BorderSize = 0;
            this.btnMinimize.Text = "_";
            this.btnMinimize.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnMinimize.BackColor = System.Drawing.Color.Transparent;
            this.btnMinimize.Click += new System.EventHandler(this.BtnMinimize_Click);
            this.btnMinimize.Visible = false;

            // btnClose (в правом верхнем углу карточки)
            this.btnClose.Size = new System.Drawing.Size(28, 24);
            this.btnClose.Location = new System.Drawing.Point(this.cardPanel.Width - 24 - 24, 12);
            this.btnClose.FlatStyle = FlatStyle.Flat;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.Text = "✕";
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnClose.BackColor = System.Drawing.Color.Transparent;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            this.btnClose.Visible = false;

            // pbLogo
            this.pbLogo.Size = new System.Drawing.Size(78, 78);
            this.pbLogo.Location = new System.Drawing.Point((this.cardPanel.Width - this.pbLogo.Width) / 2, 8);
            this.pbLogo.Anchor = AnchorStyles.Top;
            this.pbLogo.BackColor = System.Drawing.Color.Transparent;
            this.pbLogo.Paint += new System.Windows.Forms.PaintEventHandler(this.PbLogo_Paint);
            // allow dragging by logo too
            this.pbLogo.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PbLogo_MouseDown);

            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(34, 34, 34);
            this.lblTitle.Text = "ScooterShare";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblTitle.Location = new System.Drawing.Point((this.cardPanel.Width - 220) / 2, 96);

            // lblSubtitle
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.Gray;
            this.lblSubtitle.Text = "Войдите в свой аккаунт";
            this.lblSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblSubtitle.Location = new System.Drawing.Point((this.cardPanel.Width - 200) / 2, 126);

            // lblEmailCaption
            this.lblEmailCaption.AutoSize = true;
            this.lblEmailCaption.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblEmailCaption.ForeColor = System.Drawing.Color.FromArgb(34, 34, 34);
            this.lblEmailCaption.Text = "Электронная почта";
            this.lblEmailCaption.Location = new System.Drawing.Point(24, 150);

            // pnlEmail
            this.pnlEmail.Size = new System.Drawing.Size(this.cardPanel.Width - 48, 44);
            this.pnlEmail.Location = new System.Drawing.Point(24, 172);
            this.pnlEmail.BackColor = System.Drawing.Color.White;
            this.pnlEmail.BorderStyle = BorderStyle.None;

            // pbEmailIcon
            this.pbEmailIcon.Size = new System.Drawing.Size(28, 28);
            this.pbEmailIcon.Location = new System.Drawing.Point(8, 8);
            this.pbEmailIcon.BackColor = System.Drawing.Color.Transparent;
            this.pbEmailIcon.Paint += new System.Windows.Forms.PaintEventHandler(this.PbEmailIcon_Paint);

            // txtEmail
            this.txtEmail.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtEmail.Location = new System.Drawing.Point(44, 12);
            this.txtEmail.Size = new System.Drawing.Size(this.pnlEmail.Width - 56, 18);
            this.txtEmail.Font = new System.Drawing.Font("Segoe UI", 10F);
            // По умолчанию — админские данные (логика в LoginForm.cs остаётся прежней)
            this.txtEmail.ForeColor = System.Drawing.Color.FromArgb(34, 34, 34);
            this.txtEmail.Text = "admin@gmail.com";
            this.txtEmail.Enter += new System.EventHandler(this.TxtEmail_Enter);
            this.txtEmail.Leave += new System.EventHandler(this.TxtEmail_Leave);

            // lblPasswordCaption
            this.lblPasswordCaption.AutoSize = true;
            this.lblPasswordCaption.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblPasswordCaption.ForeColor = System.Drawing.Color.FromArgb(34, 34, 34);
            this.lblPasswordCaption.Text = "Пароль";
            this.lblPasswordCaption.Location = new System.Drawing.Point(24, 212);

            // pnlPassword
            this.pnlPassword.Size = new System.Drawing.Size(this.cardPanel.Width - 48, 44);
            this.pnlPassword.Location = new System.Drawing.Point(24, 234);
            this.pnlPassword.BackColor = System.Drawing.Color.White;
            this.pnlPassword.BorderStyle = BorderStyle.None;

            // pbPasswordIcon
            this.pbPasswordIcon.Size = new System.Drawing.Size(28, 28);
            this.pbPasswordIcon.Location = new System.Drawing.Point(8, 8);
            this.pbPasswordIcon.Paint += new System.Windows.Forms.PaintEventHandler(this.PbPasswordIcon_Paint);

            // txtPassword
            this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtPassword.Location = new System.Drawing.Point(44, 12);
            this.txtPassword.Size = new System.Drawing.Size(this.pnlPassword.Width - 100, 18);
            this.txtPassword.Font = new System.Drawing.Font("Segoe UI", 10F);
            // По умолчанию — админский пароль
            this.txtPassword.ForeColor = System.Drawing.Color.FromArgb(34, 34, 34);
            this.txtPassword.Text = "admin123";
            this.txtPassword.Enter += new System.EventHandler(this.TxtPassword_Enter);
            this.txtPassword.Leave += new System.EventHandler(this.TxtPassword_Leave);
            this.txtPassword.UseSystemPasswordChar = true;

            // btnTogglePassword
            this.btnTogglePassword.Size = new System.Drawing.Size(36, 28);
            this.btnTogglePassword.Location = new System.Drawing.Point(this.pnlPassword.Width - 44, 8);
            this.btnTogglePassword.FlatStyle = FlatStyle.Flat;
            this.btnTogglePassword.FlatAppearance.BorderSize = 0;
            this.btnTogglePassword.Text = "👁";
            this.btnTogglePassword.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnTogglePassword.BackColor = System.Drawing.Color.Transparent;
            this.btnTogglePassword.Click += new System.EventHandler(this.BtnTogglePassword_Click);

            // chkRemember
            this.chkRemember.AutoSize = true;
            this.chkRemember.Location = new System.Drawing.Point(24, 300);
            this.chkRemember.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkRemember.Text = "Запомнить меня";
            this.chkRemember.ForeColor = System.Drawing.Color.FromArgb(85, 85, 85);

            // lblForgotPassword — внутри карточки, справа
            this.lblForgotPassword.AutoSize = true;
            this.lblForgotPassword.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.lblForgotPassword.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblForgotPassword.Text = "Забыли пароль?";
            this.lblForgotPassword.Location = new System.Drawing.Point(this.cardPanel.Width - 24 - 120, 302);
            this.lblForgotPassword.LinkClicked += (s, e) =>
                System.Windows.Forms.MessageBox.Show("Свяжитесь с администратором", "Восстановление",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);

            // btnLogin
            this.btnLogin.Size = new System.Drawing.Size(this.cardPanel.Width - 48, 46);
            this.btnLogin.Location = new System.Drawing.Point(24, 330);
            this.btnLogin.FlatStyle = FlatStyle.Flat;
            this.btnLogin.FlatAppearance.BorderSize = 0;
            this.btnLogin.BackColor = System.Drawing.Color.FromArgb(12, 10, 23);
            this.btnLogin.ForeColor = System.Drawing.Color.White;
            this.btnLogin.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnLogin.Text = "Войти";
            this.btnLogin.Click += new System.EventHandler(this.BtnLogin_Click);
            this.btnLogin.Paint += new System.Windows.Forms.PaintEventHandler(this.BtnLogin_Paint);

            // separator / OR
            this.sepLineLeft.Size = new System.Drawing.Size((this.cardPanel.Width - 48) / 2 - 30, 1);
            this.sepLineLeft.BackColor = System.Drawing.Color.FromArgb(230, 230, 230);
            this.sepLineLeft.Location = new System.Drawing.Point(24, 390);

            this.sepLineRight.Size = new System.Drawing.Size((this.cardPanel.Width - 48) / 2 - 30, 1);
            this.sepLineRight.BackColor = System.Drawing.Color.FromArgb(230, 230, 230);
            this.sepLineRight.Location = new System.Drawing.Point(24 + (this.cardPanel.Width - 48) / 2 + 30, 390);

            this.lblOr.AutoSize = true;
            this.lblOr.Text = "или";
            this.lblOr.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblOr.ForeColor = System.Drawing.Color.Gray;
            this.lblOr.Location = new System.Drawing.Point((this.cardPanel.Width - this.lblOr.PreferredWidth) / 2, 382);

            // btnGuest
            this.btnGuest.Size = new System.Drawing.Size(this.cardPanel.Width - 48, 42);
            this.btnGuest.Location = new System.Drawing.Point(24, 410);
            this.btnGuest.FlatStyle = FlatStyle.Flat;
            this.btnGuest.FlatAppearance.BorderSize = 1;
            this.btnGuest.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(220, 220, 220);
            this.btnGuest.BackColor = System.Drawing.Color.White;
            this.btnGuest.ForeColor = System.Drawing.Color.FromArgb(34, 34, 34);
            this.btnGuest.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnGuest.Text = "Войти как гость";
            this.btnGuest.Click += new System.EventHandler(this.BtnGuest_Click);

            // lblRegister — перенёс внутрь карточки, ближе к низу; выравнивание будет в Load
            this.lblRegister.AutoSize = true;
            this.lblRegister.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblRegister.Text = "Нет аккаунта? Зарегистрироваться";
            this.lblRegister.LinkClicked += new LinkLabelLinkClickedEventHandler(this.LblRegister_LinkClicked);

            // lblAdminLink — также внутри карточки, под регистрацией
            this.lblAdminLink.AutoSize = true;
            this.lblAdminLink.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblAdminLink.Text = "Администратор? Войти в админ-панель";
            this.lblAdminLink.LinkClicked += new LinkLabelLinkClickedEventHandler(this.LblAdminLink_LinkClicked);

            // lblTerms — перенёс ниже карточки (позиция установится в коде формы)
            this.lblTerms.AutoSize = true;
            this.lblTerms.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblTerms.ForeColor = System.Drawing.Color.Gray;
            this.lblTerms.MaximumSize = new System.Drawing.Size(this.cardPanel.Width, 0);
            this.lblTerms.Text = "Продолжая, вы соглашаетесь с нашими условиями использования и политикой конфиденциальности";
            this.lblTerms.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // assemble small containers
            this.pnlEmail.Controls.Add(this.pbEmailIcon);
            this.pnlEmail.Controls.Add(this.txtEmail);

            this.pnlPassword.Controls.Add(this.pbPasswordIcon);
            this.pnlPassword.Controls.Add(this.txtPassword);
            this.pnlPassword.Controls.Add(this.btnTogglePassword);

            // добавляем элементы в карточку (включая кнопки управления окном)
            this.cardPanel.Controls.Add(this.pbLogo);
            this.cardPanel.Controls.Add(this.lblTitle);
            this.cardPanel.Controls.Add(this.lblSubtitle);
            this.cardPanel.Controls.Add(this.lblEmailCaption);
            this.cardPanel.Controls.Add(this.pnlEmail);
            this.cardPanel.Controls.Add(this.lblPasswordCaption);
            this.cardPanel.Controls.Add(this.pnlPassword);
            this.cardPanel.Controls.Add(this.chkRemember);
            this.cardPanel.Controls.Add(this.lblForgotPassword);
            this.cardPanel.Controls.Add(this.btnLogin);
            this.cardPanel.Controls.Add(this.sepLineLeft);
            this.cardPanel.Controls.Add(this.sepLineRight);
            this.cardPanel.Controls.Add(this.lblOr);
            this.cardPanel.Controls.Add(this.btnGuest);
            this.cardPanel.Controls.Add(this.btnMinimize);
            this.cardPanel.Controls.Add(this.btnClose);
            this.cardPanel.Controls.Add(this.lblRegister);
            this.cardPanel.Controls.Add(this.lblAdminLink);

            // add to form (без внешнего фона)
            this.Controls.Add(this.cardPanel);

            // Условия скрываем (в макете их нет)
            this.lblTerms.Visible = false;

            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}