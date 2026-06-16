using HomeFlow.Domain.Entities;
using HomeFlow.Domain.Repositories;
using HomeFlow.Infrastructure.Database;
using Npgsql;

namespace HomeFlow.Infrastructure.Repositories;

public class RecurringTaskTemplateRepository(IDbConnectionFactory db) : IRecurringTaskTemplateRepository
{
    public async Task<RecurringTaskTemplate?> GetByIdAsync(Guid id)
    {
        await using var conn = db.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT id, title, description, frequency_days, current_assignee_index, last_generated_date, created_at FROM recurring_task_templates WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;
        return MapFromReader(reader);
    }

    public async Task<IEnumerable<RecurringTaskTemplate>> GetAllAsync()
    {
        await using var conn = db.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT id, title, description, frequency_days, current_assignee_index, last_generated_date, created_at FROM recurring_task_templates ORDER BY created_at DESC", conn);
        var results = new List<RecurringTaskTemplate>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            results.Add(MapFromReader(reader));
        return results;
    }

    public async Task<RecurringTaskTemplate> CreateAsync(RecurringTaskTemplate template)
    {
        await using var conn = db.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO recurring_task_templates (title, description, frequency_days, current_assignee_index, last_generated_date, created_at)
            VALUES (@title, @description, @frequencyDays, @currentAssigneeIndex, @lastGeneratedDate, @createdAt)
            RETURNING id
            """, conn);
        cmd.Parameters.AddWithValue("title", template.Title);
        cmd.Parameters.AddWithValue("description", (object?)template.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("frequencyDays", template.FrequencyDays);
        cmd.Parameters.AddWithValue("currentAssigneeIndex", template.CurrentAssigneeIndex);
        cmd.Parameters.AddWithValue("lastGeneratedDate", (object?)template.LastGeneratedDate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("createdAt", template.CreatedAt);

        template.Id = (Guid)(await cmd.ExecuteScalarAsync())!;
        return template;
    }

    public async Task<RecurringTaskTemplate> UpdateAsync(RecurringTaskTemplate template)
    {
        await using var conn = db.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            """
            UPDATE recurring_task_templates
            SET title = @title, description = @description, frequency_days = @frequencyDays,
                current_assignee_index = @currentAssigneeIndex, last_generated_date = @lastGeneratedDate
            WHERE id = @id
            """, conn);
        cmd.Parameters.AddWithValue("id", template.Id);
        cmd.Parameters.AddWithValue("title", template.Title);
        cmd.Parameters.AddWithValue("description", (object?)template.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("frequencyDays", template.FrequencyDays);
        cmd.Parameters.AddWithValue("currentAssigneeIndex", template.CurrentAssigneeIndex);
        cmd.Parameters.AddWithValue("lastGeneratedDate", (object?)template.LastGeneratedDate ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync();
        return template;
    }

    public async System.Threading.Tasks.Task DeleteAsync(Guid id)
    {
        await using var conn = db.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand("DELETE FROM recurring_task_templates WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        await cmd.ExecuteNonQueryAsync();
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
