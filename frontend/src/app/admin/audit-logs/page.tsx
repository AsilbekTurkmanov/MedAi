'use client';

import React, { useEffect, useState } from 'react';
import Navbar from '@/components/Navbar';
import Sidebar from '@/components/Sidebar';
import { adminService } from '@/services/allServices';
import { FileText, ShieldCheck } from 'lucide-react';

export default function AdminAuditLogsPage() {
  const [logs, setLogs] = useState<any[]>([]);

  useEffect(() => {
    adminService.getAuditLogs().then(res => {
      if (res.success) setLogs(res.data);
    });
  }, []);

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 flex flex-col">
      <Navbar />

      <div className="flex flex-1">
        <Sidebar />

        <main className="flex-1 p-6 md:p-8 overflow-y-auto max-w-7xl">
          <div className="mb-8">
            <h1 className="text-3xl font-extrabold tracking-tight text-white flex items-center gap-3">
              <FileText className="w-8 h-8 text-indigo-400" /> Medical Audit Logs
            </h1>
            <p className="text-sm text-slate-400 mt-1">
              Security compliance tracking for all sensitive patient data queries, AI sessions, and record mutations.
            </p>
          </div>

          <div className="p-6 rounded-3xl bg-slate-900/90 border border-slate-800 shadow-xl overflow-x-auto">
            <table className="w-full text-left text-xs">
              <thead className="border-b border-slate-800 text-slate-400 uppercase tracking-wider">
                <tr>
                  <th className="pb-3">User</th>
                  <th className="pb-3">Action</th>
                  <th className="pb-3">Entity Type</th>
                  <th className="pb-3">IP Address</th>
                  <th className="pb-3">Timestamp</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-800/60">
                {logs.map((log) => (
                  <tr key={log.id} className="hover:bg-slate-950/50">
                    <td className="py-3 font-bold text-white">{log.userEmail}</td>
                    <td className="py-3 text-cyan-400 font-medium">{log.action}</td>
                    <td className="py-3 text-slate-300">{log.entityType}</td>
                    <td className="py-3 font-mono text-slate-400">{log.ipAddress}</td>
                    <td className="py-3 text-slate-400">{new Date(log.createdAt).toLocaleString()}</td>
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
