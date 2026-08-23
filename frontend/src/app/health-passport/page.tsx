'use client';

import React, { useEffect, useState } from 'react';
import Navbar from '@/components/Navbar';
import Sidebar from '@/components/Sidebar';
import { patientService } from '@/services/allServices';
import { HealthPassport } from '@/types';
import { Award, HeartPulse, Pill, FlaskConical } from 'lucide-react';

export default function HealthPassportPage() {
  const [passport, setPassport] = useState<HealthPassport | null>(null);

  useEffect(() => {
    patientService.getHealthPassport().then(res => {
      if (res.success) setPassport(res.data);
    });
  }, []);

  return (
    <div className="min-h-screen bg-slate-50 dark:bg-slate-950 text-slate-900 dark:text-slate-100 flex flex-col transition-colors duration-300">
      <Navbar />

      <div className="flex flex-1">
        <Sidebar />

        <main className="flex-1 p-6 md:p-8 overflow-y-auto max-w-7xl">
          <div className="mb-8">
            <h1 className="text-3xl font-extrabold tracking-tight text-slate-900 dark:text-white flex items-center gap-3">
              <Award className="w-8 h-8 text-cyan-600 dark:text-cyan-400" /> Patient Health Passport
            </h1>
            <p className="text-sm text-slate-400 mt-1">
              Your official digital medical ID card for quick clinical reference and emergency identification.
            </p>
          </div>

          {/* Digital Passport Card */}
          <div className="p-8 rounded-3xl bg-gradient-to-br from-slate-900 via-slate-900 to-blue-950 border border-blue-500/30 shadow-2xl relative overflow-hidden mb-8">
            <div className="absolute top-0 right-0 w-64 h-64 bg-cyan-500/10 rounded-full blur-3xl -mr-20 -mt-20 pointer-events-none" />

            <div className="flex flex-wrap items-center justify-between gap-6 pb-6 border-b border-slate-800">
              <div className="flex items-center gap-4">
                <div className="w-16 h-16 rounded-2xl bg-cyan-500/20 text-cyan-400 border border-cyan-500/30 flex items-center justify-center font-black text-xl">
                  {passport?.bloodType || 'A+'}
                </div>
                <div>
                  <h2 className="text-2xl font-black text-white tracking-wide">{passport?.fullName || 'Sarah Jenkins'}</h2>
                  <span className="text-xs text-slate-400 block mt-0.5">
                    Age: {passport?.age || 30} • Gender: {passport?.gender || 'Female'} • DOB: {passport?.dateOfBirth ? new Date(passport.dateOfBirth).toLocaleDateString() : '1995-08-12'}
                  </span>
                </div>
              </div>

              <div className="text-right text-xs">
                <span className="px-3 py-1 rounded-full bg-emerald-500/20 text-emerald-400 font-bold border border-emerald-500/30 uppercase tracking-wider">
                  Verified MedAI Passport
                </span>
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-6 pt-6 text-xs">
              <div>
                <span className="text-slate-400 font-bold uppercase tracking-wider block mb-2">Emergency Contact</span>
                <div className="p-3.5 rounded-xl bg-slate-950/80 border border-slate-800 space-y-1">
                  <span className="font-bold text-white block">{passport?.emergencyContactName || 'Robert Jenkins'}</span>
                  <span className="text-cyan-400 block">{passport?.emergencyContactPhone || '+1 555-0199'}</span>
                </div>
              </div>

              <div>
                <span className="text-slate-400 font-bold uppercase tracking-wider block mb-2">Active Allergies</span>
                <div className="p-3.5 rounded-xl bg-slate-950/80 border border-slate-800 text-amber-300 font-medium">
                  Penicillin (Mild Cutaneous Reaction)
                </div>
              </div>

              <div>
                <span className="text-slate-400 font-bold uppercase tracking-wider block mb-2">Primary Care Clinic</span>
                <div className="p-3.5 rounded-xl bg-slate-950/80 border border-slate-800 text-slate-200">
                  MedAI Central Specialty Clinic
                </div>
              </div>
            </div>
          </div>

          {/* Active Conditions and Medications */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
            <div className="p-6 rounded-3xl bg-slate-900/90 border border-slate-800">
              <h3 className="text-base font-bold text-white flex items-center gap-2 mb-4">
                <Pill className="w-5 h-5 text-cyan-400" /> Verified Prescriptions
              </h3>
              <div className="space-y-3 text-xs">
                {(passport?.activeMedications ?? []).map((m, i) => (
                  <div key={i} className="p-3.5 rounded-xl bg-slate-950 border border-slate-800 flex justify-between items-center">
                    <div>
                      <span className="font-bold text-white text-sm block">{m.name}</span>
                      <span className="text-slate-400">{m.frequency}</span>
                    </div>
                    <span className="text-cyan-400 font-semibold">{m.dosage}</span>
                  </div>
                ))}
              </div>
            </div>

            <div className="p-6 rounded-3xl bg-slate-900/90 border border-slate-800">
              <h3 className="text-base font-bold text-white flex items-center gap-2 mb-4">
                <FlaskConical className="w-5 h-5 text-teal-400" /> Recent Lab Indicators
              </h3>
              <div className="space-y-3 text-xs">
                {(passport?.recentLabResults ?? []).map((l, i) => (
                  <div key={i} className="p-3.5 rounded-xl bg-slate-950 border border-slate-800 flex justify-between items-center">
                    <div>
                      <span className="font-bold text-white text-sm block">{l.testName}</span>
                      <span className="text-slate-400">{new Date(l.testDate).toLocaleDateString()}</span>
                    </div>
                    <div className="text-right">
                      <span className="font-bold text-white block">{l.value} {l.unit}</span>
                      <span className={`text-[10px] font-bold px-2 py-0.5 rounded-full ${
                        l.status === 'Normal' ? 'bg-emerald-500/20 text-emerald-400' : 'bg-amber-500/20 text-amber-400'
                      }`}>
                        {l.status}
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </main>
      </div>
    </div>
  );
}
