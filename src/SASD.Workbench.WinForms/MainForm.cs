using SASD.Workbench.Application.Interfaces;
using SASD.Workbench.Application.Services;
using SASD.Workbench.Domain.Entities;
using SASD.Workbench.Domain.Metadata;
using SASD.Workbench.Infrastructure.Configuration;

namespace SASD.Workbench.WinForms;

/// <summary>
/// Hosts the common V1 desktop workflow and delegates focused Core tools to separate dialogs.
/// </summary>
/// <remarks>
/// This form intentionally remains a UI coordinator. Search, templates, tags, attachments,
/// collections, relations and activity have their own dialogs; persistence, export and backup rules
/// remain below the WinForms layer.
/// </remarks>
public sealed class MainForm : Form
{
    private readonly ProjectService _projectService;
    private readonly EntryService _entryService;
    private readonly TemplateService _templateService;
    private readonly TagService _tagService;
    private readonly AttachmentService _attachmentService;
    private readonly SearchService _searchService;
    private readonly CollectionService _collectionService;
    private readonly EntryLinkService _entryLinkService;
    private readonly ActivityLogService _activityLogService;
    private readonly IProjectExportService _exportService;
    private readonly IBackupService _backupService;
    private readonly WorkbenchDataPaths _paths;

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

    private readonly ToolStripButton _searchButton = new("Search");
    private readonly ToolStripButton _templatesButton = new("Templates");
    private readonly ToolStripButton _tagsButton = new("Tags");
    private readonly ToolStripButton _attachmentsButton = new("Attachments");
    private readonly ToolStripButton _collectionsButton = new("Collections");
    private readonly ToolStripButton _relationsButton = new("Relations");
    private readonly ToolStripButton _activityButton = new("Activity");
    private readonly ToolStripButton _exportButton = new("Export Markdown");
    private readonly ToolStripButton _backupButton = new("Backup");
    private readonly ToolStripButton _restoreButton = new("Restore");

    private bool _loadingSelection;

    public MainForm(
        ProjectService projectService,
        EntryService entryService,
        TemplateService templateService,
        TagService tagService,
        AttachmentService attachmentService,
        SearchService searchService,
        CollectionService collectionService,
        EntryLinkService entryLinkService,
        ActivityLogService activityLogService,
        IProjectExportService exportService,
        IBackupService backupService,
        WorkbenchDataPaths paths)
    {
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _entryService = entryService ?? throw new ArgumentNullException(nameof(entryService));
        _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
        _tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
        _attachmentService = attachmentService ?? throw new ArgumentNullException(nameof(attachmentService));
        _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
        _collectionService = collectionService ?? throw new ArgumentNullException(nameof(collectionService));
        _entryLinkService = entryLinkService ?? throw new ArgumentNullException(nameof(entryLinkService));
        _activityLogService = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));

        Text = "SASD Workbench";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1100, 700);
        Width = 1450;
        Height = 850;

        BuildLayout(paths.DatabasePath);
        UpdateToolAvailability();
        Shown += MainForm_Shown;
    }

    private void BuildLayout(string databasePath)
    {
        var shell = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        shell.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        shell.Controls.Add(BuildToolStrip(), 0, 0);

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

        shell.Controls.Add(root, 0, 1);
        Controls.Add(shell);
    }

    private ToolStrip BuildToolStrip()
    {
        var toolStrip = new ToolStrip
        {
            GripStyle = ToolStripGripStyle.Hidden,
            Dock = DockStyle.Fill,
            AutoSize = true
        };

        _searchButton.Click += SearchButton_Click;
        _templatesButton.Click += TemplatesButton_Click;
        _tagsButton.Click += TagsButton_Click;
        _attachmentsButton.Click += AttachmentsButton_Click;
        _collectionsButton.Click += CollectionsButton_Click;
        _relationsButton.Click += RelationsButton_Click;
        _activityButton.Click += ActivityButton_Click;
        _exportButton.Click += ExportButton_Click;
        _backupButton.Click += BackupButton_Click;
        _restoreButton.Click += RestoreButton_Click;

        toolStrip.Items.AddRange(
        [
            _searchButton,
            _templatesButton,
            _tagsButton,
            _attachmentsButton,
            _collectionsButton,
            _relationsButton,
            _activityButton,
            new ToolStripSeparator(),
            _exportButton,
            new ToolStripSeparator(),
            _backupButton,
            _restoreButton
        ]);
        return toolStrip;
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
        => await ReloadProjectsAsync().ConfigureAwait(true);

    private async void RefreshButton_Click(object? sender, EventArgs e)
        => await ReloadProjectsAsync().ConfigureAwait(true);

    private async void NewProjectButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new ProjectDialog();
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var project = await _projectService.CreateAsync(dialog.ProjectName, dialog.ProjectDescription).ConfigureAwait(true);
            await ReloadProjectsAsync(project.Id).ConfigureAwait(true);
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
            UpdateToolAvailability();
            return;
        }

        _newEntryButton.Enabled = true;
        await ReloadEntriesAsync(project.Id).ConfigureAwait(true);
        UpdateToolAvailability();
    }

    private void EntryList_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_loadingSelection)
        {
            return;
        }

        if (_entryList.SelectedItem is not Entry entry)
        {
            ClearEditor();
            UpdateToolAvailability();
            return;
        }

        PopulateEditor(entry);
        UpdateToolAvailability();
    }

    private async void NewEntryButton_Click(object? sender, EventArgs e)
    {
        if (_projectList.SelectedItem is not Project project)
        {
            return;
        }

        try
        {
            var entry = await _entryService.CreateAsync(
                project.Id,
                CoreEntryTypes.Note,
                "New entry",
                contentMarkdown: "# New entry\r\n").ConfigureAwait(true);
            await ReloadEntriesAsync(project.Id, entry.Id).ConfigureAwait(true);
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
            // Pass the version that populated this editor. The Application service must reject the
            // save if another operation has updated the Entry since then rather than silently applying
            // these stale editor fields to a newer persisted version.
            var saved = await _entryService.UpdateAsync(
                selectedEntry.Id,
                selectedEntry.Version,
                _titleTextBox.Text,
                _summaryTextBox.Text,
                _contentTextBox.Text,
                _typeTextBox.Text,
                _statusTextBox.Text).ConfigureAwait(true);

            await ReloadEntriesAsync(saved.ProjectId, saved.Id).ConfigureAwait(true);
            SetStatus($"Entry saved at {saved.UpdatedAtUtc.ToLocalTime():G}.");
        }
        catch (SASD.Workbench.Application.Exceptions.OptimisticConcurrencyException ex)
        {
            // Do not reload the Entry here. A reload would destroy exactly the unsaved editor text the
            // concurrency guard protected. The user must first decide what to preserve/merge, then
            // consciously reload the newer persisted state.
            var conflict = ConcurrencyConflictPresentation.ForEntry(ex);
            SetStatus(conflict.Status);
            MessageBox.Show(
                this,
                conflict.Message,
                conflict.Caption,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            ShowError("The entry could not be saved.", ex);
        }
    }

    private void SearchButton_Click(object? sender, EventArgs e)
    {
        if (_projectList.SelectedItem is not Project project)
        {
            return;
        }

        using var dialog = new SearchDialog(_searchService, project.Id);
        if (dialog.ShowDialog(this) == DialogResult.OK && dialog.SelectedEntry is Entry selected)
        {
            _ = ReloadEntriesFromDialogAsync(project.Id, selected.Id);
        }
    }

    private async void TemplatesButton_Click(object? sender, EventArgs e)
    {
        if (_projectList.SelectedItem is not Project project)
        {
            return;
        }

        using var dialog = new TemplatesDialog(_templateService, project, _entryList.SelectedItem as Entry);
        if (dialog.ShowDialog(this) == DialogResult.OK && dialog.CreatedEntry is Entry createdEntry)
        {
            await ReloadEntriesAsync(project.Id, createdEntry.Id).ConfigureAwait(true);
            SetStatus($"Entry '{createdEntry.Title}' created from template.");
        }
    }

    private void TagsButton_Click(object? sender, EventArgs e)
    {
        if (_entryList.SelectedItem is not Entry entry)
        {
            return;
        }

        using var dialog = new TagsDialog(_tagService, entry);
        dialog.ShowDialog(this);
    }

    private void AttachmentsButton_Click(object? sender, EventArgs e)
    {
        if (_entryList.SelectedItem is not Entry entry)
        {
            return;
        }

        using var dialog = new AttachmentsDialog(_attachmentService, entry);
        dialog.ShowDialog(this);
    }

    private void CollectionsButton_Click(object? sender, EventArgs e)
    {
        if (_projectList.SelectedItem is not Project project)
        {
            return;
        }

        using var dialog = new CollectionsDialog(_collectionService, project.Id, _entryList.SelectedItem as Entry);
        dialog.ShowDialog(this);
    }

    private void RelationsButton_Click(object? sender, EventArgs e)
    {
        if (_entryList.SelectedItem is not Entry entry)
        {
            return;
        }

        using var dialog = new RelationsDialog(_entryLinkService, _entryService, entry);
        dialog.ShowDialog(this);
    }

    private void ActivityButton_Click(object? sender, EventArgs e)
    {
        if (_projectList.SelectedItem is not Project project)
        {
            return;
        }

        var entryId = (_entryList.SelectedItem as Entry)?.Id;
        using var dialog = new ActivityLogDialog(_activityLogService, project.Id, entryId);
        dialog.ShowDialog(this);
    }

    private async void ExportButton_Click(object? sender, EventArgs e)
    {
        if (_projectList.SelectedItem is not Project project)
        {
            return;
        }

        using var dialog = new FolderBrowserDialog
        {
            Description = "Choose the parent directory for the portable Markdown project export.",
            ShowNewFolderButton = true,
            SelectedPath = Directory.Exists(_paths.ExportsDirectory) ? _paths.ExportsDirectory : _paths.RootDirectory
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var result = await _exportService.ExportMarkdownAsync(project.Id, dialog.SelectedPath).ConfigureAwait(true);
            SetStatus($"Project exported to {result.ExportDirectory}");
            MessageBox.Show(
                this,
                $"Export completed.\n\nEntries: {result.EntryCount}\nAttachments: {result.AttachmentCount}\n\n{result.ExportDirectory}",
                "SASD Workbench",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            ShowError("The project export failed.", ex);
        }
    }

    private async void BackupButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Choose the directory for the full Workbench backup archive.",
            ShowNewFolderButton = true,
            SelectedPath = Directory.Exists(_paths.BackupsDirectory) ? _paths.BackupsDirectory : _paths.RootDirectory
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var result = await _backupService.CreateBackupAsync(dialog.SelectedPath).ConfigureAwait(true);
            SetStatus($"Backup created: {result.ArchivePath}");
            MessageBox.Show(
                this,
                $"Full backup created successfully.\n\n{result.ArchivePath}",
                "SASD Workbench",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            ShowError("The full backup failed.", ex);
        }
    }

    private async void RestoreButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Restore SASD Workbench backup",
            Filter = "SASD Workbench backup (*.zip)|*.zip|All files (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false,
            InitialDirectory = Directory.Exists(_paths.BackupsDirectory) ? _paths.BackupsDirectory : _paths.RootDirectory
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var answer = MessageBox.Show(
            this,
            "Restore replaces the current Workbench database and attachment store with the selected backup.\n\nA safety backup of the current state is created first when possible. Continue?",
            "Restore Workbench backup",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);
        if (answer != DialogResult.Yes)
        {
            return;
        }

        try
        {
            UseWaitCursor = true;
            Enabled = false;
            var result = await _backupService.RestoreBackupAsync(dialog.FileName).ConfigureAwait(true);
            Enabled = true;
            UseWaitCursor = false;
            await ReloadProjectsAsync().ConfigureAwait(true);
            SetStatus($"Backup restored from {result.ArchivePath}");

            var safetyText = string.IsNullOrWhiteSpace(result.SafetyBackupPath)
                ? "No previous database state required a safety backup."
                : $"Safety backup of replaced state:\n{result.SafetyBackupPath}";
            MessageBox.Show(
                this,
                $"Restore completed successfully.\n\n{safetyText}",
                "SASD Workbench",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            Enabled = true;
            UseWaitCursor = false;
            ShowError("The restore failed. The restore service attempted to preserve or roll back the previous state.", ex);
        }
    }

    private async Task ReloadEntriesFromDialogAsync(Guid projectId, Guid entryId)
    {
        await ReloadEntriesAsync(projectId, entryId).ConfigureAwait(true);
        SetStatus("Search result opened.");
    }

    private async Task ReloadProjectsAsync(Guid? selectProjectId = null)
    {
        try
        {
            var previousId = selectProjectId ?? (_projectList.SelectedItem as Project)?.Id;
            var projects = await _projectService.ListAsync().ConfigureAwait(true);

            _loadingSelection = true;
            _projectList.DataSource = projects.ToList();
            _projectList.DisplayMember = nameof(Project.Name);
            SelectById(_projectList, previousId, static item => ((Project)item).Id);
            _loadingSelection = false;

            if (_projectList.SelectedItem is Project project)
            {
                _newEntryButton.Enabled = true;
                await ReloadEntriesAsync(project.Id).ConfigureAwait(true);
            }
            else
            {
                _entryList.DataSource = null;
                _newEntryButton.Enabled = false;
                ClearEditor();
            }

            UpdateToolAvailability();
        }
        catch (Exception ex)
        {
            _loadingSelection = false;
            UpdateToolAvailability();
            ShowError("Projects could not be loaded.", ex);
        }
    }

    private async Task ReloadEntriesAsync(Guid projectId, Guid? selectEntryId = null)
    {
        try
        {
            var previousId = selectEntryId ?? (_entryList.SelectedItem as Entry)?.Id;
            var entries = await _entryService.ListByProjectAsync(projectId).ConfigureAwait(true);

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

            UpdateToolAvailability();
        }
        catch (Exception ex)
        {
            _loadingSelection = false;
            UpdateToolAvailability();
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

    private void UpdateToolAvailability()
    {
        var hasProject = _projectList.SelectedItem is Project;
        var hasEntry = _entryList.SelectedItem is Entry;
        _searchButton.Enabled = hasProject;
        _templatesButton.Enabled = hasProject;
        _tagsButton.Enabled = hasEntry;
        _attachmentsButton.Enabled = hasEntry;
        _collectionsButton.Enabled = hasProject;
        _relationsButton.Enabled = hasEntry;
        _activityButton.Enabled = hasProject;
        _exportButton.Enabled = hasProject;
        _backupButton.Enabled = true;
        _restoreButton.Enabled = true;
    }

    private void SetStatus(string message) => _statusLabel.Text = message;

    private void ShowError(string message, Exception exception)
    {
        SetStatus(message);
        MessageBox.Show(this, $"{message}\n\n{exception.Message}", "SASD Workbench", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
