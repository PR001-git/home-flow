import { useState } from 'react';
import { TaskStatus, TaskType, type Task } from '../types';
import { useTasks, useCompleteTask, useDeleteTask, type TaskFilter } from '../hooks/useTasks';
import { useUsers } from '../hooks/useUsers';
import { TaskCard } from '../components/Tasks/TaskCard';
import { TaskForm } from '../components/Tasks/TaskForm';

export function TasksPage() {
  const [filter, setFilter] = useState<TaskFilter>({});
  const [editing, setEditing] = useState<Task | null>(null);
  const [creating, setCreating] = useState(false);
  const { data: tasks = [], isLoading } = useTasks(Object.keys(filter).length ? filter : undefined);
  const { data: members = [] } = useUsers();
  const complete = useCompleteTask();
  const remove = useDeleteTask();

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="hf-h1">Tasks</h1>
        <button onClick={() => setCreating(true)} className="hf-btn-primary">New task</button>
      </div>

      <div className="flex flex-wrap gap-2">
        <select className="hf-select"
          onChange={(e) => setFilter((f) => ({ ...f, assignedToUserId: e.target.value || undefined }))}>
          <option value="">All assignees</option>
          {members.map((m) => <option key={m.id} value={m.id}>{m.displayName}</option>)}
        </select>
        <select className="hf-select"
          onChange={(e) => setFilter((f) => ({ ...f, status: e.target.value === '' ? undefined : Number(e.target.value) as TaskStatus }))}>
          <option value="">All statuses</option>
          <option value={TaskStatus.Pending}>Pending</option>
          <option value={TaskStatus.InProgress}>In progress</option>
          <option value={TaskStatus.Completed}>Completed</option>
          <option value={TaskStatus.Overdue}>Overdue</option>
        </select>
        <select className="hf-select"
          onChange={(e) => setFilter((f) => ({ ...f, taskType: e.target.value === '' ? undefined : Number(e.target.value) as TaskType }))}>
          <option value="">All types</option>
          <option value={TaskType.OneOff}>One-off</option>
          <option value={TaskType.Recurring}>Recurring</option>
        </select>
      </div>

      {isLoading ? <p className="text-slate-500">Loading…</p> : (
        <div className="space-y-3">
          {tasks.map((t) => (
            <TaskCard key={t.id} task={t} members={members}
              onComplete={(id) => complete.mutate(id)}
              onDelete={(id) => remove.mutate(id)}
              onEdit={(task) => setEditing(task)} />
          ))}
          {tasks.length === 0 && <p className="text-slate-500">No tasks.</p>}
        </div>
      )}

      {(creating || editing) && (
        <TaskForm task={editing} onClose={() => { setCreating(false); setEditing(null); }} />
      )}
    </div>
  );
}
