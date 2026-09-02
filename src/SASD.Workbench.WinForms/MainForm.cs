using SASD.Workbench.Application.Services;
using SASD.Workbench.Domain.Entities;
using SASD.Workbench.Infrastructure.Configuration;

namespace SASD.Workbench.WinForms;

/// <summary>
/// Provides the deliberately small V0.1 desktop shell used to prove the complete core flow.
/// </summary>
public sealed class MainForm : Form
{
    private readonly ProjectService _projectService;
    private readonly EntryService _entryService;

    private readonly ListBox _projectList = new();
    private readonly ListBox _entryList = new();
    private readonly TextBox _titleTextBox = new();
    private readonly TextBox _summaryTextBox = new();
    private readonly TextBox _typeTextBox = new();
    private readonly TextBox _statusTextBox = new();
    private readonly TextBox _contentTextBox = new();
    private readonly Button _saveEntryButton = new();
    private readonly Button _newEntryButton = new();
    private readonly Label _statusLabel = new();

    private bool _loadingSelection;

    public MainForm(ProjectService projectService, EntryService entryService, WorkbenchDataPaths paths)
    {
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _entryService = entryService ?? throw new ArgumentNullException(nameof(entryService));
        ArgumentNullException.ThrowIfNull(paths);

        Text = "SASD Workbench";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1100, 700);
        Width = 1450;
        Height = 850;

        BuildLayout(paths.DatabasePath);
        Shown += MainForm_Shown;
    }

    private void BuildLayout(string databasePath)
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 2,
            Padding = new Padding(8)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        root.Controls.Add(BuildProjectPanel(), 0, 0);
        root.Controls.Add(BuildEntryPanel(), 1, 0);
        root.Controls.Add(BuildEditorPanel(), 2, 0);

        _statusLabel.AutoSize = true;
        _statusLabel.Padding = new Padding(4, 6, 4, 2);
        _statusLabel.Text = $"Data: {databasePath}";
        root.Controls.Add(_statusLabel, 0, 1);
        root.SetColumnSpan(_statusLabel, 3);

        Controls.Add(root);
    }

    private Control BuildProjectPanel()
    {
        var panel = CreateSection("Projects", out var body);

        _projectList.Dock = DockStyle.Fill;
        _projectList.DisplayMember = nameof(Project.Name);
        _projectList.SelectedIndexChanged += ProjectList_SelectedIndexChanged;
        body.Controls.Add(_projectList);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true };
        var newButton = new Button { Text = "New project", AutoSize = true };
        var refreshButton = new Button { Text = "Refresh", AutoSize = true };
        newButton.Click += NewProjectButton_Click;
        refreshButton.Click += RefreshButton_Click;
        buttons.Controls.Add(newButton);
        buttons.Controls.Add(refreshButton);
        body.Controls.Add(buttons);

        return panel;
    }

    private Control BuildEntryPanel()
    {
        var panel = CreateSection("Entries", out var body);

        _entryList.Dock = DockStyle.Fill;
        _entryList.DisplayMember = nameof(Entry.Title);
        _entryList.SelectedIndexChanged += EntryList_SelectedIndexChanged;
        body.Controls.Add(_entryList);

        _newEntryButton.Text = "New entry";
        _newEntryButton.AutoSize = true;
        _newEntryButton.Enabled = false;
        _newEntryButton.Click += NewEntryButton_Click;

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true };
        buttons.Controls.Add(_newEntryButton);
        body.Controls.Add(buttons);

        return panel;
    }

    private Control BuildEditorPanel()
    {
        var panel = CreateSection("Editor", out var body);
        var editor = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6,
            Padding = new Padding(4)
        };
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        editor.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        editor.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        editor.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        editor.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        editor.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        editor.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        AddEditorRow(editor, 0, "Title", _titleTextBox);
        AddEditorRow(editor, 1, "Summary", _summaryTextBox);
        AddEditorRow(editor, 2, "Type", _typeTextBox);
        AddEditorRow(editor, 3, "Status", _statusTextBox);

        var contentLabel = new Label { Text = "Markdown", AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Left };
        _contentTextBox.Dock = DockStyle.Fill;
        _contentTextBox.Multiline = true;
        _contentTextBox.ScrollBars = ScrollBars.Both;
        _contentTextBox.AcceptsTab = true;
        _contentTextBox.Font = new Font(FontFamily.GenericMonospace, 10f);
        editor.Controls.Add(contentLabel, 0, 4);
        editor.Controls.Add(_contentTextBox, 1, 4);

        _saveEntryButton.Text = "Save entry";
        _saveEntryButton.AutoSize = true;
        _saveEntryButton.Enabled = false;
        _saveEntryButton.Click += SaveEntryButton_Click;
        editor.Controls.Add(_saveEntryButton, 1, 5);

        body.Controls.Add(editor);
        SetEditorEnabled(false);
        return panel;
    }

    private static Panel CreateSection(string title, out Panel body)
    {
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4) };
        var label = new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = 32,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            Padding = new Padding(4, 7, 4, 4)
        };
        body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4, 36, 4, 4) };
        panel.Controls.Add(body);
        panel.Controls.Add(label);
        return panel;
    }

    private static void AddEditorRow(TableLayoutPanel editor, int row, string label, TextBox textBox)
    {
        editor.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
        textBox.Dock = DockStyle.Fill;
        editor.Controls.Add(textBox, 1, row);
    }

    private async void MainForm_Shown(object? sender, EventArgs e)
        => await ReloadProjectsAsync();

    private async void RefreshButton_Click(object? sender, EventArgs e)
        => await ReloadProjectsAsync();

    private async void NewProjectButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new ProjectDialog();
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var project = await _projectService.CreateAsync(dialog.ProjectName, dialog.ProjectDescription);
            await ReloadProjectsAsync(project.Id);
            SetStatus($"Project '{project.Name}' created.");
        }
        catch (Exception ex)
        {
            ShowError("The project could not be created.", ex);
        }
    }

    private async void ProjectList_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_loadingSelection)
        {
            return;
        }

        if (_projectList.SelectedItem is not Project project)
        {
            _entryList.DataSource = null;
            _newEntryButton.Enabled = false;
            ClearEditor();
            return;
        }

        _newEntryButton.Enabled = true;
        await ReloadEntriesAsync(project.Id);
    }

    private async void EntryList_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_loadingSelection)
        {
            return;
        }

        if (_entryList.SelectedItem is not Entry entry)
        {
            ClearEditor();
            return;
        }

        PopulateEditor(entry);
        await Task.CompletedTask;
    }

    private async void NewEntryButton_Click(object? sender, EventArgs e)
    {
        if (_projectList.SelectedItem is not Project project)
        {
            return;
        }

        try
        {
            var entry = await _entryService.CreateAsync(project.Id, "note", "New entry", contentMarkdown: "# New entry\r\n");
            await ReloadEntriesAsync(project.Id, entry.Id);
            SetStatus("New entry created. Edit it on the right and save your changes.");
        }
        catch (Exception ex)
        {
            ShowError("The entry could not be created.", ex);
        }
    }

    private async void SaveEntryButton_Click(object? sender, EventArgs e)
    {
        if (_entryList.SelectedItem is not Entry selectedEntry)
        {
            return;
        }

        try
        {
            var saved = await _entryService.UpdateAsync(
                selectedEntry.Id,
                _titleTextBox.Text,
                _summaryTextBox.Text,
                _contentTextBox.Text,
                _typeTextBox.Text,
                _statusTextBox.Text);

            await ReloadEntriesAsync(saved.ProjectId, saved.Id);
            SetStatus($"Entry saved at {saved.UpdatedAtUtc.ToLocalTime():G}.");
        }
        catch (Exception ex)
        {
            ShowError("The entry could not be saved.", ex);
        }
    }

    private async Task ReloadProjectsAsync(Guid? selectProjectId = null)
    {
        try
        {
            var previousId = selectProjectId ?? (_projectList.SelectedItem as Project)?.Id;
            var projects = await _projectService.ListAsync();

            _loadingSelection = true;
            _projectList.DataSource = projects.ToList();
            _projectList.DisplayMember = nameof(Project.Name);
            SelectById(_projectList, previousId, static item => ((Project)item).Id);
            _loadingSelection = false;

            if (_projectList.SelectedItem is Project project)
            {
                _newEntryButton.Enabled = true;
                await ReloadEntriesAsync(project.Id);
            }
            else
            {
                _entryList.DataSource = null;
                _newEntryButton.Enabled = false;
                ClearEditor();
            }
        }
        catch (Exception ex)
        {
            _loadingSelection = false;
            ShowError("Projects could not be loaded.", ex);
        }
    }

    private async Task ReloadEntriesAsync(Guid projectId, Guid? selectEntryId = null)
    {
        try
        {
            var previousId = selectEntryId ?? (_entryList.SelectedItem as Entry)?.Id;
            var entries = await _entryService.ListByProjectAsync(projectId);

            _loadingSelection = true;
            _entryList.DataSource = entries.ToList();
            _entryList.DisplayMember = nameof(Entry.Title);
            SelectById(_entryList, previousId, static item => ((Entry)item).Id);
            _loadingSelection = false;

            if (_entryList.SelectedItem is Entry entry)
            {
                PopulateEditor(entry);
            }
            else
            {
                ClearEditor();
            }
        }
        catch (Exception ex)
        {
            _loadingSelection = false;
            ShowError("Entries could not be loaded.", ex);
        }
    }

    private static void SelectById(ListBox listBox, Guid? id, Func<object, Guid> idSelector)
    {
        if (!id.HasValue)
        {
            listBox.SelectedIndex = listBox.Items.Count > 0 ? 0 : -1;
            return;
        }

        for (var index = 0; index < listBox.Items.Count; index++)
        {
            if (idSelector(listBox.Items[index]) == id.Value)
            {
                listBox.SelectedIndex = index;
                return;
            }
        }

        listBox.SelectedIndex = listBox.Items.Count > 0 ? 0 : -1;
    }

    private void PopulateEditor(Entry entry)
    {
        _titleTextBox.Text = entry.Title;
        _summaryTextBox.Text = entry.Summary ?? string.Empty;
        _typeTextBox.Text = entry.EntryType;
        _statusTextBox.Text = entry.Status;
        _contentTextBox.Text = entry.ContentMarkdown;
        SetEditorEnabled(true);
    }

    private void ClearEditor()
    {
        _titleTextBox.Clear();
        _summaryTextBox.Clear();
        _typeTextBox.Clear();
        _statusTextBox.Clear();
        _contentTextBox.Clear();
        SetEditorEnabled(false);
    }

    private void SetEditorEnabled(bool enabled)
    {
        _titleTextBox.Enabled = enabled;
        _summaryTextBox.Enabled = enabled;
        _typeTextBox.Enabled = enabled;
        _statusTextBox.Enabled = enabled;
        _contentTextBox.Enabled = enabled;
        _saveEntryButton.Enabled = enabled;
    }

    private void SetStatus(string message) => _statusLabel.Text = message;

    private void ShowError(string message, Exception exception)
    {
        SetStatus(message);
        MessageBox.Show(this, $"{message}\n\n{exception.Message}", "SASD Workbench", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
