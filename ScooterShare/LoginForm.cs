using System;
using System.Drawing;
using System.Drawing.Drawing2D;
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

        // P/Invoke для перетаскивания окна
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        public LoginForm()
        {
            InitializeComponent();

            errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            errorProvider.ContainerControl = this;

            txtEmail.TextChanged += (_, __) => ValidateInputsAndUpdateUi();
            txtPassword.TextChanged += (_, __) => ValidateInputsAndUpdateUi();

            // Регион с закруглениями и позиционирование элементов
            this.Load += (s, e) =>
            {
                // скругляем белую карточку
                SetRoundedRegion(cardPanel, 16);

                // скруглить кнопки
                SetRoundedRegion(btnLogin, 10);
                SetRoundedRegion(btnGuest, 10);

                // позиционируем элементы
                CenterCardHorizontally();

                // переместить и центрировать ссылки внутри карточки
                int centerX = (cardPanel.Width) / 2;

                // размещаем регистрационную ссылку и админ‑ссылку внутри карточки, под кнопкой гостя
                lblRegister.Left = centerX - lblRegister.PreferredWidth / 2;
                lblRegister.Top = btnGuest.Bottom + 12;

                lblAdminLink.Left = centerX - lblAdminLink.PreferredWidth / 2;
                lblAdminLink.Top = lblRegister.Bottom + 6;

                // разместить текст условий ниже карточки и центрировать по форме
                lblTerms.Left = (this.ClientSize.Width / 2) - (lblTerms.PreferredWidth / 2);
                lblTerms.Top = cardPanel.Bottom + 12;

                // скорректируем позицию контрольных кнопок (если ширина карточки изменится)
                btnMinimize.Left = cardPanel.Width - 24 - 56;
                btnClose.Left = cardPanel.Width - 24 - 24;
            };

            ValidateInputsAndUpdateUi();
        }

        private void CenterCardHorizontally()
        {
            cardPanel.Left = (this.ClientSize.Width - cardPanel.Width) / 2;
            pbLogo.Left = (cardPanel.Width - pbLogo.Width) / 2;
            lblTitle.Left = (cardPanel.Width - lblTitle.PreferredWidth) / 2;
            lblSubtitle.Left = (cardPanel.Width - lblSubtitle.PreferredWidth) / 2;
            lblOr.Left = (cardPanel.Width - lblOr.PreferredWidth) / 2;

            // позиция внутри cardPanel
            lblForgotPassword.Left = cardPanel.Width - 24 - lblForgotPassword.PreferredWidth;
            lblForgotPassword.Top = 292;
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
            using (var pen = new Pen(Color.FromArgb(88, 180, 255), 3))
            {
                g.DrawLine(pen, pbLogo.Width * 0.25f, pbLogo.Height * 0.6f, pbLogo.Width * 0.6f, pbLogo.Height * 0.45f);
                g.DrawLine(pen, pbLogo.Width * 0.6f, pbLogo.Height * 0.45f, pbLogo.Width * 0.72f, pbLogo.Height * 0.32f);
                g.DrawEllipse(pen, pbLogo.Width * 0.18f, pbLogo.Height * 0.62f, pbLogo.Width * 0.18f, pbLogo.Width * 0.18f);
                g.DrawEllipse(pen, pbLogo.Width * 0.62f, pbLogo.Height * 0.6f, pbLogo.Width * 0.18f, pbLogo.Width * 0.18f);
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
            if (!ValidateInputsAndUpdateUi())
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
            if (!ValidateInputsAndUpdateUi())
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

        private bool ValidateInputsAndUpdateUi()
        {
            string email = (txtEmail.ForeColor == placeholderColor) ? string.Empty : (txtEmail.Text ?? string.Empty);
            string password = (txtPassword.ForeColor == placeholderColor) ? string.Empty : (txtPassword.Text ?? string.Empty);

            bool ok = true;
            errorProvider.SetError(txtEmail, string.Empty);
            errorProvider.SetError(txtPassword, string.Empty);

            if (string.IsNullOrWhiteSpace(email))
            {
                ok = false;
                errorProvider.SetError(txtEmail, "Укажи email.");
            }
            else if (!IsValidEmail(email))
            {
                ok = false;
                errorProvider.SetError(txtEmail, "Неверный формат email.");
            }

            if (string.IsNullOrWhiteSpace(password) || string.Equals(password, "Пароль", StringComparison.Ordinal))
            {
                ok = false;
                errorProvider.SetError(txtPassword, "Укажи пароль.");
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