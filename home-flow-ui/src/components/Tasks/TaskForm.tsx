import { useState } from 'react';
import type { Task } from '../../types';
import { useCreateTask, useUpdateTask, type TaskInput } from '../../hooks/useTasks';
import { useUsers } from '../../hooks/useUsers';

interface Props { task?: Task | null; onClose: () => void }

export function TaskForm({ task, onClose }: Props) {
  const { data: members = [] } = useUsers();
  const create = useCreateTask();
  const update = useUpdateTask();
  const [title, setTitle] = useState(task?.title ?? '');
  const [description, setDescription] = useState(task?.description ?? '');
  const [dueDate, setDueDate] = useState(task?.dueDate ? task.dueDate.slice(0, 10) : '');
  const [assignee, setAssignee] = useState(task?.assignedToUserId ?? '');
  const [error, setError] = useState('');

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!title.trim()) { setError('Title is required'); return; }
    if (title.length > 200) { setError('Title must be 200 characters or fewer'); return; }
    const input: TaskInput = {
      title: title.trim(),
      description: description.trim() || null,
      dueDate: dueDate ? new Date(dueDate).toISOString() : null,
      assignedToUserId: assignee || null,
    };
    try {
      if (task) await update.mutateAsync({ id: task.id, input });
      else await create.mutateAsync(input);
      onClose();
    } catch (err) {
      setError(err instanceof Error && err.message ? err.message : 'Could not save the task');
    }
  }

  return (
    <div className="fixed inset-0 z-10 flex items-center justify-center bg-black/40 p-4">
      <form onSubmit={onSubmit} className="w-full max-w-md space-y-3 rounded-xl bg-white p-5">
        <h2 className="text-lg font-semibold">{task ? 'Edit task' : 'New task'}</h2>
        <div className="space-y-1">
          <label htmlFor="title" className="text-sm font-medium">Title</label>
          <input id="title" value={title} onChange={(e) => setTitle(e.target.value)} className="w-full rounded border px-3 py-2" />
        </div>
        <div className="space-y-1">
          <label htmlFor="description" className="text-sm font-medium">Description</label>
          <textarea id="description" value={description} onChange={(e) => setDescription(e.target.value)} className="w-full rounded border px-3 py-2" />
        </div>
        <div className="space-y-1">
          <label htmlFor="dueDate" className="text-sm font-medium">Due date</label>
          <input id="dueDate" type="date" value={dueDate} onChange={(e) => setDueDate(e.target.value)} className="w-full rounded border px-3 py-2" />
        </div>
        <div className="space-y-1">
          <label htmlFor="assignee" className="text-sm font-medium">Assignee</label>
          <select id="assignee" value={assignee} onChange={(e) => setAssignee(e.target.value)} className="w-full rounded border px-3 py-2">
            <option value="">Unassigned</option>
            {members.map((m) => <option key={m.id} value={m.id}>{m.displayName}</option>)}
          </select>
        </div>
        {error && <p className="text-sm text-red-600">{error}</p>}
        <div className="flex justify-end gap-2">
          <button type="button" onClick={onClose} className="rounded px-3 py-2 text-sm">Cancel</button>
          <button type="submit" className="rounded bg-slate-900 px-3 py-2 text-sm text-white">Save</button>
        </div>
      </form>
    </div>
  );
}
