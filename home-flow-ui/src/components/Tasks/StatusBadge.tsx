import { TaskStatus } from '../../types';

const styles: Record<TaskStatus, string> = {
  [TaskStatus.Pending]: 'bg-slate-100 text-slate-700',
  [TaskStatus.InProgress]: 'bg-blue-100 text-blue-700',
  [TaskStatus.Completed]: 'bg-green-100 text-green-700',
  [TaskStatus.Overdue]: 'bg-red-100 text-red-700',
};
const labels: Record<TaskStatus, string> = {
  [TaskStatus.Pending]: 'Pending',
  [TaskStatus.InProgress]: 'In progress',
  [TaskStatus.Completed]: 'Completed',
  [TaskStatus.Overdue]: 'Overdue',
};

export function StatusBadge({ status }: { status: TaskStatus }) {
  return <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${styles[status]}`}>{labels[status]}</span>;
}
