'use client';

import React, { useState } from 'react';
import Link from 'next/link';
import { useRouter, usePathname } from 'next/navigation';
import {
  Activity,
  Cpu,
  LogOut,
  Stethoscope,
  Shield,
  HeartPulse,
  Moon,
  Sun,
  Menu,
  X,
  Award,
  Clock,
  FlaskConical,
  Pill,
  FolderOpen,
  Calendar,
  Users,
  Building2,
  FileText,
  ShieldCheck,
  Sparkles,
  UserCheck
} from 'lucide-react';
import { useLanguage, Language } from '@/context/LanguageContext';
import { useTheme } from '@/context/ThemeContext';
import { useAuth } from '@/context/AuthContext';

export default function Navbar() {
  const pathname = usePathname();
  const { language, setLanguage, t } = useLanguage();
  const { theme, toggleTheme } = useTheme();
  const { user, role, isAuthenticated, logout } = useAuth();
  const [showLangMenu, setShowLangMenu] = useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  const activeRole = role || (isAuthenticated ? 'Patient' : null);

  const languages: { code: Language; label: string; flag: string }[] = [
    { code: 'uz', label: "O'zbekcha", flag: "🇺🇿" },
    { code: 'ru', label: "Русский", flag: "🇷🇺" },
    { code: 'en', label: "English", flag: "🇬🇧" }
  ];

  const currentLangObj = languages.find(l => l.code === language) || languages[0];

  // Role-specific nav items for mobile menu & top links
  const patientItems = [
    { label: t.overview || 'Overview', href: '/dashboard', icon: Activity },
    { label: t.aiAssistant || 'AI Assistant', href: '/ai-assistant', icon: Cpu },
    { label: t.healthPassport || 'Health Passport', href: '/health-passport', icon: Award },
    { label: t.healthTimeline || 'Health Timeline', href: '/timeline', icon: Clock },
    { label: t.labResults || 'Lab Results', href: '/lab-results', icon: FlaskConical },
    { label: t.medications || 'Medications', href: '/medications', icon: Pill },
    { label: t.documents || 'Documents', href: '/documents', icon: FolderOpen },
    { label: t.appointments || 'Appointments', href: '/appointments', icon: Calendar },
    { label: t.familyHub || 'Family Hub', href: '/family', icon: Users },
  ];

  const doctorItems = [
    { label: t.doctorPortal || 'Doctor Portal', href: '/doctors/dashboard', icon: Stethoscope },
    { label: t.patientRoster || 'Patient Roster', href: '/doctors/patients', icon: Users },
    { label: t.doctorBrief || 'Doctor Copilot', href: '/doctors/copilot', icon: Cpu },
    { label: t.appointments || 'Appointments', href: '/doctors/appointments', icon: Calendar },
    { label: t.aiAssistant || 'Clinical AI', href: '/ai-assistant', icon: Sparkles },
  ];

  const adminItems = [
    { label: t.adminPortal || 'Admin Portal', href: '/admin/dashboard', icon: ShieldCheck },
    { label: t.userDirectory || 'User Directory', href: '/admin/users', icon: Users },
    { label: t.clinicManagement || 'Clinic Management', href: '/admin/clinics', icon: Building2 },
    { label: t.auditLogs || 'Audit Logs', href: '/admin/audit-logs', icon: FileText },
    { label: t.aiAssistant || 'AI Advisor', href: '/ai-assistant', icon: Cpu },
  ];

  let currentRoleItems = patientItems;
  if (activeRole === 'Doctor') currentRoleItems = doctorItems;
  else if (activeRole === 'SuperAdmin' || activeRole === 'ClinicAdmin') currentRoleItems = adminItems;

  return (
    <header className="sticky top-0 z-50 glass-panel border-b border-slate-200 dark:border-slate-800 px-4 md:px-6 py-3.5 flex items-center justify-between transition-colors">
      <div className="flex items-center gap-3">
        {/* Mobile menu toggle button */}
        <button
          onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
          className="md:hidden p-2 rounded-xl bg-slate-100 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 text-slate-700 dark:text-slate-300 hover:text-cyan-500 transition-colors"
          aria-label="Toggle Mobile Menu"
        >
          {mobileMenuOpen ? <X className="w-5 h-5" /> : <Menu className="w-5 h-5" />}
        </button>

        <Link href="/" className="flex items-center gap-3 group">
          <div className="w-10 h-10 rounded-xl bg-gradient-to-tr from-blue-600 via-teal-500 to-cyan-400 p-0.5 shadow-md shadow-blue-500/20 group-hover:scale-105 transition-transform">
            <div className="w-full h-full bg-slate-900 rounded-[10px] flex items-center justify-center relative overflow-hidden">
              <HeartPulse className="w-5 h-5 text-cyan-400 z-10" />
              <Cpu className="w-6 h-6 text-blue-500/30 absolute animate-pulse" />
            </div>
          </div>
          <div>
            <span className="text-xl font-bold tracking-tight bg-gradient-to-r from-blue-600 via-teal-600 to-cyan-500 bg-clip-text text-transparent">
              MEDAI
            </span>
            <span className="text-[10px] block text-slate-400 font-medium tracking-wider uppercase -mt-1">
              Healthcare Ecosystem
            </span>
          </div>
        </Link>
      </div>

      {/* Dynamic Desktop Header Navigation based on Role */}
      <nav className="hidden lg:flex items-center gap-5 text-sm font-medium text-slate-700 dark:text-slate-300">
        {activeRole === 'Doctor' ? (
          <>
            <Link
              href="/doctors/dashboard"
              className={`flex items-center gap-1.5 transition-colors ${
                pathname.startsWith('/doctors') ? 'text-teal-600 dark:text-teal-400 font-bold' : 'hover:text-teal-600 dark:hover:text-teal-400'
              }`}
            >
              <Stethoscope className="w-4 h-4 text-teal-500" /> {t.doctorPortal}
            </Link>
            <Link
              href="/ai-assistant"
              className={`flex items-center gap-1.5 transition-colors ${
                pathname === '/ai-assistant' ? 'text-cyan-600 dark:text-cyan-400 font-bold' : 'hover:text-cyan-600 dark:hover:text-cyan-400'
              }`}
            >
              <Cpu className="w-4 h-4 text-cyan-500" /> {t.aiAssistant}
            </Link>
          </>
        ) : activeRole === 'SuperAdmin' || activeRole === 'ClinicAdmin' ? (
          <>
            <Link
              href="/admin/dashboard"
              className={`flex items-center gap-1.5 transition-colors ${
                pathname.startsWith('/admin') ? 'text-indigo-600 dark:text-indigo-400 font-bold' : 'hover:text-indigo-600 dark:hover:text-indigo-400'
              }`}
            >
              <Shield className="w-4 h-4 text-indigo-500" /> {t.adminPortal}
            </Link>
            <Link
              href="/ai-assistant"
              className={`flex items-center gap-1.5 transition-colors ${
                pathname === '/ai-assistant' ? 'text-cyan-600 dark:text-cyan-400 font-bold' : 'hover:text-cyan-600 dark:hover:text-cyan-400'
              }`}
            >
              <Cpu className="w-4 h-4 text-cyan-500" /> {t.aiAssistant}
            </Link>
          </>
        ) : activeRole === 'Patient' ? (
          <>
            <Link
              href="/dashboard"
              className={`flex items-center gap-1.5 transition-colors ${
                pathname === '/dashboard' ? 'text-blue-600 dark:text-blue-400 font-bold' : 'hover:text-blue-600 dark:hover:text-blue-400'
              }`}
            >
              <Activity className="w-4 h-4 text-blue-500" /> {t.patientPortal}
            </Link>
            <Link
              href="/ai-assistant"
              className={`flex items-center gap-1.5 transition-colors ${
                pathname === '/ai-assistant' ? 'text-cyan-600 dark:text-cyan-400 font-bold' : 'hover:text-cyan-600 dark:hover:text-cyan-400'
              }`}
            >
              <Cpu className="w-4 h-4 text-cyan-500" /> {t.aiAssistant}
            </Link>
            <Link
              href="/health-passport"
              className={`flex items-center gap-1.5 transition-colors ${
                pathname === '/health-passport' ? 'text-blue-600 dark:text-blue-400 font-bold' : 'hover:text-blue-600 dark:hover:text-blue-400'
              }`}
            >
              <Award className="w-4 h-4 text-blue-500" /> {t.healthPassport}
            </Link>
            <Link
              href="/appointments"
              className={`flex items-center gap-1.5 transition-colors ${
                pathname === '/appointments' ? 'text-blue-600 dark:text-blue-400 font-bold' : 'hover:text-blue-600 dark:hover:text-blue-400'
              }`}
            >
              <Calendar className="w-4 h-4 text-blue-500" /> {t.appointments}
            </Link>
          </>
        ) : (
          <>
            <Link href="/dashboard" className="hover:text-blue-600 dark:hover:text-blue-400 transition-colors flex items-center gap-1.5">
              <Activity className="w-4 h-4 text-blue-500" /> {t.patientPortal}
            </Link>
            <Link href="/ai-assistant" className="hover:text-cyan-600 dark:hover:text-cyan-400 transition-colors flex items-center gap-1.5">
              <Cpu className="w-4 h-4 text-cyan-500" /> {t.aiAssistant}
            </Link>
            <Link href="/doctors/dashboard" className="hover:text-teal-600 dark:hover:text-teal-400 transition-colors flex items-center gap-1.5">
              <Stethoscope className="w-4 h-4 text-teal-500" /> {t.doctorPortal}
            </Link>
          </>
        )}
      </nav>

      <div className="flex items-center gap-2.5">
        {/* Dark / Light Theme Toggle Button */}
        <button
          onClick={toggleTheme}
          className="p-2 rounded-xl bg-slate-100 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 text-slate-700 dark:text-slate-300 hover:text-cyan-600 dark:hover:text-cyan-400 transition-colors"
          title={theme === 'dark' ? "Switch to Light Mode" : "Switch to Dark Mode"}
        >
          {theme === 'dark' ? <Sun className="w-4 h-4 text-amber-400" /> : <Moon className="w-4 h-4 text-slate-600" />}
        </button>

        {/* Language Switcher Dropdown */}
        <div className="relative">
          <button
            onClick={() => setShowLangMenu(!showLangMenu)}
            className="flex items-center gap-1.5 px-2.5 py-1.5 rounded-xl bg-slate-100 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 text-xs font-semibold text-slate-800 dark:text-slate-200 hover:border-cyan-500/50 transition-colors"
          >
            <span className="text-sm">{currentLangObj.flag}</span>
            <span className="hidden sm:inline">{currentLangObj.label}</span>
          </button>

          {showLangMenu && (
            <div className="absolute right-0 mt-2 w-36 py-1 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-xl shadow-2xl z-50">
              {languages.map((lang) => (
                <button
                  key={lang.code}
                  onClick={() => {
                    setLanguage(lang.code);
                    setShowLangMenu(false);
                  }}
                  className={`w-full text-left px-3 py-2 text-xs font-semibold flex items-center gap-2 hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors ${
                    language === lang.code ? 'text-cyan-600 dark:text-cyan-400 font-bold' : 'text-slate-700 dark:text-slate-300'
                  }`}
                >
                  <span className="text-sm">{lang.flag}</span>
                  <span>{lang.label}</span>
                </button>
              ))}
            </div>
          )}
        </div>

        {isAuthenticated && (user || role) ? (
          <div className="flex items-center gap-2">
            <div className="text-right hidden sm:block">
              <span className="text-xs font-semibold text-slate-800 dark:text-slate-200 block">
                {user?.firstName || 'User'} {user?.lastName || ''}
              </span>
              <span className={`text-[10px] px-2 py-0.5 rounded-full font-bold uppercase tracking-wider ${
                activeRole === 'Doctor'
                  ? 'bg-teal-100 dark:bg-teal-500/20 text-teal-700 dark:text-teal-400'
                  : activeRole === 'SuperAdmin' || activeRole === 'ClinicAdmin'
                  ? 'bg-indigo-100 dark:bg-indigo-500/20 text-indigo-700 dark:text-indigo-400'
                  : 'bg-blue-100 dark:bg-blue-500/20 text-blue-700 dark:text-blue-400'
              }`}>
                {activeRole}
              </span>
            </div>
            <button
              onClick={logout}
              className="p-2 text-slate-500 dark:text-slate-400 hover:text-red-600 dark:hover:text-red-400 hover:bg-red-50 dark:hover:bg-red-950/40 rounded-xl transition-colors"
              title={t.signOut || 'Chiqish'}
            >
              <LogOut className="w-5 h-5" />
            </button>
          </div>
        ) : (
          <div className="flex items-center gap-2">
            <Link
              href="/login"
              className="px-3 py-1.5 text-xs font-semibold text-slate-700 dark:text-slate-300 hover:text-cyan-600 dark:hover:text-cyan-400 transition-colors"
            >
              {t.signIn}
            </Link>
            <Link
              href="/register"
              className="px-3.5 py-1.5 text-xs font-bold text-white bg-blue-600 hover:bg-blue-500 rounded-xl shadow-md shadow-blue-500/20 transition-all hover:scale-105"
            >
              {t.getStarted}
            </Link>
          </div>
        )}
      </div>

      {/* Mobile Navigation Drawer - Strictly Filtered By User Role */}
      {mobileMenuOpen && (
        <div className="fixed inset-0 top-[65px] bg-slate-950/70 backdrop-blur-md z-40 md:hidden flex flex-col p-4 overflow-y-auto">
          <div className="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-3xl p-5 shadow-2xl space-y-3">
            <div className="flex items-center justify-between pb-2 border-b border-slate-200 dark:border-slate-800">
              <span className="text-xs font-bold text-slate-400 uppercase tracking-wider">
                {activeRole ? `${activeRole} Menyusi` : 'Menyu'}
              </span>
              {activeRole && (
                <span className="text-[10px] font-bold px-2 py-0.5 rounded-full bg-cyan-500/10 text-cyan-500">
                  {activeRole}
                </span>
              )}
            </div>
            <div className="grid grid-cols-1 gap-1">
              {currentRoleItems.map((item) => {
                const Icon = item.icon;
                const active = pathname === item.href;
                return (
                  <Link
                    key={item.href}
                    href={item.href}
                    onClick={() => setMobileMenuOpen(false)}
                    className={`flex items-center gap-3 px-3.5 py-2.5 rounded-xl text-sm font-medium transition-all ${
                      active
                        ? 'bg-blue-600 text-white font-bold shadow-md shadow-blue-500/20'
                        : 'text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800'
                    }`}
                  >
                    <Icon className={`w-4 h-4 ${active ? 'text-white' : 'text-blue-500'}`} />
                    {item.label}
                  </Link>
                );
              })}
            </div>

            {isAuthenticated && (
              <button
                onClick={() => {
                  setMobileMenuOpen(false);
                  logout();
                }}
                className="w-full mt-3 py-2.5 px-3 rounded-xl bg-red-50 dark:bg-red-950/40 text-red-600 dark:text-red-400 text-sm font-bold flex items-center justify-center gap-2 border border-red-200 dark:border-red-800"
              >
                <LogOut className="w-4 h-4" /> {t.signOut || 'Chiqish'}
              </button>
            )}
          </div>
        </div>
      )}
    </header>
  );
}
