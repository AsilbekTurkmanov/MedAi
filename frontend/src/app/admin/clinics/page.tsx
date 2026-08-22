'use client';

import React, { useState } from 'react';
import Navbar from '@/components/Navbar';
import Sidebar from '@/components/Sidebar';
import { useLanguage } from '@/context/LanguageContext';
import { Building2, Plus, MapPin, Phone, CheckCircle2 } from 'lucide-react';

export default function AdminClinicsPage() {
  const { t } = useLanguage();
  const [clinics, setClinics] = useState([
    { id: '1', name: 'MedAI Central Clinic', city: 'Toshkent', address: 'Amir Temur shoh ko\'chasi 105', phone: '+998 71 200 00 00', status: 'Active' },
    { id: '2', name: 'MedAI Diagnostic Center', city: 'Samarqand', address: 'Registon ko\'chasi 42', phone: '+998 66 200 00 00', status: 'Active' }
  ]);
  const [showAdd, setShowAdd] = useState(false);
  const [name, setName] = useState('');
  const [city, setCity] = useState('Toshkent');
  const [address, setAddress] = useState('');
  const [phone, setPhone] = useState('');

  const handleAddClinic = (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;
    setClinics(prev => [
      ...prev,
      { id: Date.now().toString(), name, city, address, phone, status: 'Active' }
    ]);
    setName('');
    setAddress('');
    setPhone('');
    setShowAdd(false);
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
                <Building2 className="w-8 h-8 text-indigo-500" /> {t.clinicManagement}
              </h1>
              <p className="text-sm text-slate-600 dark:text-slate-400 mt-1">
                Tizimga ulangan klinikalar va tibbiy markazlarni ro'yxatga olish va boshqarish.
              </p>
            </div>

            <button
              onClick={() => setShowAdd(true)}
              className="px-4 py-2.5 bg-indigo-600 hover:bg-indigo-500 text-white text-sm font-bold rounded-xl shadow-lg shadow-indigo-500/20 flex items-center gap-2"
            >
              <Plus className="w-4 h-4" /> Yangi Klinika Qo'shish
            </button>
          </div>

          {showAdd && (
            <div className="mb-8 p-6 rounded-3xl bg-white dark:bg-slate-900/90 border border-slate-200 dark:border-slate-800 shadow-xl">
              <h3 className="text-base font-bold text-slate-900 dark:text-white mb-4">Klinika Ma'lumotlarini Kiritish</h3>
              <form onSubmit={handleAddClinic} className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <input
                  type="text"
                  required
                  placeholder="Klinika Nomi"
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  className="px-4 py-2.5 bg-slate-50 dark:bg-slate-950 border border-slate-300 dark:border-slate-800 rounded-xl text-sm"
                />
                <input
                  type="text"
                  required
                  placeholder="Shahar"
                  value={city}
                  onChange={(e) => setCity(e.target.value)}
                  className="px-4 py-2.5 bg-slate-50 dark:bg-slate-950 border border-slate-300 dark:border-slate-800 rounded-xl text-sm"
                />
                <input
                  type="text"
                  required
                  placeholder="Manzil"
                  value={address}
                  onChange={(e) => setAddress(e.target.value)}
                  className="px-4 py-2.5 bg-slate-50 dark:bg-slate-950 border border-slate-300 dark:border-slate-800 rounded-xl text-sm"
                />
                <input
                  type="text"
                  required
                  placeholder="Telefon"
                  value={phone}
                  onChange={(e) => setPhone(e.target.value)}
                  className="px-4 py-2.5 bg-slate-50 dark:bg-slate-950 border border-slate-300 dark:border-slate-800 rounded-xl text-sm"
                />
                <div className="md:col-span-2">
                  <button type="submit" className="px-6 py-2.5 bg-indigo-600 text-white font-bold rounded-xl text-sm">
                    Klinikani Saqlash
                  </button>
                </div>
              </form>
            </div>
          )}

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {clinics.map(c => (
              <div key={c.id} className="p-6 rounded-3xl bg-white dark:bg-slate-900/90 border border-slate-200 dark:border-slate-800 shadow-sm flex items-center justify-between">
                <div className="flex items-center gap-4">
                  <div className="w-12 h-12 rounded-2xl bg-indigo-500/10 text-indigo-500 flex items-center justify-center font-bold">
                    <Building2 className="w-6 h-6" />
                  </div>
                  <div>
                    <h4 className="text-base font-bold text-slate-900 dark:text-white">{c.name}</h4>
                    <span className="text-xs text-slate-500 dark:text-slate-400 flex items-center gap-1 mt-0.5"><MapPin className="w-3.5 h-3.5" /> {c.city}, {c.address}</span>
                    <span className="text-xs text-indigo-600 dark:text-indigo-400 flex items-center gap-1 mt-0.5"><Phone className="w-3.5 h-3.5" /> {c.phone}</span>
                  </div>
                </div>
                <span className="text-[10px] font-bold px-2.5 py-1 rounded-full bg-emerald-500/20 text-emerald-600 dark:text-emerald-400">
                  {c.status}
                </span>
              </div>
            ))}
          </div>
        </main>
      </div>
    </div>
  );
}
