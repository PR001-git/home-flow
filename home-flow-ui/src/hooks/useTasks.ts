import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../api/client';
import { TaskStatus, TaskType, type Task } from '../types';

export interface TaskFilter {
  assignedToUserId?: string;
  status?: TaskStatus;
  taskType?: TaskType;
}

function toQueryString(filter?: TaskFilter): string {
  if (!filter) return '';
  const params = new URLSearchParams();
  if (filter.assignedToUserId) params.set('assignedToUserId', filter.assignedToUserId);
  if (filter.status !== undefined) params.set('status', String(filter.status));
  if (filter.taskType !== undefined) params.set('taskType', String(filter.taskType));
  const s = params.toString();
  return s ? `?${s}` : '';
}

export function useTasks(filter?: TaskFilter) {
  return useQuery({
    queryKey: ['tasks', filter ?? null],
    queryFn: () => apiClient.get<Task[]>(`/api/tasks${toQueryString(filter)}`),
  });
}

export interface TaskInput {
  title: string;
  description: string | null;
  dueDate: string | null;
  assignedToUserId: string | null;
}

export function useCreateTask() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (input: TaskInput) => apiClient.post<Task>('/api/tasks', input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['tasks'] });
      qc.invalidateQueries({ queryKey: ['dashboard'] });
    },
  });
}

export function useUpdateTask() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: TaskInput }) =>
      apiClient.put<Task>(`/api/tasks/${id}`, input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['tasks'] });
      qc.invalidateQueries({ queryKey: ['dashboard'] });
    },
  });
}

export function useDeleteTask() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => apiClient.del(`/api/tasks/${id}`),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['tasks'] });
      qc.invalidateQueries({ queryKey: ['dashboard'] });
    },
  });
}

export function useCompleteTask() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => apiClient.patch<Task>(`/api/tasks/${id}/complete`),
    onMutate: async (id: string) => {
      await qc.cancelQueries({ queryKey: ['tasks'] });
      const snapshots = qc.getQueriesData<Task[]>({ queryKey: ['tasks'] });
      snapshots.forEach(([key, data]) => {
        if (!data) return;
        qc.setQueryData<Task[]>(
          key,
          data.map((t) => (t.id === id ? { ...t, status: TaskStatus.Completed } : t)),
        );
      });
      return { snapshots };
    },
    onError: (_err, _id, context) => {
      context?.snapshots.forEach(([key, data]) => qc.setQueryData(key, data));
    },
    onSettled: () => {
      qc.invalidateQueries({ queryKey: ['tasks'] });
      qc.invalidateQueries({ queryKey: ['dashboard'] });
    },
  });
}
