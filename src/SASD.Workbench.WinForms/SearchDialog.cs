using SASD.Workbench.Application.Models;
using SASD.Workbench.Application.Services;
using SASD.Workbench.Domain.Entities;
using SASD.Workbench.Domain.Metadata;

namespace SASD.Workbench.WinForms;

/// <summary>
/// Provides the V1 project-scoped search UI without exposing persistence details to WinForms.
/// </summary>
public sealed class SearchDialog : Form
{
    private readonly SearchService _searchService;
    private readonly Guid _projectId;
    private readonly TextBox _textTextBox = new();
    private readonly ComboBox _typeComboBox = new();
    private readonly TextBox _statusTextBox = new();
    private readonly ListBox _resultsListBox = new();
    private readonly Label _resultCountLabel = new();
    private readonly Button _openButton = new();

    public SearchDialog(SearchService searchService, Guid projectId)
    {
        _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
        if (projectId == Guid.Empty)
        {
            throw new ArgumentException("Project id must not be empty.", nameof(projectId));
        }

        _projectId = projectId;
        Text = "Search entries";
        StartPosition = FormStartPosition.CenterParent;
        Width = 900;
        Height = 620;
        MinimumSize = new Size(700, 450);
        ShowInTaskbar = false;

        BuildLayout();
        Shown += async (_, _) => await SearchAsync().ConfigureAwait(true);
    }

    /// <summary>
    /// Gets the entry chosen by the user, if the dialog completed with <see cref="DialogResult.OK"/>.
    /// </summary>
    public Entry? SelectedEntry => _resultsListBox.SelectedItem as Entry;

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(10)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var filters = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 4,
            RowCount = 2
        };
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));

        filters.Controls.Add(new Label { Text = "Text", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        _textTextBox.Dock = DockStyle.Fill;
        filters.Controls.Add(_textTextBox, 1, 0);

        filters.Controls.Add(new Label { Text = "Type", AutoSize = true, Anchor = AnchorStyles.Left }, 2, 0);
        _typeComboBox.Dock = DockStyle.Fill;
        _typeComboBox.DropDownStyle = ComboBoxStyle.DropDown;
        _typeComboBox.Items.Add(string.Empty);
        foreach (var entryType in CoreEntryTypes.BuiltIn)
        {
            _typeComboBox.Items.Add(entryType);
        }
        filters.Controls.Add(_typeComboBox, 3, 0);

        filters.Controls.Add(new Label { Text = "Status", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
        _statusTextBox.Dock = DockStyle.Fill;
        filters.Controls.Add(_statusTextBox, 1, 1);

        var searchButton = new Button { Text = "Search", AutoSize = true, Anchor = AnchorStyles.Right };
        searchButton.Click += async (_, _) => await SearchAsync().ConfigureAwait(true);
        filters.Controls.Add(searchButton, 3, 1);
        root.Controls.Add(filters, 0, 0);

        _resultsListBox.Dock = DockStyle.Fill;
        _resultsListBox.DisplayMember = nameof(Entry.Title);
        _resultsListBox.DoubleClick += (_, _) => AcceptSelection();
        _resultsListBox.SelectedIndexChanged += (_, _) => _openButton.Enabled = SelectedEntry is not null;
        root.Controls.Add(_resultsListBox, 0, 1);

        var footer = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2 };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _resultCountLabel.AutoSize = true;
        _resultCountLabel.Anchor = AnchorStyles.Left;
        footer.Controls.Add(_resultCountLabel, 0, 0);

        var buttons = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.RightToLeft };
        var closeButton = new Button { Text = "Close", DialogResult = DialogResult.Cancel, AutoSize = true };
        _openButton.Text = "Open entry";
        _openButton.AutoSize = true;
        _openButton.Enabled = false;
        _openButton.Click += (_, _) => AcceptSelection();
        buttons.Controls.Add(closeButton);
        buttons.Controls.Add(_openButton);
        footer.Controls.Add(buttons, 1, 0);
        root.Controls.Add(footer, 0, 2);

        AcceptButton = _openButton;
        CancelButton = closeButton;
        Controls.Add(root);
    }

    private async Task SearchAsync()
    {
        try
        {
            var query = new EntrySearchQuery(
                Text: NormalizeOptional(_textTextBox.Text),
                ProjectId: _projectId,
                EntryType: NormalizeOptional(_typeComboBox.Text),
                Status: NormalizeOptional(_statusTextBox.Text),
                Limit: 500);

            var entries = await _searchService.SearchAsync(query).ConfigureAwait(true);
            _resultsListBox.DataSource = entries.ToList();
            _resultsListBox.DisplayMember = nameof(Entry.Title);
            _resultCountLabel.Text = entries.Count == 1 ? "1 entry" : $"{entries.Count} entries";
            _openButton.Enabled = SelectedEntry is not null;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Search failed.\n\n{ex.Message}", "SASD Workbench", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AcceptSelection()
    {
        if (SelectedEntry is null)
        {
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
