import { NavLink } from 'react-router-dom';
import { FileText, Shield, LayoutDashboard } from 'lucide-react';

const NAV_ITEMS = [
  { to: '/',         label: 'Dashboard',  icon: LayoutDashboard },
  { to: '/sinistros',label: 'Sinistros',  icon: FileText },
  { to: '/apolices', label: 'Apólices',   icon: Shield },
];

export function Sidebar() {
  return (
    <aside className="w-60 shrink-0 h-full bg-surface-900 border-r border-surface-800 flex flex-col">
      {/* Logo */}
      <div className="px-6 py-5 border-b border-surface-800">
        <div className="flex items-center gap-2.5">
          <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-brand-500 to-brand-700 flex items-center justify-center shadow-lg shadow-brand-500/30">
            <Shield className="w-4 h-4 text-white" />
          </div>
          <div>
            <p className="text-sm font-bold text-slate-100 leading-tight">Segfy</p>
            <p className="text-[10px] text-slate-500 uppercase tracking-wider">Sinistros</p>
          </div>
        </div>
      </div>

      {/* Nav */}
      <nav className="flex-1 px-3 py-4 space-y-0.5">
        {NAV_ITEMS.map(({ to, label, icon: Icon }) => (
          <NavLink
            key={to}
            to={to}
            end={to === '/'}
            className={({ isActive }) =>
              `flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-all duration-150 ${
                isActive
                  ? 'bg-brand-600/20 text-brand-300 border border-brand-500/20'
                  : 'text-slate-400 hover:bg-surface-800 hover:text-slate-200'
              }`
            }
          >
            <Icon className="w-4 h-4 shrink-0" />
            {label}
          </NavLink>
        ))}
      </nav>

      {/* Footer */}
      <div className="px-4 py-4 border-t border-surface-800">
        <p className="text-[10px] text-slate-600 text-center">v1.0.0 — Segfy Desafio</p>
      </div>
    </aside>
  );
}
