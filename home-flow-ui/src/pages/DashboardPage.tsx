import { useDashboard } from '../hooks/useDashboard';
import { StatCard } from '../components/Dashboard/StatCard';
import { MemberDistributionCard } from '../components/Dashboard/MemberDistribution';

export function DashboardPage() {
  const { data, isLoading } = useDashboard();
  if (isLoading || !data) return <p>Loading…</p>;

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold">Dashboard</h1>
      <div className="grid grid-cols-2 gap-3 md:grid-cols-4">
        <StatCard label="Overdue" value={data.overdueCount} />
        <StatCard label="Due today" value={data.todaysTasks.length} />
        <StatCard label="Pending" value={data.totalsByStatus.pending} />
        <StatCard label="Completed" value={data.totalsByStatus.completed} />
      </div>
      <div className="grid gap-4 md:grid-cols-2">
        <MemberDistributionCard items={data.distribution} />
        <div className="rounded-xl border p-4">
          <h2 className="font-semibold mb-2">Due today</h2>
          {data.todaysTasks.length === 0 ? <p className="text-sm text-slate-500">Nothing due today.</p> : (
            <ul className="space-y-1 text-sm">
              {data.todaysTasks.map((t) => <li key={t.id}>{t.title}</li>)}
            </ul>
          )}
        </div>
      </div>
    </div>
  );
}
