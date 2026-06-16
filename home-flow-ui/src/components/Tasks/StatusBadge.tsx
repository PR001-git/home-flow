import { TaskStatus } from '../../types';

const modifiers: Record<TaskStatus, string> = {
  [TaskStatus.Pending]: 'hf-badge--pending',
  [TaskStatus.InProgress]: 'hf-badge--inprogress',
  [TaskStatus.Completed]: 'hf-badge--completed',
  [TaskStatus.Overdue]: 'hf-badge--overdue',
};
const labels: Record<TaskStatus, string> = {
  [TaskStatus.Pending]: 'Pending',
  [TaskStatus.InProgress]: 'In progress',
  [TaskStatus.Completed]: 'Completed',
  [TaskStatus.Overdue]: 'Overdue',
};

export function StatusBadge({ status }: { status: TaskStatus }) {
  return <span className={`hf-badge ${modifiers[status]}`}>{labels[status]}</span>;
}
