using SASD.Workbench.Application.Services;
using SASD.Workbench.Domain.Entities;
using SASD.Workbench.Domain.Metadata;

namespace SASD.Workbench.WinForms;

/// <summary>
/// Displays and edits semantic relations for one generic Workbench entry.
/// </summary>
public sealed class RelationsDialog : Form
{
    private readonly EntryLinkService _linkService;
    private readonly EntryService _entryService;
    private readonly Entry _entry;
    private readonly ListView _relationsList = new();
    private readonly ComboBox _targetComboBox = new();
    private readonly ComboBox _relationTypeComboBox = new();
    private readonly TextBox _commentTextBox = new();
    private readonly Button _deleteButton = new();
    private Dictionary<Guid, Entry> _entriesById = new();

    public RelationsDialog(EntryLinkService linkService, EntryService entryService, Entry entry)
    {
        _linkService = linkService ?? throw new ArgumentNullException(nameof(linkService));
        _entryService = entryService ?? throw new ArgumentNullException(nameof(entryService));
        _entry = entry ?? throw new ArgumentNullException(nameof(entry));

        Text = $"Relations — {entry.Title}";
        StartPosition = FormStartPosition.CenterParent;
        Width = 950;
        Height = 620;
        MinimumSize = new Size(760, 500);
        ShowInTaskbar = false;

        BuildLayout();
        Shown += async (_, _) => await ReloadAsync().ConfigureAwait(true);
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(10)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _relationsList.Dock = DockStyle.Fill;
        _relationsList.View = View.Details;
        _relationsList.FullRowSelect = true;
        _relationsList.HideSelection = false;
        _relationsList.Columns.Add("Direction", 90);
        _relationsList.Columns.Add("Relation", 170);
        _relationsList.Columns.Add("Other entry", 280);
        _relationsList.Columns.Add("Comment", 320);
        _relationsList.SelectedIndexChanged += (_, _) => _deleteButton.Enabled = _relationsList.SelectedItems.Count == 1;
        root.Controls.Add(_relationsList, 0, 0);

        var createGroup = new GroupBox
        {
            Text = "Create relation",
            Dock = DockStyle.Fill,
            AutoSize = true,
            Padding = new Padding(10)
        };
        var editor = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 4,
            RowCount = 2
        };
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        editor.Controls.Add(new Label { Text = "Target", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        _targetComboBox.Dock = DockStyle.Fill;
        _targetComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        editor.Controls.Add(_targetComboBox, 1, 0);

        editor.Controls.Add(new Label { Text = "Relation", AutoSize = true, Anchor = AnchorStyles.Left }, 2, 0);
        _relationTypeComboBox.Dock = DockStyle.Fill;
        _relationTypeComboBox.DropDownStyle = ComboBoxStyle.DropDown;
        foreach (var relationType in EntryRelationTypes.BuiltIn)
        {
            _relationTypeComboBox.Items.Add(relationType);
        }
        _relationTypeComboBox.Text = EntryRelationTypes.RelatedTo;
        editor.Controls.Add(_relationTypeComboBox, 3, 0);

        editor.Controls.Add(new Label { Text = "Comment", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
        _commentTextBox.Dock = DockStyle.Fill;
        editor.Controls.Add(_commentTextBox, 1, 1);
        editor.SetColumnSpan(_commentTextBox, 2);

        var createButton = new Button { Text = "Add relation", AutoSize = true, Anchor = AnchorStyles.Right };
        createButton.Click += async (_, _) => await CreateRelationAsync().ConfigureAwait(true);
        editor.Controls.Add(createButton, 3, 1);
        createGroup.Controls.Add(editor);
        root.Controls.Add(createGroup, 0, 1);

        var footer = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 8, 0, 0)
        };
        var closeButton = new Button { Text = "Close", DialogResult = DialogResult.Cancel, AutoSize = true };
        _deleteButton.Text = "Delete selected relation";
        _deleteButton.AutoSize = true;
        _deleteButton.Enabled = false;
        _deleteButton.Click += async (_, _) => await DeleteSelectedAsync().ConfigureAwait(true);
        footer.Controls.Add(closeButton);
        footer.Controls.Add(_deleteButton);
        root.Controls.Add(footer, 0, 2);

        CancelButton = closeButton;
        Controls.Add(root);
    }

    private async Task ReloadAsync()
    {
        try
        {
            var projectEntries = await _entryService.ListByProjectAsync(_entry.ProjectId).ConfigureAwait(true);
            _entriesById = projectEntries.ToDictionary(candidate => candidate.Id);

            var targets = projectEntries
                .Where(candidate => candidate.Id != _entry.Id)
                .OrderBy(candidate => candidate.Title, StringComparer.CurrentCultureIgnoreCase)
                .Select(candidate => new EntryChoice(candidate.Id, candidate.Title))
                .ToList();
            _targetComboBox.DataSource = targets;
            _targetComboBox.DisplayMember = nameof(EntryChoice.Title);

            var links = await _linkService.ListForEntryAsync(_entry.Id).ConfigureAwait(true);
            _relationsList.BeginUpdate();
            try
            {
                _relationsList.Items.Clear();
                foreach (var link in links.OrderByDescending(candidate => candidate.CreatedAtUtc))
                {
                    var outgoing = link.SourceEntryId == _entry.Id;
                    var otherId = outgoing ? link.TargetEntryId : link.SourceEntryId;
                    var otherTitle = _entriesById.TryGetValue(otherId, out var other)
                        ? other.Title
                        : $"[{otherId:D}]";
                    var item = new ListViewItem(outgoing ? "outgoing" : "incoming")
                    {
                        Tag = link
                    };
                    item.SubItems.Add(link.RelationType);
                    item.SubItems.Add(otherTitle);
                    item.SubItems.Add(link.Comment ?? string.Empty);
                    _relationsList.Items.Add(item);
                }
            }
            finally
            {
                _relationsList.EndUpdate();
            }

            _deleteButton.Enabled = false;
        }
        catch (Exception ex)
        {
            ShowError("Relations could not be loaded.", ex);
        }
    }

    private async Task CreateRelationAsync()
    {
        if (_targetComboBox.SelectedItem is not EntryChoice target)
        {
            MessageBox.Show(this, "There is no target entry available.", "SASD Workbench", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            await _linkService.CreateAsync(
                _entry.Id,
                target.Id,
                _relationTypeComboBox.Text,
                NormalizeOptional(_commentTextBox.Text)).ConfigureAwait(true);
            _commentTextBox.Clear();
            await ReloadAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            ShowError("The relation could not be created.", ex);
        }
    }

    private async Task DeleteSelectedAsync()
    {
        if (_relationsList.SelectedItems.Count != 1
            || _relationsList.SelectedItems[0].Tag is not EntryLink link)
        {
            return;
        }

        var answer = MessageBox.Show(
            this,
            $"Delete relation '{link.RelationType}'?",
            "SASD Workbench",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);
        if (answer != DialogResult.Yes)
        {
            return;
        }

        try
        {
            await _linkService.DeleteAsync(link.Id).ConfigureAwait(true);
            await ReloadAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            ShowError("The relation could not be deleted.", ex);
        }
    }

    private void ShowError(string message, Exception exception)
        => MessageBox.Show(this, $"{message}\n\n{exception.Message}", "SASD Workbench", MessageBoxButtons.OK, MessageBoxIcon.Error);

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed record EntryChoice(Guid Id, string Title);
}
