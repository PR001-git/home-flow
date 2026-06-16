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
      className="flex items-center justify-between rounded-lg border p-3">
      <div className="min-w-0">
        <div className="flex items-center gap-2">
          <span className="font-medium truncate">{task.title}</span>
          <StatusBadge status={task.status} />
        </div>
        <div className="text-xs text-slate-500">
          {assignee ? assignee.displayName : 'Unassigned'}
          {task.dueDate ? ` · due ${new Date(task.dueDate).toLocaleDateString()}` : ''}
        </div>
      </div>
      <div className="flex gap-2 text-sm">
        {task.status !== TaskStatus.Completed && (
          <button onClick={() => onComplete(task.id)} className="text-green-700">Done</button>
        )}
        <button onClick={() => onEdit(task)} className="text-slate-600">Edit</button>
        <button onClick={() => onDelete(task.id)} className="text-red-600">Delete</button>
      </div>
    </motion.div>
  );
}
