'use client';

import React, { useEffect, useState } from 'react';
import Navbar from '@/components/Navbar';
import Sidebar from '@/components/Sidebar';
import { doctorService, aiService } from '@/services/allServices';
import { Appointment, PatientProfile, DoctorBriefResponse } from '@/types';
import { useLanguage } from '@/context/LanguageContext';
import { Stethoscope, Users, Calendar, Bot, Sparkles, Activity, ChevronRight, CheckCircle2 } from 'lucide-react';
import Link from 'next/link';

export default function DoctorDashboard() {
  const { t } = useLanguage();
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [patients, setPatients] = useState<PatientProfile[]>([]);
  const [brief, setBrief] = useState<DoctorBriefResponse | null>(null);

  useEffect(() => {
    doctorService.getMyAppointments().then(res => {
      if (res.success) setAppointments(res.data);
    });
    doctorService.getMyPatients().then(res => {
      if (res.success) {
        setPatients(res.data);
        if (res.data.length > 0) {
          aiService.getDoctorBrief(res.data[0].id).then(bRes => {
            if (bRes.success) setBrief(bRes.data);
          });
        }
      }
    });
  }, []);

  return (
    <div className="min-h-screen bg-slate-50 dark:bg-slate-950 text-slate-900 dark:text-slate-100 flex flex-col transition-colors duration-300">
      <Navbar />

      <div className="flex flex-1">
        <Sidebar />

        <main className="flex-1 p-6 md:p-8 overflow-y-auto max-w-7xl">
          <div className="flex flex-wrap items-center justify-between gap-4 mb-8">
            <div>
              <h1 className="text-3xl font-extrabold tracking-tight text-slate-900 dark:text-white flex items-center gap-3">
                <Stethoscope className="w-8 h-8 text-teal-500" /> {t.doctorHubTitle}
              </h1>
              <p className="text-sm text-slate-600 dark:text-slate-400 mt-1">
                Bemorlar qabul jadvali, klinik ma'lumotlar va AI Copilot xulosalari.
              </p>
            </div>

            <Link
              href="/doctors/copilot"
              className="px-5 py-3 bg-gradient-to-r from-teal-500 to-cyan-500 hover:from-teal-400 hover:to-cyan-400 text-white font-bold text-sm rounded-xl shadow-lg shadow-teal-500/20 flex items-center gap-2"
            >
              <Bot className="w-4 h-4" /> AI Shifokor Kopilotini Ochish
            </Link>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            {/* Appointments and Patients */}
            <div className="lg:col-span-2 space-y-8">
              {/* Today's Schedule */}
              <div className="p-6 rounded-3xl bg-white dark:bg-slate-900/90 border border-slate-200 dark:border-slate-800 shadow-sm dark:shadow-xl">
                <h3 className="text-base font-bold text-slate-900 dark:text-white flex items-center gap-2 mb-4">
                  <Calendar className="w-5 h-5 text-teal-500" /> {t.patientSchedule}
                </h3>

                <div className="space-y-3">
                  {appointments.map((a) => (
                    <div key={a.id} className="p-4 rounded-2xl bg-slate-50 dark:bg-slate-950 border border-slate-200 dark:border-slate-800 flex justify-between items-center text-xs">
                      <div>
                        <span className="font-bold text-slate-900 dark:text-white text-sm block">{a.patientName}</span>
                        <span className="text-slate-500 dark:text-slate-400 block mt-0.5">{a.reason}</span>
                      </div>
                      <div className="text-right">
                        <span className="font-bold text-teal-600 dark:text-teal-400 block">{a.startTime}</span>
                        <span className="text-[10px] font-bold px-2 py-0.5 rounded-full bg-teal-500/20 text-teal-700 dark:text-teal-300 uppercase">
                          {a.status}
                        </span>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>

            {/* AI Copilot Brief Card */}
            <div className="p-6 rounded-3xl bg-white dark:bg-slate-900/90 border border-slate-200 dark:border-slate-800 shadow-sm dark:shadow-xl space-y-4">
              <h3 className="text-base font-bold text-slate-900 dark:text-white flex items-center gap-2">
                <Sparkles className="w-5 h-5 text-cyan-500" /> AI Shifokor Xulosasi
              </h3>

              {brief ? (
                <div className="space-y-4 text-xs">
                  <div className="p-4 rounded-2xl bg-slate-50 dark:bg-slate-950 border border-slate-200 dark:border-slate-800">
                    <span className="font-bold text-slate-900 dark:text-white block text-sm mb-1">{brief.patientName}</span>
                    <p className="text-slate-700 dark:text-slate-300 leading-relaxed">{brief.overview}</p>
                  </div>

                  <div className="p-4 rounded-2xl bg-slate-50 dark:bg-slate-950 border border-slate-200 dark:border-slate-800">
                    <span className="text-cyan-600 dark:text-cyan-400 font-bold uppercase tracking-wider block mb-2">{t.recommendedFocus}</span>
                    <ul className="space-y-1.5 text-slate-700 dark:text-slate-300">
                      {brief.recommendedClinicalFocus.map((rec, i) => (
                        <li key={i} className="flex items-start gap-2">
                          <CheckCircle2 className="w-3.5 h-3.5 text-cyan-500 shrink-0 mt-0.5" /> {rec}
                        </li>
                      ))}
                    </ul>
                  </div>
                </div>
              ) : (
                <p className="text-slate-500 dark:text-slate-400 text-xs italic">AI Xulosasini olish uchun bemorni tanlang.</p>
              )}
            </div>
          </div>
        </main>
      </div>
    </div>
  );
}
