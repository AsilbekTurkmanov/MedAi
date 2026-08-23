'use client';

import React, { useEffect, useState } from 'react';
import Navbar from '@/components/Navbar';
import Sidebar from '@/components/Sidebar';
import { patientService, medicationService } from '@/services/allServices';
import { Medication } from '@/types';
import { useLanguage } from '@/context/LanguageContext';
import { Pill, Plus } from 'lucide-react';

export default function MedicationsPage() {
  const { t } = useLanguage();
  const [meds, setMeds] = useState<Medication[]>([]);
  const [showModal, setShowModal] = useState(false);
  const [name, setName] = useState('');
  const [dosage, setDosage] = useState('500mg');
  const [frequency, setFrequency] = useState('Kuniga 1 mahal (Once daily)');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    patientService.getMedications().then(res => {
      if (res.success) setMeds(res.data);
    });
  }, []);

  const handleAddMedication = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;
    setLoading(true);

    const newMedItem: Medication = {
      id: Date.now().toString(),
      patientId: 'patient-1',
      name: name.trim(),
      dosage: dosage.trim() || '500mg',
      frequency: frequency.trim() || 'Kuniga 1 mahal',
      startDate: new Date().toISOString(),
      notes: 'Kunlik qabul qilinishi zarur.',
      createdAt: new Date().toISOString()
    };

    try {
      let patientId = '';
      try {
        const meRes = await patientService.getHealthPassport();
        if (meRes?.success && meRes?.data) {
          patientId = meRes.data.patientId;
        }
      } catch (e) {
        // pass
      }

      const res = await medicationService.create({
        patientId: patientId || '00000000-0000-0000-0000-000000000001',
        name: name.trim(),
        dosage: dosage.trim(),
        frequency: frequency.trim(),
        startDate: new Date().toISOString()
      });

      if (res && res.success && res.data) {
        setMeds(prev => [res.data, ...prev]);
      } else {
        setMeds(prev => [newMedItem, ...prev]);
      }
    } catch (err) {
      console.error('Failed to create medication via API, adding locally:', err);
      setMeds(prev => [newMedItem, ...prev]);
    } finally {
      setShowModal(false);
      setName('');
      setLoading(false);
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
                <Pill className="w-8 h-8 text-cyan-500" /> {t.medications}
              </h1>
              <p className="text-sm text-slate-600 dark:text-slate-400 mt-1">
                Davolash kursi, dozalash tartibi va shifokor retseptlari jurnali.
              </p>
            </div>

            <button
              onClick={() => setShowModal(true)}
              className="px-5 py-3 bg-cyan-500 hover:bg-cyan-400 text-white font-bold text-sm rounded-xl shadow-lg shadow-cyan-500/20 flex items-center gap-2"
            >
              <Plus className="w-4 h-4" /> Dori Eslatmasini Qo'shish
            </button>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {meds.length === 0 ? (
              <div className="md:col-span-2 py-16 text-center">
                <Pill className="w-12 h-12 text-slate-300 dark:text-slate-600 mx-auto mb-4" />
                <p className="text-slate-500 dark:text-slate-400 text-sm">Hozircha dori-darmonlar ro'yxati bo'sh.</p>
              </div>
            ) : (
            meds.map((med) => (
              <div key={med.id} className="p-6 rounded-3xl bg-white dark:bg-slate-900/90 border border-slate-200 dark:border-slate-800 shadow-sm dark:shadow-xl space-y-4">
                <div className="flex justify-between items-start">
                  <div>
                    <h3 className="text-lg font-bold text-slate-900 dark:text-white">{med.name}</h3>
                    <span className="text-xs text-cyan-600 dark:text-cyan-400 font-bold block mt-0.5">{med.dosage} • {med.frequency}</span>
                  </div>
                  <span className="px-2.5 py-1 rounded-full bg-emerald-500/20 text-emerald-600 dark:text-emerald-400 text-xs font-bold border border-emerald-500/30">
                    Faol Kurs
                  </span>
                </div>

                <div className="p-3.5 rounded-2xl bg-slate-50 dark:bg-slate-950 border border-slate-200 dark:border-slate-800/80 text-xs text-slate-700 dark:text-slate-300">
                  <span className="text-slate-500 dark:text-slate-400 block font-semibold mb-1">Shifokor Ko'rsatmasi</span>
                  {med.notes || 'Kunlik qabul qilinishi zarur.'}
                </div>

                <div className="flex justify-between items-center text-xs text-slate-500 dark:text-slate-400 pt-2 border-t border-slate-200 dark:border-slate-800/60">
                  <span>Boshlangan: {new Date(med.startDate).toLocaleDateString()}</span>
                  <span>{med.endDate ? `Tugash: ${new Date(med.endDate).toLocaleDateString()}` : 'Davom etmoqda'}</span>
                </div>
              </div>
            ))
            )}
          </div>

          {/* Add Medication Modal */}
          {showModal && (
            <div className="fixed inset-0 bg-slate-950/70 backdrop-blur-sm z-50 flex items-center justify-center p-4">
              <div className="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-6 rounded-3xl w-full max-w-md shadow-2xl">
                <h3 className="text-xl font-bold text-slate-900 dark:text-white mb-4">Yangi Dori Eslatmasini Qo'shish</h3>

                <form onSubmit={handleAddMedication} className="space-y-4 text-xs">
                  <div>
                    <label className="block text-slate-700 dark:text-slate-300 font-semibold mb-1">Dori Nomi</label>
                    <input
                      type="text"
                      required
                      value={name}
                      onChange={(e) => setName(e.target.value)}
                      placeholder="Magne B6, Vitamin C..."
                      className="w-full p-3 bg-slate-50 dark:bg-slate-950 border border-slate-300 dark:border-slate-800 rounded-xl text-slate-900 dark:text-white text-sm"
                    />
                  </div>

                  <div>
                    <label className="block text-slate-700 dark:text-slate-300 font-semibold mb-1">Dozalash</label>
                    <input
                      type="text"
                      required
                      value={dosage}
                      onChange={(e) => setDosage(e.target.value)}
                      className="w-full p-3 bg-slate-50 dark:bg-slate-950 border border-slate-300 dark:border-slate-800 rounded-xl text-slate-900 dark:text-white text-sm"
                    />
                  </div>

                  <div>
                    <label className="block text-slate-700 dark:text-slate-300 font-semibold mb-1">Qabul Qilish Tartibi</label>
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
                      onClick={() => setShowModal(false)}
                      className="px-4 py-2.5 bg-slate-200 dark:bg-slate-800 hover:bg-slate-300 dark:hover:bg-slate-700 text-slate-800 dark:text-white rounded-xl font-bold"
                    >
                      Bekor Qilish
                    </button>
                    <button
                      type="submit"
                      disabled={loading}
                      className="px-5 py-2.5 bg-cyan-500 hover:bg-cyan-400 text-white rounded-xl font-bold shadow-lg shadow-cyan-500/20"
                    >
                      {loading ? 'Saqlanmoqda...' : 'Eslatmani Saqlash'}
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
