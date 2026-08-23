'use client';

import React, { useEffect, useState } from 'react';
import Navbar from '@/components/Navbar';
import Sidebar from '@/components/Sidebar';
import { adminService } from '@/services/allServices';
import { useLanguage } from '@/context/LanguageContext';
import { Users, Lock, Unlock } from 'lucide-react';

export default function AdminUsersPage() {
  const { t } = useLanguage();
  const [users, setUsers] = useState<any[]>([]);

  useEffect(() => {
    adminService.getUsers().then(res => {
      if (res.success) setUsers(res.data);
    });
  }, []);

  const handleToggleStatus = async (id: string, currentActive: boolean) => {
    try {
      await adminService.toggleUserStatus(id);
      setUsers(prev => prev.map(u => u.id === id ? { ...u, isActive: !currentActive } : u));
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
          <div className="mb-8">
            <h1 className="text-3xl font-extrabold tracking-tight text-slate-900 dark:text-white flex items-center gap-3">
              <Users className="w-8 h-8 text-indigo-500" /> {t.userDirectory}
            </h1>
            <p className="text-sm text-slate-600 dark:text-slate-400 mt-1">
              Ro'yxatdan o'tgan akkuntlar, tayinlangan rollar va faollik holati boshqaruvi.
            </p>
          </div>

          <div className="p-6 rounded-3xl bg-white dark:bg-slate-900/90 border border-slate-200 dark:border-slate-800 shadow-sm dark:shadow-xl overflow-x-auto">
            <table className="w-full text-left text-xs">
              <thead className="border-b border-slate-200 dark:border-slate-800 text-slate-500 dark:text-slate-400 uppercase tracking-wider">
                <tr>
                  <th className="pb-3">Ism va Familiya</th>
                  <th className="pb-3">Email</th>
                  <th className="pb-3">Rol</th>
                  <th className="pb-3">Holat</th>
                  <th className="pb-3">Sana</th>
                  <th className="pb-3 text-right">Amallar</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-200 dark:divide-slate-800/60">
                {users.map((u) => (
                  <tr key={u.id} className="hover:bg-slate-50 dark:hover:bg-slate-950/50">
                    <td className="py-3.5 font-bold text-slate-900 dark:text-white">{u.firstName} {u.lastName}</td>
                    <td className="py-3.5 text-slate-600 dark:text-slate-300">{u.email}</td>
                    <td className="py-3.5">
                      <span className="px-2.5 py-1 rounded-full bg-blue-500/20 text-blue-700 dark:text-blue-300 font-bold uppercase text-[10px]">
                        {u.role}
                      </span>
                    </td>
                    <td className="py-3.5">
                      <span className={`px-2 py-0.5 rounded-full font-bold text-[10px] ${
                        u.isActive ? 'bg-emerald-500/20 text-emerald-600 dark:text-emerald-400' : 'bg-red-500/20 text-red-600 dark:text-red-400'
                      }`}>
                        {u.isActive ? 'Faol (Active)' : 'Bloklangan (Locked)'}
                      </span>
                    </td>
                    <td className="py-3.5 text-slate-500 dark:text-slate-400">{new Date(u.createdAt).toLocaleDateString()}</td>
                    <td className="py-3.5 text-right">
                      <button
                        onClick={() => handleToggleStatus(u.id, u.isActive)}
                        className={`px-3 py-1 rounded-xl text-[11px] font-bold flex items-center gap-1.5 ml-auto ${
                          u.isActive
                            ? 'bg-amber-500/20 text-amber-700 dark:text-amber-400 hover:bg-amber-500 hover:text-white'
                            : 'bg-emerald-500/20 text-emerald-700 dark:text-emerald-400 hover:bg-emerald-500 hover:text-white'
                        }`}
                      >
                        {u.isActive ? <Lock className="w-3 h-3" /> : <Unlock className="w-3 h-3" />}
                        {u.isActive ? 'Bloklash' : 'Faollashtirish'}
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </main>
      </div>
    </div>
  );
}
