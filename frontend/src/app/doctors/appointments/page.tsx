'use client';

import React, { useEffect, useState } from 'react';
import Navbar from '@/components/Navbar';
import Sidebar from '@/components/Sidebar';
import { useLanguage } from '@/context/LanguageContext';
import { doctorService } from '@/services/allServices';
import { Appointment } from '@/types';
import { Calendar, User, Clock, CheckCircle2, XCircle } from 'lucide-react';

export default function DoctorAppointmentsPage() {
  const { t } = useLanguage();
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    doctorService.getAppointments()
      .then(res => {
        if (res.success) setAppointments(res.data);
      })
      .finally(() => setLoading(false));
  }, []);

  const handleUpdateStatus = async (id: string, status: string) => {
    try {
      await doctorService.updateAppointmentStatus(id, status);
      setAppointments(prev => prev.map(a => a.id === id ? { ...a, status } : a));
    } catch (err) {
      console.error(err);
    }
  };

  return (
    <div className="min-h-screen bg-slate-50 dark:bg-slate-950 text-slate-900 dark:text-slate-100 flex flex-col transition-colors duration-300">
      <Navbar />

      <div className="flex flex-1">
        <Sidebar />

        <main className="flex-1 p-6 md:p-8 overflow-y-auto max-w-7xl">
          <div className="flex flex-wrap items-center justify-between gap-4 mb-8">
            <div>
              <h1 className="text-3xl font-extrabold tracking-tight text-slate-900 dark:text-white flex items-center gap-3">
                <Calendar className="w-8 h-8 text-teal-500" /> Shifokor Qabullar Jadvali
              </h1>
              <p className="text-sm text-slate-600 dark:text-slate-400 mt-1">
                Bemorlarning uchrashuv so'rovlarini ko'rib chiqish va holatini yangilash.
              </p>
            </div>
          </div>

          <div className="space-y-4">
            {appointments.map(appt => (
              <div key={appt.id} className="p-6 rounded-3xl bg-white dark:bg-slate-900/90 border border-slate-200 dark:border-slate-800 shadow-sm flex flex-wrap items-center justify-between gap-4">
                <div className="flex items-center gap-4">
                  <div className="w-12 h-12 rounded-2xl bg-teal-500/10 text-teal-500 flex items-center justify-center font-bold text-lg">
                    <User className="w-6 h-6" />
                  </div>
                  <div>
                    <h4 className="text-base font-bold text-slate-900 dark:text-white">{appt.patientName}</h4>
                    <p className="text-xs text-slate-500 dark:text-slate-400 mt-0.5">{appt.reason}</p>
                    <div className="flex items-center gap-3 text-xs text-teal-600 dark:text-teal-400 font-semibold mt-1">
                      <span className="flex items-center gap-1"><Calendar className="w-3.5 h-3.5" /> {new Date(appt.appointmentDate).toLocaleDateString()}</span>
                      <span className="flex items-center gap-1"><Clock className="w-3.5 h-3.5" /> {appt.startTime}</span>
                    </div>
                  </div>
                </div>

                <div className="flex items-center gap-3">
                  <span className={`text-xs font-bold px-3 py-1 rounded-full ${
                    appt.status === 'Confirmed' ? 'bg-emerald-500/20 text-emerald-600 dark:text-emerald-400' : 'bg-amber-500/20 text-amber-600 dark:text-amber-400'
                  }`}>
                    {appt.status}
                  </span>

                  {appt.status !== 'Confirmed' && (
                    <button
                      onClick={() => handleUpdateStatus(appt.id, 'Confirmed')}
                      className="px-3.5 py-1.5 bg-emerald-600 hover:bg-emerald-500 text-white font-bold rounded-xl text-xs flex items-center gap-1.5 shadow-sm"
                    >
                      <CheckCircle2 className="w-4 h-4" /> Tasdiqlash
                    </button>
                  )}
                  {appt.status !== 'Cancelled' && (
                    <button
                      onClick={() => handleUpdateStatus(appt.id, 'Cancelled')}
                      className="px-3.5 py-1.5 bg-red-600/20 text-red-600 dark:text-red-400 hover:bg-red-600 hover:text-white font-bold rounded-xl text-xs flex items-center gap-1.5 transition-colors"
                    >
                      <XCircle className="w-4 h-4" /> Bekor Qilish
                    </button>
                  )}
                </div>
              </div>
            ))}
          </div>
        </main>
      </div>
    </div>
  );
}
