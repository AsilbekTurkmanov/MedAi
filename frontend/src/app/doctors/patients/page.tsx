'use client';

import React, { useEffect, useState } from 'react';
import Navbar from '@/components/Navbar';
import Sidebar from '@/components/Sidebar';
import { doctorService, medicationService } from '@/services/allServices';
import { PatientProfile } from '@/types';
import { useLanguage } from '@/context/LanguageContext';
import { Users, Search, Phone, Mail, MapPin, HeartPulse, Pill, Bot, Plus } from 'lucide-react';
import Link from 'next/link';

export default function DoctorPatientsPage() {
  const { t } = useLanguage();
  const [patients, setPatients] = useState<PatientProfile[]>([]);
  const [search, setSearch] = useState('');
  const [showRxModal, setShowRxModal] = useState(false);
  const [selectedPatient, setSelectedPatient] = useState<PatientProfile | null>(null);
  const [medName, setMedName] = useState('');
  const [dosage, setDosage] = useState('500mg');
  const [frequency, setFrequency] = useState('Kuniga 2 mahal (Twice daily)');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    doctorService.getMyPatients().then(res => {
      if (res.success) setPatients(res.data);
    });
  }, []);

  const handlePrescribe = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedPatient || !medName.trim()) return;
    setLoading(true);
    try {
      await medicationService.create({
        patientId: selectedPatient.id,
        name: medName,
        dosage,
        frequency,
        startDate: new Date().toISOString()
      });
      setShowRxModal(false);
      setMedName('');
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const filtered = patients.filter(p =>
    `${p.firstName} ${p.lastName}`.toLowerCase().includes(search.toLowerCase()) ||
    p.email.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="min-h-screen bg-slate-50 dark:bg-slate-950 text-slate-900 dark:text-slate-100 flex flex-col transition-colors duration-300">
      <Navbar />

      <div className="flex flex-1">
        <Sidebar />

        <main className="flex-1 p-6 md:p-8 overflow-y-auto max-w-7xl">
          <div className="flex flex-wrap items-center justify-between gap-4 mb-8">
            <div>
              <h1 className="text-3xl font-extrabold tracking-tight text-slate-900 dark:text-white flex items-center gap-3">
                <Users className="w-8 h-8 text-teal-500" /> {t.patientRoster}
              </h1>
              <p className="text-sm text-slate-600 dark:text-slate-400 mt-1">
                Bemorlar profili, favqulodda kontaktlar va qon guruhlari jurnali.
              </p>
            </div>

            <div className="relative">
              <Search className="w-4 h-4 text-slate-400 absolute left-3.5 top-3.5" />
              <input
                type="text"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder="Bemor ismi yoki emaili..."
                className="pl-10 pr-4 py-2.5 bg-white dark:bg-slate-900 border border-slate-300 dark:border-slate-800 rounded-xl text-sm text-slate-900 dark:text-white focus:outline-none focus:border-teal-500"
              />
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {filtered.map((patient) => (
              <div key={patient.id} className="p-6 rounded-3xl bg-white dark:bg-slate-900/90 border border-slate-200 dark:border-slate-800 shadow-sm dark:shadow-xl space-y-4">
                <div className="flex justify-between items-start">
                  <div>
                    <h3 className="text-lg font-bold text-slate-900 dark:text-white">{patient.firstName} {patient.lastName}</h3>
                    <span className="text-xs text-slate-500 dark:text-slate-400 block mt-0.5">{patient.email}</span>
                  </div>
                  <span className="w-10 h-10 rounded-xl bg-teal-500/10 text-teal-600 dark:text-teal-400 font-extrabold flex items-center justify-center border border-teal-500/30 text-sm">
                    {patient.bloodType}
                  </span>
                </div>

                <div className="p-3.5 rounded-2xl bg-slate-50 dark:bg-slate-950 border border-slate-200 dark:border-slate-800 text-xs space-y-1">
                  <div className="flex items-center gap-2 text-slate-700 dark:text-slate-300">
                    <Phone className="w-3.5 h-3.5 text-teal-500" /> Telefon: {patient.phoneNumber}
                  </div>
                  <div className="flex items-center gap-2 text-slate-700 dark:text-slate-300">
                    <MapPin className="w-3.5 h-3.5 text-teal-500" /> Favqulodda: {patient.emergencyContactName} ({patient.emergencyContactPhone})
                  </div>
                </div>

                <div className="flex gap-3 pt-2">
                  <button
                    onClick={() => { setSelectedPatient(patient); setShowRxModal(true); }}
                    className="flex-1 py-2 px-3 bg-teal-600 hover:bg-teal-500 text-white font-bold rounded-xl text-xs flex items-center justify-center gap-1.5 shadow-sm"
                  >
                    <Pill className="w-4 h-4" /> Retsept Yozish
                  </button>
                  <Link
                    href="/doctors/copilot"
                    className="py-2 px-3 bg-slate-200 dark:bg-slate-800 hover:bg-slate-300 dark:hover:bg-slate-700 text-slate-800 dark:text-white font-bold rounded-xl text-xs flex items-center justify-center gap-1.5"
                  >
                    <Bot className="w-4 h-4 text-cyan-500" /> AI Xulosa
                  </Link>
                </div>
              </div>
            ))}
          </div>

          {/* Prescription Modal */}
          {showRxModal && selectedPatient && (
            <div className="fixed inset-0 bg-slate-950/70 backdrop-blur-sm z-50 flex items-center justify-center p-4">
              <div className="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-6 rounded-3xl w-full max-w-md shadow-2xl">
                <h3 className="text-xl font-bold text-slate-900 dark:text-white mb-2">Retsept Yozish</h3>
                <p className="text-xs text-slate-500 dark:text-slate-400 mb-4">Bemor: {selectedPatient.firstName} {selectedPatient.lastName}</p>

                <form onSubmit={handlePrescribe} className="space-y-4 text-xs">
                  <div>
                    <label className="block text-slate-700 dark:text-slate-300 font-semibold mb-1">Dori Nomi</label>
                    <input
                      type="text"
                      required
                      value={medName}
                      onChange={(e) => setMedName(e.target.value)}
                      placeholder="Amoxicillin..."
                      className="w-full p-3 bg-slate-50 dark:bg-slate-950 border border-slate-300 dark:border-slate-800 rounded-xl text-slate-900 dark:text-white text-sm"
                    />
                  </div>

                  <div>
                    <label className="block text-slate-700 dark:text-slate-300 font-semibold mb-1">Dozalash (Dosage)</label>
                    <input
                      type="text"
                      required
                      value={dosage}
                      onChange={(e) => setDosage(e.target.value)}
                      className="w-full p-3 bg-slate-50 dark:bg-slate-950 border border-slate-300 dark:border-slate-800 rounded-xl text-slate-900 dark:text-white text-sm"
                    />
                  </div>

                  <div>
                    <label className="block text-slate-700 dark:text-slate-300 font-semibold mb-1">Qabul Qilish Tartibi (Frequency)</label>
                    <input
                      type="text"
                      required
                      value={frequency}
                      onChange={(e) => setFrequency(e.target.value)}
                      className="w-full p-3 bg-slate-50 dark:bg-slate-950 border border-slate-300 dark:border-slate-800 rounded-xl text-slate-900 dark:text-white text-sm"
                    />
                  </div>

                  <div className="flex justify-end gap-3 pt-4">
                    <button
                      type="button"
                      onClick={() => setShowRxModal(false)}
                      className="px-4 py-2.5 bg-slate-200 dark:bg-slate-800 hover:bg-slate-300 dark:hover:bg-slate-700 text-slate-800 dark:text-white rounded-xl font-bold"
                    >
                      Bekor Qilish
                    </button>
                    <button
                      type="submit"
                      disabled={loading}
                      className="px-5 py-2.5 bg-teal-600 hover:bg-teal-500 text-white rounded-xl font-bold shadow-lg shadow-teal-500/20"
                    >
                      {loading ? 'Saqlanmoqda...' : 'Retseptni Saqlash'}
                    </button>
                  </div>
                </form>
              </div>
            </div>
          )}
        </main>
      </div>
    </div>
  );
}
