using HomeFlow.Domain.Entities;
using HomeFlow.Domain.Enums;
using HomeFlow.Domain.Repositories;
using HomeFlow.Infrastructure.Database;
using Npgsql;

namespace HomeFlow.Infrastructure.Repositories;

public class TaskRepository(IDbConnectionFactory db) : ITaskRepository
{
    public async Task<HouseholdTask?> GetByIdAsync(Guid id)
    {
        await using var conn = db.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT id, title, description, task_type, status, due_date, assigned_to_user_id, created_by_user_id, template_id, created_at, completed_at FROM household_tasks WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        return await ReadTaskAsync(cmd);
    }

    public async Task<IEnumerable<HouseholdTask>> GetAllAsync(TaskFilter? filter)
    {
        await using var conn = db.CreateConnection();
        await conn.OpenAsync();

        var sql = "SELECT id, title, description, task_type, status, due_date, assigned_to_user_id, created_by_user_id, template_id, created_at, completed_at FROM household_tasks WHERE 1=1";
        var parameters = new List<NpgsqlParameter>();

        if (filter?.AssignedToUserId.HasValue == true)
        {
            sql += " AND assigned_to_user_id = @assignedTo";
            parameters.Add(new NpgsqlParameter("assignedTo", filter.AssignedToUserId.Value));
        }
        if (filter?.Status.HasValue == true)
        {
            if (filter.Status.Value == HouseholdTaskStatus.Overdue)
            {
                sql += " AND status IN (0, 1) AND due_date < NOW()";
            }
            else
            {
                sql += " AND status = @status";
                parameters.Add(new NpgsqlParameter("status", (short)filter.Status.Value));
            }
        }
        if (filter?.TaskType.HasValue == true)
        {
            sql += " AND task_type = @taskType";
            parameters.Add(new NpgsqlParameter("taskType", (short)filter.TaskType.Value));
        }

        sql += " ORDER BY created_at DESC";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters.ToArray());

        var tasks = new List<HouseholdTask>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tasks.Add(MapFromReader(reader));
        }
        return tasks;
    }

    public async Task<HouseholdTask> CreateAsync(HouseholdTask task)
    {
        await using var conn = db.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO household_tasks (title, description, task_type, status, due_date, assigned_to_user_id, created_by_user_id, template_id, created_at, completed_at)
            VALUES (@title, @description, @taskType, @status, @dueDate, @assignedTo, @createdBy, @templateId, @createdAt, @completedAt)
            RETURNING id
            """, conn);
        cmd.Parameters.AddWithValue("title", task.Title);
        cmd.Parameters.AddWithValue("description", (object?)task.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("taskType", (short)task.TaskType);
        cmd.Parameters.AddWithValue("status", (short)task.Status);
        cmd.Parameters.AddWithValue("dueDate", (object?)task.DueDate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("assignedTo", (object?)task.AssignedToUserId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("createdBy", task.CreatedByUserId);
        cmd.Parameters.AddWithValue("templateId", (object?)task.TemplateId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("createdAt", task.CreatedAt);
        cmd.Parameters.AddWithValue("completedAt", (object?)task.CompletedAt ?? DBNull.Value);

        task.Id = (Guid)(await cmd.ExecuteScalarAsync())!;
        return task;
    }

    public async Task<HouseholdTask> UpdateAsync(HouseholdTask task)
    {
        await using var conn = db.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            """
            UPDATE household_tasks
            SET title = @title, description = @description, status = @status,
                due_date = @dueDate, assigned_to_user_id = @assignedTo, completed_at = @completedAt
            WHERE id = @id
            """, conn);
        cmd.Parameters.AddWithValue("id", task.Id);
        cmd.Parameters.AddWithValue("title", task.Title);
        cmd.Parameters.AddWithValue("description", (object?)task.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("status", (short)task.Status);
        cmd.Parameters.AddWithValue("dueDate", (object?)task.DueDate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("assignedTo", (object?)task.AssignedToUserId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("completedAt", (object?)task.CompletedAt ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync();
        return task;
    }

    public async System.Threading.Tasks.Task DeleteAsync(Guid id)
    {
        await using var conn = db.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand("DELETE FROM household_tasks WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task<HouseholdTask?> ReadTaskAsync(NpgsqlCommand cmd)
    {
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;
        return MapFromReader(reader);
    }

    private static HouseholdTask MapFromReader(NpgsqlDataReader reader)
    {
        return new HouseholdTask
        {
            Id = reader.Get<Guid>("id"),
            Title = reader.Get<string>("title"),
            Description = reader.GetNullableString("description"),
            TaskType = reader.GetEnum<HouseholdTaskType>("task_type"),
            Status = reader.GetEnum<HouseholdTaskStatus>("status"),
            DueDate = reader.GetNullable<DateTime>("due_date"),
            AssignedToUserId = reader.GetNullable<Guid>("assigned_to_user_id"),
            CreatedByUserId = reader.Get<Guid>("created_by_user_id"),
            TemplateId = reader.GetNullable<Guid>("template_id"),
            CreatedAt = reader.Get<DateTime>("created_at"),
            CompletedAt = reader.GetNullable<DateTime>("completed_at")
        };
    }
}
