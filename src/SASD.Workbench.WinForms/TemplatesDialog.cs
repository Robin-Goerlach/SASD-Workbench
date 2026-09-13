using SASD.Workbench.Application.Services;
using SASD.Workbench.Domain.Entities;

namespace SASD.Workbench.WinForms;

/// <summary>
/// Provides the V1 desktop workflow for reusable templates without moving template rules into WinForms.
/// </summary>
internal sealed class TemplatesDialog : Form
{
    private readonly TemplateService _templateService;
    private readonly Project _project;
    private readonly Entry? _currentEntry;

    private readonly ListBox _templateList = new();
    private readonly Label _detailsLabel = new();
    private readonly TextBox _entryTitleTextBox = new();
    private readonly Button _createEntryButton = new();
    private readonly Button _saveCurrentButton = new();
    private readonly Button _deleteButton = new();

    public TemplatesDialog(TemplateService templateService, Project project, Entry? currentEntry)
    {
        _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
        _project = project ?? throw new ArgumentNullException(nameof(project));
        _currentEntry = currentEntry;

        Text = $"Templates — {project.Name}";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(760, 520);
        Width = 900;
        Height = 620;

        BuildLayout();
        Shown += TemplatesDialog_Shown;
    }

    /// <summary>
    /// Gets the entry created from a template, if the dialog completed that operation.
    /// </summary>
    public Entry? CreatedEntry { get; private set; }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(10)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));

        _templateList.Dock = DockStyle.Fill;
        _templateList.DisplayMember = nameof(Template.Name);
        _templateList.SelectedIndexChanged += TemplateList_SelectedIndexChanged;
        root.Controls.Add(_templateList, 0, 0);

        var right = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Padding = new Padding(10, 0, 0, 0)
        };
        right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        right.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        right.Controls.Add(new Label
        {
            Text = "Selected template",
            AutoSize = true,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            Padding = new Padding(0, 0, 0, 6)
        }, 0, 0);

        _detailsLabel.Dock = DockStyle.Fill;
        _detailsLabel.AutoSize = false;
        _detailsLabel.Padding = new Padding(0, 0, 0, 8);
        right.Controls.Add(_detailsLabel, 0, 1);

        var titlePanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 2,
            Padding = new Padding(0, 8, 0, 8)
        };
        titlePanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        titlePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        titlePanel.Controls.Add(new Label { Text = "New entry title", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        _entryTitleTextBox.Dock = DockStyle.Fill;
        titlePanel.Controls.Add(_entryTitleTextBox, 1, 0);
        right.Controls.Add(titlePanel, 0, 2);

        _createEntryButton.Text = "Create entry from selected template";
        _createEntryButton.AutoSize = true;
        _createEntryButton.Enabled = false;
        _createEntryButton.Click += CreateEntryButton_Click;
        right.Controls.Add(_createEntryButton, 0, 3);

        _saveCurrentButton.Text = "Save current entry as template...";
        _saveCurrentButton.AutoSize = true;
        _saveCurrentButton.Enabled = _currentEntry is not null;
        _saveCurrentButton.Click += SaveCurrentButton_Click;
        right.Controls.Add(_saveCurrentButton, 0, 4);

        var bottom = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 12, 0, 0)
        };
        var closeButton = new Button { Text = "Close", DialogResult = DialogResult.Cancel, AutoSize = true };
        _deleteButton.Text = "Delete selected template";
        _deleteButton.AutoSize = true;
        _deleteButton.Enabled = false;
        _deleteButton.Click += DeleteButton_Click;
        bottom.Controls.Add(closeButton);
        bottom.Controls.Add(_deleteButton);
        right.Controls.Add(bottom, 0, 5);

        CancelButton = closeButton;
        root.Controls.Add(right, 1, 0);
        Controls.Add(root);
    }

    private async void TemplatesDialog_Shown(object? sender, EventArgs e)
        => await ReloadTemplatesAsync().ConfigureAwait(true);

    private void TemplateList_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_templateList.SelectedItem is not Template template)
        {
            _detailsLabel.Text = "No template selected.";
            _createEntryButton.Enabled = false;
            _deleteButton.Enabled = false;
            return;
        }

        var scope = template.ProjectId.HasValue
            ? "Current project"
            : string.Equals(template.ProfileKey, "general", StringComparison.OrdinalIgnoreCase)
                ? "Shared general"
                : $"Profile-wide ({template.ProfileKey})";
        _detailsLabel.Text =
            $"Name: {template.Name}\r\n" +
            $"Scope: {scope}\r\n" +
            $"Entry type: {template.EntryType}\r\n" +
            $"Default status: {template.DefaultStatus}\r\n\r\n" +
            $"{template.Description ?? "No description."}\r\n\r\n" +
            "Creating an entry copies the template. Later template changes do not change existing entries.";

        _createEntryButton.Enabled = true;
        _deleteButton.Enabled = !template.IsSystemTemplate;
        if (string.IsNullOrWhiteSpace(_entryTitleTextBox.Text))
        {
            _entryTitleTextBox.Text = template.Name;
        }
    }

    private async void CreateEntryButton_Click(object? sender, EventArgs e)
    {
        if (_templateList.SelectedItem is not Template template)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_entryTitleTextBox.Text))
        {
            MessageBox.Show(this, "Please enter a title for the new entry.", "SASD Workbench", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _entryTitleTextBox.Focus();
            return;
        }

        try
        {
            CreatedEntry = await _templateService.CreateEntryAsync(
                _project.Id,
                template.Id,
                _entryTitleTextBox.Text.Trim()).ConfigureAwait(true);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            ShowError("The entry could not be created from the selected template.", ex);
        }
    }

    private async void SaveCurrentButton_Click(object? sender, EventArgs e)
    {
        if (_currentEntry is null)
        {
            return;
        }

        using var dialog = new TemplateSaveDialog($"{_currentEntry.Title} template", _project.ProfileKey);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var template = await _templateService.CreateAsync(
                dialog.TemplateName,
                _currentEntry.EntryType,
                _currentEntry.Status,
                _currentEntry.ContentMarkdown,
                projectId: dialog.IsProfileWide ? null : _project.Id,
                profileKey: _project.ProfileKey,
                description: _currentEntry.Summary).ConfigureAwait(true);
            await ReloadTemplatesAsync(template.Id).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            ShowError("The current entry could not be saved as a template.", ex);
        }
    }

    private async void DeleteButton_Click(object? sender, EventArgs e)
    {
        if (_templateList.SelectedItem is not Template template || template.IsSystemTemplate)
        {
            return;
        }

        var scopeWarning = template.ProjectId.HasValue
            ? "This template belongs only to the current project."
            : "This template is shared and may be used by other projects.";
        var answer = MessageBox.Show(
            this,
            $"Delete template '{template.Name}'?\n\n{scopeWarning}\nExisting entries created from it are not changed.",
            "Delete template",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);
        if (answer != DialogResult.Yes)
        {
            return;
        }

        try
        {
            await _templateService.DeleteAsync(template.Id).ConfigureAwait(true);
            await ReloadTemplatesAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            ShowError("The template could not be deleted.", ex);
        }
    }

    private async Task ReloadTemplatesAsync(Guid? selectTemplateId = null)
    {
        try
        {
            var templates = await _templateService.ListAsync(_project.Id, _project.ProfileKey).ConfigureAwait(true);
            _templateList.DataSource = templates.ToList();
            _templateList.DisplayMember = nameof(Template.Name);

            if (selectTemplateId.HasValue)
            {
                for (var index = 0; index < _templateList.Items.Count; index++)
                {
                    if (_templateList.Items[index] is Template candidate && candidate.Id == selectTemplateId.Value)
                    {
                        _templateList.SelectedIndex = index;
                        return;
                    }
                }
            }

            _templateList.SelectedIndex = _templateList.Items.Count > 0 ? 0 : -1;
            if (_templateList.Items.Count == 0)
            {
                _detailsLabel.Text = "No templates are available for this project/profile yet.\r\n\r\nSelect an entry first and use 'Save current entry as template...' to create one.";
            }
        }
        catch (Exception ex)
        {
            ShowError("Templates could not be loaded.", ex);
        }
    }

    private void ShowError(string message, Exception exception)
        => MessageBox.Show(this, $"{message}\n\n{exception.Message}", "SASD Workbench", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
