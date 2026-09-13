using SASD.Workbench.Application.Services;
using SASD.Workbench.Domain.Entities;

namespace SASD.Workbench.WinForms;

/// <summary>
/// Shows the lightweight V1 activity history for a project or one selected entry.
/// </summary>
public sealed class ActivityLogDialog : Form
{
    private readonly ActivityLogService _activityService;
    private readonly Guid _projectId;
    private readonly Guid? _entryId;
    private readonly ListView _activityList = new();
    private readonly CheckBox _entryOnlyCheckBox = new();
    private readonly Label _countLabel = new();

    public ActivityLogDialog(ActivityLogService activityService, Guid projectId, Guid? entryId = null)
    {
        _activityService = activityService ?? throw new ArgumentNullException(nameof(activityService));
        if (projectId == Guid.Empty)
        {
            throw new ArgumentException("Project id must not be empty.", nameof(projectId));
        }

        if (entryId == Guid.Empty)
        {
            throw new ArgumentException("Entry id must be null or non-empty.", nameof(entryId));
        }

        _projectId = projectId;
        _entryId = entryId;
        Text = "Activity log";
        StartPosition = FormStartPosition.CenterParent;
        Width = 1050;
        Height = 620;
        MinimumSize = new Size(780, 450);
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
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var header = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2 };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        var note = new Label
        {
            Text = "V1 activity history is operational history, not a tamper-proof audit trail.",
            AutoSize = true,
            Anchor = AnchorStyles.Left
        };
        header.Controls.Add(note, 0, 0);

        _entryOnlyCheckBox.Text = "Selected entry only";
        _entryOnlyCheckBox.AutoSize = true;
        _entryOnlyCheckBox.Enabled = _entryId.HasValue;
        _entryOnlyCheckBox.Checked = _entryId.HasValue;
        _entryOnlyCheckBox.CheckedChanged += async (_, _) => await ReloadAsync().ConfigureAwait(true);
        header.Controls.Add(_entryOnlyCheckBox, 1, 0);
        root.Controls.Add(header, 0, 0);

        _activityList.Dock = DockStyle.Fill;
        _activityList.View = View.Details;
        _activityList.FullRowSelect = true;
        _activityList.HideSelection = false;
        _activityList.Columns.Add("Time", 150);
        _activityList.Columns.Add("Action", 150);
        _activityList.Columns.Add("Description", 430);
        _activityList.Columns.Add("Old value", 150);
        _activityList.Columns.Add("New value", 150);
        root.Controls.Add(_activityList, 0, 1);

        var footer = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2 };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _countLabel.AutoSize = true;
        _countLabel.Anchor = AnchorStyles.Left;
        footer.Controls.Add(_countLabel, 0, 0);

        var buttons = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.RightToLeft };
        var closeButton = new Button { Text = "Close", DialogResult = DialogResult.Cancel, AutoSize = true };
        var refreshButton = new Button { Text = "Refresh", AutoSize = true };
        refreshButton.Click += async (_, _) => await ReloadAsync().ConfigureAwait(true);
        buttons.Controls.Add(closeButton);
        buttons.Controls.Add(refreshButton);
        footer.Controls.Add(buttons, 1, 0);
        root.Controls.Add(footer, 0, 2);

        CancelButton = closeButton;
        Controls.Add(root);
    }

    private async Task ReloadAsync()
    {
        try
        {
            var entryFilter = _entryOnlyCheckBox.Checked ? _entryId : null;
            var items = await _activityService.ListAsync(_projectId, entryFilter, limit: 500).ConfigureAwait(true);

            _activityList.BeginUpdate();
            try
            {
                _activityList.Items.Clear();
                foreach (var activity in items)
                {
                    _activityList.Items.Add(CreateListItem(activity));
                }
            }
            finally
            {
                _activityList.EndUpdate();
            }

            _countLabel.Text = items.Count == 1 ? "1 activity" : $"{items.Count} activities";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Activity history could not be loaded.\n\n{ex.Message}", "SASD Workbench", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static ListViewItem CreateListItem(ActivityLogItem activity)
    {
        var item = new ListViewItem(activity.CreatedAtUtc.ToLocalTime().ToString("g"));
        item.SubItems.Add(activity.ActionType);
        item.SubItems.Add(activity.Description);
        item.SubItems.Add(activity.OldValue ?? string.Empty);
        item.SubItems.Add(activity.NewValue ?? string.Empty);
        return item;
    }
}
