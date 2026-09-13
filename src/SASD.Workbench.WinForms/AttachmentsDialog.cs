using SASD.Workbench.Application.Services;
using SASD.Workbench.Domain.Entities;

namespace SASD.Workbench.WinForms;

/// <summary>
/// Provides the V1 attachment workflow while keeping controlled storage rules in AttachmentService.
/// </summary>
internal sealed class AttachmentsDialog : Form
{
    private readonly AttachmentService _attachmentService;
    private readonly Entry _entry;

    private readonly ListView _attachmentList = new();
    private readonly Button _editCommentButton = new();
    private readonly Button _removeButton = new();
    private readonly Label _detailsLabel = new();

    public AttachmentsDialog(AttachmentService attachmentService, Entry entry)
    {
        _attachmentService = attachmentService ?? throw new ArgumentNullException(nameof(attachmentService));
        _entry = entry ?? throw new ArgumentNullException(nameof(entry));

        Text = $"Attachments — {entry.Title}";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(820, 520);
        Width = 980;
        Height = 650;

        BuildLayout();
        Shown += AttachmentsDialog_Shown;
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(10)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        root.Controls.Add(new Label
        {
            Text = "Attachments are copied into controlled Workbench storage. The original source file remains untouched.",
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 8)
        }, 0, 0);

        _attachmentList.Dock = DockStyle.Fill;
        _attachmentList.View = View.Details;
        _attachmentList.FullRowSelect = true;
        _attachmentList.MultiSelect = false;
        _attachmentList.HideSelection = false;
        _attachmentList.Columns.Add("File", 270);
        _attachmentList.Columns.Add("Size", 90);
        _attachmentList.Columns.Add("Comment", 320);
        _attachmentList.Columns.Add("SHA-256", 520);
        _attachmentList.SelectedIndexChanged += AttachmentList_SelectedIndexChanged;
        root.Controls.Add(_attachmentList, 0, 1);

        _detailsLabel.AutoSize = true;
        _detailsLabel.Padding = new Padding(0, 8, 0, 4);
        _detailsLabel.Text = "Select an attachment to inspect its metadata.";
        root.Controls.Add(_detailsLabel, 0, 2);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 8, 0, 0)
        };
        var addButton = new Button { Text = "Add file...", AutoSize = true };
        addButton.Click += AddButton_Click;
        _editCommentButton.Text = "Edit comment...";
        _editCommentButton.AutoSize = true;
        _editCommentButton.Enabled = false;
        _editCommentButton.Click += EditCommentButton_Click;
        _removeButton.Text = "Remove from entry";
        _removeButton.AutoSize = true;
        _removeButton.Enabled = false;
        _removeButton.Click += RemoveButton_Click;
        var closeButton = new Button { Text = "Close", DialogResult = DialogResult.Cancel, AutoSize = true };
        buttons.Controls.Add(addButton);
        buttons.Controls.Add(_editCommentButton);
        buttons.Controls.Add(_removeButton);
        buttons.Controls.Add(closeButton);
        root.Controls.Add(buttons, 0, 3);

        CancelButton = closeButton;
        Controls.Add(root);
    }

    private async void AttachmentsDialog_Shown(object? sender, EventArgs e)
        => await ReloadAsync().ConfigureAwait(true);

    private void AttachmentList_SelectedIndexChanged(object? sender, EventArgs e)
    {
        var attachment = SelectedAttachment;
        _editCommentButton.Enabled = attachment is not null;
        _removeButton.Enabled = attachment is not null;
        _detailsLabel.Text = attachment is null
            ? "Select an attachment to inspect its metadata."
            : $"Stored: {attachment.StoredFileName}   |   Type: {attachment.MimeType ?? "unknown"}   |   Added: {attachment.CreatedAtUtc.ToLocalTime():G}";
    }

    private async void AddButton_Click(object? sender, EventArgs e)
    {
        using var fileDialog = new OpenFileDialog
        {
            Title = "Add attachment to SASD Workbench",
            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = false,
            Filter = "All files (*.*)|*.*"
        };
        if (fileDialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        using var commentDialog = new AttachmentCommentDialog($"Comment — {Path.GetFileName(fileDialog.FileName)}");
        if (commentDialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var attachment = await _attachmentService.AddAsync(
                _entry.Id,
                fileDialog.FileName,
                commentDialog.Comment).ConfigureAwait(true);
            await ReloadAsync(attachment.Id).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            ShowError("The attachment could not be added.", ex);
        }
    }

    private async void EditCommentButton_Click(object? sender, EventArgs e)
    {
        var attachment = SelectedAttachment;
        if (attachment is null)
        {
            return;
        }

        using var dialog = new AttachmentCommentDialog($"Edit comment — {attachment.OriginalFileName}", attachment.Comment);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var updated = await _attachmentService.UpdateCommentAsync(attachment.Id, dialog.Comment).ConfigureAwait(true);
            await ReloadAsync(updated.Id).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            ShowError("The attachment comment could not be updated.", ex);
        }
    }

    private async void RemoveButton_Click(object? sender, EventArgs e)
    {
        var attachment = SelectedAttachment;
        if (attachment is null)
        {
            return;
        }

        var answer = MessageBox.Show(
            this,
            $"Remove '{attachment.OriginalFileName}' from this entry?\n\n" +
            "The attachment metadata is soft-deleted. The controlled file is intentionally retained for recovery and a later cleanup policy.",
            "Remove attachment",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);
        if (answer != DialogResult.Yes)
        {
            return;
        }

        try
        {
            await _attachmentService.DeleteAsync(attachment.Id).ConfigureAwait(true);
            await ReloadAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            ShowError("The attachment could not be removed from the entry.", ex);
        }
    }

    private Attachment? SelectedAttachment
        => _attachmentList.SelectedItems.Count == 1
            ? _attachmentList.SelectedItems[0].Tag as Attachment
            : null;

    private async Task ReloadAsync(Guid? selectAttachmentId = null)
    {
        try
        {
            var attachments = await _attachmentService.ListByEntryAsync(_entry.Id).ConfigureAwait(true);
            _attachmentList.BeginUpdate();
            _attachmentList.Items.Clear();

            ListViewItem? itemToSelect = null;
            foreach (var attachment in attachments)
            {
                var item = new ListViewItem(attachment.OriginalFileName)
                {
                    Tag = attachment
                };
                item.SubItems.Add(FormatFileSize(attachment.FileSize));
                item.SubItems.Add(attachment.Comment ?? string.Empty);
                item.SubItems.Add(attachment.Sha256Hash);
                _attachmentList.Items.Add(item);

                if (selectAttachmentId.HasValue && attachment.Id == selectAttachmentId.Value)
                {
                    itemToSelect = item;
                }
            }

            _attachmentList.EndUpdate();
            if (itemToSelect is not null)
            {
                itemToSelect.Selected = true;
                itemToSelect.EnsureVisible();
            }
            else if (_attachmentList.Items.Count > 0)
            {
                _attachmentList.Items[0].Selected = true;
            }
            else
            {
                AttachmentList_SelectedIndexChanged(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            _attachmentList.EndUpdate();
            ShowError("Attachments could not be loaded.", ex);
        }
    }

    private static string FormatFileSize(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        var value = (double)bytes;
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return unit == 0 ? $"{bytes} B" : $"{value:0.##} {units[unit]}";
    }

    private void ShowError(string message, Exception exception)
        => MessageBox.Show(this, $"{message}\n\n{exception.Message}", "SASD Workbench", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
