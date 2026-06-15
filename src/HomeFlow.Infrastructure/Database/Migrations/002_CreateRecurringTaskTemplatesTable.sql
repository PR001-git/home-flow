CREATE TABLE IF NOT EXISTS recurring_task_templates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title VARCHAR(200) NOT NULL,
    description TEXT,
    frequency_days INT NOT NULL,
    current_assignee_index INT NOT NULL DEFAULT 0,
    last_generated_date TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);
