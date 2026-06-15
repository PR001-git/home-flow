INSERT INTO users (id, username, email, password_hash, display_name)
VALUES
    ('a1b2c3d4-0001-0000-0000-000000000001', 'pedro', 'pedro@homeflow.com',
     '$2a$11$gk/kS92M9ILryYbY51RWDune9xa/mg5NcciXyWfIaVVVhF9wMUVWi', 'Pedro'),
    ('a1b2c3d4-0002-0000-0000-000000000002', 'maria', 'maria@homeflow.com',
     '$2a$11$gk/kS92M9ILryYbY51RWDune9xa/mg5NcciXyWfIaVVVhF9wMUVWi', 'Maria'),
    ('a1b2c3d4-0003-0000-0000-000000000003', 'joao', 'joao@homeflow.com',
     '$2a$11$gk/kS92M9ILryYbY51RWDune9xa/mg5NcciXyWfIaVVVhF9wMUVWi', 'João'),
    ('a1b2c3d4-0004-0000-0000-000000000004', 'ana', 'ana@homeflow.com',
     '$2a$11$gk/kS92M9ILryYbY51RWDune9xa/mg5NcciXyWfIaVVVhF9wMUVWi', 'Ana')
ON CONFLICT (username) DO NOTHING;

INSERT INTO recurring_task_templates (id, title, description, frequency_days, current_assignee_index)
VALUES
    ('b1b2c3d4-0001-0000-0000-000000000001', 'Clean kitchen', 'Deep clean the kitchen including counters, stove, and floor', 7, 0),
    ('b1b2c3d4-0002-0000-0000-000000000002', 'Take out trash', 'Take all trash bags to the dumpster', 3, 0)
ON CONFLICT DO NOTHING;

INSERT INTO rotation_entries (template_id, user_id, rotation_order)
VALUES
    ('b1b2c3d4-0001-0000-0000-000000000001', 'a1b2c3d4-0001-0000-0000-000000000001', 0),
    ('b1b2c3d4-0001-0000-0000-000000000001', 'a1b2c3d4-0002-0000-0000-000000000002', 1),
    ('b1b2c3d4-0001-0000-0000-000000000001', 'a1b2c3d4-0003-0000-0000-000000000003', 2),
    ('b1b2c3d4-0001-0000-0000-000000000001', 'a1b2c3d4-0004-0000-0000-000000000004', 3)
ON CONFLICT DO NOTHING;

INSERT INTO rotation_entries (template_id, user_id, rotation_order)
VALUES
    ('b1b2c3d4-0002-0000-0000-000000000002', 'a1b2c3d4-0004-0000-0000-000000000004', 0),
    ('b1b2c3d4-0002-0000-0000-000000000002', 'a1b2c3d4-0003-0000-0000-000000000003', 1),
    ('b1b2c3d4-0002-0000-0000-000000000002', 'a1b2c3d4-0002-0000-0000-000000000002', 2),
    ('b1b2c3d4-0002-0000-0000-000000000002', 'a1b2c3d4-0001-0000-0000-000000000001', 3)
ON CONFLICT DO NOTHING;

INSERT INTO household_tasks (title, description, task_type, status, due_date, assigned_to_user_id, created_by_user_id)
VALUES
    ('Buy groceries', 'Weekly grocery shopping at the supermarket', 0, 0,
     NOW() + INTERVAL '1 day', 'a1b2c3d4-0001-0000-0000-000000000001', 'a1b2c3d4-0001-0000-0000-000000000001'),
    ('Fix bathroom faucet', 'The faucet in the main bathroom is leaking', 0, 1,
     NOW() + INTERVAL '3 days', 'a1b2c3d4-0003-0000-0000-000000000003', 'a1b2c3d4-0001-0000-0000-000000000001'),
    ('Pay electricity bill', 'Monthly electricity bill payment', 0, 2,
     NULL, 'a1b2c3d4-0002-0000-0000-000000000002', 'a1b2c3d4-0002-0000-0000-000000000002')
ON CONFLICT DO NOTHING;
