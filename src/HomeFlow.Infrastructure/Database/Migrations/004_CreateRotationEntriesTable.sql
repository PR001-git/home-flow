CREATE TABLE IF NOT EXISTS rotation_entries (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    template_id UUID NOT NULL REFERENCES recurring_task_templates(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(id),
    rotation_order INT NOT NULL
);
