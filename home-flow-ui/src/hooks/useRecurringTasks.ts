import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../api/client';
import type { RecurringTemplate } from '../types';

export interface TemplateInput {
  title: string;
  description: string | null;
  frequencyDays: number;
  userIdsInOrder: string[];
}

export function useRecurringTasks() {
  return useQuery({ queryKey: ['recurring'], queryFn: () => apiClient.get<RecurringTemplate[]>('/api/recurring-tasks') });
}

export function useCreateTemplate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (input: TemplateInput) => apiClient.post<RecurringTemplate>('/api/recurring-tasks', input),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['recurring'] }),
  });
}

export function useUpdateTemplate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: TemplateInput }) => apiClient.put<RecurringTemplate>(`/api/recurring-tasks/${id}`, input),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['recurring'] }),
  });
}

export function useDeleteTemplate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => apiClient.del(`/api/recurring-tasks/${id}`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['recurring'] }),
  });
}

export function useGenerateTask() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => apiClient.post(`/api/recurring-tasks/${id}/generate`),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['recurring'] });
      qc.invalidateQueries({ queryKey: ['tasks'] });
      qc.invalidateQueries({ queryKey: ['dashboard'] });
    },
  });
}
