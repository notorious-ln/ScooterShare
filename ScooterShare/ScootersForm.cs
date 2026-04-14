using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScooterShare
{
    public partial class ScootersForm : Form
    {
        public ScootersForm()
        {
            InitializeComponent();
            LoadScooters();
            LoadStats();
        }

        private void DgvScooters_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // draw status badge for Status column (index 1) and battery small icon in Charge (index 2)
            try
            {
                if (e.RowIndex >= 0 && (e.ColumnIndex == 1 || e.ColumnIndex == 2))
                {
                    e.Handled = true;
                    e.PaintBackground(e.ClipBounds, true);
                    var g = e.Graphics;
                    var val = e.Value?.ToString() ?? "";
                    if (e.ColumnIndex == 1)
                    {
                        // status badge
                        Color back = Color.FromArgb(235, 246, 235);
                        Color fore = Color.FromArgb(40, 180, 100);
                        if (val.ToLower().Contains("низкий")) { back = Color.FromArgb(255, 249, 230); fore = Color.FromArgb(230, 160, 0); }
                        if (val.ToLower().Contains("обслужив")) { back = Color.FromArgb(255, 237, 237); fore = Color.FromArgb(220, 70, 70); }
                        if (val.ToLower().Contains("использ")) { back = Color.FromArgb(235, 244, 255); fore = Color.FromArgb(80, 130, 240); }

                        var rect = new Rectangle(e.CellBounds.Left + 8, e.CellBounds.Top + 10, e.CellBounds.Width - 16, e.CellBounds.Height - 20);
                        using (var b = new SolidBrush(back)) FillRoundedRectangle(g, b, rect, 12);
                        using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        using (var f = new Font("Segoe UI", 9F))
                        using (var br = new SolidBrush(fore))
                        {
                            g.DrawString(val, f, br, rect, sf);
                        }
                    }
                    else if (e.ColumnIndex == 2)
                    {
                        // battery icon + percent
                        int w = 24, h = 12;
                        int x = e.CellBounds.Left + 8;
                        int y = e.CellBounds.Top + (e.CellBounds.Height - h) / 2;
                        int pct = 0; int.TryParse(val.Replace("%", ""), out pct);
                        Color col = pct >= 75 ? Color.FromArgb(46, 204, 113) : (pct >= 40 ? Color.FromArgb(249, 199, 79) : Color.FromArgb(231, 76, 60));
                        using (var pen = new Pen(Color.FromArgb(200, 200, 200), 1))
                        {
                            g.DrawRectangle(pen, x, y, w, h);
                            g.FillRectangle(new SolidBrush(col), x + 1, y + 1, (int)((w - 1) * (pct / 100.0)), h - 1);
                            // small cap
                            g.FillRectangle(new SolidBrush(Color.FromArgb(200, 200, 200)), x + w + 2, y + 3, 4, h - 6);
                        }
                        using (var f = new Font("Segoe UI", 9F))
                        using (var br = new SolidBrush(Color.Gray))
                        {
                            g.DrawString(val, f, br, x + w + 10, e.CellBounds.Top + 8);
                        }
                    }
                    e.Paint(e.ClipBounds, DataGridViewPaintParts.Border);
                }
            }
            catch { }
        }

        private void BtnAddScooter_Click(object sender, EventArgs e)
        {
            try
            {
                string model = ShowInputBox("Модель:", "Добавить самокат", "Model X");
                if (string.IsNullOrWhiteSpace(model)) return;
                string condStr = ShowInputBox("condition_id (число):", "Добавить самокат", "1");
                if (!int.TryParse(condStr, out int condId)) condId = 1;
                string yearStr = ShowInputBox("Год выпуска (YYYY-MM-DD):", "Добавить самокат", DateTime.Today.ToString("yyyy-MM-dd"));
                DateTime yearValue;
                if (string.IsNullOrWhiteSpace(yearStr) || !DateTime.TryParse(yearStr, out yearValue)) yearValue = DateTime.Today;

                string insert = "INSERT INTO Scooters (model, condition_id, yearOfRelease) VALUES (@model, @cond, @year); SELECT SCOPE_IDENTITY();";
                SqlParameter[] ps = new SqlParameter[] {
                    new SqlParameter("@model", model),
                    new SqlParameter("@cond", condId),
                    new SqlParameter("@year", yearValue)
                };
                var newIdObj = DatabaseHelper.ExecuteScalar(insert, ps);
                int newId = 0;
                if (newIdObj != null && newIdObj != DBNull.Value) { try { newId = Convert.ToInt32(Convert.ToDecimal(newIdObj)); } catch { } }
                try { DatabaseHelper.LogActivity($"Добавлен самокат #{newId}: {model}"); } catch { }
                try { DatabaseHelper.ReseedIdentityToMax("Scooters", "scooter_id"); } catch { }
                LoadScooters();
                LoadStats();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void LoadScooters()
        {
            if (dgvScooters == null)
            {
                MessageBox.Show("Ошибка инициализации таблицы", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DataTable dt = null;
            try
            {
                dt = DatabaseHelper.ExecuteQuery("SELECT s.scooter_id, s.model, ISNULL(c.condition_name, '') AS status, s.yearOfRelease FROM Scooters s LEFT JOIN Conditions c ON s.condition_id = c.condition_id ORDER BY s.scooter_id DESC");
            }
            catch
            {
                dt = new DataTable();
                dt.Columns.Add("scooter_id"); dt.Columns.Add("model"); dt.Columns.Add("status"); dt.Columns.Add("yearOfRelease");
            }

            dgvScooters.Columns.Clear();
            dgvScooters.Rows.Clear();
            dgvScooters.Font = new Font("Segoe UI", 10);
            dgvScooters.RowTemplate.Height = 48;

            dgvScooters.EnableHeadersVisualStyles = false;
            dgvScooters.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 250);
            dgvScooters.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(80, 80, 80);

            // columns: ID, Status, Charge, Location, LastRide, Edit, Delete
            dgvScooters.Columns.Add("ID", "ID");
            dgvScooters.Columns.Add("Status", "Статус");
            dgvScooters.Columns.Add("Charge", "Заряд");
            dgvScooters.Columns.Add("Location", "Местоположение");
            dgvScooters.Columns.Add("LastRide", "Последняя поездка");

            var editCol = new DataGridViewButtonColumn { Text = "✏", UseColumnTextForButtonValue = true, Width = 40, Name = "Edit" };
            var delCol = new DataGridViewButtonColumn { Text = "🗑", UseColumnTextForButtonValue = true, Width = 40, Name = "Delete" };
            dgvScooters.Columns.Add(editCol);
            dgvScooters.Columns.Add(delCol);

            // populate rows with additional calculated fields
            foreach (DataRow r in dt.Rows)
            {
                int id = r["scooter_id"] == DBNull.Value ? 0 : Convert.ToInt32(r["scooter_id"]);
                string status = r["status"] == DBNull.Value ? "" : r["status"].ToString();

                // deterministic pseudo-random battery percent by id
                int percent = (id * 73) % 100;
                if (percent < 10) percent += 10;

                // try to get last ride start_time for this scooter
                DateTime? lastRide = null;
                try
                {
                    var obj = DatabaseHelper.ExecuteScalar("SELECT TOP 1 a.start_time FROM Active_rentals a JOIN Rental_scooters rs ON a.activeRental_id = rs.activeRental_id WHERE rs.scooter_id = @id ORDER BY a.start_time DESC", new SqlParameter[] { new SqlParameter("@id", id) });
                    if (obj != null && obj != DBNull.Value) lastRide = Convert.ToDateTime(obj);
                }
                catch { }

                string lastRideText = lastRide.HasValue ? GetRelativeTime(lastRide.Value) : "—";
                string location = "Неизвестно";

                dgvScooters.Rows.Add(id, status, percent + "%", location, lastRideText);
            }

            dgvScooters.CellContentClick -= DgvScooters_CellContentClick;
            dgvScooters.CellContentClick += DgvScooters_CellContentClick;

            dgvScooters.CellPainting -= DgvScooters_CellPainting;
            dgvScooters.CellPainting += DgvScooters_CellPainting;
        }

        private void DgvScooters_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value != null && e.RowIndex >= 0)
            {
                // find by column name
                var dgv = sender as DataGridView;
                if (dgv != null && dgv.Columns[e.ColumnIndex].Name == "Status")
                {
                    string status = e.Value.ToString();
                    if (status == "Доступен") e.CellStyle.ForeColor = Color.Green;
                    else if (status == "Низкий заряд") e.CellStyle.ForeColor = Color.Orange;
                    else if (status == "В использовании") e.CellStyle.ForeColor = Color.Blue;
                    else if (status == "На обслуживании") e.CellStyle.ForeColor = Color.Red;
                }
            }
        }

        private void DgvScooters_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var dgv = sender as DataGridView;
            if (e.RowIndex < 0) return;
            if (dgv.Columns[e.ColumnIndex].Name == "Edit")
            {
                var id = dgv.Rows[e.RowIndex].Cells[0].Value;
                var curModel = dgv.Rows[e.RowIndex].Cells[1].Value?.ToString();
                var curStatus = dgv.Rows[e.RowIndex].Cells[2].Value?.ToString();
                var curYear = dgv.Rows[e.RowIndex].Cells[3].Value?.ToString();

                string newModel = ShowInputBox("Новая модель:", "Редактировать", curModel);
                if (string.IsNullOrWhiteSpace(newModel)) return;
                string condStr = ShowInputBox("condition_id (число):", "Редактировать", "1");
                if (!int.TryParse(condStr, out int condId)) condId = 1;
                string yearStr = ShowInputBox("Год выпуска (YYYY-MM-DD):", "Редактировать", curYear ?? DateTime.Today.ToString("yyyy-MM-dd"));
                DateTime yearValue;
                if (string.IsNullOrWhiteSpace(yearStr) || !DateTime.TryParse(yearStr, out yearValue)) yearValue = DateTime.Today;

                DatabaseHelper.ExecuteNonQuery("UPDATE Scooters SET model=@m, condition_id=@c, yearOfRelease=@y WHERE scooter_id=@id",
                    new SqlParameter[] { new SqlParameter("@m", newModel), new SqlParameter("@c", condId), new SqlParameter("@y", yearValue), new SqlParameter("@id", id) });
                try { DatabaseHelper.LogActivity($"Изменён самокат #{id}: модель -> {newModel}"); } catch { }
                LoadScooters();
                LoadStats();
            }
            else if (dgv.Columns[e.ColumnIndex].Name == "Delete")
            {
                var id = dgv.Rows[e.RowIndex].Cells[0].Value;
                if (MessageBox.Show("Удалить самокат?", "Подтвердите", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    DatabaseHelper.ExecuteNonQuery("DELETE FROM Scooters WHERE scooter_id=@id", new SqlParameter[] { new SqlParameter("@id", id) });
                    try { DatabaseHelper.LogActivity($"Удалён самокат #{id}"); } catch { }
                    try { DatabaseHelper.ReseedIdentityToMax("Scooters", "scooter_id"); } catch { }
                    LoadScooters();
                    LoadStats();
                }
            }
        }

        // Helper - draw rounded rectangle (non-extension)
        private static void FillRoundedRectangle(Graphics g, Brush brush, Rectangle rect, int radius)
        {
            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                int d = radius * 2;
                path.AddArc(rect.X, rect.Y, d, d, 180, 90);
                path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
                g.FillPath(brush, path);
            }
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

        private void BtnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private static string ShowInputBox(string prompt, string title, string defaultValue = "")
        {
            using (Form frm = new Form())
            {
                frm.Width = 420;
                frm.Height = 160;
                frm.FormBorderStyle = FormBorderStyle.FixedDialog;
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.MinimizeBox = false;
                frm.MaximizeBox = false;
                frm.Text = title;

                Label lbl = new Label() { Left = 12, Top = 9, Text = prompt, AutoSize = true };
                TextBox txt = new TextBox() { Left = 12, Top = 32, Width = 380, Text = defaultValue };
                Button ok = new Button() { Text = "OK", Left = 220, Width = 80, Top = 66, DialogResult = DialogResult.OK };
                Button cancel = new Button() { Text = "Отмена", Left = 312, Width = 80, Top = 66, DialogResult = DialogResult.Cancel };

                frm.Controls.Add(lbl);
                frm.Controls.Add(txt);
                frm.Controls.Add(ok);
                frm.Controls.Add(cancel);
                frm.AcceptButton = ok;
                frm.CancelButton = cancel;

                return frm.ShowDialog() == DialogResult.OK ? txt.Text : null;
            }
        }
        private void LoadStats()
        {
            try
            {
                var tot = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Scooters");
                var avail = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Scooters WHERE condition_id = 1");
                var att = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Scooters WHERE condition_id <> 1");
                if (lblTotalScooters != null) lblTotalScooters.Text = (tot ?? 0).ToString();
                if (lblAvailable != null) lblAvailable.Text = (avail ?? 0).ToString();
                if (lblAttention != null) lblAttention.Text = (att ?? 0).ToString();
                if (lblWarning != null) lblWarning.Text = "";
            }
            catch
            {
                if (lblTotalScooters != null) lblTotalScooters.Text = "0";
                if (lblAvailable != null) lblAvailable.Text = "0";
                if (lblAttention != null) lblAttention.Text = "0";
            }
        }
    }
}
