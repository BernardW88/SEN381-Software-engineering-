using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Presentation
{
    public class StatusPickerForm : Form
    {
        private ComboBox cmbStatuses;
        private Button btnOk;
        private Button btnCancel;

        public string SelectedStatus { get; private set; }

        // mapping: display -> canonical
        private readonly Dictionary<string, string> _displayToCanonical = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Done", "Resolved" },
            { "Resolved", "Resolved" },
            { "In Progress", "In Progress" },
            { "Closed", "Closed" },
            { "Submitted", "Submitted" }
        };

        public StatusPickerForm(string currentStatus, IEnumerable<string> canonicalOptions)
        {
            this.Text = "Select Target Status";
            this.Size = new Size(360, 140);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label lbl = new Label { Text = $"Current: {currentStatus}", Location = new Point(12, 12), AutoSize = true };
            cmbStatuses = new ComboBox { Location = new Point(12, 36), Width = 320, DropDownStyle = ComboBoxStyle.DropDownList };

            // Friendly display: prefer "Done" label for Resolved
            var displayItems = new List<string>();
            foreach (var c in canonicalOptions.Distinct())
            {
                if (string.Equals(c, "Resolved", StringComparison.OrdinalIgnoreCase))
                    displayItems.Add("Done");
                else
                    displayItems.Add(c);
            }

            cmbStatuses.Items.AddRange(displayItems.ToArray());
            if (cmbStatuses.Items.Count > 0) cmbStatuses.SelectedIndex = 0;

            btnOk = new Button { Text = "OK", Location = new Point(172, 72), Width = 75, DialogResult = DialogResult.OK };
            btnCancel = new Button { Text = "Cancel", Location = new Point(252, 72), Width = 75, DialogResult = DialogResult.Cancel };

            btnOk.Click += BtnOk_Click;

            this.Controls.AddRange(new Control[] { lbl, cmbStatuses, btnOk, btnCancel });
            // Apply theme if available
            try { Theme.ApplyTheme(this, Theme.IsDark); } catch { }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (cmbStatuses.SelectedItem == null)
            {
                MessageBox.Show("Please select a status.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.None;
                return;
            }

            var display = cmbStatuses.SelectedItem.ToString();
            if (_displayToCanonical.TryGetValue(display, out var canonical))
            {
                SelectedStatus = canonical;
            }
            else
            {
                // fallback to display as canonical
                SelectedStatus = display;
            }
        }
    }
}
