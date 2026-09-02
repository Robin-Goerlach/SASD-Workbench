namespace SASD.Workbench.WinForms;

/// <summary>
/// Collects the minimal project metadata required by the V0.1 core.
/// </summary>
public sealed class ProjectDialog : Form
{
    private readonly TextBox _nameTextBox = new();
    private readonly TextBox _descriptionTextBox = new();

    public ProjectDialog()
    {
        Text = "New project";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        Width = 520;
        Height = 300;

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
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        layout.Controls.Add(new Label { Text = "Name", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        _nameTextBox.Dock = DockStyle.Fill;
        layout.Controls.Add(_nameTextBox, 1, 0);

        layout.Controls.Add(new Label { Text = "Description", AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Left }, 0, 1);
        _descriptionTextBox.Dock = DockStyle.Fill;
        _descriptionTextBox.Multiline = true;
        _descriptionTextBox.ScrollBars = ScrollBars.Vertical;
        layout.Controls.Add(_descriptionTextBox, 1, 1);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true
        };
        var okButton = new Button { Text = "Create", DialogResult = DialogResult.OK, AutoSize = true };
        var cancelButton = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
        okButton.Click += OkButton_Click;
        buttons.Controls.Add(okButton);
        buttons.Controls.Add(cancelButton);
        layout.Controls.Add(buttons, 1, 2);

        AcceptButton = okButton;
        CancelButton = cancelButton;
        Controls.Add(layout);
    }

    public string ProjectName => _nameTextBox.Text.Trim();
    public string? ProjectDescription => string.IsNullOrWhiteSpace(_descriptionTextBox.Text) ? null : _descriptionTextBox.Text.Trim();

    private void OkButton_Click(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_nameTextBox.Text))
        {
            return;
        }

        MessageBox.Show(this, "Please enter a project name.", "SASD Workbench", MessageBoxButtons.OK, MessageBoxIcon.Information);
        DialogResult = DialogResult.None;
        _nameTextBox.Focus();
    }
}
