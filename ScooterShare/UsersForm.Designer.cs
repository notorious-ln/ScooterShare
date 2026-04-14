using System.Drawing;
using System.Windows.Forms;

namespace ScooterShare
{
    partial class UsersForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.FlowLayoutPanel flowUsersPanel;
        private System.Windows.Forms.Button btnBack;
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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

            this.flowUsersPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.btnBack = new System.Windows.Forms.Button();
            this.SuspendLayout();

            this.Text = "Админ-панель - Управление пользователями";
            this.BackColor = System.Drawing.Color.FromArgb(240, 242, 245);

            // Заголовок
            Label lblTitle = new Label
            {
                Text = "Управление пользователями",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(350, 40),
                ForeColor = Color.FromArgb(44, 62, 80)
            };
            this.Controls.Add(lblTitle);

            // FlowLayoutPanel для карточек
            this.flowUsersPanel.Location = new System.Drawing.Point(20, 80);
            this.flowUsersPanel.Size = new System.Drawing.Size(1040, 500);
            this.flowUsersPanel.AutoScroll = true;
            this.Controls.Add(this.flowUsersPanel);

            // Кнопка назад
            this.btnBack.Text = "Вернуться в приложение";
            this.btnBack.Size = new System.Drawing.Size(200, 40);
            this.btnBack.Location = new System.Drawing.Point(20, 600);
            this.btnBack.BackColor = System.Drawing.Color.FromArgb(44, 62, 80);
            this.btnBack.ForeColor = System.Drawing.Color.White;
            this.btnBack.FlatStyle = FlatStyle.Flat;
            this.btnBack.Click += new System.EventHandler(this.BtnBack_Click);
            this.Controls.Add(this.btnBack);

            this.ResumeLayout(false);
        }
    }
}