CREATE TABLE IF NOT EXISTS household_tasks (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title VARCHAR(200) NOT NULL,
    description TEXT,
    task_type SMALLINT NOT NULL,
    status SMALLINT NOT NULL DEFAULT 0,
    due_date TIMESTAMP,
    assigned_to_user_id UUID REFERENCES users(id),
    created_by_user_id UUID NOT NULL REFERENCES users(id),
    template_id UUID REFERENCES recurring_task_templates(id),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    completed_at TIMESTAMP
);
