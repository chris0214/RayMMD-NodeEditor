namespace RayMmdNodeEditor.Controls;

internal static class EditorTheme
{
    public static readonly Color WindowBack = Color.FromArgb(244, 246, 249);
    public static readonly Color Chrome = Color.FromArgb(251, 252, 254);
    public static readonly Color Panel = Color.FromArgb(255, 255, 255);
    public static readonly Color PanelAlt = Color.FromArgb(250, 251, 253);
    public static readonly Color PanelRaised = Color.FromArgb(247, 249, 252);
    public static readonly Color Border = Color.FromArgb(208, 214, 223);
    public static readonly Color BorderSoft = Color.FromArgb(222, 227, 234);
    public static readonly Color Accent = Color.FromArgb(92, 149, 255);
    public static readonly Color AccentMuted = Color.FromArgb(61, 86, 126);
    public static readonly Color TextPrimary = Color.FromArgb(26, 30, 36);
    public static readonly Color TextSecondary = Color.FromArgb(78, 86, 98);
    public static readonly Color TextMuted = Color.FromArgb(116, 123, 134);
    public static readonly ToolStripRenderer ToolStripRenderer = new ToolStripProfessionalRenderer(new EditorColorTable());

    public static void StyleTextBox(TextBox textBox)
    {
        textBox.BackColor = PanelAlt;
        textBox.ForeColor = TextPrimary;
        textBox.BorderStyle = BorderStyle.FixedSingle;
    }

    public static void StyleRichTextBox(RichTextBox textBox)
    {
        textBox.BackColor = PanelAlt;
        textBox.ForeColor = TextPrimary;
        textBox.BorderStyle = BorderStyle.None;
    }

    public static void StyleComboBox(ComboBox comboBox)
    {
        comboBox.BackColor = PanelAlt;
        comboBox.ForeColor = TextPrimary;
        comboBox.FlatStyle = FlatStyle.Flat;
    }

    public static void StyleButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.BackColor = PanelRaised;
        button.ForeColor = TextPrimary;
        button.FlatAppearance.BorderColor = BorderSoft;
        button.FlatAppearance.MouseDownBackColor = Blend(PanelRaised, Accent, 0.24f);
        button.FlatAppearance.MouseOverBackColor = Blend(PanelRaised, Accent, 0.14f);
    }

    public static void StyleNumeric(NumericUpDown numeric)
    {
        numeric.BackColor = PanelAlt;
        numeric.ForeColor = TextPrimary;
        numeric.BorderStyle = BorderStyle.FixedSingle;
    }

    public static void StyleGroupBox(GroupBox groupBox)
    {
        groupBox.BackColor = Panel;
        groupBox.ForeColor = TextSecondary;
    }

    public static void ApplyThemeRecursive(Control root)
    {
        switch (root)
        {
            case TextBox textBox:
                StyleTextBox(textBox);
                break;
            case RichTextBox richTextBox:
                StyleRichTextBox(richTextBox);
                break;
            case ComboBox comboBox:
                StyleComboBox(comboBox);
                break;
            case Button button:
                StyleButton(button);
                break;
            case NumericUpDown numeric:
                StyleNumeric(numeric);
                break;
            case GroupBox groupBox:
                StyleGroupBox(groupBox);
                break;
            case Label label:
                label.ForeColor = label.Font.Bold ? TextPrimary : TextSecondary;
                label.BackColor = Color.Transparent;
                break;
        }

        foreach (Control child in root.Controls)
        {
            ApplyThemeRecursive(child);
        }
    }

    public static Color Blend(Color baseColor, Color overlay, float amount)
    {
        var clamped = Math.Clamp(amount, 0f, 1f);
        return Color.FromArgb(
            (int)Math.Round(baseColor.R + (overlay.R - baseColor.R) * clamped),
            (int)Math.Round(baseColor.G + (overlay.G - baseColor.G) * clamped),
            (int)Math.Round(baseColor.B + (overlay.B - baseColor.B) * clamped));
    }

    private sealed class EditorColorTable : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground => Panel;
        public override Color MenuBorder => BorderSoft;
        public override Color MenuItemBorder => BorderSoft;
        public override Color MenuItemSelected => Blend(PanelRaised, Accent, 0.18f);
        public override Color MenuItemSelectedGradientBegin => Blend(PanelRaised, Accent, 0.18f);
        public override Color MenuItemSelectedGradientEnd => Blend(PanelRaised, Accent, 0.18f);
        public override Color MenuItemPressedGradientBegin => PanelRaised;
        public override Color MenuItemPressedGradientEnd => PanelRaised;
        public override Color MenuItemPressedGradientMiddle => PanelRaised;
        public override Color ImageMarginGradientBegin => Panel;
        public override Color ImageMarginGradientMiddle => Panel;
        public override Color ImageMarginGradientEnd => Panel;
        public override Color ToolStripGradientBegin => Chrome;
        public override Color ToolStripGradientMiddle => Chrome;
        public override Color ToolStripGradientEnd => Chrome;
        public override Color MenuStripGradientBegin => Chrome;
        public override Color MenuStripGradientEnd => Chrome;
        public override Color StatusStripGradientBegin => Chrome;
        public override Color StatusStripGradientEnd => Chrome;
        public override Color ButtonSelectedHighlight => Blend(PanelRaised, Accent, 0.14f);
        public override Color ButtonSelectedHighlightBorder => BorderSoft;
        public override Color ButtonPressedHighlight => Blend(PanelRaised, Accent, 0.24f);
        public override Color ButtonPressedHighlightBorder => AccentMuted;
        public override Color SeparatorDark => BorderSoft;
        public override Color SeparatorLight => BorderSoft;
    }
}
