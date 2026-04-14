using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScooterShare
{
    public partial class UsersForm : Form
    {
        public UsersForm()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1100, 700);
            LoadUsers();
        }
        private void LoadUsers()
        {
            flowUsersPanel.Controls.Clear();

            AddUserCard("ИП", "Иван Петров", "Активен", "ivan.petrov@example.com",
                       "Присоединился 15.03.2025", "+7 (999) 123-45-67", "47 поездок • P18750");

            AddUserCard("МИ", "Мария Иванова", "Активен", "maria.ivanova@example.com",
                       "Присоединился 20.02.2025", "+7 (999) 234-56-78", "63 поездок • P24580");

            AddUserCard("АС", "Алексей Сидоров", "Активен", "alexey.sidorov@example.com",
                       "Присоединился 01.04.2025", "+7 (999) 345-67-89", "28 поездок • P11240");

            AddUserCard("ЕК", "Елена Кузнецова", "Активен", "elena.kuznetsova@example.com",
                       "Присоединился 10.01.2025", "+7 (999) 456-78-90", "92 поездок • P35620");
        }

        private void AddUserCard(string initials, string fullName, string status, string email,
                                 string joined, string phone, string stats)
        {
            Panel card = new Panel
            {
                Size = new Size(480, 180),
                Margin = new Padding(10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Инициалы (круг)
            Panel initialsCircle = new Panel
            {
                Size = new Size(60, 60),
                Location = new Point(20, 20),
                BackColor = Color.FromArgb(52, 152, 219)
            };
            // Делаем круг
            initialsCircle.Paint += (s, e) => {
                System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddEllipse(0, 0, 60, 60);
                initialsCircle.Region = new Region(path);
            };

            Label lblInitials = new Label
            {
                Text = initials,
                Location = new Point(15, 15),
                Size = new Size(30, 30),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };
            initialsCircle.Controls.Add(lblInitials);

            // Имя
            Label lblName = new Label
            {
                Text = fullName,
                Location = new Point(100, 20),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80)
            };

            // Статус
            Label lblStatus = new Label
            {
                Text = status,
                Location = new Point(100, 45),
                Size = new Size(80, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Green
            };

            // Email
            Label lblEmail = new Label
            {
                Text = email,
                Location = new Point(20, 100),
                Size = new Size(250, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray
            };

            // Дата присоединения
            Label lblJoined = new Label
            {
                Text = joined,
                Location = new Point(20, 120),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray
            };

            // Телефон
            Label lblPhone = new Label
            {
                Text = phone,
                Location = new Point(280, 100),
                Size = new Size(180, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray
            };

            // Статистика
            Label lblStats = new Label
            {
                Text = stats,
                Location = new Point(280, 120),
                Size = new Size(180, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 152, 219)
            };

            card.Controls.Add(initialsCircle);
            card.Controls.Add(lblName);
            card.Controls.Add(lblStatus);
            card.Controls.Add(lblEmail);
            card.Controls.Add(lblJoined);
            card.Controls.Add(lblPhone);
            card.Controls.Add(lblStats);

            flowUsersPanel.Controls.Add(card);
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
