using SASD.Workbench.Domain.Entities;
using SASD.Workbench.Domain.Metadata;

namespace SASD.Workbench.Domain.Tests;

/// <summary>
/// Fast tests for the two central mutable Core entities. These tests deliberately avoid persistence so
/// validation, versioning and lifecycle regressions are reported independently from SQLite behavior.
/// </summary>
public sealed class ProjectAndEntryTests
{
    private static readonly DateTime CreatedAt = new(2026, 9, 13, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Project_NewProject_NormalizesValuesAndStartsAtVersionOne()
    {
        var project = new Project(Guid.NewGuid(), "  Research  ", "  Shared notebook  ", "  general  ", CreatedAt);

        Assert.Equal("Research", project.Name);
        Assert.Equal("Shared notebook", project.Description);
        Assert.Equal("general", project.ProfileKey);
        Assert.Equal("active", project.Status);
        Assert.Equal(1, project.Version);
        Assert.Equal(CreatedAt, project.CreatedAtUtc);
        Assert.Equal(CreatedAt, project.UpdatedAtUtc);
        Assert.False(project.IsArchived);
        Assert.False(project.IsDeleted);
    }

    [Fact]
    public void Project_Update_AdvancesVersionExactlyOnce()
    {
        var project = new Project(Guid.NewGuid(), "Research", null, "general", CreatedAt);
        var updatedAt = CreatedAt.AddMinutes(5);

        project.Update("Research 2", "Updated", "lab", updatedAt);

        Assert.Equal(2, project.Version);
        Assert.Equal(updatedAt, project.UpdatedAtUtc);
        Assert.Equal("Research 2", project.Name);
        Assert.Equal("lab", project.ProfileKey);
    }

    [Fact]
    public void Project_Delete_IsIdempotentAndPreventsFurtherMutation()
    {
        var project = new Project(Guid.NewGuid(), "Research", null, "general", CreatedAt);
        var deletedAt = CreatedAt.AddMinutes(1);

        project.Delete(deletedAt);
        var versionAfterFirstDelete = project.Version;
        project.Delete(deletedAt.AddMinutes(1));

        Assert.True(project.IsDeleted);
        Assert.Equal("deleted", project.Status);
        Assert.Equal(deletedAt, project.DeletedAtUtc);
        Assert.Equal(versionAfterFirstDelete, project.Version);
        Assert.Throws<InvalidOperationException>(() => project.Update("Changed", null, "general", deletedAt.AddMinutes(2)));
    }

    [Fact]
    public void Project_ArchiveAndUnarchive_UpdateStatusAndVersion()
    {
        var project = new Project(Guid.NewGuid(), "Research", null, "general", CreatedAt);

        project.Archive(CreatedAt.AddMinutes(1));
        Assert.True(project.IsArchived);
        Assert.Equal("archived", project.Status);
        Assert.Equal(2, project.Version);

        project.Unarchive(CreatedAt.AddMinutes(2));
        Assert.False(project.IsArchived);
        Assert.Equal("active", project.Status);
        Assert.Equal(3, project.Version);
    }

    [Fact]
    public void Entry_NewEntry_NormalizesFieldsAndUsesOpenEntryTypeVocabulary()
    {
        var projectId = Guid.NewGuid();
        var entry = new Entry(
            Guid.NewGuid(),
            projectId,
            "  custom_profile_type  ",
            "  First finding  ",
            "  Summary  ",
            null,
            CreatedAt,
            "  draft  ");

        Assert.Equal(projectId, entry.ProjectId);
        Assert.Equal("custom_profile_type", entry.EntryType);
        Assert.Equal("First finding", entry.Title);
        Assert.Equal("Summary", entry.Summary);
        Assert.Equal(string.Empty, entry.ContentMarkdown);
        Assert.Equal("draft", entry.Status);
        Assert.Equal(1, entry.Version);
    }

    [Fact]
    public void Entry_Update_IsOneLogicalVersionIncrement()
    {
        var entry = CreateEntry();
        var updatedAt = CreatedAt.AddMinutes(1);

        entry.Update("Changed", "Summary", "# Changed", CoreEntryTypes.Finding, "in_work", updatedAt);

        Assert.Equal(2, entry.Version);
        Assert.Equal(updatedAt, entry.UpdatedAtUtc);
        Assert.Equal("Changed", entry.Title);
        Assert.Equal(CoreEntryTypes.Finding, entry.EntryType);
        Assert.Equal("in_work", entry.Status);
    }

    [Fact]
    public void Entry_ArchiveAndUnarchive_PreserveDocumentWhileChangingLifecycleState()
    {
        var entry = CreateEntry();

        entry.Archive(CreatedAt.AddMinutes(1));
        Assert.True(entry.IsArchived);
        Assert.Equal("archived", entry.Status);

        entry.Unarchive(CreatedAt.AddMinutes(2));
        Assert.False(entry.IsArchived);
        Assert.Equal("draft", entry.Status);
        Assert.Equal("# Body", entry.ContentMarkdown);
        Assert.Equal(3, entry.Version);
    }

    [Fact]
    public void Entry_Delete_IsIdempotentAndBlocksEditing()
    {
        var entry = CreateEntry();
        var deletedAt = CreatedAt.AddMinutes(1);

        entry.Delete(deletedAt);
        var versionAfterFirstDelete = entry.Version;
        entry.Delete(deletedAt.AddMinutes(1));

        Assert.True(entry.IsDeleted);
        Assert.Equal("deleted", entry.Status);
        Assert.Equal(deletedAt, entry.DeletedAtUtc);
        Assert.Equal(versionAfterFirstDelete, entry.Version);
        Assert.Throws<InvalidOperationException>(() =>
            entry.Update("Changed", null, null, CoreEntryTypes.Note, "draft", deletedAt.AddMinutes(2)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Project_RejectsBlankName(string name)
        => Assert.Throws<ArgumentException>(() => new Project(Guid.NewGuid(), name, null, "general", CreatedAt));

    [Fact]
    public void Entry_RejectsEmptyProjectId()
        => Assert.Throws<ArgumentException>(() =>
            new Entry(Guid.NewGuid(), Guid.Empty, CoreEntryTypes.Note, "Title", null, null, CreatedAt));

    private static Entry CreateEntry()
        => new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            CoreEntryTypes.Note,
            "First",
            null,
            "# Body",
            CreatedAt);
}
