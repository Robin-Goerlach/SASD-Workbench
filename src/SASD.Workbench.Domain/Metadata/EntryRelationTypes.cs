namespace SASD.Workbench.Domain.Metadata;

/// <summary>
/// Defines the shared semantic relation vocabulary used by generic Workbench entries.
/// </summary>
/// <remarks>
/// The built-in list is not a closed enum. Future profiles may introduce additional relation keys,
/// provided they follow the same stable machine-key syntax. This keeps the Core extensible while
/// avoiding spelling variants such as <c>Related To</c>, <c>related-to</c> and <c>related_to</c>.
/// </remarks>
public static class EntryRelationTypes
{
    public const string RelatedTo = "related_to";
    public const string References = "references";
    public const string BasedOn = "based_on";
    public const string Supports = "supports";
    public const string Contradicts = "contradicts";
    public const string Confirms = "confirms";
    public const string Refutes = "refutes";
    public const string PossiblyRelatedTo = "possibly_related_to";
    public const string PartOf = "part_of";
    public const string Precedes = "precedes";
    public const string Follows = "follows";
    public const string VariantOf = "variant_of";
    public const string Replaces = "replaces";
    public const string Uses = "uses";
    public const string ComparesWith = "compares_with";

    private static readonly string[] BuiltInValues =
    [
        RelatedTo,
        References,
        BasedOn,
        Supports,
        Contradicts,
        Confirms,
        Refutes,
        PossiblyRelatedTo,
        PartOf,
        Precedes,
        Follows,
        VariantOf,
        Replaces,
        Uses,
        ComparesWith
    ];

    /// <summary>
    /// Gets the relation keys that are understood consistently by the profile-neutral Core.
    /// </summary>
    public static IReadOnlyList<string> BuiltIn => BuiltInValues;

    /// <summary>
    /// Normalizes and validates a relation key while still allowing future profile-defined keys.
    /// </summary>
    /// <param name="value">Relation key to normalize.</param>
    /// <returns>A lowercase snake-case machine key.</returns>
    /// <exception cref="ArgumentException">Thrown when the key is empty or contains unsupported characters.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the key exceeds 100 characters.</exception>
    public static string Normalize(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Relation type must not exceed 100 characters.");
        }

        if (!IsAsciiLetter(normalized[0]))
        {
            throw new ArgumentException("Relation type must start with an ASCII letter.", nameof(value));
        }

        for (var index = 1; index < normalized.Length; index++)
        {
            var character = normalized[index];
            if (!IsAsciiLetter(character) && !char.IsAsciiDigit(character) && character != '_')
            {
                throw new ArgumentException(
                    "Relation type may contain only ASCII letters, digits and underscores.",
                    nameof(value));
            }
        }

        return normalized;
    }

    /// <summary>
    /// Returns whether a relation key belongs to the Core's standard vocabulary.
    /// </summary>
    public static bool IsBuiltIn(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        try
        {
            var normalized = Normalize(value);
            return BuiltInValues.Contains(normalized, StringComparer.Ordinal);
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static bool IsAsciiLetter(char value)
        => value is >= 'a' and <= 'z';
}
