using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace ScooterShare
{
    internal sealed class MaintenanceControl : UserControl
    {
        private readonly Panel header;
        private readonly Label title;

        private readonly Panel addCard;
        private readonly Panel listCard;

        private readonly ComboBox cbScooter;
        private readonly ComboBox cbServiceType;
        private readonly TextBox txtNewServiceType;
        private readonly DateTimePicker dtpDate;
        private readonly TextBox txtCompletedWork;
        private readonly TextBox txtExpenses;
        private readonly Button btnSave;

        private readonly DataGridView dgv;

        private const string NewServiceTypeSentinel = "__NEW__";

        public MaintenanceControl()
        {
            BackColor = Color.FromArgb(245, 247, 250);
            Dock = DockStyle.Fill;
            Padding = new Padding(24, 22, 24, 22);

            header = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.Transparent };
            title = new Label
            {
                Text = "Техническое обслуживание",
                AutoSize = true,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(34, 34, 34),
                Location = new Point(0, 6)
            };
            header.Controls.Add(title);

            addCard = CreateCard();
            addCard.Dock = DockStyle.Top;
            addCard.Height = 230;
            addCard.Padding = new Padding(18);
            addCard.Margin = new Padding(0, 0, 0, 14);

            listCard = CreateCard();
            listCard.Dock = DockStyle.Fill;
            listCard.Padding = new Padding(18);

            Controls.Add(listCard);
            Controls.Add(addCard);
            Controls.Add(header);

            // --- Add form layout ---
            var lblHint = new Label
            {
                Text = "Добавить ТО",
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(0, 0)
            };
            addCard.Controls.Add(lblHint);

            // Layout: table to avoid "съезд" элементов
            var formLayout = new TableLayoutPanel
            {
                Left = 0,
                Top = 34,
                Width = addCard.ClientSize.Width - 4,
                Height = addCard.ClientSize.Height - 34,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ColumnCount = 4,
                RowCount = 6
            };
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22F));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16F));
            for (int i = 0; i < 6; i++)
            {
                formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, i % 2 == 0 ? 18F : 36F));
            }

            cbScooter = new ComboBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList };
            dtpDate = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd.MM.yyyy HH:mm",
                Font = new Font("Segoe UI", 10F),
                Value = DateTime.Now
            };
            cbServiceType = new ComboBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList };

            txtCompletedWork = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F) };
            txtNewServiceType = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F), Visible = false, Enabled = false };
            txtExpenses = new TextBox { Dock = DockStyle.Left, Width = 180, Font = new Font("Segoe UI", 10F) };

            btnSave = new Button
            {
                Text = "Сохранить",
                Height = 36,
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(12, 10, 23),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;

            // Row 0 labels
            formLayout.Controls.Add(CreateFieldLabel("Самокат", 0, 0), 0, 0);
            formLayout.Controls.Add(CreateFieldLabel("Дата", 0, 0), 1, 0);
            formLayout.Controls.Add(CreateFieldLabel("Тип обслуживания", 0, 0), 2, 0);
            formLayout.Controls.Add(new Label { Text = "", AutoSize = true }, 3, 0);

            // Row 1 inputs
            formLayout.Controls.Add(cbScooter, 0, 1);
            formLayout.Controls.Add(dtpDate, 1, 1);
            formLayout.Controls.Add(cbServiceType, 2, 1);
            formLayout.Controls.Add(btnSave, 3, 1);

            // Row 2 labels
            var lblWork = CreateFieldLabel("Выполненные работы", 0, 0);
            formLayout.Controls.Add(lblWork, 0, 2);
            formLayout.SetColumnSpan(lblWork, 2);
            formLayout.Controls.Add(CreateFieldLabel("Новый тип (если выбрано добавление)", 0, 0), 2, 2);
            formLayout.Controls.Add(CreateFieldLabel("Расходы (₽)", 0, 0), 3, 2);

            // Row 3 inputs
            formLayout.Controls.Add(txtCompletedWork, 0, 3);
            formLayout.SetColumnSpan(txtCompletedWork, 2);
            formLayout.Controls.Add(txtNewServiceType, 2, 3);
            formLayout.Controls.Add(txtExpenses, 3, 3);

            addCard.Controls.Add(formLayout);

            // --- List layout ---
            var lblList = new Label
            {
                Text = "История ТО",
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Dock = DockStyle.Top,
                Height = 30
            };
            listCard.Controls.Add(lblList);

            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false
            };
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10F);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(236, 242, 247);
            dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(12, 10, 23);
            listCard.Controls.Add(dgv);
            dgv.BringToFront();

            Load += (_, __) => InitializeData();
            cbServiceType.SelectedIndexChanged += (_, __) => SyncNewTypeVisibility();
            btnSave.Click += (_, __) => SaveMaintenance();
            addCard.Resize += (_, __) => { formLayout.Width = Math.Max(200, addCard.ClientSize.Width - 4); };
        }

        private void InitializeData()
        {
            ReloadCombos();
            ReloadGrid();
        }

        private void ReloadAll()
        {
            ReloadCombos();
            ReloadGrid();
        }

        private void ReloadCombos()
        {
            LoadScooters();
            LoadServiceTypes();
        }

        private void LoadScooters()
        {
            DataTable dt = DatabaseHelper.ExecuteQuery("SELECT scooter_id, model FROM Scooters ORDER BY scooter_id DESC");
            cbScooter.DisplayMember = "Display";
            cbScooter.ValueMember = "Id";

            var source = dt.AsEnumerable()
                .Select(r => new ComboItem(
                    Convert.ToInt32(r["scooter_id"]),
                    $"#{Convert.ToInt32(r["scooter_id"])} — {r["model"]}"))
                .ToList();

            cbScooter.DataSource = source;
            if (source.Count > 0) cbScooter.SelectedIndex = 0;
        }

        private void LoadServiceTypes()
        {
            DataTable dt = DatabaseHelper.ExecuteQuery("SELECT typeOfService_id, typeOfService_name FROM typesOfServices ORDER BY typeOfService_name");
            cbServiceType.DisplayMember = "Display";
            cbServiceType.ValueMember = "Key";

            var source = dt.AsEnumerable()
                .Select(r => new ComboKeyItem(
                    Convert.ToInt32(r["typeOfService_id"]).ToString(),
                    r["typeOfService_name"] == DBNull.Value ? "" : r["typeOfService_name"].ToString()))
                .ToList();

            source.Insert(0, new ComboKeyItem(NewServiceTypeSentinel, "➕ Добавить новый тип..."));
            cbServiceType.DataSource = source;
            cbServiceType.SelectedIndex = 0;
            SyncNewTypeVisibility();
        }

        private void SyncNewTypeVisibility()
        {
            var selected = cbServiceType.SelectedItem as ComboKeyItem;
            bool isNew = selected != null && string.Equals(selected.Key, NewServiceTypeSentinel, StringComparison.Ordinal);
            txtNewServiceType.Visible = isNew;
            txtNewServiceType.Enabled = isNew;
            if (!isNew) txtNewServiceType.Text = "";
        }

        private void ReloadGrid()
        {
            string q = @"
SELECT
    tm.TM_id AS [TM ID],
    tm.TM_date AS [Дата],
    s.scooter_id AS [Самокат ID],
    s.model AS [Модель],
    ts.typeOfService_name AS [Тип обслуживания],
    tm.completedWork AS [Работы],
    tm.expences AS [Расходы]
FROM TechnicalMaintenance tm
JOIN typesOfServices ts ON ts.typeOfService_id = tm.typeOfService_id
LEFT JOIN Scooter_TM stm ON stm.TM_id = tm.TM_id
LEFT JOIN Scooters s ON s.scooter_id = stm.scooter_id
ORDER BY tm.TM_id DESC;";

            DataTable dt = DatabaseHelper.ExecuteQuery(q);
            dgv.DataSource = dt;
            if (dgv.Columns.Contains("Работы"))
            {
                dgv.Columns["Работы"].FillWeight = 180f;
            }
        }

        private void SaveMaintenance()
        {
            var scooter = cbScooter.SelectedItem as ComboItem;
            if (scooter == null)
            {
                MessageBox.Show("Выберите самокат.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string completedWork = (txtCompletedWork.Text ?? string.Empty).Trim();
            if (completedWork.Length == 0)
            {
                MessageBox.Show("Заполните поле «Выполненные работы».", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int typeId = EnsureServiceTypeAndGetId();
            if (typeId <= 0) return;

            DateTime dt = dtpDate.Value;

            decimal expenses = 0m;
            string expRaw = (txtExpenses.Text ?? string.Empty).Trim();
            if (expRaw.Length > 0)
            {
                expRaw = expRaw.Replace(" ", "");
                if (!decimal.TryParse(expRaw, NumberStyles.Any, CultureInfo.CurrentCulture, out expenses) &&
                    !decimal.TryParse(expRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out expenses))
                {
                    MessageBox.Show("Поле «Расходы» должно быть числом.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            try
            {
                object newTmIdObj = DatabaseHelper.ExecuteScalar(@"
INSERT INTO TechnicalMaintenance (TM_date, completedWork, typeOfService_id, expences)
VALUES (@d, @w, @t, @e);
SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    new[]
                    {
                        new SqlParameter("@d", dt),
                        new SqlParameter("@w", completedWork),
                        new SqlParameter("@t", typeId),
                        new SqlParameter("@e", expenses)
                    });

                int newTmId = newTmIdObj == null || newTmIdObj == DBNull.Value ? 0 : Convert.ToInt32(newTmIdObj);
                if (newTmId <= 0)
                {
                    MessageBox.Show("Не удалось создать запись ТО.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // link scooter <-> TM
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO Scooter_TM (TM_id, scooter_id) VALUES (@tm, @s)",
                    new[] { new SqlParameter("@tm", newTmId), new SqlParameter("@s", scooter.Id) });

                try { DatabaseHelper.LogActivity($"Добавлено ТО #{newTmId} для самоката #{scooter.Id}"); } catch { }

                txtCompletedWork.Text = "";
                txtExpenses.Text = "";
                ReloadAll();
                MessageBox.Show("ТО сохранено.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения ТО:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int EnsureServiceTypeAndGetId()
        {
            var selected = cbServiceType.SelectedItem as ComboKeyItem;
            if (selected == null)
            {
                MessageBox.Show("Выберите тип обслуживания.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return -1;
            }

            if (!string.Equals(selected.Key, NewServiceTypeSentinel, StringComparison.Ordinal))
            {
                int id;
                return int.TryParse(selected.Key, out id) ? id : -1;
            }

            string newTypeName = (txtNewServiceType.Text ?? string.Empty).Trim();
            if (newTypeName.Length == 0)
            {
                MessageBox.Show("Введите название нового типа обслуживания.", "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return -1;
            }

            try
            {
                object existing = DatabaseHelper.ExecuteScalar(
                    "SELECT TOP 1 typeOfService_id FROM typesOfServices WHERE LTRIM(RTRIM(typeOfService_name)) = LTRIM(RTRIM(@n))",
                    new[] { new SqlParameter("@n", newTypeName) });

                if (existing != null && existing != DBNull.Value)
                {
                    return Convert.ToInt32(existing);
                }

                object inserted = DatabaseHelper.ExecuteScalar(@"
INSERT INTO typesOfServices (typeOfService_name) VALUES (@n);
SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    new[] { new SqlParameter("@n", newTypeName) });

                int newId = inserted == null || inserted == DBNull.Value ? -1 : Convert.ToInt32(inserted);
                if (newId > 0)
                {
                    try { DatabaseHelper.LogActivity($"Добавлен тип обслуживания: {newTypeName}"); } catch { }
                    LoadServiceTypes();

                    var items = cbServiceType.DataSource as System.Collections.Generic.List<ComboKeyItem>;
                    if (items != null)
                    {
                        int idx = items.FindIndex(x => x.Key == newId.ToString());
                        if (idx >= 0) cbServiceType.SelectedIndex = idx;
                    }

                    return newId;
                }

                MessageBox.Show("Не удалось добавить новый тип обслуживания.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления типа обслуживания:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }

        private static Panel CreateCard()
        {
            var p = new Panel
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            p.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.FromArgb(235, 235, 235), 1f))
                {
                    var rect = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
                    using (var path = CreateRoundRectPath(rect, 12))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                }
            };
            p.Resize += (_, __) =>
            {
                if (p.Width <= 0 || p.Height <= 0) return;
                using (var path = CreateRoundRectPath(new Rectangle(0, 0, p.Width, p.Height), 12))
                {
                    p.Region = new Region(path);
                }
            };
            return p;
        }

        private static GraphicsPath CreateRoundRectPath(Rectangle rect, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static Label CreateFieldLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Left = x,
                Top = y,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(120, 130, 140)
            };
        }

        private static ComboBox CreateComboBox(int x, int y, int width)
        {
            return new ComboBox
            {
                Left = x,
                Top = y,
                Width = width,
                Height = 32,
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
        }

        private sealed class ComboItem
        {
            public int Id { get; }
            public string Display { get; }
            public ComboItem(int id, string display) { Id = id; Display = display; }
            public override string ToString() { return Display; }
        }

        private sealed class ComboKeyItem
        {
            public string Key { get; }
            public string Display { get; }
            public ComboKeyItem(string key, string display) { Key = key; Display = display; }
            public override string ToString() { return Display; }
        }
    }
}

