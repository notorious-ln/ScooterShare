using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ScooterShare
{
    public partial class LoginForm : Form
    {
        private readonly Color placeholderColor = Color.Gray;
        private readonly Color textColor = Color.FromArgb(34, 34, 34);
        private readonly ErrorProvider errorProvider = new ErrorProvider();

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        public LoginForm()
        {
            InitializeComponent();
            this.Icon = AppIcon.WindowIcon;

            errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            errorProvider.ContainerControl = this;
            errorProvider.SetIconAlignment(pnlEmail, ErrorIconAlignment.MiddleRight);
            errorProvider.SetIconAlignment(pnlPassword, ErrorIconAlignment.MiddleRight);
            // Одинаковое положение значка: всегда справа от поля
            errorProvider.SetIconPadding(pnlEmail, 6);
            errorProvider.SetIconPadding(pnlPassword, 6);

            txtEmail.TextChanged += (_, __) => ValidateInputsAndUpdateUi(false);
            txtPassword.TextChanged += (_, __) => ValidateInputsAndUpdateUi(false);

            // Регион с закруглениями и позиционирование элементов
            this.Load += (s, e) =>
            {
                // Прячем прямоугольный фон формы — оставляем только округлённую карточку
                this.TransparencyKey = Color.Fuchsia;
                this.BackColor = Color.Fuchsia;

                // скругляем белую карточку
                SetRoundedRegion(cardPanel, 16);

                // скругляем поля ввода (визуально как в макете)
                SetRoundedRegion(pnlEmail, 12);
                SetRoundedRegion(pnlPassword, 12);
                pnlEmail.Paint += DrawInputOutline;
                pnlPassword.Paint += DrawInputOutline;

                // скруглить кнопки
                SetRoundedRegion(btnLogin, 10);
                SetRoundedRegion(btnGuest, 10);

                // Убираем управление окном — в макете его нет
                btnMinimize.Visible = false;
                btnClose.Visible = false;

                // переместить и центрировать ссылки внутри карточки
                int centerX = (cardPanel.Width) / 2;

                // размещаем регистрационную ссылку и админ‑ссылку внутри карточки, под кнопкой гостя
                lblRegister.Left = centerX - lblRegister.PreferredWidth / 2;
                lblRegister.Top = btnGuest.Bottom + 12;

                lblAdminLink.Left = centerX - lblAdminLink.PreferredWidth / 2;
                lblAdminLink.Top = lblRegister.Bottom + 6;

                // В макете нет текста условий
                lblTerms.Visible = false;

                LayoutHeader();
            };

            ValidateInputsAndUpdateUi(false);
        }

        private void LayoutHeader()
        {
            pbLogo.Left = (cardPanel.Width - pbLogo.Width) / 2;
            lblTitle.Left = (cardPanel.Width - lblTitle.PreferredWidth) / 2;
            lblSubtitle.Left = (cardPanel.Width - lblSubtitle.PreferredWidth) / 2;
            lblOr.Left = (cardPanel.Width - lblOr.PreferredWidth) / 2;

            // позиция внутри cardPanel
            lblForgotPassword.Left = cardPanel.Width - 24 - lblForgotPassword.PreferredWidth;
        }

        private void DrawInputOutline(object sender, PaintEventArgs e)
        {
            var p = sender as Panel;
            if (p == null)
            {
                return;
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
            using (var pen = new Pen(Color.FromArgb(230, 230, 230), 1f))
            {
                using (var path = new GraphicsPath())
                {
                    int radius = 12;
                    int d = radius * 2;
                    path.AddArc(rect.X, rect.Y, d, d, 180, 90);
                    path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                    path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
                    path.CloseFigure();
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }

        private void SetRoundedRegion(Control ctrl, int radius)
        {
            var path = new GraphicsPath();
            var rect = new Rectangle(0, 0, ctrl.Width, ctrl.Height);
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            ctrl.Region = new Region(path);
        }

        private void PbLogo_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, pbLogo.Width - 1, pbLogo.Height - 1);
            using (var brush = new SolidBrush(Color.FromArgb(12, 10, 23)))
                g.FillEllipse(brush, rect);

            var src = AppIcon.LogoBitmap;
            int innerSize = (int)(pbLogo.Width * 0.70f);
            int ix = (pbLogo.Width - innerSize) / 2;
            int iy = (pbLogo.Height - innerSize) / 2;
            var innerRect = new Rectangle(ix, iy, innerSize, innerSize);

            using (var white = new SolidBrush(Color.White))
            {
                g.FillEllipse(white, innerRect);
            }

            int imgSize = (int)(innerSize * 0.78f);
            int x = (pbLogo.Width - imgSize) / 2;
            int y = (pbLogo.Height - imgSize) / 2;
            var dest = new Rectangle(x, y, imgSize, imgSize);

            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            using (var ia = new ImageAttributes())
            {
                ia.SetColorKey(Color.White, Color.White);
                g.DrawImage(
                    src,
                    dest,
                    0,
                    0,
                    src.Width,
                    src.Height,
                    GraphicsUnit.Pixel,
                    ia);
            }
        }

        private void PbEmailIcon_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var pen = new Pen(Color.FromArgb(150, 150, 150), 1.8f))
            {
                var r = new Rectangle(4, 6, 20, 12);
                g.DrawRectangle(pen, r);
                g.DrawLine(pen, 4, 6, 14, 14);
                g.DrawLine(pen, 24, 6, 14, 14);
            }
        }

        private void PbPasswordIcon_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var pen = new Pen(Color.FromArgb(150, 150, 150), 1.6f))
            {
                var rc = new Rectangle(6, 9, 16, 14);
                g.DrawRectangle(pen, rc);
                g.DrawArc(pen, 6, 2, 16, 14, 180, 180);
            }
        }

        private void TxtEmail_Enter(object sender, EventArgs e)
        {
            if (txtEmail.ForeColor == placeholderColor)
            {
                txtEmail.Text = "";
                txtEmail.ForeColor = textColor;
            }
        }

        private void TxtEmail_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                txtEmail.Text = "your@email.com";
                txtEmail.ForeColor = placeholderColor;
            }
        }

        private void TxtPassword_Enter(object sender, EventArgs e)
        {
            if (txtPassword.ForeColor == placeholderColor || txtPassword.Text == "Пароль")
            {
                txtPassword.Text = "";
                txtPassword.ForeColor = textColor;
                txtPassword.UseSystemPasswordChar = true;
            }
        }

        private void TxtPassword_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                txtPassword.UseSystemPasswordChar = false;
                txtPassword.Text = "Пароль";
                txtPassword.ForeColor = placeholderColor;
            }
        }

        private void BtnTogglePassword_Click(object sender, EventArgs e)
        {
            if (txtPassword.UseSystemPasswordChar)
            {
                txtPassword.UseSystemPasswordChar = false;
                btnTogglePassword.Text = "🙈";
            }
            else
            {
                if (txtPassword.Text != "Пароль")
                {
                    txtPassword.UseSystemPasswordChar = true;
                }
                btnTogglePassword.Text = "👁";
            }
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            if (!ValidateInputsAndUpdateUi(true))
            {
                return;
            }

            string email = (txtEmail.ForeColor == placeholderColor) ? "" : txtEmail.Text;
            string password = (txtPassword.ForeColor == placeholderColor) ? "" : txtPassword.Text;

            if (email == "admin@example.com" && password == "admin123")
            {
                MainAdminForm mainForm = new MainAdminForm();
                mainForm.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Неверный email или пароль!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGuest_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Гостевой режим: ограниченный доступ", "Гость",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Новые обработчики
        private void BtnMinimize_Click(object sender, EventArgs e)
        {
            // "Скрыть" — минимизируем окно
            this.WindowState = FormWindowState.Minimized;
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            // Закрыть форму (и приложение, если это главный)
            this.Close();
        }

        private void LblAdminLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Проверяем поля ввода (учитываем плейсхолдеры)
            if (!ValidateInputsAndUpdateUi(true))
            {
                return;
            }

            string email = (txtEmail.ForeColor == placeholderColor) ? "" : txtEmail.Text;
            string password = (txtPassword.ForeColor == placeholderColor) ? "" : txtPassword.Text;

            if (email == "admin@gmail.com" && password == "admin123")
            {
                MainAdminForm mainForm = new MainAdminForm();
                mainForm.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Требуются учётные данные администратора (admin@gmail.com / admin123).", "Доступ запрещён",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LblRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("Форма регистрации (заглушка).", "Регистрация",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Перетаскивание окна при зажатой кнопке мыши по карточке или логотипу
        private void CardPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
            }
        }

        private void PbLogo_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
            }
        }

        private void BtnLogin_Paint(object sender, PaintEventArgs e)
        {
            var b = sender as Button;
            if (b == null)
            {
                return;
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, b.Width - 1, b.Height - 1);
            using (var path = new GraphicsPath())
            {
                int radius = 10;
                int d = radius * 2;
                path.AddArc(rect.X, rect.Y, d, d, 180, 90);
                path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
                path.CloseFigure();

                e.Graphics.SetClip(path);
                using (var brush = new LinearGradientBrush(
                           rect,
                           Color.FromArgb(16, 14, 30),
                           Color.FromArgb(6, 6, 14),
                           LinearGradientMode.Horizontal))
                {
                    e.Graphics.FillRectangle(brush, rect);
                }

                if (!b.Enabled)
                {
                    using (var overlay = new SolidBrush(Color.FromArgb(120, Color.White)))
                    {
                        e.Graphics.FillRectangle(overlay, rect);
                    }
                }
            }

            TextRenderer.DrawText(
                e.Graphics,
                b.Text,
                b.Font,
                rect,
                b.ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private bool ValidateInputsAndUpdateUi(bool strict)
        {
            string email = (txtEmail.ForeColor == placeholderColor) ? string.Empty : (txtEmail.Text ?? string.Empty);
            string password = (txtPassword.ForeColor == placeholderColor) ? string.Empty : (txtPassword.Text ?? string.Empty);

            bool ok = true;
            errorProvider.SetError(pnlEmail, string.Empty);
            errorProvider.SetError(pnlPassword, string.Empty);

            if (string.IsNullOrWhiteSpace(email))
            {
                ok = false;
                errorProvider.SetError(pnlEmail, "Укажи email.");
            }
            else if (strict && !IsValidEmail(email))
            {
                ok = false;
                errorProvider.SetError(pnlEmail, "Неверный формат email.");
            }

            if (string.IsNullOrWhiteSpace(password) || string.Equals(password, "Пароль", StringComparison.Ordinal))
            {
                ok = false;
                errorProvider.SetError(pnlPassword, "Укажи пароль.");
            }

            btnLogin.Enabled = ok;
            return ok;
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return string.Equals(addr.Address, email, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}