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
  Award,
  Sparkles,
  ShieldAlert,
  UserCheck
} from 'lucide-react';
import { useLanguage } from '@/context/LanguageContext';
import { useAuth } from '@/context/AuthContext';

export default function Sidebar() {
  const pathname = usePathname();
  const { t } = useLanguage();
  const { user, role, isAuthenticated } = useAuth();

  // Determine current active role (default to Patient if role is not set)
  const activeRole = role || 'Patient';

  // Navigation configurations for each specific role
  const patientNav = [
    { label: t.overview || 'Overview', href: '/dashboard', icon: LayoutDashboard },
    { label: t.aiAssistant || 'AI Assistant', href: '/ai-assistant', icon: Bot, badge: 'AI' },
    { label: t.healthPassport || 'Health Passport', href: '/health-passport', icon: Award },
    { label: t.healthTimeline || 'Health Timeline', href: '/timeline', icon: Clock },
    { label: t.labResults || 'Lab Results', href: '/lab-results', icon: FlaskConical },
    { label: t.medications || 'Medications', href: '/medications', icon: Pill },
    { label: t.documents || 'Documents', href: '/documents', icon: FolderOpen },
    { label: t.appointments || 'Appointments', href: '/appointments', icon: Calendar },
    { label: t.familyHub || 'Family Hub', href: '/family', icon: Users },
  ];

  const doctorNav = [
    { label: t.doctorPortal || 'Doctor Portal', href: '/doctors/dashboard', icon: Stethoscope },
    { label: t.patientRoster || 'Patient Roster', href: '/doctors/patients', icon: Users },
    { label: t.doctorBrief || 'Doctor Copilot', href: '/doctors/copilot', icon: Bot, badge: 'AI' },
    { label: t.appointments || 'Appointments', href: '/doctors/appointments', icon: Calendar },
    { label: t.aiAssistant || 'Clinical AI', href: '/ai-assistant', icon: Sparkles },
  ];

  const adminNav = [
    { label: t.adminPortal || 'Admin Portal', href: '/admin/dashboard', icon: ShieldCheck },
    { label: t.userDirectory || 'User Directory', href: '/admin/users', icon: Users },
    { label: t.clinicManagement || 'Clinic Management', href: '/admin/clinics', icon: Building2 },
    { label: t.auditLogs || 'Audit Logs', href: '/admin/audit-logs', icon: FileText },
    { label: t.aiAssistant || 'AI Advisor', href: '/ai-assistant', icon: Bot },
  ];

  // Pick ONLY the navigation list corresponding to the logged in role
  let navItems = patientNav;
  let portalTitle = t.patientPortal || 'Bemor Portali';
  let roleBadgeColor = 'bg-blue-100 dark:bg-blue-950/60 text-blue-700 dark:text-blue-400 border-blue-200 dark:border-blue-800';
  let activeItemBg = 'bg-blue-600 text-white shadow-md shadow-blue-500/25';
  let activeIconColor = 'text-white';

  if (activeRole === 'Doctor') {
    navItems = doctorNav;
    portalTitle = t.doctorPortal || 'Shifokor Portali';
    roleBadgeColor = 'bg-teal-100 dark:bg-teal-950/60 text-teal-700 dark:text-teal-400 border-teal-200 dark:border-teal-800';
    activeItemBg = 'bg-teal-600 text-white shadow-md shadow-teal-500/25';
  } else if (activeRole === 'SuperAdmin' || activeRole === 'ClinicAdmin') {
    navItems = adminNav;
    portalTitle = t.adminPortal || 'Admin Paneli';
    roleBadgeColor = 'bg-indigo-100 dark:bg-indigo-950/60 text-indigo-700 dark:text-indigo-400 border-indigo-200 dark:border-indigo-800';
    activeItemBg = 'bg-slate-800 dark:bg-indigo-600 text-white shadow-md shadow-indigo-500/25';
  }

  return (
    <aside className="w-64 glass-panel min-h-[calc(100vh-65px)] border-r border-slate-200 dark:border-slate-800 p-4 flex flex-col justify-between hidden md:flex shrink-0">
      <div className="space-y-4">
        {/* Role Header Banner */}
        <div className={`p-3 rounded-2xl border ${roleBadgeColor} flex items-center justify-between`}>
          <div>
            <span className="text-[10px] font-bold uppercase tracking-wider block opacity-75">
              Faol Sessiya
            </span>
            <span className="text-xs font-extrabold flex items-center gap-1.5 mt-0.5">
              {activeRole === 'Doctor' ? (
                <Stethoscope className="w-3.5 h-3.5" />
              ) : activeRole === 'SuperAdmin' || activeRole === 'ClinicAdmin' ? (
                <ShieldCheck className="w-3.5 h-3.5" />
              ) : (
                <UserCheck className="w-3.5 h-3.5" />
              )}
              {portalTitle}
            </span>
          </div>
          <span className="text-[10px] px-2 py-0.5 rounded-full font-bold uppercase tracking-wider bg-white/50 dark:bg-black/30">
            {activeRole}
          </span>
        </div>

        {/* Role-Specific Navigation Menu */}
        <div>
          <span className="text-[11px] font-bold text-slate-400 dark:text-slate-500 uppercase tracking-wider px-3 block mb-2">
            Asosiy Bo'limlar
          </span>
          <nav className="space-y-1">
            {navItems.map((item) => {
              const Icon = item.icon;
              const active = pathname === item.href;
              return (
                <Link
                  key={item.href}
                  href={item.href}
                  className={`flex items-center justify-between px-3 py-2.5 rounded-xl text-sm font-medium transition-all ${
                    active
                      ? `${activeItemBg} font-semibold`
                      : 'text-slate-600 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-900 hover:text-slate-900 dark:hover:text-white'
                  }`}
                >
                  <div className="flex items-center gap-3">
                    <Icon className={`w-4 h-4 ${active ? activeIconColor : 'text-slate-400 dark:text-slate-400'}`} />
                    <span>{item.label}</span>
                  </div>
                  {item.badge && (
                    <span className={`text-[9px] px-1.5 py-0.5 rounded-md font-bold uppercase tracking-wider ${
                      active ? 'bg-white/20 text-white' : 'bg-cyan-500/10 text-cyan-500 border border-cyan-500/20'
                    }`}>
                      {item.badge}
                    </span>
                  )}
                </Link>
              );
            })}
          </nav>
        </div>
      </div>

      {/* Safety / AI Helper Card */}
      <div className="p-3 bg-slate-100 dark:bg-slate-900 rounded-2xl border border-slate-200 dark:border-slate-800 text-xs text-slate-600 dark:text-slate-300 mt-6">
        <div className="flex items-center gap-2 font-bold text-cyan-500 dark:text-cyan-400 mb-1">
          <Bot className="w-4 h-4 text-cyan-500 shrink-0" /> {t.aiSafetyNoticeTitle || 'AI Xavfsizlik Eslatmasi'}
        </div>
        <p className="text-[11px] leading-relaxed opacity-90">
          {t.aiSafetyNoticeDesc || "MEDAI intellektual yordamchi hisoblanadi. AI javoblari shifokor ko'rigi o'rnini bosa olmaydi."}
        </p>
      </div>
    </aside>
  );
}
