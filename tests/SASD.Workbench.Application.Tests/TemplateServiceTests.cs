using SASD.Workbench.Application.Services;
using SASD.Workbench.Domain.Entities;
using SASD.Workbench.Domain.Metadata;

namespace SASD.Workbench.Application.Tests;

public sealed class TemplateServiceTests
{
    private static readonly DateTime Now = new(2026, 9, 13, 16, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task CreateEntryAsync_AllowsGeneralTemplateInSpecialistProfile()
    {
        var projects = new InMemoryProjectRepository();
        var entries = new InMemoryEntryRepository();
        var templates = new InMemoryTemplateRepository();
        var clock = new TestClock(Now);
        var project = CreateProject("biblical");
        var template = CreateTemplate(profileKey: "general");
        projects.Seed(project);
        templates.Seed(template);
        var service = new TemplateService(templates, projects, entries, clock);

        var entry = await service.CreateEntryAsync(project.Id, template.Id, "Nathanael study");

        Assert.Equal(project.Id, entry.ProjectId);
        Assert.Equal(CoreEntryTypes.ResearchNote, entry.EntryType);
        Assert.Equal("draft", entry.Status);
        Assert.Equal("# Template body", entry.ContentMarkdown);
        Assert.Equal(Now, entry.CreatedAtUtc);
        Assert.Contains(entries.All, candidate => candidate.Id == entry.Id);
    }

    [Fact]
    public async Task CreateEntryAsync_RejectsTemplateFromDifferentSpecialistProfile()
    {
        var projects = new InMemoryProjectRepository();
        var entries = new InMemoryEntryRepository();
        var templates = new InMemoryTemplateRepository();
        var project = CreateProject("biblical");
        var template = CreateTemplate(profileKey: "health");
        projects.Seed(project);
        templates.Seed(template);
        var service = new TemplateService(templates, projects, entries, new TestClock(Now));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateEntryAsync(project.Id, template.Id, "Should fail"));

        Assert.Contains("does not match", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(entries.All);
    }

    [Fact]
    public async Task CreateEntryAsync_RejectsProjectLocalTemplateFromAnotherProject()
    {
        var projects = new InMemoryProjectRepository();
        var entries = new InMemoryEntryRepository();
        var templates = new InMemoryTemplateRepository();
        var project = CreateProject("general");
        var otherProject = CreateProject("general");
        var template = CreateTemplate(projectId: otherProject.Id, profileKey: "general");
        projects.Seed(project);
        projects.Seed(otherProject);
        templates.Seed(template);
        var service = new TemplateService(templates, projects, entries, new TestClock(Now));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateEntryAsync(project.Id, template.Id, "Should fail"));
        Assert.Empty(entries.All);
    }

    [Fact]
    public async Task DeleteAsync_RejectsSystemTemplate()
    {
        var projects = new InMemoryProjectRepository();
        var entries = new InMemoryEntryRepository();
        var templates = new InMemoryTemplateRepository();
        var systemTemplate = CreateTemplate(profileKey: "general", isSystemTemplate: true);
        templates.Seed(systemTemplate);
        var service = new TemplateService(templates, projects, entries, new TestClock(Now));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(systemTemplate.Id));

        Assert.Contains("System templates", exception.Message, StringComparison.Ordinal);
        Assert.False(systemTemplate.IsDeleted);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesUserTemplateWithoutTouchingCreatedEntries()
    {
        var projects = new InMemoryProjectRepository();
        var entries = new InMemoryEntryRepository();
        var templates = new InMemoryTemplateRepository();
        var project = CreateProject("general");
        var template = CreateTemplate(projectId: project.Id, profileKey: "general");
        projects.Seed(project);
        templates.Seed(template);
        var service = new TemplateService(templates, projects, entries, new TestClock(Now));
        var entry = await service.CreateEntryAsync(project.Id, template.Id, "Independent copy");

        await service.DeleteAsync(template.Id);

        Assert.True(template.IsDeleted);
        Assert.False(entry.IsDeleted);
        Assert.Equal("# Template body", entry.ContentMarkdown);
    }

    private static Project CreateProject(string profileKey)
        => new(Guid.NewGuid(), "Project", null, profileKey, Now);

    private static Template CreateTemplate(
        Guid? projectId = null,
        string profileKey = "general",
        bool isSystemTemplate = false)
        => new(
            Guid.NewGuid(),
            "Research note",
            CoreEntryTypes.ResearchNote,
            "draft",
            "# Template body",
            Now,
            projectId,
            profileKey,
            "Reusable description",
            isSystemTemplate);
}
