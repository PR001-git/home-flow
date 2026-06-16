export enum TaskStatus { Pending = 0, InProgress = 1, Completed = 2, Overdue = 3 }
export enum TaskType { OneOff = 0, Recurring = 1 }

export interface User { id: string; username: string; displayName: string }

export interface Task {
  id: string;
  title: string;
  description: string | null;
  taskType: TaskType;
  status: TaskStatus;
  dueDate: string | null;
  assignedToUserId: string | null;
  createdByUserId: string;
  templateId: string | null;
  createdAt: string;
  completedAt: string | null;
}

/** Matches RotationEntryResponse from the backend — no displayName in the DTO */
export interface RotationMember { userId: string; rotationOrder: number }

/** Matches RecurringTaskResponse — rotation field is rotationEntries */
export interface RecurringTemplate {
  id: string;
  title: string;
  description: string | null;
  frequencyDays: number;
  currentAssigneeIndex: number;
  lastGeneratedDate: string | null;
  createdAt: string;
  rotationEntries: RotationMember[];
}

export interface MemberDistribution { userId: string; displayName: string; activeCount: number }
export interface StatusTotals { pending: number; inProgress: number; completed: number; overdue: number }
export interface Dashboard {
  todaysTasks: Task[];
  overdueCount: number;
  totalsByStatus: StatusTotals;
  distribution: MemberDistribution[];
}

export interface AuthUser { userId: string; username: string; displayName: string; token: string }
