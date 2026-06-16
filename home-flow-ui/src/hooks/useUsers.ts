import { useQuery } from '@tanstack/react-query';
import { apiClient } from '../api/client';
import type { User } from '../types';

export function useUsers() {
  return useQuery({ queryKey: ['users'], queryFn: () => apiClient.get<User[]>('/api/users') });
}
