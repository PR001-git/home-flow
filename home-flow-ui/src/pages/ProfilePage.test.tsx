import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { ProfilePage } from './ProfilePage';
import { AuthContext } from '../context/AuthContext';

describe('ProfilePage', () => {
  it('shows the current user', () => {
    render(
      <AuthContext.Provider value={{ user: { userId: '1', username: 'pedro', displayName: 'Pedro', token: 't' }, isLoading: false, login: vi.fn(), logout: vi.fn() }}>
        <ProfilePage />
      </AuthContext.Provider>,
    );
    expect(screen.getByText('Pedro')).toBeInTheDocument();
    expect(screen.getByText(/pedro/)).toBeInTheDocument();
  });
});
