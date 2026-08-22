'use client';

import React from 'react';
import Link from 'next/link';
import Navbar from '@/components/Navbar';
import {
  HeartPulse,
  Cpu,
  ShieldCheck,
  Stethoscope,
  ArrowRight,
  Activity,
  Award,
  Sparkles,
  FileCheck,
  Building2,
  Users
} from 'lucide-react';
import { useLanguage } from '@/context/LanguageContext';

export default function Home() {
  const { t } = useLanguage();

  return (
    <div className="min-h-screen bg-slate-50 dark:bg-slate-950 text-slate-900 dark:text-slate-100 flex flex-col font-sans selection:bg-cyan-500 selection:text-slate-950 transition-colors duration-300">
      <Navbar />

      {/* Hero Section */}
      <section className="relative pt-20 pb-32 overflow-hidden bg-gradient-hero">
        <div className="absolute inset-0 bg-[linear-gradient(to_right,#1e293b15_1px,transparent_1px),linear-gradient(to_bottom,#1e293b15_1px,transparent_1px)] bg-[size:4rem_4rem]"></div>
        
        <div className="max-w-7xl mx-auto px-6 relative z-10 text-center">
          <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-cyan-500/10 border border-cyan-500/30 text-cyan-600 dark:text-cyan-400 text-xs font-semibold uppercase tracking-wider mb-8 animate-bounce">
            <Sparkles className="w-4 h-4" />
            {t.heroTag}
          </div>

          <h1 className="text-5xl md:text-7xl font-extrabold tracking-tight leading-tight max-w-5xl mx-auto text-slate-900 dark:text-white">
            {t.heroTitle1}{' '}
            <span className="bg-gradient-to-r from-blue-600 via-teal-500 to-cyan-400 bg-clip-text text-transparent">
              {t.heroTitleHighlight}
            </span>
          </h1>

          <p className="mt-6 text-lg md:text-xl text-slate-600 dark:text-slate-400 max-w-3xl mx-auto leading-relaxed">
            {t.heroDesc}
          </p>

          <div className="mt-10 flex flex-wrap items-center justify-center gap-4">
            <Link
              href="/register"
              className="px-8 py-4 text-base font-bold text-white bg-gradient-to-r from-blue-600 to-cyan-500 rounded-2xl shadow-xl shadow-blue-500/25 hover:shadow-cyan-500/40 hover:scale-105 transition-all flex items-center gap-3"
            >
              {t.launchPatientPortal} <ArrowRight className="w-5 h-5" />
            </Link>
            <Link
              href="/doctors/dashboard"
              className="px-8 py-4 text-base font-bold text-slate-800 dark:text-slate-200 bg-white dark:bg-slate-900/90 border border-slate-300 dark:border-slate-700/80 hover:border-cyan-500/50 rounded-2xl hover:bg-slate-100 dark:hover:bg-slate-800 transition-all flex items-center gap-2"
            >
              <Stethoscope className="w-5 h-5 text-teal-600 dark:text-teal-400" /> {t.doctorCopilot}
            </Link>
          </div>

          {/* Quick Demo Accounts Banner */}
          <div className="mt-12 p-4 max-w-2xl mx-auto bg-white/90 dark:bg-slate-900/80 border border-slate-200 dark:border-slate-800 rounded-2xl text-xs text-slate-600 dark:text-slate-400 shadow-sm flex flex-wrap justify-around gap-2">
            <div><span className="text-blue-600 dark:text-cyan-400 font-bold">Demo Bemor:</span> patient@medai.com / Patient123!</div>
            <div><span className="text-teal-600 dark:text-teal-400 font-bold">Demo Shifokor:</span> doctor@medai.com / Doctor123!</div>
            <div><span className="text-indigo-600 dark:text-indigo-400 font-bold">Demo Admin:</span> admin@medai.com / Admin123!</div>
          </div>
        </div>
      </section>

      {/* Feature Grid */}
      <section className="py-24 bg-white dark:bg-slate-900 border-t border-slate-200 dark:border-slate-800 transition-colors">
        <div className="max-w-7xl mx-auto px-6">
          <div className="text-center max-w-3xl mx-auto mb-16">
            <h2 className="text-3xl md:text-4xl font-bold text-slate-900 dark:text-white">
              Intellektual Tibbiyot Ekotizimi
            </h2>
            <p className="text-slate-600 dark:text-slate-400 mt-4">
              Klinik aniqlik, bemor xavfsizligi va tezkor tibbiy jarayonlar uchun mo'ljallangan.
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            <div className="p-8 rounded-3xl bg-slate-50 dark:bg-slate-950/80 border border-slate-200 dark:border-slate-800 hover:border-blue-500/50 transition-all group shadow-sm">
              <div className="w-12 h-12 rounded-2xl bg-blue-500/10 text-blue-600 dark:text-blue-400 flex items-center justify-center mb-6 group-hover:scale-110 transition-transform">
                <Cpu className="w-6 h-6" />
              </div>
              <h3 className="text-xl font-bold text-slate-900 dark:text-white mb-3">{t.symptomAnalyzer}</h3>
              <p className="text-slate-600 dark:text-slate-400 text-sm leading-relaxed">
                Tibbiy simptomlarni aqlli tahlil qilish, xavf darajasini baholash va xavfsizlik ogohlantirishlari.
              </p>
            </div>

            <div className="p-8 rounded-3xl bg-slate-50 dark:bg-slate-950/80 border border-slate-200 dark:border-slate-800 hover:border-teal-500/50 transition-all group shadow-sm">
              <div className="w-12 h-12 rounded-2xl bg-teal-500/10 text-teal-600 dark:text-teal-400 flex items-center justify-center mb-6 group-hover:scale-110 transition-transform">
                <Award className="w-6 h-6" />
              </div>
              <h3 className="text-xl font-bold text-slate-900 dark:text-white mb-3">{t.healthPassport}</h3>
              <p className="text-slate-600 dark:text-slate-400 text-sm leading-relaxed">
                Qon guruhi, favqulodda kontaktlar, faol retseptlar va tahlil xulosalari markazlashtirilgan raqamli pasport.
              </p>
            </div>

            <div className="p-8 rounded-3xl bg-slate-50 dark:bg-slate-950/80 border border-slate-200 dark:border-slate-800 hover:border-cyan-500/50 transition-all group shadow-sm">
              <div className="w-12 h-12 rounded-2xl bg-cyan-500/10 text-cyan-600 dark:text-cyan-400 flex items-center justify-center mb-6 group-hover:scale-110 transition-transform">
                <Stethoscope className="w-6 h-6" />
              </div>
              <h3 className="text-xl font-bold text-slate-900 dark:text-white mb-3">{t.doctorCopilot}</h3>
              <p className="text-slate-600 dark:text-slate-400 text-sm leading-relaxed">
                Shifokor qabuli oldidan bemor tarixi bo'yicha onlayn AI sharhi va muhim tahlil anomaliyalari haqida xabar berish.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className="py-8 bg-slate-100 dark:bg-slate-950 border-t border-slate-200 dark:border-slate-800 text-center text-xs text-slate-500 dark:text-slate-400 transition-colors">
        <p>© 2026 MEDAI Platform Inc. — "{t.heroTitle1} {t.heroTitleHighlight}"</p>
      </footer>
    </div>
  );
}
