using SASD.Workbench.Domain.Entities;
using SASD.Workbench.Domain.Metadata;

namespace SASD.Workbench.Domain.Tests;

/// <summary>
/// Protects the stable machine-key vocabulary while keeping the Core intentionally extensible for
/// specialist Workbench profiles.
/// </summary>
public sealed class MetadataAndRelationTests
{
    [Theory]
    [InlineData(" Related_To ", "related_to")]
    [InlineData("CUSTOM_RELATION_2", "custom_relation_2")]
    [InlineData("biblical_supports", "biblical_supports")]
    public void RelationType_Normalize_ReturnsStableMachineKey(string input, string expected)
        => Assert.Equal(expected, EntryRelationTypes.Normalize(input));

    [Theory]
    [InlineData("related to")]
    [InlineData("related-to")]
    [InlineData("_related")]
    [InlineData("2related")]
    [InlineData("ä_related")]
    public void RelationType_Normalize_RejectsUnstableKeys(string input)
        => Assert.Throws<ArgumentException>(() => EntryRelationTypes.Normalize(input));

    [Fact]
    public void RelationType_Normalize_RejectsKeysLongerThanOneHundredCharacters()
        => Assert.Throws<ArgumentOutOfRangeException>(() => EntryRelationTypes.Normalize("a" + new string('b', 100)));

    [Fact]
    public void RelationType_BuiltInCatalog_HasUniqueNormalizedKeys()
    {
        var normalized = EntryRelationTypes.BuiltIn.Select(EntryRelationTypes.Normalize).ToArray();

        Assert.Equal(normalized.Length, normalized.Distinct(StringComparer.Ordinal).Count());
        Assert.Contains(EntryRelationTypes.RelatedTo, normalized);
        Assert.Contains(EntryRelationTypes.Supports, normalized);
        Assert.Contains(EntryRelationTypes.Contradicts, normalized);
    }

    [Fact]
    public void CoreEntryTypes_AreStableButCatalogRemainsOpen()
    {
        Assert.True(CoreEntryTypes.IsBuiltIn(CoreEntryTypes.ResearchQuestion));
        Assert.True(CoreEntryTypes.IsBuiltIn(" RESEARCH_SOURCE "));
        Assert.False(CoreEntryTypes.IsBuiltIn("biblical_person"));

        // A profile-defined key is deliberately allowed by Entry itself. CoreEntryTypes is a catalog,
        // not an enum that would force every future profile to modify the shared Domain assembly.
        var entry = new Entry(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "biblical_person",
            "Nathanael",
            null,
            string.Empty,
            DateTime.UtcNow);
        Assert.Equal("biblical_person", entry.EntryType);
    }

    [Fact]
    public void EntryLink_NormalizesCustomRelationAndRejectsSelfLink()
    {
        var sourceId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var link = new EntryLink(
            Guid.NewGuid(),
            sourceId,
            targetId,
            " Custom_Relation ",
            DateTime.UtcNow,
            "  context  ");

        Assert.Equal("custom_relation", link.RelationType);
        Assert.Equal("context", link.Comment);
        Assert.Throws<ArgumentException>(() =>
            new EntryLink(Guid.NewGuid(), sourceId, sourceId, EntryRelationTypes.RelatedTo, DateTime.UtcNow));
    }
}
