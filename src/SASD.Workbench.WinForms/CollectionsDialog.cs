using SASD.Workbench.Application.Services;
using SASD.Workbench.Domain.Entities;

namespace SASD.Workbench.WinForms;

/// <summary>
/// Manages project collections and, when an entry is supplied, its many-to-many collection membership.
/// </summary>
public sealed class CollectionsDialog : Form
{
    private readonly CollectionService _collectionService;
    private readonly Guid _projectId;
    private readonly Entry? _entry;
    private readonly CheckedListBox _collectionList = new();
    private readonly TextBox _nameTextBox = new();
    private readonly TextBox _descriptionTextBox = new();
    private readonly ComboBox _parentComboBox = new();
    private readonly Button _applyButton = new();
    private readonly Button _deleteButton = new();
    private IReadOnlyList<Collection> _collections = Array.Empty<Collection>();

    public CollectionsDialog(CollectionService collectionService, Guid projectId, Entry? entry = null)
    {
        _collectionService = collectionService ?? throw new ArgumentNullException(nameof(collectionService));
        if (projectId == Guid.Empty)
        {
            throw new ArgumentException("Project id must not be empty.", nameof(projectId));
        }

        if (entry is not null && entry.ProjectId != projectId)
        {
            throw new ArgumentException("The selected entry must belong to the supplied project.", nameof(entry));
        }

        _projectId = projectId;
        _entry = entry;
        Text = entry is null ? "Collections" : $"Collections — {entry.Title}";
        StartPosition = FormStartPosition.CenterParent;
        Width = 820;
        Height = 600;
        MinimumSize = new Size(680, 480);
        ShowInTaskbar = false;

        BuildLayout();
        Shown += async (_, _) => await ReloadAsync().ConfigureAwait(true);
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(10)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var listPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 28, 8, 0) };
        listPanel.Controls.Add(_collectionList);
        listPanel.Controls.Add(new Label
        {
            Text = _entry is null ? "Project collections" : "Check collections containing this entry",
            Dock = DockStyle.Top,
            Height = 24,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold)
        });
        _collectionList.Dock = DockStyle.Fill;
        _collectionList.CheckOnClick = true;
        _collectionList.SelectedIndexChanged += (_, _) => _deleteButton.Enabled = _collectionList.SelectedItem is CollectionListItem;
        root.Controls.Add(listPanel, 0, 0);

        var editor = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(8)
        };
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        editor.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        editor.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        editor.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        editor.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        editor.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var heading = new Label { Text = "New collection", AutoSize = true, Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold) };
        editor.Controls.Add(heading, 0, 0);
        editor.SetColumnSpan(heading, 2);

        editor.Controls.Add(new Label { Text = "Name", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
        _nameTextBox.Dock = DockStyle.Fill;
        editor.Controls.Add(_nameTextBox, 1, 1);

        editor.Controls.Add(new Label { Text = "Description", AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Left }, 0, 2);
        _descriptionTextBox.Dock = DockStyle.Fill;
        _descriptionTextBox.Multiline = true;
        _descriptionTextBox.ScrollBars = ScrollBars.Vertical;
        editor.Controls.Add(_descriptionTextBox, 1, 2);

        editor.Controls.Add(new Label { Text = "Parent", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 3);
        _parentComboBox.Dock = DockStyle.Fill;
        _parentComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        editor.Controls.Add(_parentComboBox, 1, 3);

        var createButton = new Button { Text = "Create collection", AutoSize = true };
        createButton.Click += async (_, _) => await CreateCollectionAsync().ConfigureAwait(true);
        editor.Controls.Add(createButton, 1, 4);
        root.Controls.Add(editor, 1, 0);

        var footer = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 8, 0, 0)
        };
        var closeButton = new Button { Text = "Close", DialogResult = DialogResult.Cancel, AutoSize = true };
        _applyButton.Text = "Apply membership";
        _applyButton.AutoSize = true;
        _applyButton.Enabled = _entry is not null;
        _applyButton.Click += async (_, _) => await ApplyMembershipAsync().ConfigureAwait(true);
        _deleteButton.Text = "Delete selected collection";
        _deleteButton.AutoSize = true;
        _deleteButton.Enabled = false;
        _deleteButton.Click += async (_, _) => await DeleteSelectedAsync().ConfigureAwait(true);
        footer.Controls.Add(closeButton);
        footer.Controls.Add(_applyButton);
        footer.Controls.Add(_deleteButton);
        root.Controls.Add(footer, 0, 1);
        root.SetColumnSpan(footer, 2);

        CancelButton = closeButton;
        Controls.Add(root);
    }

    private async Task ReloadAsync()
    {
        try
        {
            _collections = await _collectionService.ListByProjectAsync(_projectId).ConfigureAwait(true);
            var membership = _entry is null
                ? Array.Empty<Collection>()
                : await _collectionService.ListByEntryAsync(_entry.Id).ConfigureAwait(true);
            var memberIds = membership.Select(item => item.Id).ToHashSet();
            var labels = BuildCollectionLabels(_collections);

            _collectionList.Items.Clear();
            foreach (var collection in _collections)
            {
                var item = new CollectionListItem(collection, labels[collection.Id]);
                _collectionList.Items.Add(item, memberIds.Contains(collection.Id));
            }

            var parents = new List<ParentChoice> { new(null, "(root)") };
            parents.AddRange(_collections.Select(collection => new ParentChoice(collection.Id, labels[collection.Id])));
            _parentComboBox.DataSource = parents;
            _parentComboBox.DisplayMember = nameof(ParentChoice.Label);
            _deleteButton.Enabled = false;
        }
        catch (Exception ex)
        {
            ShowError("Collections could not be loaded.", ex);
        }
    }

    private async Task CreateCollectionAsync()
    {
        if (string.IsNullOrWhiteSpace(_nameTextBox.Text))
        {
            MessageBox.Show(this, "Please enter a collection name.", "SASD Workbench", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _nameTextBox.Focus();
            return;
        }

        try
        {
            var parentId = (_parentComboBox.SelectedItem as ParentChoice)?.Id;
            await _collectionService.CreateAsync(
                _projectId,
                _nameTextBox.Text,
                NormalizeOptional(_descriptionTextBox.Text),
                parentId).ConfigureAwait(true);

            _nameTextBox.Clear();
            _descriptionTextBox.Clear();
            await ReloadAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            ShowError("The collection could not be created.", ex);
        }
    }

    private async Task ApplyMembershipAsync()
    {
        if (_entry is null)
        {
            return;
        }

        try
        {
            var current = await _collectionService.ListByEntryAsync(_entry.Id).ConfigureAwait(true);
            var currentIds = current.Select(item => item.Id).ToHashSet();
            var checkedIds = _collectionList.CheckedItems
                .OfType<CollectionListItem>()
                .Select(item => item.Collection.Id)
                .ToHashSet();

            foreach (var collectionId in checkedIds.Except(currentIds))
            {
                await _collectionService.AddEntryAsync(collectionId, _entry.Id).ConfigureAwait(true);
            }

            foreach (var collectionId in currentIds.Except(checkedIds))
            {
                await _collectionService.RemoveEntryAsync(collectionId, _entry.Id).ConfigureAwait(true);
            }

            MessageBox.Show(this, "Collection membership was updated.", "SASD Workbench", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            ShowError("Collection membership could not be updated.", ex);
        }
    }

    private async Task DeleteSelectedAsync()
    {
        if (_collectionList.SelectedItem is not CollectionListItem selected)
        {
            return;
        }

        var answer = MessageBox.Show(
            this,
            $"Delete collection '{selected.Collection.Name}'? Entries are not deleted.",
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
            await _collectionService.DeleteAsync(selected.Collection.Id).ConfigureAwait(true);
            await ReloadAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            ShowError("The collection could not be deleted.", ex);
        }
    }

    private static Dictionary<Guid, string> BuildCollectionLabels(IReadOnlyList<Collection> collections)
    {
        var byId = collections.ToDictionary(item => item.Id);
        var cache = new Dictionary<Guid, string>();

        string BuildLabel(Collection collection, HashSet<Guid> path)
        {
            if (cache.TryGetValue(collection.Id, out var cached))
            {
                return cached;
            }

            // Corrupt/cyclic persistence data must not recurse indefinitely in the UI. The domain
            // prevents direct self-parenting; this guard also protects against historic/manual data.
            if (!path.Add(collection.Id))
            {
                return $"[cycle] / {collection.Name}";
            }

            var label = collection.ParentCollectionId.HasValue
                && byId.TryGetValue(collection.ParentCollectionId.Value, out var parent)
                    ? $"{BuildLabel(parent, path)} / {collection.Name}"
                    : collection.Name;
            path.Remove(collection.Id);
            cache[collection.Id] = label;
            return label;
        }

        foreach (var collection in collections)
        {
            BuildLabel(collection, new HashSet<Guid>());
        }

        return cache;
    }

    private void ShowError(string message, Exception exception)
        => MessageBox.Show(this, $"{message}\n\n{exception.Message}", "SASD Workbench", MessageBoxButtons.OK, MessageBoxIcon.Error);

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed record ParentChoice(Guid? Id, string Label);

    private sealed record CollectionListItem(Collection Collection, string Label)
    {
        public override string ToString() => Label;
    }
}
