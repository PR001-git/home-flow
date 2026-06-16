import { useState } from 'react';
import type { RecurringTemplate } from '../types';
import { useRecurringTasks, useDeleteTemplate, useGenerateTask } from '../hooks/useRecurringTasks';
import { TemplateForm } from '../components/Recurring/TemplateForm';

export function RecurringPage() {
  const { data: templates = [], isLoading } = useRecurringTasks();
  const remove = useDeleteTemplate();
  const generate = useGenerateTask();
  const [editing, setEditing] = useState<RecurringTemplate | null>(null);
  const [creating, setCreating] = useState(false);

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Recurring tasks</h1>
        <button onClick={() => setCreating(true)} className="rounded bg-slate-900 px-3 py-1.5 text-sm text-white">New template</button>
      </div>

      {isLoading ? <p>Loading…</p> : (
        <div className="space-y-2">
          {templates.map((t) => (
            <div key={t.id} className="flex items-center justify-between rounded-lg border p-3">
              <div>
                <div className="font-medium">{t.title}</div>
                <div className="text-xs text-slate-500">Every {t.frequencyDays} days · {t.rotationEntries.length} in rotation</div>
              </div>
              <div className="flex gap-2 text-sm">
                <button onClick={() => generate.mutate(t.id)} className="text-blue-700">Generate</button>
                <button onClick={() => setEditing(t)} className="text-slate-600">Edit</button>
                <button onClick={() => remove.mutate(t.id)} className="text-red-600">Delete</button>
              </div>
            </div>
          ))}
          {templates.length === 0 && <p className="text-slate-500">No templates.</p>}
        </div>
      )}

      {(creating || editing) && (
        <TemplateForm template={editing} onClose={() => { setCreating(false); setEditing(null); }} />
      )}
    </div>
  );
}
