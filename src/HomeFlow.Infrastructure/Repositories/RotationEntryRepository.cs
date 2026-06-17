using HomeFlow.Domain.Entities;
using HomeFlow.Domain.Repositories;
using HomeFlow.Infrastructure.Database;
using Npgsql;

namespace HomeFlow.Infrastructure.Repositories;

public class RotationEntryRepository(UnitOfWork db) : IRotationEntryRepository
{
    /// <summary>Returns all rotation entries for the given template, ordered by rotation order.</summary>
    public async Task<IEnumerable<RotationEntry>> GetByTemplateIdAsync(Guid templateId, CancellationToken ct = default)
    {
        var conn = await db.GetConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT id, template_id, user_id, rotation_order FROM rotation_entries WHERE template_id = @templateId ORDER BY rotation_order", conn, db.Transaction);
        cmd.Parameters.AddWithValue("templateId", templateId);

        var results = new List<RotationEntry>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            results.Add(MapFromReader(reader));
        }
        return results;
    }

    /// <summary>Inserts a new rotation entry row and populates the entity's generated ID.</summary>
    public async System.Threading.Tasks.Task CreateAsync(RotationEntry entry, CancellationToken ct = default)
    {
        var conn = await db.GetConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO rotation_entries (template_id, user_id, rotation_order)
            VALUES (@templateId, @userId, @rotationOrder)
            RETURNING id
            """, conn, db.Transaction);
        cmd.Parameters.AddWithValue("templateId", entry.TemplateId);
        cmd.Parameters.AddWithValue("userId", entry.UserId);
        cmd.Parameters.AddWithValue("rotationOrder", entry.RotationOrder);

        entry.Id = (Guid)(await cmd.ExecuteScalarAsync(ct))!;
    }

    /// <summary>Removes all rotation entry rows belonging to the given template.</summary>
    public async System.Threading.Tasks.Task DeleteByTemplateIdAsync(Guid templateId, CancellationToken ct = default)
    {
        var conn = await db.GetConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "DELETE FROM rotation_entries WHERE template_id = @templateId", conn, db.Transaction);
        cmd.Parameters.AddWithValue("templateId", templateId);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static RotationEntry MapFromReader(NpgsqlDataReader reader)
    {
        return new RotationEntry
        {
            Id = reader.Get<Guid>("id"),
            TemplateId = reader.Get<Guid>("template_id"),
            UserId = reader.Get<Guid>("user_id"),
            RotationOrder = reader.Get<int>("rotation_order")
        };
    }
}
