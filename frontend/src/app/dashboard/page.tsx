'use client';

import React, { useEffect, useState } from 'react';
import Navbar from '@/components/Navbar';
import Sidebar from '@/components/Sidebar';
import { patientService } from '@/services/allServices';
import { HealthPassport, Appointment, LabResult, TimelineItem } from '@/types';
import { useLanguage } from '@/context/LanguageContext';
import {
  Activity,
  Bot,
  Calendar,
  FlaskConical,
  Pill,
  ChevronRight,
  HeartPulse
} from 'lucide-react';
import Link from 'next/link';

export default function PatientDashboard() {
  const { t } = useLanguage();
  const [passport, setPassport] = useState<HealthPassport | null>(null);
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [labResults, setLabResults] = useState<LabResult[]>([]);
  const [timeline, setTimeline] = useState<TimelineItem[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function loadData() {
      try {
        const [passRes, apptRes, labRes, timeRes] = await Promise.all([
          patientService.getHealthPassport(),
          patientService.getAppointments(),
          patientService.getLabResults(),
          patientService.getTimeline()
        ]);

        if (passRes.success) setPassport(passRes.data);
        if (apptRes.success) setAppointments(apptRes.data);
        if (labRes.success) setLabResults(labRes.data);
        if (timeRes.success) setTimeline(timeRes.data);
      } catch (err) {
        console.error('Failed to load dashboard data:', err);
      } finally {
        setLoading(false);
      }
    }
    loadData();
  }, []);

  const upcomingAppt = appointments.find(a => a.status === 'Confirmed' || a.status === 'Pending');

  return (
    <div className="min-h-screen bg-slate-50 dark:bg-slate-950 text-slate-900 dark:text-slate-100 flex flex-col transition-colors duration-300">
      <Navbar />

      <div className="flex flex-1">
        <Sidebar />

        <main className="flex-1 p-6 md:p-8 overflow-y-auto max-w-7xl">
          {/* Header */}
          <div className="flex flex-wrap items-center justify-between gap-4 mb-8">
            <div>
              <h1 className="text-3xl font-extrabold tracking-tight text-slate-900 dark:text-white flex items-center gap-3">
                {t.patientOverviewTitle}
                <span className="text-xs px-2.5 py-1 rounded-full bg-cyan-500/10 border border-cyan-500/30 text-cyan-600 dark:text-cyan-400 font-semibold">
                  AI Active
                </span>
              </h1>
              <p className="text-sm text-slate-600 dark:text-slate-400 mt-1">
                {passport ? `${passport.fullName}` : 'Patient'}
              </p>
            </div>

            <div className="flex gap-3">
              <Link
                href="/ai-assistant"
                className="px-4 py-2.5 bg-gradient-to-r from-cyan-500 to-blue-600 hover:from-cyan-400 hover:to-blue-500 text-white text-sm font-bold rounded-xl shadow-lg shadow-cyan-500/20 flex items-center gap-2"
              >
                <Bot className="w-4 h-4" /> {t.startAiChat}
              </Link>
            </div>
          </div>

          {/* Quick Metrics Grid */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5 mb-8">
            <div className="p-5 rounded-2xl bg-white dark:bg-slate-900/80 border border-slate-200 dark:border-slate-800 shadow-sm flex items-center gap-4">
              <div className="w-12 h-12 rounded-xl bg-blue-500/10 text-blue-600 dark:text-blue-400 flex items-center justify-center shrink-0">
                <HeartPulse className="w-6 h-6" />
              </div>
              <div>
                <span className="text-xs font-semibold text-slate-500 dark:text-slate-400 block">{t.bloodType}</span>
                <span className="text-2xl font-extrabold text-slate-900 dark:text-white">{passport?.bloodType || 'A+'}</span>
              </div>
            </div>

            <div className="p-5 rounded-2xl bg-white dark:bg-slate-900/80 border border-slate-200 dark:border-slate-800 shadow-sm flex items-center gap-4">
              <div className="w-12 h-12 rounded-xl bg-teal-500/10 text-teal-600 dark:text-teal-400 flex items-center justify-center shrink-0">
                <Pill className="w-6 h-6" />
              </div>
              <div>
                <span className="text-xs font-semibold text-slate-500 dark:text-slate-400 block">{t.activeMedications}</span>
                <span className="text-2xl font-extrabold text-slate-900 dark:text-white">{passport?.activeMedications.length || 0}</span>
              </div>
            </div>

            <div className="p-5 rounded-2xl bg-white dark:bg-slate-900/80 border border-slate-200 dark:border-slate-800 shadow-sm flex items-center gap-4">
              <div className="w-12 h-12 rounded-xl bg-cyan-500/10 text-cyan-600 dark:text-cyan-400 flex items-center justify-center shrink-0">
                <FlaskConical className="w-6 h-6" />
              </div>
              <div>
                <span className="text-xs font-semibold text-slate-500 dark:text-slate-400 block">{t.recentLabPanels}</span>
                <span className="text-2xl font-extrabold text-slate-900 dark:text-white">{labResults.length}</span>
              </div>
            </div>

            <div className="p-5 rounded-2xl bg-white dark:bg-slate-900/80 border border-slate-200 dark:border-slate-800 shadow-sm flex items-center gap-4">
              <div className="w-12 h-12 rounded-xl bg-indigo-500/10 text-indigo-600 dark:text-indigo-400 flex items-center justify-center shrink-0">
                <Calendar className="w-6 h-6" />
              </div>
              <div>
                <span className="text-xs font-semibold text-slate-500 dark:text-slate-400 block">{t.appointments}</span>
                <span className="text-2xl font-extrabold text-slate-900 dark:text-white">{appointments.length}</span>
              </div>
            </div>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            {/* Main Column */}
            <div className="lg:col-span-2 space-y-8">
              {/* Upcoming Appointment */}
              <div className="p-6 rounded-3xl bg-white dark:bg-slate-900/90 border border-slate-200 dark:border-slate-800 shadow-sm dark:shadow-xl">
                <div className="flex items-center justify-between mb-4">
                  <h3 className="text-base font-bold text-slate-900 dark:text-white flex items-center gap-2">
                    <Calendar className="w-5 h-5 text-blue-500" /> {t.upcomingAppointments}
                  </h3>
                  <Link href="/appointments" className="text-xs font-semibold text-blue-600 dark:text-blue-400 hover:underline flex items-center gap-1">
                    {t.manageAll} <ChevronRight className="w-4 h-4" />
                  </Link>
                </div>

                {upcomingAppt ? (
                  <div className="p-4 rounded-2xl bg-blue-50 dark:bg-blue-950/40 border border-blue-200 dark:border-blue-800/50 flex flex-wrap items-center justify-between gap-4">
                    <div>
                      <span className="text-xs font-semibold text-blue-700 dark:text-blue-300 uppercase tracking-wider block">
                        {upcomingAppt.doctorSpecialization}
                      </span>
                      <h4 className="text-lg font-bold text-slate-900 dark:text-white">{upcomingAppt.doctorName}</h4>
                      <p className="text-xs text-slate-600 dark:text-slate-400 mt-1">{upcomingAppt.clinicName} • {upcomingAppt.reason}</p>
                    </div>
                    <div className="text-right">
                      <span className="text-sm font-extrabold text-blue-600 dark:text-cyan-300 block">
                        {new Date(upcomingAppt.appointmentDate).toLocaleDateString()}
                      </span>
                      <span className="text-xs text-slate-500 dark:text-slate-400">{upcomingAppt.startTime}</span>
                      <span className="mt-1 block text-[10px] font-bold px-2 py-0.5 rounded-full bg-blue-500/20 text-blue-700 dark:text-blue-300 uppercase">
                        {upcomingAppt.status}
                      </span>
                    </div>
                  </div>
                ) : (
                  <p className="text-sm text-slate-500 dark:text-slate-400 italic py-2">{t.noAppointments}</p>
                )}
              </div>

              {/* Lab Results Overview */}
              <div className="p-6 rounded-3xl bg-white dark:bg-slate-900/90 border border-slate-200 dark:border-slate-800 shadow-sm dark:shadow-xl">
                <div className="flex items-center justify-between mb-4">
                  <h3 className="text-base font-bold text-slate-900 dark:text-white flex items-center gap-2">
                    <FlaskConical className="w-5 h-5 text-teal-500" /> {t.recentLabPanels}
                  </h3>
                  <Link href="/lab-results" className="text-xs font-semibold text-teal-600 dark:text-teal-400 hover:underline flex items-center gap-1">
                    {t.viewFullReports} <ChevronRight className="w-4 h-4" />
                  </Link>
                </div>

                <div className="space-y-3">
                  {labResults.slice(0, 3).map((lab) => (
                    <div key={lab.id} className="p-4 rounded-2xl bg-slate-50 dark:bg-slate-950 border border-slate-200 dark:border-slate-800/80 flex items-center justify-between">
                      <div>
                        <h4 className="text-sm font-bold text-slate-900 dark:text-white">{lab.testName}</h4>
                        <span className="text-xs text-slate-500 dark:text-slate-400">{lab.notes || 'Routine panel'}</span>
                      </div>
                      <div className="text-right">
                        <span className="text-sm font-extrabold text-slate-900 dark:text-white block">{lab.value} {lab.unit}</span>
                        <span className={`text-[10px] font-bold px-2 py-0.5 rounded-full ${
                          lab.status === 'Normal' ? 'bg-emerald-500/20 text-emerald-600 dark:text-emerald-400' : 'bg-amber-500/20 text-amber-600 dark:text-amber-400'
                        }`}>
                          {lab.status}
                        </span>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>

            {/* Right Sidebar */}
            <div className="space-y-8">
              {/* Active Prescriptions */}
              <div className="p-6 rounded-3xl bg-white dark:bg-slate-900/90 border border-slate-200 dark:border-slate-800 shadow-sm dark:shadow-xl">
                <h3 className="text-base font-bold text-slate-900 dark:text-white flex items-center gap-2 mb-4">
                  <Pill className="w-5 h-5 text-cyan-500" /> {t.activeMedications}
                </h3>

                <div className="space-y-3">
                  {passport?.activeMedications.map((med, idx) => (
                    <div key={idx} className="p-3.5 rounded-2xl bg-slate-50 dark:bg-slate-950 border border-slate-200 dark:border-slate-800 text-xs">
                      <span className="font-bold text-slate-900 dark:text-white block text-sm">{med.name}</span>
                      <span className="text-cyan-600 dark:text-cyan-400 block mt-0.5">{med.dosage} • {med.frequency}</span>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        </main>
      </div>
    </div>
  );
}
