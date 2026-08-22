'use client';

import React, { useEffect, useState } from 'react';
import Navbar from '@/components/Navbar';
import Sidebar from '@/components/Sidebar';
import { patientService, aiService } from '@/services/allServices';
import { LabResult } from '@/types';
import { useLanguage } from '@/context/LanguageContext';
import { FlaskConical, Bot, Sparkles, AlertCircle, CheckCircle2, ChevronRight } from 'lucide-react';

export default function LabResultsPage() {
  const { t } = useLanguage();
  const [labs, setLabs] = useState<LabResult[]>([]);
  const [selectedLab, setSelectedLab] = useState<LabResult | null>(null);
  const [explanation, setExplanation] = useState<any>(null);
  const [loadingAi, setLoadingAi] = useState(false);

  useEffect(() => {
    patientService.getLabResults().then(res => {
      if (res.success) {
        setLabs(res.data);
        if (res.data.length > 0) setSelectedLab(res.data[0]);
      }
    });
  }, []);

  const handleExplain = async (lab: LabResult) => {
    setSelectedLab(lab);
    setLoadingAi(true);
    setExplanation(null);
    try {
      const res = await aiService.explainLabResult(lab.id);
      if (res.success) setExplanation(res.data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoadingAi(false);
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
              <FlaskConical className="w-8 h-8 text-teal-500" /> {t.labResults}
            </h1>
            <p className="text-sm text-slate-600 dark:text-slate-400 mt-1">
              Rasmiy laboratoriya tahlillari va ularning sun'iy intellekt tomonidan tushuntirilishi.
            </p>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            {/* Lab List */}
            <div className="lg:col-span-1 space-y-3">
              {labs.map((lab) => (
                <div
                  key={lab.id}
                  onClick={() => handleExplain(lab)}
                  className={`p-4 rounded-2xl border cursor-pointer transition-all ${
                    selectedLab?.id === lab.id
                      ? 'bg-white dark:bg-slate-900 border-cyan-500 shadow-md shadow-cyan-500/10'
                      : 'bg-white dark:bg-slate-950 border-slate-200 dark:border-slate-800/80 hover:border-cyan-500/50'
                  }`}
                >
                  <div className="flex justify-between items-start mb-1">
                    <h3 className="text-sm font-bold text-slate-900 dark:text-white">{lab.testName}</h3>
                    <span className={`text-[10px] font-bold px-2 py-0.5 rounded-full ${
                      lab.status === 'Normal' ? 'bg-emerald-500/20 text-emerald-600 dark:text-emerald-400' : 'bg-amber-500/20 text-amber-600 dark:text-amber-400'
                    }`}>
                      {lab.status}
                    </span>
                  </div>
                  <span className="text-xs text-cyan-600 dark:text-cyan-400 font-extrabold block">{lab.value} {lab.unit}</span>
                  <span className="text-[11px] text-slate-500 dark:text-slate-400 block mt-1">
                    Me'yor: {lab.referenceRange} • {new Date(lab.testDate).toLocaleDateString()}
                  </span>
                </div>
              ))}
            </div>

            {/* AI Lab Breakdown Panel */}
            <div className="lg:col-span-2 p-6 rounded-3xl bg-white dark:bg-slate-900/90 border border-slate-200 dark:border-slate-800 shadow-sm dark:shadow-xl">
              <div className="flex items-center justify-between mb-6 pb-4 border-b border-slate-200 dark:border-slate-800">
                <div>
                  <h2 className="text-xl font-bold text-slate-900 dark:text-white">{selectedLab?.testName || 'Tahlilni Tanlang'}</h2>
                  <span className="text-xs text-slate-500 dark:text-slate-400">{selectedLab?.doctorName} tomonidan tasdiqlangan</span>
                </div>
                <button
                  onClick={() => selectedLab && handleExplain(selectedLab)}
                  disabled={loadingAi}
                  className="px-4 py-2 bg-gradient-to-r from-cyan-500 to-blue-600 text-white font-bold text-xs rounded-xl flex items-center gap-2 shadow-lg shadow-cyan-500/20"
                >
                  <Sparkles className="w-4 h-4" /> {t.explainLabResult}
                </button>
              </div>

              {explanation ? (
                <div className="space-y-6 text-xs">
                  <div className="p-4 rounded-2xl bg-slate-50 dark:bg-slate-950 border border-slate-200 dark:border-slate-800">
                    <span className="text-cyan-600 dark:text-cyan-400 font-bold uppercase tracking-wider block mb-1">{t.plainLangSummary}</span>
                    <p className="text-slate-800 dark:text-slate-200 text-sm leading-relaxed">{explanation.simpleExplanation}</p>
                  </div>

                  <div className="p-4 rounded-2xl bg-slate-50 dark:bg-slate-950 border border-slate-200 dark:border-slate-800">
                    <span className="text-slate-500 dark:text-slate-400 font-bold uppercase tracking-wider block mb-2">{t.questionsForDoctor}</span>
                    <ul className="space-y-2 text-slate-700 dark:text-slate-300">
                      {explanation.questionsForDoctor?.map((q: string, i: number) => (
                        <li key={i} className="flex items-start gap-2">
                          <CheckCircle2 className="w-4 h-4 text-cyan-500 shrink-0 mt-0.5" /> {q}
                        </li>
                      ))}
                    </ul>
                  </div>

                  <div className="p-3 rounded-xl bg-cyan-50 dark:bg-cyan-950/40 border border-cyan-200 dark:border-cyan-800/60 text-cyan-800 dark:text-cyan-300 text-[11px]">
                    {explanation.safetyDisclaimer}
                  </div>
                </div>
              ) : (
                <div className="py-12 text-center text-slate-500 dark:text-slate-400 text-sm">
                  {loadingAi ? 'AI tahlil natijalarini sharhlamoqda...' : 'Natijani ommabop tilda tushunish uchun "Explain Result" tugmasini bosing.'}
                </div>
              )}
            </div>
          </div>
        </main>
      </div>
    </div>
  );
}
