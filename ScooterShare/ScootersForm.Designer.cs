using System.Drawing;
using System.Windows.Forms;

namespace ScooterShare
{
    partial class ScootersForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dgvScooters;
        private System.Windows.Forms.Label lblTotalScooters;
        private System.Windows.Forms.Label lblAvailable;
        private System.Windows.Forms.Label lblAttention;
        private System.Windows.Forms.Label lblWarning;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Button btnAddScooter;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.dgvScooters = new System.Windows.Forms.DataGridView();
            this.btnBack = new System.Windows.Forms.Button();
            this.btnAddScooter = new System.Windows.Forms.Button();
            this.lblTotalScooters = new System.Windows.Forms.Label();
            this.lblAvailable = new System.Windows.Forms.Label();
            this.lblAttention = new System.Windows.Forms.Label();
            this.lblWarning = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvScooters)).BeginInit();
            this.SuspendLayout();

            // dgvScooters
            this.dgvScooters.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvScooters.Location = new System.Drawing.Point(20, 180);
            this.dgvScooters.Name = "dgvScooters";
            this.dgvScooters.RowHeadersWidth = 51;
            this.dgvScooters.RowTemplate.Height = 40;
            this.dgvScooters.Size = new System.Drawing.Size(1040, 400);
            this.dgvScooters.BackgroundColor = System.Drawing.Color.White;
            this.dgvScooters.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgvScooters.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvScooters.TabIndex = 0;

            // btnAddScooter
            this.btnAddScooter.Text = "+ Добавить самокат";
            this.btnAddScooter.Size = new System.Drawing.Size(160, 40);
            this.btnAddScooter.Location = new System.Drawing.Point(880, 20);
            this.btnAddScooter.BackColor = System.Drawing.Color.FromArgb(18, 10, 37);
            this.btnAddScooter.ForeColor = System.Drawing.Color.White;
            this.btnAddScooter.FlatStyle = FlatStyle.Flat;
            this.btnAddScooter.FlatAppearance.BorderSize = 0;
            this.btnAddScooter.Name = "btnAddScooter";
            this.btnAddScooter.TabIndex = 2;
            this.btnAddScooter.Click += new System.EventHandler(this.BtnAddScooter_Click);


            // btnBack
            this.btnBack.Text = "Вернуться в приложение";
            this.btnBack.Size = new System.Drawing.Size(200, 40);
            this.btnBack.Location = new System.Drawing.Point(20, 640);
            this.btnBack.BackColor = System.Drawing.Color.FromArgb(44, 62, 80);
            this.btnBack.ForeColor = System.Drawing.Color.White;
            this.btnBack.FlatStyle = FlatStyle.Flat;
            this.btnBack.Name = "btnBack";
            this.btnBack.TabIndex = 1;
            this.btnBack.Click += new System.EventHandler(this.BtnBack_Click);

            // lblTotalScooters
            this.lblTotalScooters = new System.Windows.Forms.Label();
            this.lblTotalScooters.Text = "0";
            this.lblTotalScooters.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lblTotalScooters.ForeColor = System.Drawing.Color.FromArgb(44, 62, 80);

            // lblAvailable
            this.lblAvailable = new System.Windows.Forms.Label();
            this.lblAvailable.Text = "0";
            this.lblAvailable.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lblAvailable.ForeColor = System.Drawing.Color.FromArgb(44, 62, 80);

            // lblAttention
            this.lblAttention = new System.Windows.Forms.Label();
            this.lblAttention.Text = "0";
            this.lblAttention.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lblAttention.ForeColor = System.Drawing.Color.FromArgb(44, 62, 80);

            // lblWarning
            this.lblWarning.Text = "";
            this.lblWarning.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblWarning.ForeColor = System.Drawing.Color.Red;

            // ScootersForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(240, 242, 245);
            this.ClientSize = new System.Drawing.Size(1100, 700);

            // Добавляем заголовок
            Label lblTitle = new Label
            {
                Text = "Управление самокатами",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(300, 40),
                ForeColor = Color.FromArgb(44, 62, 80),
                Name = "lblTitle"
            };
            this.Controls.Add(lblTitle);

            // Создаем карточки статистики
            CreateStatCard("Всего самокатов", lblTotalScooters, 20, 80);
            CreateStatCard("Доступно", lblAvailable, 200, 80);
            CreateStatCard("Требуют внимания", lblAttention, 380, 80);

            this.Controls.Add(this.dgvScooters);
            this.Controls.Add(this.lblWarning);
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.btnAddScooter);

            this.Name = "ScootersForm";
            this.Text = "Админ-панель - Управление самокатами";
            ((System.ComponentModel.ISupportInitialize)(this.dgvScooters)).EndInit();
            this.ResumeLayout(false);
        }

        private void CreateStatCard(string title, Label valueLabel, int x, int y)
        {
            Panel card = new Panel
            {
                BackColor = System.Drawing.Color.White,
                Location = new System.Drawing.Point(x, y),
                Size = new System.Drawing.Size(160, 70),
                BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle,
                Name = $"card_{title.Replace(" ", "_")}"
            };

            Label titleLabel = new Label
            {
                Text = title,
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(140, 20),
                Font = new System.Drawing.Font("Segoe UI", 10F),
                ForeColor = System.Drawing.Color.Gray,
                Name = $"lblTitle_{title.Replace(" ", "_")}"
            };

            valueLabel.Location = new System.Drawing.Point(10, 35);
            valueLabel.Size = new System.Drawing.Size(140, 30);
            valueLabel.Name = $"lblValue_{title.Replace(" ", "_")}";

            card.Controls.Add(titleLabel);
            card.Controls.Add(valueLabel);
            this.Controls.Add(card);
        }
    }
}