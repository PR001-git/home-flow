import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { TasksPage } from './TasksPage';
import { TaskStatus, TaskType } from '../types';

function wrap(ui: React.ReactNode) {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(<QueryClientProvider client={qc}>{ui}</QueryClientProvider>);
}

describe('TasksPage', () => {
  beforeEach(() => { localStorage.clear(); vi.restoreAllMocks(); });

  it('renders fetched tasks', async () => {
    const task = { id: '1', title: 'Buy milk', description: null, taskType: TaskType.OneOff, status: TaskStatus.Pending, dueDate: null, assignedToUserId: null, createdByUserId: 'u', templateId: null, createdAt: '', completedAt: null };
    vi.stubGlobal('fetch', vi.fn().mockImplementation((url: string) => {
      const body = url.includes('/api/users') ? '[]' : JSON.stringify([task]);
      return Promise.resolve(new Response(body, { status: 200, headers: { 'Content-Type': 'application/json' } }));
    }));

    wrap(<TasksPage />);
    await waitFor(() => expect(screen.getByText('Buy milk')).toBeInTheDocument());
  });
});
