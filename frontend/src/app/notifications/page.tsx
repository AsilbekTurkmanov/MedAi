'use client';

import React from 'react';
import Navbar from '@/components/Navbar';
import Sidebar from '@/components/Sidebar';
import { useLanguage } from '@/context/LanguageContext';
import { Bell, CheckCircle2, Calendar, Pill } from 'lucide-react';

export default function NotificationsPage() {
  const { t } = useLanguage();

  const notifications = [
    { id: '1', title: 'Qabul Eslatmasi', message: 'Ertaga soat 10:00 da Dr. Jamshid Alimov bilan uchrashuv rejalashtirilgan.', type: 'Appointment', time: '10 daqiqa oldin' },
    { id: '2', title: 'Tahlil Natijasi Tayyor', message: 'Umumiy qon tahlili natijalaringiz laboratoriyadan kelib tushdi va AI tahlilidan o\'tdi.', type: 'LabResult', time: '1 soat oldin' },
    { id: '3', title: 'Dori Qabul Qilish Vaqti', message: 'Amoxicillin 500mg dorisini qabul qilish vaqti bo\'ldi.', type: 'Medication', time: '3 soat oldin' }
  ];

  return (
    <div className="min-h-screen bg-slate-50 dark:bg-slate-950 text-slate-900 dark:text-slate-100 flex flex-col transition-colors duration-300">
      <Navbar />

      <div className="flex flex-1">
        <Sidebar />

        <main className="flex-1 p-6 md:p-8 overflow-y-auto max-w-7xl">
          <div className="flex flex-wrap items-center justify-between gap-4 mb-8">
            <div>
              <h1 className="text-3xl font-extrabold tracking-tight text-slate-900 dark:text-white flex items-center gap-3">
                <Bell className="w-8 h-8 text-cyan-500" /> Bildirishnomalar Jurnali
              </h1>
              <p className="text-sm text-slate-600 dark:text-slate-400 mt-1">
                Salomatlik eslatmalari, shifokor javoblari va retsept bildirishnomalari.
              </p>
            </div>
          </div>

          <div className="space-y-4">
            {notifications.map(n => (
              <div key={n.id} className="p-5 rounded-2xl bg-white dark:bg-slate-900/90 border border-slate-200 dark:border-slate-800 shadow-sm flex items-start justify-between gap-4">
                <div className="flex items-start gap-4">
                  <div className="w-10 h-10 rounded-xl bg-cyan-500/10 text-cyan-500 flex items-center justify-center shrink-0 mt-0.5">
                    {n.type === 'Appointment' ? <Calendar className="w-5 h-5" /> : n.type === 'Medication' ? <Pill className="w-5 h-5" /> : <CheckCircle2 className="w-5 h-5" />}
                  </div>
                  <div>
                    <h4 className="text-base font-bold text-slate-900 dark:text-white">{n.title}</h4>
                    <p className="text-sm text-slate-600 dark:text-slate-400 mt-1 leading-relaxed">{n.message}</p>
                  </div>
                </div>
                <span className="text-xs text-slate-400 whitespace-nowrap">{n.time}</span>
              </div>
            ))}
          </div>
        </main>
      </div>
    </div>
  );
}
