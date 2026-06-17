/* eslint-disable react-refresh/only-export-components */
import { createContext, useCallback, useEffect, useMemo, useState } from 'react';
import { apiClient, TOKEN_STORAGE_KEY } from '../api/client';
import type { AuthUser, User } from '../types';

interface AuthContextValue {
  user: AuthUser | null;
  isLoading: boolean;
  login: (username: string, password: string) => Promise<void>;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(
    () => !!localStorage.getItem(TOKEN_STORAGE_KEY),
  );

  const logout = useCallback(() => {
    localStorage.removeItem(TOKEN_STORAGE_KEY);
    setUser(null);
  }, []);

  const loadMe = useCallback(async (token: string) => {
    const me = await apiClient.get<User>('/api/auth/me');
    setUser({ userId: me.id, username: me.username, displayName: me.displayName, token });
  }, []);

  const login = useCallback(
    async (username: string, password: string) => {
      const res = await apiClient.post<AuthUser>('/api/auth/login', { username, password });
      localStorage.setItem(TOKEN_STORAGE_KEY, res.token);
      await loadMe(res.token);
    },
    [loadMe],
  );

  useEffect(() => {
    const token = localStorage.getItem(TOKEN_STORAGE_KEY);
    if (!token) return;
    // eslint-disable-next-line react-hooks/set-state-in-effect
    loadMe(token).catch(logout).finally(() => setIsLoading(false));
  }, [loadMe, logout]);

  useEffect(() => {
    const handler = () => logout();
    window.addEventListener('homeflow:unauthorized', handler);
    return () => window.removeEventListener('homeflow:unauthorized', handler);
  }, [logout]);

  const value = useMemo(
    () => ({ user, isLoading, login, logout }),
    [user, isLoading, login, logout],
  );
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
