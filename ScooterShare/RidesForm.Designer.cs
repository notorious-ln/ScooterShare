using System.Windows.Forms;

namespace ScooterShare
{
    partial class RidesForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dgvRides;
        private System.Windows.Forms.Label lblRidesToday;
        private System.Windows.Forms.Label lblRidesWeek;
        private System.Windows.Forms.Label lblRevenue;
        private System.Windows.Forms.Label lblAvgDuration;
        private System.Windows.Forms.Label lblRidesTodayLabel;
        private System.Windows.Forms.Label lblRidesWeekLabel;
        private System.Windows.Forms.Label lblRevenueLabel;
        private System.Windows.Forms.Label lblAvgDurationLabel;
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
            this.dgvRides = new System.Windows.Forms.DataGridView();
            this.lblRidesToday = new System.Windows.Forms.Label();
            this.lblRidesWeek = new System.Windows.Forms.Label();
            this.lblRevenue = new System.Windows.Forms.Label();
            this.lblAvgDuration = new System.Windows.Forms.Label();
            this.lblRidesTodayLabel = new System.Windows.Forms.Label();
            this.lblRidesWeekLabel = new System.Windows.Forms.Label();
            this.lblRevenueLabel = new System.Windows.Forms.Label();
            this.lblAvgDurationLabel = new System.Windows.Forms.Label();
            this.btnBack = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRides)).BeginInit();
            this.SuspendLayout();

            this.dgvRides.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRides.Location = new System.Drawing.Point(20, 120);
            this.dgvRides.Size = new System.Drawing.Size(860, 400);
            this.dgvRides.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Stats labels
            this.lblRidesTodayLabel.Text = "Поездок сегодня";
            this.lblRidesTodayLabel.Location = new System.Drawing.Point(20, 20);
            this.lblRidesTodayLabel.Size = new System.Drawing.Size(120, 20);

            this.lblRidesToday.Text = "0";
            this.lblRidesToday.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblRidesToday.Location = new System.Drawing.Point(20, 40);
            this.lblRidesToday.Size = new System.Drawing.Size(120, 35);

            this.lblRidesWeekLabel.Text = "За эту неделю";
            this.lblRidesWeekLabel.Location = new System.Drawing.Point(160, 20);
            this.lblRidesWeekLabel.Size = new System.Drawing.Size(120, 20);

            this.lblRidesWeek.Text = "0";
            this.lblRidesWeek.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblRidesWeek.Location = new System.Drawing.Point(160, 40);
            this.lblRidesWeek.Size = new System.Drawing.Size(120, 35);

            this.lblRevenueLabel.Text = "Доход сегодня";
            this.lblRevenueLabel.Location = new System.Drawing.Point(300, 20);
            this.lblRevenueLabel.Size = new System.Drawing.Size(120, 20);

            this.lblRevenue.Text = "₽0";
            this.lblRevenue.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblRevenue.Location = new System.Drawing.Point(300, 40);
            this.lblRevenue.Size = new System.Drawing.Size(140, 35);

            this.lblAvgDurationLabel.Text = "Средняя длительность";
            this.lblAvgDurationLabel.Location = new System.Drawing.Point(460, 20);
            this.lblAvgDurationLabel.Size = new System.Drawing.Size(150, 20);

            this.lblAvgDuration.Text = "0 мин";
            this.lblAvgDuration.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblAvgDuration.Location = new System.Drawing.Point(460, 40);
            this.lblAvgDuration.Size = new System.Drawing.Size(140, 35);

            this.btnBack.Location = new System.Drawing.Point(20, 540);
            this.btnBack.Size = new System.Drawing.Size(150, 35);
            this.btnBack.Text = "Вернуться в приложение";
            this.btnBack.Click += (s, e) => this.Close();

            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.lblAvgDuration);
            this.Controls.Add(this.lblAvgDurationLabel);
            this.Controls.Add(this.lblRevenue);
            this.Controls.Add(this.lblRevenueLabel);
            this.Controls.Add(this.lblRidesWeek);
            this.Controls.Add(this.lblRidesWeekLabel);
            this.Controls.Add(this.lblRidesToday);
            this.Controls.Add(this.lblRidesTodayLabel);
            this.Controls.Add(this.dgvRides);
            this.Text = "ScooterShare - Управление поездками";
            ((System.ComponentModel.ISupportInitialize)(this.dgvRides)).EndInit();
            this.ResumeLayout(false);
        }
        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
       

        #endregion
    }
}