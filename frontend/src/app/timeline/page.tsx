'use client';

import React, { useEffect, useState } from 'react';
import Navbar from '@/components/Navbar';
import Sidebar from '@/components/Sidebar';
import { patientService } from '@/services/allServices';
import { TimelineItem } from '@/types';
import { Clock, Calendar, FlaskConical, FolderOpen, Activity, Filter } from 'lucide-react';

export default function TimelinePage() {
  const [items, setItems] = useState<TimelineItem[]>([]);
  const [filter, setFilter] = useState('All');

  useEffect(() => {
    patientService.getTimeline().then(res => {
      if (res.success) setItems(res.data);
    });
  }, []);

  const filtered = filter === 'All' ? items : items.filter(i => i.category === filter);

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 flex flex-col">
      <Navbar />

      <div className="flex flex-1">
        <Sidebar />

        <main className="flex-1 p-6 md:p-8 overflow-y-auto max-w-7xl">
          <div className="flex flex-wrap items-center justify-between gap-4 mb-8">
            <div>
              <h1 className="text-3xl font-extrabold tracking-tight text-white flex items-center gap-3">
                <Clock className="w-8 h-8 text-indigo-400" /> Longitudinal Health Timeline
              </h1>
              <p className="text-sm text-slate-400 mt-1">
                Integrated chronological view of appointments, lab panels, uploaded records, and health events.
              </p>
            </div>

            <div className="flex bg-slate-900 p-1 rounded-2xl border border-slate-800 text-xs font-semibold">
              {['All', 'Appointment', 'LabResult', 'Document'].map((cat) => (
                <button
                  key={cat}
                  onClick={() => setFilter(cat)}
                  className={`px-4 py-2 rounded-xl transition-colors ${
                    filter === cat ? 'bg-blue-600 text-white font-bold' : 'text-slate-400 hover:text-white'
                  }`}
                >
                  {cat}
                </button>
              ))}
            </div>
          </div>

          <div className="space-y-6 relative before:absolute before:left-4 before:top-3 before:bottom-3 before:w-0.5 before:bg-slate-800">
            {filtered.map((item) => (
              <div key={item.id} className="pl-10 relative">
                <div className="w-8 h-8 rounded-full bg-slate-900 border-2 border-blue-500 absolute left-0 top-0 flex items-center justify-center text-blue-400">
                  <Activity className="w-4 h-4" />
                </div>

                <div className="p-5 rounded-2xl bg-slate-900/90 border border-slate-800 shadow-xl">
                  <div className="flex items-center justify-between mb-2">
                    <span className="text-xs font-bold text-cyan-400 uppercase tracking-wider">
                      {item.category}
                    </span>
                    <span className="text-xs text-slate-400">
                      {new Date(item.date).toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' })}
                    </span>
                  </div>

                  <h3 className="text-base font-bold text-white mb-1">{item.title}</h3>
                  <p className="text-xs text-slate-300 leading-relaxed">{item.description}</p>
                </div>
              </div>
            ))}
          </div>
        </main>
      </div>
    </div>
  );
}
