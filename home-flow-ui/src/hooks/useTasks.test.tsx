import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useTasks } from './useTasks';
import { TaskStatus, TaskType } from '../types';

function wrapper() {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={qc}>{children}</QueryClientProvider>
  );
}

describe('useTasks', () => {
  beforeEach(() => { localStorage.clear(); vi.restoreAllMocks(); });

  it('fetches tasks', async () => {
    const task = { id: '1', title: 'T', description: null, taskType: TaskType.OneOff, status: TaskStatus.Pending, dueDate: null, assignedToUserId: null, createdByUserId: 'u', templateId: null, createdAt: '', completedAt: null };
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(JSON.stringify([task]), { status: 200, headers: { 'Content-Type': 'application/json' } })));

    const { result } = renderHook(() => useTasks(), { wrapper: wrapper() });
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toHaveLength(1);
  });
});
