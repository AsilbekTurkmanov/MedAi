'use client';

import React, { useEffect, useState } from 'react';
import Navbar from '@/components/Navbar';
import Sidebar from '@/components/Sidebar';
import { doctorService, aiService } from '@/services/allServices';
import { PatientProfile, DoctorBriefResponse } from '@/types';
import { Bot, Sparkles, User, Activity, AlertTriangle, CheckCircle2, FileText, HeartPulse } from 'lucide-react';

export default function DoctorCopilotPage() {
  const [patients, setPatients] = useState<PatientProfile[]>([]);
  const [selectedPatientId, setSelectedPatientId] = useState<string>('');
  const [brief, setBrief] = useState<DoctorBriefResponse | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    doctorService.getMyPatients().then(res => {
      if (res.success && res.data.length > 0) {
        setPatients(res.data);
        setSelectedPatientId(res.data[0].id);
        fetchBrief(res.data[0].id);
      }
    });
  }, []);

  const fetchBrief = async (pId: string) => {
    setLoading(true);
    try {
      const res = await aiService.getDoctorBrief(pId);
      if (res.success) setBrief(res.data);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 flex flex-col">
      <Navbar />

      <div className="flex flex-1">
        <Sidebar />

        <main className="flex-1 p-6 md:p-8 overflow-y-auto max-w-7xl">
          <div className="mb-8">
            <h1 className="text-3xl font-extrabold tracking-tight text-white flex items-center gap-3">
              <Bot className="w-8 h-8 text-cyan-400" /> AI Doctor Copilot Workspace
            </h1>
            <p className="text-sm text-slate-400 mt-1">
              Automated pre-consultation briefings, critical lab alerts, and recommended clinical focus areas.
            </p>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            {/* Patient Selector */}
            <div className="lg:col-span-1 space-y-4">
              <h3 className="text-sm font-bold text-slate-400 uppercase tracking-wider">Select Patient</h3>
              {patients.map((p) => (
                <div
                  key={p.id}
                  onClick={() => { setSelectedPatientId(p.id); fetchBrief(p.id); }}
                  className={`p-4 rounded-2xl border cursor-pointer transition-all ${
                    selectedPatientId === p.id
                      ? 'bg-slate-900 border-cyan-500 shadow-lg shadow-cyan-500/10'
                      : 'bg-slate-950 border-slate-800 hover:bg-slate-900/60'
                  }`}
                >
                  <h4 className="text-sm font-bold text-white">{p.firstName} {p.lastName}</h4>
                  <span className="text-xs text-slate-400 block mt-0.5">Blood Type: {p.bloodType}</span>
                </div>
              ))}
            </div>

            {/* Copilot Brief Output */}
            <div className="lg:col-span-2 p-6 rounded-3xl bg-slate-900/90 border border-slate-800 shadow-xl space-y-6">
              <div className="flex items-center justify-between pb-4 border-b border-slate-800">
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 rounded-xl bg-cyan-500/20 text-cyan-400 flex items-center justify-center font-bold">
                    <Sparkles className="w-5 h-5" />
                  </div>
                  <div>
                    <h2 className="text-lg font-bold text-white">{brief?.patientName || 'Patient Brief'}</h2>
                    <span className="text-xs text-slate-400">
                      Age: {brief?.age} • Gender: {brief?.gender} • Blood Type: {brief?.bloodType}
                    </span>
                  </div>
                </div>

                <span className="px-3 py-1 rounded-full bg-cyan-500/10 text-cyan-400 text-xs font-bold border border-cyan-500/30">
                  AI Generated Brief
                </span>
              </div>

              {loading ? (
                <p className="text-slate-400 text-sm italic text-center py-8">Generating clinical brief...</p>
              ) : (
                <div className="space-y-6 text-xs">
                  <div className="p-4 rounded-2xl bg-slate-950 border border-slate-800">
                    <span className="text-cyan-400 font-bold uppercase tracking-wider block mb-1">Clinical Overview</span>
                    <p className="text-slate-200 text-sm leading-relaxed">{brief?.overview}</p>
                  </div>

                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div className="p-4 rounded-2xl bg-slate-950 border border-slate-800">
                      <span className="text-amber-400 font-bold uppercase tracking-wider block mb-2">Critical Anomaly Alerts</span>
                      <ul className="space-y-1 text-amber-200">
                        {brief?.criticalLabAlerts.map((a, i) => (
                          <li key={i} className="flex items-center gap-2">
                            <AlertTriangle className="w-3.5 h-3.5 text-amber-400" /> {a}
                          </li>
                        ))}
                      </ul>
                    </div>

                    <div className="p-4 rounded-2xl bg-slate-950 border border-slate-800">
                      <span className="text-teal-400 font-bold uppercase tracking-wider block mb-2">Active Prescriptions</span>
                      <ul className="space-y-1 text-slate-300">
                        {brief?.activeMedications.map((m, i) => (
                          <li key={i}>• {m}</li>
                        ))}
                      </ul>
                    </div>
                  </div>

                  <div className="p-4 rounded-2xl bg-slate-950 border border-slate-800">
                    <span className="text-cyan-400 font-bold uppercase tracking-wider block mb-2">Recommended Consultation Focus</span>
                    <ul className="space-y-2 text-slate-200">
                      {brief?.recommendedClinicalFocus.map((rec, i) => (
                        <li key={i} className="flex items-start gap-2">
                          <CheckCircle2 className="w-4 h-4 text-cyan-400 shrink-0 mt-0.5" /> {rec}
                        </li>
                      ))}
                    </ul>
                  </div>
                </div>
              )}
            </div>
          </div>
        </main>
      </div>
    </div>
  );
}
