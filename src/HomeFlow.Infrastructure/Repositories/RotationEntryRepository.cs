using HomeFlow.Domain.Entities;
using HomeFlow.Domain.Repositories;
using HomeFlow.Infrastructure.Database;
using Npgsql;

namespace HomeFlow.Infrastructure.Repositories;

public class RotationEntryRepository(IDbConnectionFactory db) : IRotationEntryRepository
{
    public async Task<IEnumerable<RotationEntry>> GetByTemplateIdAsync(Guid templateId)
    {
        await using var conn = db.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT id, template_id, user_id, rotation_order FROM rotation_entries WHERE template_id = @templateId ORDER BY rotation_order", conn);
        cmd.Parameters.AddWithValue("templateId", templateId);

        var results = new List<RotationEntry>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(MapFromReader(reader));
        }
        return results;
    }

    public async System.Threading.Tasks.Task CreateAsync(RotationEntry entry)
    {
        await using var conn = db.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO rotation_entries (template_id, user_id, rotation_order)
            VALUES (@templateId, @userId, @rotationOrder)
            RETURNING id
            """, conn);
        cmd.Parameters.AddWithValue("templateId", entry.TemplateId);
        cmd.Parameters.AddWithValue("userId", entry.UserId);
        cmd.Parameters.AddWithValue("rotationOrder", entry.RotationOrder);

        entry.Id = (Guid)(await cmd.ExecuteScalarAsync())!;
    }

    public async System.Threading.Tasks.Task DeleteByTemplateIdAsync(Guid templateId)
    {
        await using var conn = db.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "DELETE FROM rotation_entries WHERE template_id = @templateId", conn);
        cmd.Parameters.AddWithValue("templateId", templateId);
        await cmd.ExecuteNonQueryAsync();
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
