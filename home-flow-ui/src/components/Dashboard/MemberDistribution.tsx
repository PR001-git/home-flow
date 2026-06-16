import type { MemberDistribution } from '../../types';

export function MemberDistributionCard({ items }: { items: MemberDistribution[] }) {
  const max = Math.max(1, ...items.map((i) => i.activeCount));
  return (
    <div className="rounded-xl border p-4 space-y-2">
      <h2 className="font-semibold">Active tasks per member</h2>
      {items.map((i) => (
        <div key={i.userId} className="space-y-1">
          <div className="flex justify-between text-sm"><span>{i.displayName}</span><span>{i.activeCount}</span></div>
          <div className="h-2 rounded bg-slate-100">
            <div className="h-2 rounded bg-slate-900" style={{ width: `${(i.activeCount / max) * 100}%` }} />
          </div>
        </div>
      ))}
    </div>
  );
}
