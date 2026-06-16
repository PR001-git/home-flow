using HomeFlow.Domain.Entities;
using HomeFlow.Domain.Repositories;
using HomeFlow.Infrastructure.Database;
using Npgsql;

namespace HomeFlow.Infrastructure.Repositories;

public class RecurringTaskTemplateRepository(UnitOfWork db) : IRecurringTaskTemplateRepository
{
    /// <summary>Returns the template with the given ID, or <see langword="null"/> if not found.</summary>
    public async Task<RecurringTaskTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var conn = await db.GetConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT id, title, description, frequency_days, current_assignee_index, last_generated_date, created_at FROM recurring_task_templates WHERE id = @id", conn, db.Transaction);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;
        return MapFromReader(reader);
    }

    /// <summary>Returns all recurring task templates ordered by creation date descending.</summary>
    public async Task<IEnumerable<RecurringTaskTemplate>> GetAllAsync(CancellationToken ct = default)
    {
        var conn = await db.GetConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT id, title, description, frequency_days, current_assignee_index, last_generated_date, created_at FROM recurring_task_templates ORDER BY created_at DESC", conn, db.Transaction);
        var results = new List<RecurringTaskTemplate>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            results.Add(MapFromReader(reader));
        return results;
    }

    /// <summary>Inserts a new template row and populates the entity's generated ID.</summary>
    public async Task<RecurringTaskTemplate> CreateAsync(RecurringTaskTemplate template, CancellationToken ct = default)
    {
        var conn = await db.GetConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO recurring_task_templates (title, description, frequency_days, current_assignee_index, last_generated_date, created_at)
            VALUES (@title, @description, @frequencyDays, @currentAssigneeIndex, @lastGeneratedDate, @createdAt)
            RETURNING id
            """, conn, db.Transaction);
        cmd.Parameters.AddWithValue("title", template.Title);
        cmd.Parameters.AddWithValue("description", (object?)template.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("frequencyDays", template.FrequencyDays);
        cmd.Parameters.AddWithValue("currentAssigneeIndex", template.CurrentAssigneeIndex);
        cmd.Parameters.AddWithValue("lastGeneratedDate", (object?)template.LastGeneratedDate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("createdAt", template.CreatedAt);

        template.Id = (Guid)(await cmd.ExecuteScalarAsync(ct))!;
        return template;
    }

    /// <summary>Updates the mutable columns of an existing template row.</summary>
    public async Task<RecurringTaskTemplate> UpdateAsync(RecurringTaskTemplate template, CancellationToken ct = default)
    {
        var conn = await db.GetConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(
            """
            UPDATE recurring_task_templates
            SET title = @title, description = @description, frequency_days = @frequencyDays,
                current_assignee_index = @currentAssigneeIndex, last_generated_date = @lastGeneratedDate
            WHERE id = @id
            """, conn, db.Transaction);
        cmd.Parameters.AddWithValue("id", template.Id);
        cmd.Parameters.AddWithValue("title", template.Title);
        cmd.Parameters.AddWithValue("description", (object?)template.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("frequencyDays", template.FrequencyDays);
        cmd.Parameters.AddWithValue("currentAssigneeIndex", template.CurrentAssigneeIndex);
        cmd.Parameters.AddWithValue("lastGeneratedDate", (object?)template.LastGeneratedDate ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync(ct);
        return template;
    }

    /// <summary>Deletes the template row with the given ID.</summary>
    public async System.Threading.Tasks.Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var conn = await db.GetConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand("DELETE FROM recurring_task_templates WHERE id = @id", conn, db.Transaction);
        cmd.Parameters.AddWithValue("id", id);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static RecurringTaskTemplate MapFromReader(NpgsqlDataReader reader)
    {
        return new RecurringTaskTemplate
        {
            Id = reader.Get<Guid>("id"),
            Title = reader.Get<string>("title"),
            Description = reader.GetNullableString("description"),
            FrequencyDays = reader.Get<int>("frequency_days"),
            CurrentAssigneeIndex = reader.Get<int>("current_assignee_index"),
            LastGeneratedDate = reader.GetNullable<DateTime>("last_generated_date"),
            CreatedAt = reader.Get<DateTime>("created_at")
        };
    }
}
