using System;
using System.Drawing;
using System.Windows.Forms;

namespace Presentation
{
    public static class Theme
    {
        public static bool IsDark { get; set; } = false;

        public static void ApplyTheme(Control root, bool dark)
        {
            IsDark = dark;
            Color formBg = dark ? Color.FromArgb(30, 34, 40) : Color.FromArgb(245, 247, 250);
            Color panelBg = dark ? Color.FromArgb(34, 37, 42) : Color.White;
            Color text = dark ? Color.FromArgb(230, 230, 230) : Color.FromArgb(20, 20, 20);
            Color muted = dark ? Color.FromArgb(160, 160, 160) : Color.FromArgb(100, 100, 100);
            Color inputBg = dark ? Color.FromArgb(48, 52, 58) : Color.White;

            if (root is Form f)
            {
                f.BackColor = formBg;
                f.ForeColor = text;
            }

            ApplyToControl(root, dark, formBg, panelBg, text, muted, inputBg);
        }

        private static void ApplyToControl(Control ctrl, bool dark, Color formBg, Color panelBg, Color text, Color muted, Color inputBg)
        {
            foreach (Control c in ctrl.Controls)
            {
                try
                {
                    switch (c)
                    {
                        case Panel p:
                            p.BackColor = panelBg;
                            p.ForeColor = text;
                            break;
                        case GroupBox g:
                            g.BackColor = panelBg;
                            g.ForeColor = text;
                            break;
                        case Label l:
                            l.ForeColor = text;
                            l.BackColor = Color.Transparent;
                            break;
                        case Button b:
                            // keep existing colored buttons as-is; if neutral, theme them
                            if (b.BackColor == SystemColors.Control || b.BackColor == Color.White)
                            {
                                b.BackColor = panelBg;
                                b.ForeColor = text;
                                b.FlatAppearance.BorderColor = dark ? Color.FromArgb(80, 80, 80) : Color.FromArgb(200, 200, 200);
                                b.FlatAppearance.BorderSize = 1;
                            }
                            break;
                        case TextBox t:
                            t.BackColor = inputBg;
                            t.ForeColor = text;
                            break;
                        case ComboBox cb:
                            cb.BackColor = inputBg;
                            cb.ForeColor = text;
                            break;
                        case DataGridView dgv:
                            dgv.BackgroundColor = panelBg;
                            dgv.DefaultCellStyle.BackColor = dark ? Color.FromArgb(34, 37, 42) : Color.White;
                            dgv.DefaultCellStyle.ForeColor = text;
                            dgv.AlternatingRowsDefaultCellStyle.BackColor = dark ? Color.FromArgb(38, 42, 48) : Color.FromArgb(250, 250, 252);
                            dgv.ColumnHeadersDefaultCellStyle.BackColor = dark ? Color.FromArgb(50, 55, 63) : Color.FromArgb(30, 144, 255);
                            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                            dgv.EnableHeadersVisualStyles = false;
                            break;
                        case ListView lv:
                            lv.BackColor = panelBg;
                            lv.ForeColor = text;
                            break;
                        case CheckBox cbx:
                            cbx.ForeColor = text;
                            cbx.BackColor = Color.Transparent;
                            break;
                        case RadioButton rb:
                            rb.ForeColor = text;
                            rb.BackColor = Color.Transparent;
                            break;
                    }
                }
                catch { }

                if (c.HasChildren) ApplyToControl(c, dark, formBg, panelBg, text, muted, inputBg);
            }
        }
    }
}
