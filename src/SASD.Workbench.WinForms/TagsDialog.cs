using SASD.Workbench.Application.Services;
using SASD.Workbench.Domain.Entities;

namespace SASD.Workbench.WinForms;

/// <summary>
/// Edits the many-to-many tag assignment of one entry through the shared <see cref="TagService"/>.
/// </summary>
internal sealed class TagsDialog : Form
{
    private readonly TagService _tagService;
    private readonly Entry _entry;

    private readonly CheckedListBox _tagList = new();
    private readonly TextBox _newTagTextBox = new();
    private readonly Button _applyButton = new();
    private bool _loading;

    public TagsDialog(TagService tagService, Entry entry)
    {
        _tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
        _entry = entry ?? throw new ArgumentNullException(nameof(entry));

        Text = $"Tags — {entry.Title}";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(560, 500);
        Width = 640;
        Height = 620;

        BuildLayout();
        Shown += TagsDialog_Shown;
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        root.Controls.Add(new Label
        {
            Text = "Check all tags that should be assigned to this entry. Tags are reusable across projects.",
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 8)
        }, 0, 0);

        _tagList.Dock = DockStyle.Fill;
        _tagList.CheckOnClick = true;

        // Form inherits Control.Tag (object). Fully qualify the entity here so nameof cannot bind to
        // the inherited WinForms property and silently stop being a refactoring-safe member reference.
        _tagList.DisplayMember = nameof(SASD.Workbench.Domain.Entities.Tag.Name);
        root.Controls.Add(_tagList, 0, 1);

        var createPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 3,
            Padding = new Padding(0, 10, 0, 6)
        };
        createPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        createPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        createPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        createPanel.Controls.Add(new Label { Text = "New tag", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        _newTagTextBox.Dock = DockStyle.Fill;
        createPanel.Controls.Add(_newTagTextBox, 1, 0);
        var createButton = new Button { Text = "Create + assign", AutoSize = true };
        createButton.Click += CreateButton_Click;
        createPanel.Controls.Add(createButton, 2, 0);
        root.Controls.Add(createPanel, 0, 2);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true
        };
        var closeButton = new Button { Text = "Close", DialogResult = DialogResult.Cancel, AutoSize = true };
        _applyButton.Text = "Apply assignments";
        _applyButton.AutoSize = true;
        _applyButton.Click += ApplyButton_Click;
        buttons.Controls.Add(closeButton);
        buttons.Controls.Add(_applyButton);
        root.Controls.Add(buttons, 0, 3);

        CancelButton = closeButton;
        Controls.Add(root);
    }

    private async void TagsDialog_Shown(object? sender, EventArgs e)
        => await ReloadAsync().ConfigureAwait(true);

    private async void CreateButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_newTagTextBox.Text))
        {
            MessageBox.Show(this, "Please enter a tag name.", "SASD Workbench", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _newTagTextBox.Focus();
            return;
        }

        try
        {
            var tag = await _tagService.GetOrCreateAsync(_newTagTextBox.Text).ConfigureAwait(true);
            await _tagService.AttachAsync(_entry.Id, tag.Id).ConfigureAwait(true);
            _newTagTextBox.Clear();
            await ReloadAsync(tag.Id).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            ShowError("The tag could not be created or assigned.", ex);
        }
    }

    private async void ApplyButton_Click(object? sender, EventArgs e)
    {
        if (_loading)
        {
            return;
        }

        try
        {
            _applyButton.Enabled = false;
            var assigned = await _tagService.ListByEntryAsync(_entry.Id).ConfigureAwait(true);
            var assignedIds = assigned.Select(tag => tag.Id).ToHashSet();

            for (var index = 0; index < _tagList.Items.Count; index++)
            {
                if (_tagList.Items[index] is not SASD.Workbench.Domain.Entities.Tag tag)
                {
                    continue;
                }

                var shouldBeAssigned = _tagList.GetItemChecked(index);
                var isAssigned = assignedIds.Contains(tag.Id);
                if (shouldBeAssigned && !isAssigned)
                {
                    await _tagService.AttachAsync(_entry.Id, tag.Id).ConfigureAwait(true);
                }
                else if (!shouldBeAssigned && isAssigned)
                {
                    await _tagService.DetachAsync(_entry.Id, tag.Id).ConfigureAwait(true);
                }
            }

            await ReloadAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            ShowError("Tag assignments could not be saved.", ex);
        }
        finally
        {
            _applyButton.Enabled = true;
        }
    }

    private async Task ReloadAsync(Guid? selectTagId = null)
    {
        try
        {
            _loading = true;
            var tags = await _tagService.ListAsync().ConfigureAwait(true);
            var assigned = await _tagService.ListByEntryAsync(_entry.Id).ConfigureAwait(true);
            var assignedIds = assigned.Select(tag => tag.Id).ToHashSet();

            _tagList.Items.Clear();
            var selectedIndex = -1;
            foreach (var tag in tags)
            {
                var index = _tagList.Items.Add(tag, assignedIds.Contains(tag.Id));
                if (selectTagId.HasValue && tag.Id == selectTagId.Value)
                {
                    selectedIndex = index;
                }
            }

            if (selectedIndex >= 0)
            {
                _tagList.SelectedIndex = selectedIndex;
            }
        }
        catch (Exception ex)
        {
            ShowError("Tags could not be loaded.", ex);
        }
        finally
        {
            _loading = false;
        }
    }

    private void ShowError(string message, Exception exception)
        => MessageBox.Show(this, $"{message}\n\n{exception.Message}", "SASD Workbench", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
