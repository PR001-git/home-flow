import { motion } from 'framer-motion';
import { TaskStatus, type Task, type User } from '../../types';
import { StatusBadge } from './StatusBadge';

interface Props {
  task: Task;
  members: User[];
  onComplete: (id: string) => void;
  onDelete: (id: string) => void;
  onEdit: (task: Task) => void;
}

export function TaskCard({ task, members, onComplete, onDelete, onEdit }: Props) {
  const assignee = members.find((m) => m.id === task.assignedToUserId);
  return (
    <motion.div layout initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
      className="hf-card">
      <div className="min-w-0">
        <div className="flex items-center gap-2">
          <span className="font-semibold text-slate-900 truncate">{task.title}</span>
          <StatusBadge status={task.status} />
        </div>
        <div className="mt-1 text-xs text-slate-500">
          {assignee ? assignee.displayName : 'Unassigned'}
          {task.dueDate ? ` · due ${new Date(task.dueDate).toLocaleDateString()}` : ''}
        </div>
      </div>
      <div className="flex shrink-0 items-center gap-1">
        {task.status !== TaskStatus.Completed && (
          <button onClick={() => onComplete(task.id)} aria-label="Mark done" title="Mark done"
            className="rounded-md p-1.5 text-slate-500 hover:bg-slate-100 hover:text-emerald-600">
            <svg className="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2}
              strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
              <path d="M20 6 9 17l-5-5" />
            </svg>
          </button>
        )}
        <button onClick={() => onEdit(task)} aria-label="Edit" title="Edit"
          className="rounded-md p-1.5 text-slate-500 hover:bg-slate-100 hover:text-slate-700">
          <svg className="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2}
            strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
            <path d="M12 20h9" />
            <path d="M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4Z" />
          </svg>
        </button>
        <button onClick={() => onDelete(task.id)} aria-label="Delete" title="Delete"
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
    </motion.div>
  );
}
