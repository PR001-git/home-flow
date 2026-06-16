import { describe, it, expect, vi, beforeEach } from 'vitest';
import { apiClient, ApiError } from './client';

describe('apiClient', () => {
  beforeEach(() => {
    localStorage.clear();
    vi.restoreAllMocks();
  });

  it('attaches the bearer token from localStorage', async () => {
    localStorage.setItem('homeflow_token', 'abc');
    const fetchMock = vi.fn().mockResolvedValue(
      new Response(JSON.stringify({ ok: true }), { status: 200, headers: { 'Content-Type': 'application/json' } }),
    );
    vi.stubGlobal('fetch', fetchMock);

    await apiClient.get('/api/users');

    const headers = (fetchMock.mock.calls[0][1] as RequestInit).headers as Record<string, string>;
    expect(headers.Authorization).toBe('Bearer abc');
  });

  it('throws ApiError on non-2xx', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response('nope', { status: 400 })));
    await expect(apiClient.get('/api/users')).rejects.toBeInstanceOf(ApiError);
  });

  it('dispatches homeflow:unauthorized on 401', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response('Unauthorized', { status: 401 })));
    const dispatchSpy = vi.spyOn(window, 'dispatchEvent');

    await expect(apiClient.get('/api/users')).rejects.toBeInstanceOf(ApiError);

    expect(dispatchSpy).toHaveBeenCalledWith(expect.objectContaining({ type: 'homeflow:unauthorized' }));
  });

  it('returns undefined on 204', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(null, { status: 204 })));
    const result = await apiClient.del('/api/tasks/1');
    expect(result).toBeUndefined();
  });
});
