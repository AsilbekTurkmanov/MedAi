'use client';

import React, { useEffect, useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { Activity, Cpu, User as UserIcon, LogOut, Stethoscope, Shield, HeartPulse, Moon, Sun } from 'lucide-react';
import { authService } from '@/services/allServices';
import { UserMe } from '@/types';
import { useLanguage, Language } from '@/context/LanguageContext';
import { useTheme } from '@/context/ThemeContext';

export default function Navbar() {
  const router = useRouter();
  const [user, setUser] = useState<UserMe | null>(null);
  const { language, setLanguage, t } = useLanguage();
  const { theme, toggleTheme } = useTheme();
  const [showLangMenu, setShowLangMenu] = useState(false);

  useEffect(() => {
    authService.getMe()
      .then(res => {
        if (res.success) setUser(res.data);
      })
      .catch(() => setUser(null));
  }, []);

  const handleLogout = async () => {
    await authService.logout();
    setUser(null);
    router.push('/login');
  };

  const languages: { code: Language; label: string; flag: string }[] = [
    { code: 'uz', label: "O'zbekcha", flag: "🇺🇿" },
    { code: 'ru', label: "Русский", flag: "🇷🇺" },
    { code: 'en', label: "English", flag: "🇬🇧" }
  ];

  const currentLangObj = languages.find(l => l.code === language) || languages[0];

  return (
    <header className="sticky top-0 z-50 glass-panel border-b border-slate-200 dark:border-slate-800 px-6 py-3.5 flex items-center justify-between transition-colors">
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

      <nav className="hidden md:flex items-center gap-6 text-sm font-medium text-slate-700 dark:text-slate-300">
        <Link href="/dashboard" className="hover:text-blue-600 dark:hover:text-blue-400 transition-colors flex items-center gap-1.5">
          <Activity className="w-4 h-4 text-blue-500" /> {t.patientPortal}
        </Link>
        <Link href="/ai-assistant" className="hover:text-cyan-600 dark:hover:text-cyan-400 transition-colors flex items-center gap-1.5">
          <Cpu className="w-4 h-4 text-cyan-500" /> {t.aiAssistant}
        </Link>
        <Link href="/doctors/dashboard" className="hover:text-teal-600 dark:hover:text-teal-400 transition-colors flex items-center gap-1.5">
          <Stethoscope className="w-4 h-4 text-teal-500" /> {t.doctorPortal}
        </Link>
        <Link href="/admin/dashboard" className="hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors flex items-center gap-1.5">
          <Shield className="w-4 h-4 text-indigo-500" /> {t.adminPortal}
        </Link>
      </nav>

      <div className="flex items-center gap-3">
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
            className="flex items-center gap-2 px-3 py-1.5 rounded-xl bg-slate-100 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 text-xs font-semibold text-slate-800 dark:text-slate-200 hover:border-cyan-500/50 transition-colors"
          >
            <span className="text-sm">{currentLangObj.flag}</span>
            <span>{currentLangObj.label}</span>
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

        {user ? (
          <div className="flex items-center gap-3">
            <div className="text-right hidden sm:block">
              <span className="text-xs font-semibold text-slate-800 dark:text-slate-200 block">
                {user.firstName} {user.lastName}
              </span>
              <span className="text-[10px] px-2 py-0.5 rounded-full bg-blue-100 dark:bg-blue-500/20 text-blue-700 dark:text-blue-400 font-bold uppercase tracking-wider">
                {user.role}
              </span>
            </div>
            <button
              onClick={handleLogout}
              className="p-2 text-slate-500 dark:text-slate-400 hover:text-red-600 dark:hover:text-red-400 hover:bg-red-50 dark:hover:bg-red-950/40 rounded-lg transition-colors"
              title={t.signOut}
            >
              <LogOut className="w-5 h-5" />
            </button>
          </div>
        ) : (
          <div className="flex items-center gap-2">
            <Link
              href="/login"
              className="px-3.5 py-1.5 text-xs font-semibold text-slate-700 dark:text-slate-300 hover:text-cyan-600 dark:hover:text-cyan-400 transition-colors"
            >
              {t.signIn}
            </Link>
            <Link
              href="/register"
              className="px-4 py-2 text-xs font-bold text-white bg-blue-600 hover:bg-blue-500 rounded-xl shadow-md shadow-blue-500/20 transition-all hover:scale-105"
            >
              {t.getStarted}
            </Link>
          </div>
        )}
      </div>
    </header>
  );
}
