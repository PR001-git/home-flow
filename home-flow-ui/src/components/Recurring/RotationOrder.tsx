import { Reorder } from 'framer-motion';
import type { User } from '../../types';

interface Props { members: User[]; value: string[]; onChange: (ids: string[]) => void }

export function RotationOrder({ members, value, onChange }: Props) {
  const toggle = (id: string) =>
    onChange(value.includes(id) ? value.filter((v) => v !== id) : [...value, id]);

  return (
    <div className="space-y-2">
      <div className="flex flex-wrap gap-2">
        {members.map((m) => (
          <button key={m.id} type="button" onClick={() => toggle(m.id)}
            className={`rounded-full border px-3 py-1 text-sm ${value.includes(m.id) ? 'bg-slate-900 text-white' : ''}`}>
            {m.displayName}
          </button>
        ))}
      </div>
      <Reorder.Group axis="y" values={value} onReorder={onChange} className="space-y-1">
        {value.map((id) => {
          const m = members.find((x) => x.id === id);
          return (
            <Reorder.Item key={id} value={id} className="cursor-grab rounded border bg-white px-3 py-2 text-sm">
              {m?.displayName ?? id}
            </Reorder.Item>
          );
        })}
      </Reorder.Group>
    </div>
  );
}
