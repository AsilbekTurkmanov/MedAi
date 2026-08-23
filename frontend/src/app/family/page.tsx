'use client';

import React, { useState } from 'react';
import Navbar from '@/components/Navbar';
import Sidebar from '@/components/Sidebar';
import { useLanguage } from '@/context/LanguageContext';
import { Users, Plus } from 'lucide-react';

export default function FamilyHubPage() {
  const { t } = useLanguage();
  const [members, setMembers] = useState([
    { id: '1', name: 'Malika Turkmanova', relation: 'Spouse', bloodType: 'B+', age: 29, status: 'Active' },
    { id: '2', name: 'Davron Turkmanov', relation: 'Son', bloodType: 'A+', age: 5, status: 'Active' }
  ]);
  const [showAdd, setShowAdd] = useState(false);
  const [newName, setNewName] = useState('');
  const [newRelation, setNewRelation] = useState('Child');
  const [newBlood, setNewBlood] = useState('O+');

  const handleAddMember = (e: React.FormEvent) => {
    e.preventDefault();
    if (!newName.trim()) return;
    setMembers(prev => [
      ...prev,
      { id: Date.now().toString(), name: newName, relation: newRelation, bloodType: newBlood, age: 10, status: 'Active' }
    ]);
    setNewName('');
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
                <Users className="w-8 h-8 text-blue-500" /> {t.familyHub}
              </h1>
              <p className="text-sm text-slate-600 dark:text-slate-400 mt-1">
                Oila a'zolaringiz salomatlik pasportlari va retseptlarini bir joyda boshqaring.
              </p>
            </div>

            <button
              onClick={() => setShowAdd(true)}
              className="px-4 py-2.5 bg-blue-600 hover:bg-blue-500 text-white text-sm font-bold rounded-xl shadow-lg shadow-blue-500/20 flex items-center gap-2"
            >
              <Plus className="w-4 h-4" /> Oila A'zosi Qo'shish
            </button>
          </div>

          {showAdd && (
            <div className="mb-8 p-6 rounded-3xl bg-white dark:bg-slate-900/90 border border-slate-200 dark:border-slate-800 shadow-xl">
              <h3 className="text-base font-bold text-slate-900 dark:text-white mb-4">Yangi Oila A'zosi Profilini Yaratish</h3>
              <form onSubmit={handleAddMember} className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <input
                  type="text"
                  required
                  placeholder="F.I.SH."
                  value={newName}
                  onChange={(e) => setNewName(e.target.value)}
                  className="px-4 py-2.5 bg-slate-50 dark:bg-slate-950 border border-slate-300 dark:border-slate-800 rounded-xl text-sm"
                />
                <select
                  value={newRelation}
                  onChange={(e) => setNewRelation(e.target.value)}
                  className="px-4 py-2.5 bg-slate-50 dark:bg-slate-950 border border-slate-300 dark:border-slate-800 rounded-xl text-sm"
                >
                  <option value="Spouse">Turdosh (Turmush o'rtog'i)</option>
                  <option value="Child">Farzand</option>
                  <option value="Parent">Ota-Ona</option>
                </select>
                <button
                  type="submit"
                  className="px-4 py-2.5 bg-cyan-500 text-white font-bold rounded-xl text-sm"
                >
                  Saqlash
                </button>
              </form>
            </div>
          )}

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {members.map(m => (
              <div key={m.id} className="p-6 rounded-3xl bg-white dark:bg-slate-900/90 border border-slate-200 dark:border-slate-800 shadow-sm flex items-center justify-between">
                <div className="flex items-center gap-4">
                  <div className="w-12 h-12 rounded-2xl bg-blue-500/10 text-blue-500 flex items-center justify-center font-bold text-lg">
                    {m.name.charAt(0)}
                  </div>
                  <div>
                    <h4 className="text-base font-bold text-slate-900 dark:text-white">{m.name}</h4>
                    <span className="text-xs text-slate-500 dark:text-slate-400 block">{m.relation} • {m.age} yosh</span>
                    <span className="text-xs text-cyan-600 dark:text-cyan-400 font-bold block mt-1">Qon Guruhi: {m.bloodType}</span>
                  </div>
                </div>
                <div className="text-right">
                  <span className="text-[10px] font-bold px-2.5 py-1 rounded-full bg-emerald-500/20 text-emerald-600 dark:text-emerald-400">
                    {m.status}
                  </span>
                </div>
              </div>
            ))}
          </div>
        </main>
      </div>
    </div>
  );
}
