using System;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace ScooterShare
{
    internal static class UserInputDialog
    {
        /// <summary>
        /// Shows a user-friendly single-field input dialog with hint + inline validation.
        /// Returns null when cancelled.
        /// </summary>
        public static string Prompt(
            IWin32Window owner,
            string title,
            string label,
            string defaultValue,
            string hint,
            Func<string, string> validate,
            bool lettersOnly = false,
            bool trimResult = true)
        {
            using (var frm = new Form())
            using (var errorProvider = new ErrorProvider())
            {
                frm.Text = title ?? string.Empty;
                frm.FormBorderStyle = FormBorderStyle.FixedDialog;
                frm.StartPosition = owner == null ? FormStartPosition.CenterScreen : FormStartPosition.CenterParent;
                frm.MinimizeBox = false;
                frm.MaximizeBox = false;
                frm.ShowIcon = false;
                frm.ShowInTaskbar = false;
                frm.Font = new Font("Segoe UI", 9F);
                frm.ClientSize = new Size(440, 190);

                var lbl = new Label
                {
                    Left = 14,
                    Top = 12,
                    Width = frm.ClientSize.Width - 28,
                    AutoSize = false,
                    Text = label ?? string.Empty
                };

                var txt = new TextBox
                {
                    Left = 14,
                    Top = 38,
                    Width = frm.ClientSize.Width - 28,
                    Text = defaultValue ?? string.Empty
                };

                var hintLbl = new Label
                {
                    Left = 14,
                    Top = 68,
                    Width = frm.ClientSize.Width - 28,
                    Height = 32,
                    AutoEllipsis = true,
                    ForeColor = Color.FromArgb(120, 120, 120),
                    Text = hint ?? string.Empty
                };

                var errLbl = new Label
                {
                    Left = 14,
                    Top = 102,
                    Width = frm.ClientSize.Width - 28,
                    Height = 34,
                    ForeColor = Color.FromArgb(190, 60, 60),
                    Text = string.Empty
                };

                var ok = new Button
                {
                    Text = "OK",
                    Left = frm.ClientSize.Width - 14 - 170,
                    Top = frm.ClientSize.Height - 44,
                    Width = 80,
                    DialogResult = DialogResult.OK
                };

                var cancel = new Button
                {
                    Text = "Отмена",
                    Left = frm.ClientSize.Width - 14 - 80,
                    Top = frm.ClientSize.Height - 44,
                    Width = 80,
                    DialogResult = DialogResult.Cancel
                };

                errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
                errorProvider.ContainerControl = frm;

                Action validateUi = () =>
                {
                    string value = txt.Text ?? string.Empty;
                    string error;
                    if (lettersOnly && Regex.IsMatch(value, @"\d"))
                    {
                        error = "Здесь нужно писать буквы (цифры нельзя).";
                    }
                    else
                    {
                        error = validate == null ? null : validate(value);
                    }
                    bool isOk = string.IsNullOrWhiteSpace(error);

                    ok.Enabled = isOk;
                    errLbl.Text = isOk ? string.Empty : error;
                    errorProvider.SetError(txt, isOk ? string.Empty : error);
                };

                txt.KeyPress += (_, e) =>
                {
                    if (!lettersOnly)
                    {
                        return;
                    }

                    if (char.IsControl(e.KeyChar))
                    {
                        return;
                    }

                    if (char.IsDigit(e.KeyChar))
                    {
                        e.Handled = true;
                        errLbl.Text = "Здесь нужно писать буквы (цифры нельзя).";
                        errorProvider.SetError(txt, errLbl.Text);
                        ok.Enabled = false;
                        return;
                    }
                };

                txt.TextChanged += (_, __) => validateUi();
                txt.KeyDown += (_, e) =>
                {
                    if (e.KeyCode == Keys.Enter && ok.Enabled)
                    {
                        frm.DialogResult = DialogResult.OK;
                        frm.Close();
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                };

                frm.Controls.Add(lbl);
                frm.Controls.Add(txt);
                frm.Controls.Add(hintLbl);
                frm.Controls.Add(errLbl);
                frm.Controls.Add(ok);
                frm.Controls.Add(cancel);

                frm.AcceptButton = ok;
                frm.CancelButton = cancel;

                frm.Shown += (_, __) =>
                {
                    txt.SelectAll();
                    txt.Focus();
                    validateUi();
                };

                if (frm.ShowDialog(owner) != DialogResult.OK)
                {
                    return null;
                }

                string result = txt.Text ?? string.Empty;
                if (trimResult)
                {
                    result = result.Trim();
                }
                return result;
            }
        }
    }
}

