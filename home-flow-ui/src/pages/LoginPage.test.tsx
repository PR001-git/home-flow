import { describe, it, expect, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { LoginPage } from './LoginPage';
import { AuthContext } from '../context/AuthContext';

function renderWithAuth(login: () => Promise<void>) {
  return render(
    <MemoryRouter>
      <AuthContext.Provider value={{ user: null, isLoading: false, login, logout: () => {} }}>
        <LoginPage />
      </AuthContext.Provider>
    </MemoryRouter>,
  );
}

describe('LoginPage', () => {
  it('calls login with entered credentials', async () => {
    const login = vi.fn().mockResolvedValue(undefined);
    renderWithAuth(login);
    await userEvent.type(screen.getByLabelText(/username/i), 'pedro');
    await userEvent.type(screen.getByLabelText(/password/i), 'Password123!');
    await userEvent.click(screen.getByRole('button', { name: /sign in/i }));
    await waitFor(() => expect(login).toHaveBeenCalledWith('pedro', 'Password123!'));
  });

  it('shows an error when login fails', async () => {
    const login = vi.fn().mockRejectedValue(new Error('bad'));
    renderWithAuth(login);
    await userEvent.type(screen.getByLabelText(/username/i), 'x');
    await userEvent.type(screen.getByLabelText(/password/i), 'y');
    await userEvent.click(screen.getByRole('button', { name: /sign in/i }));
    await waitFor(() => expect(screen.getByText(/invalid credentials/i)).toBeInTheDocument());
  });
});
