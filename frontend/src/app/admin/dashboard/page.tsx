'use client';

import React, { useEffect, useState } from 'react';
import Navbar from '@/components/Navbar';
import Sidebar from '@/components/Sidebar';
import { adminService } from '@/services/allServices';
import { AdminStats } from '@/types';
import { ShieldCheck, Users, Stethoscope, Building2, Calendar, Bot, Activity, FileText } from 'lucide-react';
import Link from 'next/link';

export default function AdminDashboardPage() {
  const [stats, setStats] = useState<AdminStats | null>(null);

  useEffect(() => {
    adminService.getStats().then(res => {
      if (res.success) setStats(res.data);
    });
  }, []);

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 flex flex-col">
      <Navbar />

      <div className="flex flex-1">
        <Sidebar />

        <main className="flex-1 p-6 md:p-8 overflow-y-auto max-w-7xl">
          <div className="flex flex-wrap items-center justify-between gap-4 mb-8">
            <div>
              <h1 className="text-3xl font-extrabold tracking-tight text-white flex items-center gap-3">
                <ShieldCheck className="w-8 h-8 text-indigo-400" /> Platform Administration
              </h1>
              <p className="text-sm text-slate-400 mt-1">
                System performance metrics, registered accounts, clinic rosters, and live audit logs.
              </p>
            </div>
          </div>

          {/* Stats Grid */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5 mb-8">
            <div className="p-5 rounded-2xl bg-slate-900/80 border border-slate-800 flex items-center gap-4">
              <div className="w-12 h-12 rounded-xl bg-blue-500/10 text-blue-400 flex items-center justify-center shrink-0">
                <Users className="w-6 h-6" />
              </div>
              <div>
                <span className="text-xs font-semibold text-slate-400 block">Total Users</span>
                <span className="text-2xl font-extrabold text-white">{stats?.totalUsers || 0}</span>
              </div>
            </div>

            <div className="p-5 rounded-2xl bg-slate-900/80 border border-slate-800 flex items-center gap-4">
              <div className="w-12 h-12 rounded-xl bg-teal-500/10 text-teal-400 flex items-center justify-center shrink-0">
                <Stethoscope className="w-6 h-6" />
              </div>
              <div>
                <span className="text-xs font-semibold text-slate-400 block">Verified Doctors</span>
                <span className="text-2xl font-extrabold text-white">{stats?.totalDoctors || 0}</span>
              </div>
            </div>

            <div className="p-5 rounded-2xl bg-slate-900/80 border border-slate-800 flex items-center gap-4">
              <div className="w-12 h-12 rounded-xl bg-cyan-500/10 text-cyan-400 flex items-center justify-center shrink-0">
                <Bot className="w-6 h-6" />
              </div>
              <div>
                <span className="text-xs font-semibold text-slate-400 block">AI Sessions</span>
                <span className="text-2xl font-extrabold text-white">{stats?.totalAiSessions || 0}</span>
              </div>
            </div>

            <div className="p-5 rounded-2xl bg-slate-900/80 border border-slate-800 flex items-center gap-4">
              <div className="w-12 h-12 rounded-xl bg-indigo-500/10 text-indigo-400 flex items-center justify-center shrink-0">
                <Calendar className="w-6 h-6" />
              </div>
              <div>
                <span className="text-xs font-semibold text-slate-400 block">Total Appointments</span>
                <span className="text-2xl font-extrabold text-white">{stats?.totalAppointments || 0}</span>
              </div>
            </div>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
            <div className="p-6 rounded-3xl bg-slate-900/90 border border-slate-800 shadow-xl">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-base font-bold text-white flex items-center gap-2">
                  <Users className="w-5 h-5 text-blue-400" /> Admin Navigation
                </h3>
              </div>
              <div className="grid grid-cols-2 gap-4 text-xs font-bold">
                <Link href="/admin/users" className="p-4 rounded-2xl bg-slate-950 border border-slate-800 hover:border-blue-500 text-white flex items-center gap-3">
                  <Users className="w-5 h-5 text-blue-400" /> Manage User Accounts
                </Link>
                <Link href="/admin/audit-logs" className="p-4 rounded-2xl bg-slate-950 border border-slate-800 hover:border-indigo-500 text-white flex items-center gap-3">
                  <FileText className="w-5 h-5 text-indigo-400" /> System Audit Logs
                </Link>
              </div>
            </div>

            <div className="p-6 rounded-3xl bg-slate-900/90 border border-slate-800 shadow-xl">
              <h3 className="text-base font-bold text-white flex items-center gap-2 mb-4">
                <Activity className="w-5 h-5 text-teal-400" /> System Security Status
              </h3>
              <div className="p-4 rounded-2xl bg-slate-950 border border-slate-800 text-xs space-y-2 text-slate-300">
                <div>• Role-Based Access Control (RBAC): ACTIVE</div>
                <div>• JWT Token Signing & Refresh Validation: ACTIVE</div>
                <div>• Medical Action Audit Logger: RECORDING</div>
                <div>• PostgreSQL Data Encrypted & Isolated</div>
              </div>
            </div>
          </div>
        </main>
      </div>
    </div>
  );
}
