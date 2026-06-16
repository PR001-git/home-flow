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
              <div className="flex shrink-0 items-center gap-1">
                <button onClick={() => generate.mutate(t.id)} aria-label="Generate" title="Generate"
                  className="rounded-md p-1.5 text-slate-500 hover:bg-slate-100 hover:text-blue-700">
                  <svg className="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2}
                    strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                    <path d="m12 3-1.9 5.8a2 2 0 0 1-1.3 1.3L3 12l5.8 1.9a2 2 0 0 1 1.3 1.3L12 21l1.9-5.8a2 2 0 0 1 1.3-1.3L21 12l-5.8-1.9a2 2 0 0 1-1.3-1.3Z" />
                  </svg>
                </button>
                <button onClick={() => setEditing(t)} aria-label="Edit" title="Edit"
                  className="rounded-md p-1.5 text-slate-500 hover:bg-slate-100 hover:text-slate-700">
                  <svg className="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2}
                    strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                    <path d="M12 20h9" />
                    <path d="M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4Z" />
                  </svg>
                </button>
                <button onClick={() => remove.mutate(t.id)} aria-label="Delete" title="Delete"
                  className="rounded-md p-1.5 text-slate-400 hover:bg-slate-100 hover:text-red-600">
                  <svg className="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2}
                    strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                    <path d="M3 6h18" />
                    <path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
                    <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6" />
                    <path d="M10 11v6M14 11v6" />
                  </svg>
                </button>
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
