'use client';

import React from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import {
  LayoutDashboard,
  Bot,
  FileText,
  Clock,
  FolderOpen,
  FlaskConical,
  Pill,
  Calendar,
  Users,
  Stethoscope,
  Building2,
  ShieldCheck,
  Award
} from 'lucide-react';
import { useLanguage } from '@/context/LanguageContext';

export default function Sidebar() {
  const pathname = usePathname();
  const { t } = useLanguage();

  const patientNav = [
    { label: 'Overview', href: '/dashboard', icon: LayoutDashboard },
    { label: t.aiAssistant, href: '/ai-assistant', icon: Bot },
    { label: t.healthPassport, href: '/health-passport', icon: Award },
    { label: t.healthTimeline, href: '/timeline', icon: Clock },
    { label: t.labResults, href: '/lab-results', icon: FlaskConical },
    { label: t.medications, href: '/medications', icon: Pill },
    { label: t.documents, href: '/documents', icon: FolderOpen },
    { label: t.appointments, href: '/appointments', icon: Calendar },
    { label: t.familyHub, href: '/family', icon: Users },
  ];

  const doctorNav = [
    { label: t.doctorPortal, href: '/doctors/dashboard', icon: Stethoscope },
    { label: 'Patient Roster', href: '/doctors/patients', icon: Users },
    { label: t.doctorBrief, href: '/doctors/copilot', icon: Bot },
    { label: t.appointments, href: '/doctors/appointments', icon: Calendar },
  ];

  const adminNav = [
    { label: t.adminPortal, href: '/admin/dashboard', icon: ShieldCheck },
    { label: 'User Directory', href: '/admin/users', icon: Users },
    { label: 'Clinic Management', href: '/admin/clinics', icon: Building2 },
    { label: 'Audit Logs', href: '/admin/audit-logs', icon: FileText },
  ];

  return (
    <aside className="w-64 glass-panel min-h-[calc(100vh-65px)] border-r border-slate-200 dark:border-slate-800 p-4 flex flex-col justify-between hidden md:flex">
      <div className="space-y-6">
        <div>
          <span className="text-[11px] font-bold text-slate-400 uppercase tracking-wider px-3 block mb-2">
            {t.patientPortal}
          </span>
          <nav className="space-y-1">
            {patientNav.map((item) => {
              const Icon = item.icon;
              const active = pathname === item.href;
              return (
                <Link
                  key={item.href}
                  href={item.href}
                  className={`flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition-all ${
                    active
                      ? 'bg-blue-600 text-white shadow-md shadow-blue-500/25 font-semibold'
                      : 'text-slate-600 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-900 hover:text-slate-900 dark:hover:text-white'
                  }`}
                >
                  <Icon className={`w-4 h-4 ${active ? 'text-white' : 'text-slate-400 dark:text-slate-400'}`} />
                  {item.label}
                </Link>
              );
            })}
          </nav>
        </div>

        <div>
          <span className="text-[11px] font-bold text-slate-400 uppercase tracking-wider px-3 block mb-2">
            {t.doctorPortal}
          </span>
          <nav className="space-y-1">
            {doctorNav.map((item) => {
              const Icon = item.icon;
              const active = pathname === item.href;
              return (
                <Link
                  key={item.href}
                  href={item.href}
                  className={`flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition-all ${
                    active
                      ? 'bg-teal-600 text-white shadow-md shadow-teal-500/25 font-semibold'
                      : 'text-slate-300 hover:bg-slate-900 hover:text-white'
                  }`}
                >
                  <Icon className={`w-4 h-4 ${active ? 'text-white' : 'text-slate-400'}`} />
                  {item.label}
                </Link>
              );
            })}
          </nav>
        </div>

        <div>
          <span className="text-[11px] font-bold text-slate-400 uppercase tracking-wider px-3 block mb-2">
            {t.adminPortal}
          </span>
          <nav className="space-y-1">
            {adminNav.map((item) => {
              const Icon = item.icon;
              const active = pathname === item.href;
              return (
                <Link
                  key={item.href}
                  href={item.href}
                  className={`flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition-all ${
                    active
                      ? 'bg-slate-800 text-white shadow-md shadow-slate-900/25 font-semibold'
                      : 'text-slate-300 hover:bg-slate-900 hover:text-white'
                  }`}
                >
                  <Icon className={`w-4 h-4 ${active ? 'text-white' : 'text-slate-400'}`} />
                  {item.label}
                </Link>
              );
            })}
          </nav>
        </div>
      </div>

      <div className="p-3 bg-slate-100 dark:bg-slate-900 rounded-2xl border border-slate-200 dark:border-slate-800 text-xs text-slate-600 dark:text-slate-300">
        <div className="flex items-center gap-2 font-bold text-cyan-400 mb-1">
          <Bot className="w-4 h-4 text-cyan-400" /> {t.aiSafetyNoticeTitle}
        </div>
        {t.aiSafetyNoticeDesc}
      </div>
    </aside>
  );
}
