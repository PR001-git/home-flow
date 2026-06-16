import { useAuth } from '../hooks/useAuth';

export function ProfilePage() {
  const { user, logout } = useAuth();
  if (!user) return null;
  return (
    <div className="max-w-sm space-y-4">
      <h1 className="text-2xl font-bold">Profile</h1>
      <div className="rounded-xl border p-4">
        <div className="text-lg font-semibold">{user.displayName}</div>
        <div className="text-sm text-slate-500">@{user.username}</div>
      </div>
      <button onClick={logout} className="rounded border px-3 py-2 text-sm">Log out</button>
    </div>
  );
}
