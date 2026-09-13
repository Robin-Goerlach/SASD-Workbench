namespace SASD.Workbench.WinForms;

/// <summary>
/// Collects an optional user-authored attachment comment without coupling file selection to metadata editing.
/// </summary>
internal sealed class AttachmentCommentDialog : Form
{
    private readonly TextBox _commentTextBox = new();

    public AttachmentCommentDialog(string title, string? currentComment = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        Width = 600;
        Height = 330;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(12)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        layout.Controls.Add(new Label
        {
            Text = "Comment (optional)",
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 6)
        }, 0, 0);

        _commentTextBox.Dock = DockStyle.Fill;
        _commentTextBox.Multiline = true;
        _commentTextBox.ScrollBars = ScrollBars.Vertical;
        _commentTextBox.Text = currentComment ?? string.Empty;
        layout.Controls.Add(_commentTextBox, 0, 1);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Padding = new Padding(0, 8, 0, 0)
        };
        var okButton = new Button { Text = "OK", DialogResult = DialogResult.OK, AutoSize = true };
        var cancelButton = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
        buttons.Controls.Add(okButton);
        buttons.Controls.Add(cancelButton);
        layout.Controls.Add(buttons, 0, 2);

        AcceptButton = okButton;
        CancelButton = cancelButton;
        Controls.Add(layout);
    }

    public string? Comment => string.IsNullOrWhiteSpace(_commentTextBox.Text) ? null : _commentTextBox.Text.Trim();
}
