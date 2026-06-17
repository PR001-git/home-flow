import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor, act } from '@testing-library/react';
import { AuthProvider } from './AuthContext';
import { useAuth } from '../hooks/useAuth';

function Probe() {
  const { user, login } = useAuth();
  return (
    <div>
      <span data-testid="user">{user?.displayName ?? 'anon'}</span>
      <button onClick={() => login('pedro', 'Password123!')}>login</button>
    </div>
  );
}

describe('AuthContext', () => {
  beforeEach(() => { localStorage.clear(); vi.restoreAllMocks(); });

  it('logs in and exposes the user', async () => {
    const fetchMock = vi.fn()
      .mockResolvedValueOnce(new Response(JSON.stringify({ userId: '1', username: 'pedro', displayName: 'Pedro', token: 't' }), { status: 200, headers: { 'Content-Type': 'application/json' } }))
      .mockResolvedValueOnce(new Response(JSON.stringify({ id: '1', username: 'pedro', displayName: 'Pedro' }), { status: 200, headers: { 'Content-Type': 'application/json' } }));
    vi.stubGlobal('fetch', fetchMock);

    render(<AuthProvider><Probe /></AuthProvider>);
    expect(screen.getByTestId('user').textContent).toBe('anon');

    await act(async () => { screen.getByText('login').click(); });
    await waitFor(() => expect(screen.getByTestId('user').textContent).toBe('Pedro'));
  });
});
