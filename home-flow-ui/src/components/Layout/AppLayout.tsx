import { NavLink, Outlet } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';
import { useTheme } from '../../hooks/useTheme';
import { PageTransition } from './PageTransition';

const links = [
  { to: '/', label: 'Dashboard', end: true },
  { to: '/tasks', label: 'Tasks' },
  { to: '/recurring', label: 'Recurring' },
  { to: '/profile', label: 'Profile' },
];

export function AppLayout() {
  const { logout } = useAuth();
  const { theme, toggleTheme } = useTheme();
  return (
    <div className="min-h-screen md:flex">
      <aside className="hf-sidebar">
        <h1 className="hf-brand">HomeFlow</h1>
        {links.map((l) => (
          <NavLink key={l.to} to={l.to} end={l.end}
            className={({ isActive }) => `hf-nav ${isActive ? 'hf-nav-active' : ''}`}>
            {l.label}
          </NavLink>
        ))}
        <button onClick={toggleTheme} className="hf-sidebar-action mt-auto"
          title="Switch theme">
          Theme: {theme === 'mono' ? 'Mono' : 'Plain'}
        </button>
        <button onClick={logout} className="hf-sidebar-action mt-0">Log out</button>
      </aside>
      <main className="flex-1 pb-20 md:pb-10">
        <div className="hf-container">
          <PageTransition><Outlet /></PageTransition>
        </div>
      </main>
      <nav className="md:hidden fixed bottom-0 inset-x-0 border-t border-slate-200 bg-white flex">
        {links.map((l) => (
          <NavLink key={l.to} to={l.to} end={l.end}
            className={({ isActive }) =>
              `flex-1 py-3 text-center text-xs font-medium ${isActive ? 'text-slate-900' : 'text-slate-400'}`
            }>
            {l.label}
          </NavLink>
        ))}
      </nav>
    </div>
  );
}
