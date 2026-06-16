import { useState } from 'react';
import type { RecurringTemplate } from '../../types';
import { useCreateTemplate, useUpdateTemplate, type TemplateInput } from '../../hooks/useRecurringTasks';
import { useUsers } from '../../hooks/useUsers';
import { RotationOrder } from './RotationOrder';

interface Props { template?: RecurringTemplate | null; onClose: () => void }

export function TemplateForm({ template, onClose }: Props) {
  const { data: members = [] } = useUsers();
  const create = useCreateTemplate();
  const update = useUpdateTemplate();
  const [title, setTitle] = useState(template?.title ?? '');
  const [description, setDescription] = useState(template?.description ?? '');
  const [frequencyDays, setFrequencyDays] = useState(template?.frequencyDays ?? 7);
  const [order, setOrder] = useState<string[]>(
    template?.rotationEntries?.slice().sort((a, b) => a.rotationOrder - b.rotationOrder).map((r) => r.userId) ?? [],
  );
  const [error, setError] = useState('');

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!title.trim()) { setError('Title is required'); return; }
    if (frequencyDays < 1) { setError('Frequency must be at least 1 day'); return; }
    if (order.length === 0) { setError('Select at least one member'); return; }
    const input: TemplateInput = { title: title.trim(), description: description.trim() || null, frequencyDays, userIdsInOrder: order };
    try {
      if (template) await update.mutateAsync({ id: template.id, input });
      else await create.mutateAsync(input);
      onClose();
    } catch {
      setError('Could not save the template');
    }
  }

  return (
    <div className="fixed inset-0 z-10 flex items-center justify-center bg-black/40 p-4">
      <form onSubmit={onSubmit} className="w-full max-w-md space-y-3 rounded-xl bg-white p-5">
        <h2 className="text-lg font-semibold">{template ? 'Edit template' : 'New template'}</h2>
        <div className="space-y-1">
          <label htmlFor="rtitle" className="text-sm font-medium">Title</label>
          <input id="rtitle" value={title} onChange={(e) => setTitle(e.target.value)} className="w-full rounded border px-3 py-2" />
        </div>
        <div className="space-y-1">
          <label htmlFor="rdesc" className="text-sm font-medium">Description</label>
          <textarea id="rdesc" value={description} onChange={(e) => setDescription(e.target.value)} className="w-full rounded border px-3 py-2" />
        </div>
        <div className="space-y-1">
          <label htmlFor="freq" className="text-sm font-medium">Frequency (days)</label>
          <input id="freq" type="number" min={1} value={frequencyDays} onChange={(e) => setFrequencyDays(Number(e.target.value))} className="w-full rounded border px-3 py-2" />
        </div>
        <div className="space-y-1">
          <span className="text-sm font-medium">Rotation order</span>
          <RotationOrder members={members} value={order} onChange={setOrder} />
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
