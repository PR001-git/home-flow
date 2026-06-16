import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { RecurringPage } from './RecurringPage';

function wrap(ui: React.ReactNode) {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(<QueryClientProvider client={qc}>{ui}</QueryClientProvider>);
}

describe('RecurringPage', () => {
  beforeEach(() => { localStorage.clear(); vi.restoreAllMocks(); });

  it('lists templates', async () => {
    const template = { id: '1', title: 'Clean kitchen', description: null, frequencyDays: 7, currentAssigneeIndex: 0, lastGeneratedDate: null, createdAt: '2026-01-01T00:00:00Z', rotationEntries: [] };
    vi.stubGlobal('fetch', vi.fn().mockImplementation((url: string) => {
      const body = url.includes('/api/users') ? '[]' : JSON.stringify([template]);
      return Promise.resolve(new Response(body, { status: 200, headers: { 'Content-Type': 'application/json' } }));
    }));
    wrap(<RecurringPage />);
    await waitFor(() => expect(screen.getByText('Clean kitchen')).toBeInTheDocument());
  });
});
