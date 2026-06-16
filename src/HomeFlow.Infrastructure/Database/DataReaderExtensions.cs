using Npgsql;

namespace HomeFlow.Infrastructure.Database;

/// <summary>
/// Column-name-based read helpers for <see cref="NpgsqlDataReader"/>. Reading by name
/// (rather than ordinal position) keeps mapping resilient to changes in SELECT column order.
/// </summary>
internal static class DataReaderExtensions
{
    public static T Get<T>(this NpgsqlDataReader reader, string name)
        => reader.GetFieldValue<T>(reader.GetOrdinal(name));

    public static T? GetNullable<T>(this NpgsqlDataReader reader, string name) where T : struct
    {
        int ordinal = reader.GetOrdinal(name);
        return reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<T>(ordinal);
    }

    public static string? GetNullableString(this NpgsqlDataReader reader, string name)
    {
        int ordinal = reader.GetOrdinal(name);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    /// <summary>Reads a smallint column and casts it to the given enum type.</summary>
    public static TEnum GetEnum<TEnum>(this NpgsqlDataReader reader, string name) where TEnum : struct, Enum
        => (TEnum)(object)(int)reader.GetFieldValue<short>(reader.GetOrdinal(name));
}
