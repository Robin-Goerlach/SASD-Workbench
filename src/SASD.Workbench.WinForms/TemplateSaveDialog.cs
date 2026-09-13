namespace SASD.Workbench.WinForms;

/// <summary>
/// Collects the small amount of user input required when turning the current entry into a template.
/// </summary>
internal sealed class TemplateSaveDialog : Form
{
    private readonly TextBox _nameTextBox = new();
    private readonly CheckBox _profileWideCheckBox = new();

    public TemplateSaveDialog(string suggestedName, string profileKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(suggestedName);
        ArgumentException.ThrowIfNullOrWhiteSpace(profileKey);

        Text = "Save entry as template";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        Width = 560;
        Height = 220;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            Padding = new Padding(12)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(new Label { Text = "Template name", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        _nameTextBox.Dock = DockStyle.Fill;
        _nameTextBox.Text = suggestedName;
        layout.Controls.Add(_nameTextBox, 1, 0);

        _profileWideCheckBox.AutoSize = true;
        _profileWideCheckBox.Text = $"Available to all projects in profile '{profileKey}'";
        _profileWideCheckBox.Checked = false;
        layout.Controls.Add(_profileWideCheckBox, 1, 1);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true
        };
        var saveButton = new Button { Text = "Save template", DialogResult = DialogResult.OK, AutoSize = true };
        var cancelButton = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
        saveButton.Click += SaveButton_Click;
        buttons.Controls.Add(saveButton);
        buttons.Controls.Add(cancelButton);
        layout.Controls.Add(buttons, 1, 2);

        AcceptButton = saveButton;
        CancelButton = cancelButton;
        Controls.Add(layout);
    }

    public string TemplateName => _nameTextBox.Text.Trim();
    public bool IsProfileWide => _profileWideCheckBox.Checked;

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_nameTextBox.Text))
        {
            return;
        }

        MessageBox.Show(
            this,
            "Please enter a template name.",
            "SASD Workbench",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        DialogResult = DialogResult.None;
        _nameTextBox.Focus();
    }
}
