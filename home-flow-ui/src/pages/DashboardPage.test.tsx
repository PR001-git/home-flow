import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { DashboardPage } from './DashboardPage';

function wrap(ui: React.ReactNode) {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(<QueryClientProvider client={qc}>{ui}</QueryClientProvider>);
}

describe('DashboardPage', () => {
  beforeEach(() => { localStorage.clear(); vi.restoreAllMocks(); });

  it('shows the overdue count', async () => {
    const dashboard = { todaysTasks: [], overdueCount: 3, totalsByStatus: { pending: 1, inProgress: 0, completed: 0, overdue: 3 }, distribution: [{ userId: 'u', displayName: 'Pedro', activeCount: 2 }] };
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(JSON.stringify(dashboard), { status: 200, headers: { 'Content-Type': 'application/json' } })));
    wrap(<DashboardPage />);
    await waitFor(() => expect(screen.getByText('3')).toBeInTheDocument());
    expect(screen.getByText('Pedro')).toBeInTheDocument();
  });
});
