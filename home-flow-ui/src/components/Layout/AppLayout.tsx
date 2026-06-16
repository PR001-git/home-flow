import { NavLink, Outlet } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';
import { PageTransition } from './PageTransition';

const links = [
  { to: '/', label: 'Dashboard', end: true },
  { to: '/tasks', label: 'Tasks' },
  { to: '/recurring', label: 'Recurring' },
  { to: '/profile', label: 'Profile' },
];

export function AppLayout() {
  const { logout } = useAuth();
  return (
    <div className="min-h-screen md:flex">
      <aside className="hidden md:flex md:w-56 md:flex-col border-r p-4 gap-2">
        <h1 className="text-xl font-bold mb-4">HomeFlow</h1>
        {links.map((l) => (
          <NavLink key={l.to} to={l.to} end={l.end}
            className={({ isActive }) => `rounded px-3 py-2 ${isActive ? 'bg-slate-900 text-white' : 'hover:bg-slate-100'}`}>
            {l.label}
          </NavLink>
        ))}
        <button onClick={logout} className="mt-auto text-left px-3 py-2 text-sm text-slate-500">Log out</button>
      </aside>
      <main className="flex-1 p-4 pb-20 md:pb-4"><PageTransition><Outlet /></PageTransition></main>
      <nav className="md:hidden fixed bottom-0 inset-x-0 border-t bg-white flex">
        {links.map((l) => (
          <NavLink key={l.to} to={l.to} end={l.end}
            className={({ isActive }) => `flex-1 py-3 text-center text-xs ${isActive ? 'text-slate-900 font-semibold' : 'text-slate-400'}`}>
            {l.label}
          </NavLink>
        ))}
      </nav>
    </div>
  );
}
