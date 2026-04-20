using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WpfMaterialControls.ViewModels
{
    public class UsersViewModel : ObservableObject
    {
        private readonly string middleNameColumn;
        private string searchText = string.Empty;
        private int totalUsers;
        private int activeUsers;
        private int blockedUsers;

        public ObservableCollection<UserItemViewModel> Items { get; } = new ObservableCollection<UserItemViewModel>();
        public ICollectionView FilteredItems { get; }

        public IRelayCommand AddCommand { get; }
        public IRelayCommand<object> EditCommand { get; }
        public IRelayCommand<object> DeleteCommand { get; }

        public string SearchText
        {
            get => searchText;
            set
            {
                if (SetProperty(ref searchText, value))
                {
                    FilteredItems.Refresh();
                }
            }
        }

        public int TotalUsers
        {
            get => totalUsers;
            set => SetProperty(ref totalUsers, value);
        }

        public int ActiveUsers
        {
            get => activeUsers;
            set => SetProperty(ref activeUsers, value);
        }

        public int BlockedUsers
        {
            get => blockedUsers;
            set => SetProperty(ref blockedUsers, value);
        }

        public UsersViewModel()
        {
            middleNameColumn = DetectMiddleNameColumn();
            FilteredItems = CollectionViewSource.GetDefaultView(Items);
            FilteredItems.Filter = FilterUser;

            AddCommand = new RelayCommand(OnAdd);
            EditCommand = new RelayCommand<object>(OnEdit);
            DeleteCommand = new RelayCommand<object>(OnDelete);

            Load();
        }

        private bool FilterUser(object obj)
        {
            if (!(obj is UserItemViewModel item)) return false;
            if (string.IsNullOrWhiteSpace(SearchText)) return true;

            string q = SearchText.Trim().ToLowerInvariant();
            return (item.FullName ?? string.Empty).ToLowerInvariant().Contains(q)
                || (item.Email ?? string.Empty).ToLowerInvariant().Contains(q)
                || (item.Phone ?? string.Empty).ToLowerInvariant().Contains(q);
        }

        private void Load()
        {
            Items.Clear();

            try
            {
                string middleNameSelect = string.IsNullOrEmpty(middleNameColumn)
                    ? "N'' AS middleName,"
                    : $"ISNULL(c.[{middleNameColumn}], '') AS middleName,";

                var dt = DatabaseHelper.ExecuteQuery($@"
SELECT c.client_id,
       ISNULL(c.firstName, '') AS firstName,
       ISNULL(c.lastName, '') AS lastName,
       {middleNameSelect}
       ISNULL(c.client_post, '') AS email,
       ISNULL(c.telephone_number, '') AS phone,
       c.registration_date,
       ISNULL(r.rides_count, 0) AS rides_count,
       ISNULL(r.total_revenue, 0) AS total_revenue,
       ISNULL(ic.complaints_count, 0) AS complaints_count
FROM Clients c
LEFT JOIN (
    SELECT cr.client_id,
           COUNT(*) AS rides_count,
           SUM(CASE WHEN ar.plannedfFinishTime IS NULL THEN 0
                    ELSE DATEDIFF(MINUTE, ar.start_time, ar.plannedfFinishTime) * ISNULL(rt.costPerMinute, 0)
               END) AS total_revenue
    FROM Clients_Rentals cr
    JOIN Active_rentals ar ON ar.activeRental_id = cr.activeRental_id
    LEFT JOIN Rates rt ON rt.rate_id = ar.rate_id
    GROUP BY cr.client_id
) r ON r.client_id = c.client_id
LEFT JOIN (
    SELECT client_id, COUNT(*) AS complaints_count
    FROM IncidentsAndComplaints
    GROUP BY client_id
) ic ON ic.client_id = c.client_id
ORDER BY c.client_id DESC");

                foreach (DataRow row in dt.Rows)
                {
                    int id = ToInt(row["client_id"]);
                    string firstName = ToString(row["firstName"]);
                    string lastName = ToString(row["lastName"]);
                    string middleName = ToString(row["middleName"]);
                    string email = ToString(row["email"]);
                    string phone = ToString(row["phone"]);
                    int ridesCount = ToInt(row["rides_count"]);
                    decimal totalRevenue = ToDecimal(row["total_revenue"]);
                    int complaints = ToInt(row["complaints_count"]);

                    DateTime joined = row["registration_date"] == DBNull.Value
                        ? DateTime.Today
                        : Convert.ToDateTime(row["registration_date"], CultureInfo.InvariantCulture);

                    string fullName = BuildFullName(lastName, firstName, middleName);
                    bool blocked = complaints > 0;

                    Items.Add(new UserItemViewModel
                    {
                        Id = id,
                        FullName = fullName,
                        Initials = BuildInitials(firstName, lastName),
                        Email = string.IsNullOrWhiteSpace(email) ? "—" : email,
                        Phone = string.IsNullOrWhiteSpace(phone) ? "—" : phone,
                        JoinedText = $"Присоединился {joined:dd.MM.yyyy}",
                        RidesRevenueText = $"{ridesCount} поездок • ₽{Math.Round(totalRevenue, 0):N0}".Replace(",", " "),
                        IsBlocked = blocked
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить пользователей.\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            RecalcStats();
            FilteredItems.Refresh();
        }

        private void RecalcStats()
        {
            TotalUsers = Items.Count;
            int blocked = 0;
            foreach (var item in Items)
            {
                if (item.IsBlocked) blocked++;
            }
            BlockedUsers = blocked;
            ActiveUsers = TotalUsers - BlockedUsers;
        }

        private void OnAdd()
        {
            try
            {
                Window owner = Application.Current?.MainWindow;

                string firstName = WpfMaterialControls.PromptDialog.ShowDialog(
                    owner,
                    "Добавление пользователя",
                    "Имя",
                    "Например: Иван. Поле обязательно.",
                    string.Empty,
                    ValidatePersonNameRequired,
                    lettersOnly: true);
                if (firstName == null) return;

                string lastName = WpfMaterialControls.PromptDialog.ShowDialog(
                    owner,
                    "Добавление пользователя",
                    "Фамилия",
                    "Например: Петров. Поле обязательно.",
                    string.Empty,
                    ValidatePersonNameRequired,
                    lettersOnly: true);
                if (lastName == null) return;

                string middleName = WpfMaterialControls.PromptDialog.ShowDialog(
                    owner,
                    "Добавление пользователя",
                    "Отчество (можно оставить пустым)",
                    "Например: Сергеевич. Если нет — оставь пустым.",
                    string.Empty,
                    ValidateMiddleNameOptional,
                    lettersOnly: true);
                if (middleName == null) return;

                string email = WpfMaterialControls.PromptDialog.ShowDialog(
                    owner,
                    "Добавление пользователя",
                    "Email",
                    "Пример: ivan.petrov@gmail.com",
                    string.Empty,
                    ValidateEmailRequired);
                if (email == null) return;

                string phone = WpfMaterialControls.PromptDialog.ShowDialog(
                    owner,
                    "Добавление пользователя",
                    "Телефон",
                    "Пример: +7 999 123-45-67. Можно вводить с пробелами/дефисами.",
                    string.Empty,
                    ValidatePhoneRequired);
                if (phone == null) return;

                string passport = WpfMaterialControls.PromptDialog.ShowDialog(
                    owner,
                    "Добавление пользователя",
                    "Серия и номер паспорта",
                    "Формат: 0000 000000 (4 цифры пробел 6 цифр).",
                    "0000 000000",
                    ValidatePassportRequired);
                if (passport == null) return;

                string insertQuery;
                SqlParameter[] parameters;
                if (!string.IsNullOrEmpty(middleNameColumn))
                {
                    insertQuery = $@"INSERT INTO Clients (firstName, lastName, [{middleNameColumn}], client_post, telephone_number, registration_date, passport_series_number)
                                     VALUES (@fn, @ln, @mn, @em, @ph, GETDATE(), @ps)";
                    parameters = new[]
                    {
                        new SqlParameter("@fn", firstName),
                        new SqlParameter("@ln", lastName),
                        new SqlParameter("@mn", middleName),
                        new SqlParameter("@em", email),
                        new SqlParameter("@ph", phone),
                        new SqlParameter("@ps", passport)
                    };
                }
                else
                {
                    insertQuery = @"INSERT INTO Clients (firstName, lastName, client_post, telephone_number, registration_date, passport_series_number)
                                    VALUES (@fn, @ln, @em, @ph, GETDATE(), @ps)";
                    parameters = new[]
                    {
                        new SqlParameter("@fn", firstName),
                        new SqlParameter("@ln", lastName),
                        new SqlParameter("@em", email),
                        new SqlParameter("@ph", phone),
                        new SqlParameter("@ps", passport)
                    };
                }

                DatabaseHelper.ExecuteNonQuery(insertQuery, parameters);

                try { DatabaseHelper.LogActivity($"Добавлен пользователь: {lastName} {firstName}"); } catch { }
                Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось добавить пользователя.\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnEdit(object idParam)
        {
            if (!TryGetId(idParam, out int id)) return;

            try
            {
                var row = DatabaseHelper.ExecuteQuery(
                    $@"SELECT TOP 1 firstName, lastName, {(string.IsNullOrEmpty(middleNameColumn) ? "N'' AS middleName" : $"[{middleNameColumn}] AS middleName")}, client_post, telephone_number
                       FROM Clients WHERE client_id=@id",
                    new[] { new SqlParameter("@id", id) });
                if (row.Rows.Count == 0) return;

                Window owner = Application.Current?.MainWindow;

                string firstName = WpfMaterialControls.PromptDialog.ShowDialog(
                    owner,
                    "Редактирование пользователя",
                    "Имя",
                    "Поле обязательно. Например: Иван.",
                    ToString(row.Rows[0]["firstName"]),
                    ValidatePersonNameRequired,
                    lettersOnly: true);
                if (firstName == null) return;

                string lastName = WpfMaterialControls.PromptDialog.ShowDialog(
                    owner,
                    "Редактирование пользователя",
                    "Фамилия",
                    "Поле обязательно. Например: Петров.",
                    ToString(row.Rows[0]["lastName"]),
                    ValidatePersonNameRequired,
                    lettersOnly: true);
                if (lastName == null) return;

                string middleName = WpfMaterialControls.PromptDialog.ShowDialog(
                    owner,
                    "Редактирование пользователя",
                    "Отчество (можно оставить пустым)",
                    "Если нет — оставь пустым.",
                    ToString(row.Rows[0]["middleName"]),
                    ValidateMiddleNameOptional,
                    lettersOnly: true);
                if (middleName == null) return;

                string email = WpfMaterialControls.PromptDialog.ShowDialog(
                    owner,
                    "Редактирование пользователя",
                    "Email",
                    "Пример: ivan.petrov@gmail.com",
                    ToString(row.Rows[0]["client_post"]),
                    ValidateEmailRequired);
                if (email == null) return;

                string phone = WpfMaterialControls.PromptDialog.ShowDialog(
                    owner,
                    "Редактирование пользователя",
                    "Телефон",
                    "Пример: +7 999 123-45-67",
                    ToString(row.Rows[0]["telephone_number"]),
                    ValidatePhoneRequired);
                if (phone == null) return;

                string updateQuery = !string.IsNullOrEmpty(middleNameColumn)
                    ? $@"UPDATE Clients
                         SET firstName=@fn, lastName=@ln, [{middleNameColumn}]=@mn, client_post=@em, telephone_number=@ph
                         WHERE client_id=@id"
                    : @"UPDATE Clients
                        SET firstName=@fn, lastName=@ln, client_post=@em, telephone_number=@ph
                        WHERE client_id=@id";

                DatabaseHelper.ExecuteNonQuery(
                    updateQuery,
                    !string.IsNullOrEmpty(middleNameColumn)
                    ? new[]
                    {
                        new SqlParameter("@fn", firstName),
                        new SqlParameter("@ln", lastName),
                        new SqlParameter("@mn", middleName),
                        new SqlParameter("@em", email),
                        new SqlParameter("@ph", phone),
                        new SqlParameter("@id", id)
                    }
                    : new[]
                    {
                        new SqlParameter("@fn", firstName),
                        new SqlParameter("@ln", lastName),
                        new SqlParameter("@em", email),
                        new SqlParameter("@ph", phone),
                        new SqlParameter("@id", id)
                    });

                try { DatabaseHelper.LogActivity($"Изменён пользователь #{id}: {lastName} {firstName}"); } catch { }
                Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось изменить пользователя.\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnDelete(object idParam)
        {
            if (!TryGetId(idParam, out int id)) return;

            if (MessageBox.Show($"Удалить пользователя #{id}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                int complaintsCount = 0;
                try
                {
                    var cnt = DatabaseHelper.ExecuteScalar(
                        "SELECT COUNT(1) FROM IncidentsAndComplaints WHERE client_id=@id",
                        new[] { new SqlParameter("@id", id) });
                    complaintsCount = cnt == null || cnt == DBNull.Value ? 0 : Convert.ToInt32(cnt, CultureInfo.InvariantCulture);
                }
                catch
                {
                    // ignore count errors; we'll fall back to direct delete and show DB error if any
                }

                if (complaintsCount > 0)
                {
                    var res = MessageBox.Show(
                        $"У пользователя #{id} есть связанные записи (жалобы/инциденты): {complaintsCount}.\n\n" +
                        "Удалить пользователя вместе с этими записями?",
                        "Есть связанные записи",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (res != MessageBoxResult.Yes)
                    {
                        return;
                    }

                    DatabaseHelper.ExecuteNonQuery(@"
BEGIN TRY
    BEGIN TRAN;
    DELETE FROM IncidentsAndComplaints WHERE client_id=@id;
    DELETE FROM Clients WHERE client_id=@id;
    COMMIT TRAN;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
    THROW;
END CATCH",
                        new[] { new SqlParameter("@id", id) });
                }
                else
                {
                    DatabaseHelper.ExecuteNonQuery("DELETE FROM Clients WHERE client_id=@id", new[] { new SqlParameter("@id", id) });
                }

                try { DatabaseHelper.LogActivity($"Удалён пользователь #{id}"); } catch { }
                Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось удалить пользователя.\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string ValidatePersonNameRequired(string value)
        {
            string v = (value ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(v)) return "Поле не может быть пустым. Введите значение.";
            if (v.Length < 2) return "Слишком коротко. Введите хотя бы 2 буквы.";
            if (!Regex.IsMatch(v, @"^[\p{L}\-'\s]+$")) return "Используйте только буквы (и дефис, если нужно).";
            return null;
        }

        private static string ValidateMiddleNameOptional(string value)
        {
            string v = (value ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(v)) return null;
            if (v.Length < 2) return "Слишком коротко. Либо оставьте пустым, либо введите отчество полностью.";
            if (!Regex.IsMatch(v, @"^[\p{L}\-'\s]+$")) return "Используйте только буквы (и дефис, если нужно).";
            return null;
        }

        private static string ValidateEmailRequired(string value)
        {
            string v = (value ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(v)) return "Введите email. Пример: name@example.com";
            try
            {
                var addr = new MailAddress(v);
                if (!string.Equals(addr.Address, v, StringComparison.OrdinalIgnoreCase))
                {
                    return "Неверный email. Пример: name@example.com";
                }
            }
            catch
            {
                return "Неверный email. Пример: name@example.com";
            }
            return null;
        }

        private static string ValidatePhoneRequired(string value)
        {
            string v = (value ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(v)) return "Введите телефон. Пример: +7 999 123-45-67";

            // Keep only digits to validate length.
            string digits = Regex.Replace(v, @"\D", "");
            if (digits.Length < 10) return "Слишком короткий номер. Пример: +7 999 123-45-67";
            if (digits.Length > 15) return "Слишком длинный номер. Проверьте ввод.";
            return null;
        }

        private static string ValidatePassportRequired(string value)
        {
            string v = (value ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(v)) return "Введите серию и номер. Пример: 1234 567890";

            string normalized = Regex.Replace(v, @"\s+", " ");
            if (!Regex.IsMatch(normalized, @"^\d{4}\s\d{6}$"))
            {
                return "Неверный формат. Нужно: 4 цифры пробел 6 цифр. Пример: 1234 567890";
            }
            return null;
        }

        private static int ToInt(object value)
        {
            if (value == null || value == DBNull.Value) return 0;
            return Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }

        private static decimal ToDecimal(object value)
        {
            if (value == null || value == DBNull.Value) return 0m;
            return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }

        private static string ToString(object value) => value == null || value == DBNull.Value ? string.Empty : Convert.ToString(value);

        private static string BuildFullName(string lastName, string firstName, string middleName)
        {
            string full = $"{lastName} {firstName} {middleName}".Trim();
            while (full.Contains("  ")) full = full.Replace("  ", " ");
            return full;
        }

        private static string DetectMiddleNameColumn()
        {
            try
            {
                var dt = DatabaseHelper.ExecuteQuery(
                    "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Clients'");
                string[] candidates = { "patronomyc", "patronymic", "patronymous" };
                foreach (string candidate in candidates)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string col = ToString(row["COLUMN_NAME"]);
                        if (string.Equals(col, candidate, StringComparison.OrdinalIgnoreCase))
                        {
                            return col;
                        }
                    }
                }
            }
            catch { }

            return string.Empty;
        }

        private static string BuildInitials(string firstName, string lastName)
        {
            string a = string.IsNullOrWhiteSpace(firstName) ? "?" : firstName.Trim().Substring(0, 1).ToUpperInvariant();
            string b = string.IsNullOrWhiteSpace(lastName) ? "?" : lastName.Trim().Substring(0, 1).ToUpperInvariant();
            return $"{a}{b}";
        }

        private static bool TryGetId(object value, out int id)
        {
            id = 0;
            if (value == null) return false;
            if (value is int i) { id = i; return true; }
            return int.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out id);
        }
    }
}
