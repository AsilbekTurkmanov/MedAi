'use client';

import React, { useState } from 'react';
import Navbar from '@/components/Navbar';
import Sidebar from '@/components/Sidebar';
import { aiService } from '@/services/allServices';
import { SymptomAnalysisResponse } from '@/types';
import { useLanguage } from '@/context/LanguageContext';
import {
  Bot,
  Send,
  Sparkles,
  ShieldCheck,
  Activity,
  CheckCircle2,
  RefreshCw
} from 'lucide-react';

export default function AIAssistantPage() {
  const { t } = useLanguage();
  const [activeTab, setActiveTab] = useState<'chat' | 'symptoms'>('chat');
  
  // Chat state
  const [messages, setMessages] = useState<Array<{ role: string; content: string; safetyLevel?: string }>>([
    {
      role: 'assistant',
      content: 'Assalomu alaykum! Men MedAI intellektual tibbiy yordamchingizman. Bugun salomatligingiz bo\'yicha qanday ma\'lumot berishim mumkin?',
      safetyLevel: 'Safe'
    }
  ]);
  const [inputMsg, setInputMsg] = useState('');
  const [loadingChat, setLoadingChat] = useState(false);
  const [sessionId, setSessionId] = useState<string | undefined>();

  // Symptom Analyzer state
  const [symptoms, setSymptoms] = useState('');
  const [duration, setDuration] = useState('2 kun');
  const [age, setAge] = useState(30);
  const [symptomResult, setSymptomResult] = useState<SymptomAnalysisResponse | null>(null);
  const [loadingSymptoms, setLoadingSymptoms] = useState(false);

  const handleSendChat = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!inputMsg.trim() || loadingChat) return;

    const userText = inputMsg;
    setInputMsg('');
    setMessages(prev => [...prev, { role: 'user', content: userText }]);
    setLoadingChat(true);

    try {
      const res = await aiService.chat(userText, sessionId);
      if (res.success) {
        setSessionId(res.data.sessionId);
        setMessages(prev => [
          ...prev,
          { role: 'assistant', content: res.data.response, safetyLevel: res.data.safetyLevel }
        ]);
      }
    } catch (err) {
      setMessages(prev => [...prev, { role: 'assistant', content: 'Connection issue. Please try again.', safetyLevel: 'Safe' }]);
    } finally {
      setLoadingChat(false);
    }
  };

  const handleAnalyzeSymptoms = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!symptoms.trim() || loadingSymptoms) return;

    setLoadingSymptoms(true);
    try {
      const res = await aiService.analyzeSymptoms({ symptoms, duration, age });
      if (res.success) {
        setSymptomResult(res.data);
      }
    } catch (err) {
      console.error(err);
    } finally {
      setLoadingSymptoms(false);
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
                <Bot className="w-8 h-8 text-cyan-500" /> {t.aiAssistant}
              </h1>
              <p className="text-sm text-slate-600 dark:text-slate-400 mt-1">
                Klinik AI yordamchisi: tahlillar, simptomlar va tibbiy ma'lumotlar tahlili.
              </p>
            </div>

            <div className="flex bg-slate-200 dark:bg-slate-900 p-1 rounded-2xl border border-slate-300 dark:border-slate-800 text-xs font-semibold">
              <button
                onClick={() => setActiveTab('chat')}
                className={`px-4 py-2 rounded-xl transition-colors ${
                  activeTab === 'chat' ? 'bg-cyan-500 text-white font-bold' : 'text-slate-600 dark:text-slate-400 hover:text-slate-900 dark:hover:text-white'
                }`}
              >
                {t.startAiChat}
              </button>
              <button
                onClick={() => setActiveTab('symptoms')}
                className={`px-4 py-2 rounded-xl transition-colors ${
                  activeTab === 'symptoms' ? 'bg-cyan-500 text-white font-bold' : 'text-slate-600 dark:text-slate-400 hover:text-slate-900 dark:hover:text-white'
                }`}
              >
                {t.symptomAnalyzer}
              </button>
            </div>
          </div>

          {/* AI Disclaimer */}
          <div className="mb-6 p-4 rounded-2xl bg-cyan-50 dark:bg-cyan-950/40 border border-cyan-200 dark:border-cyan-800/60 text-xs text-cyan-800 dark:text-cyan-200 flex items-center gap-3 shadow-sm">
            <ShieldCheck className="w-5 h-5 text-cyan-500 shrink-0" />
            <div>
              <span className="font-bold block">{t.safetyDisclaimerTitle}</span>
              {t.safetyDisclaimerDesc}
            </div>
          </div>

          {activeTab === 'chat' && (
            <div className="bg-white dark:bg-slate-900/90 border border-slate-200 dark:border-slate-800 rounded-3xl h-[600px] flex flex-col overflow-hidden shadow-xl">
              {/* Chat Messages */}
              <div className="flex-1 p-6 overflow-y-auto space-y-4">
                {messages.map((msg, index) => (
                  <div
                    key={index}
                    className={`flex ${msg.role === 'user' ? 'justify-end' : 'justify-start'}`}
                  >
                    <div
                      className={`max-w-2xl p-4 rounded-2xl text-sm leading-relaxed ${
                        msg.role === 'user'
                          ? 'bg-blue-600 text-white rounded-br-none shadow-md'
                          : msg.safetyLevel === 'EmergencyWarning'
                          ? 'bg-red-50 dark:bg-red-950 border border-red-200 dark:border-red-800 text-red-800 dark:text-red-200 rounded-bl-none'
                          : 'bg-slate-100 dark:bg-slate-950 border border-slate-200 dark:border-slate-800 text-slate-800 dark:text-slate-200 rounded-bl-none'
                      }`}
                    >
                      {msg.role === 'assistant' && (
                        <div className="flex items-center gap-2 mb-2 text-xs font-bold text-cyan-600 dark:text-cyan-400">
                          <Sparkles className="w-3.5 h-3.5" /> MedAI Assistant
                        </div>
                      )}
                      {msg.content}
                    </div>
                  </div>
                ))}

                {loadingChat && (
                  <div className="flex justify-start">
                    <div className="p-4 rounded-2xl bg-slate-100 dark:bg-slate-950 border border-slate-200 dark:border-slate-800 text-xs text-cyan-600 dark:text-cyan-400 flex items-center gap-2">
                      <RefreshCw className="w-4 h-4 animate-spin" /> ...
                    </div>
                  </div>
                )}
              </div>

              {/* Chat Input Form */}
              <form onSubmit={handleSendChat} className="p-4 bg-slate-50 dark:bg-slate-950 border-t border-slate-200 dark:border-slate-800 flex gap-3">
                <input
                  type="text"
                  value={inputMsg}
                  onChange={(e) => setInputMsg(e.target.value)}
                  placeholder={t.askAiPlaceholder}
                  className="flex-1 px-4 py-3 bg-white dark:bg-slate-900 border border-slate-300 dark:border-slate-800 rounded-xl text-sm text-slate-900 dark:text-white focus:outline-none focus:border-cyan-500"
                />
                <button
                  type="submit"
                  disabled={loadingChat}
                  className="px-5 py-3 bg-cyan-500 hover:bg-cyan-400 text-white font-bold rounded-xl flex items-center gap-2 shadow-lg shadow-cyan-500/20"
                >
                  <Send className="w-4 h-4" /> {t.send}
                </button>
              </form>
            </div>
          )}

          {activeTab === 'symptoms' && (
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
              {/* Symptom Input Form */}
              <div className="p-6 rounded-3xl bg-white dark:bg-slate-900/90 border border-slate-200 dark:border-slate-800 shadow-sm dark:shadow-xl">
                <h3 className="text-lg font-bold text-slate-900 dark:text-white mb-4 flex items-center gap-2">
                  <Activity className="w-5 h-5 text-cyan-500" /> {t.symptomAnalyzer}
                </h3>

                <form onSubmit={handleAnalyzeSymptoms} className="space-y-4">
                  <div>
                    <label className="block text-xs font-semibold text-slate-700 dark:text-slate-300 uppercase tracking-wider mb-2">
                      Simptomlar Tasviri
                    </label>
                    <textarea
                      required
                      rows={4}
                      value={symptoms}
                      onChange={(e) => setSymptoms(e.target.value)}
                      placeholder="masalan: 3 kundan beri quruq yo'tal va isitma..."
                      className="w-full p-3.5 bg-slate-50 dark:bg-slate-950 border border-slate-300 dark:border-slate-800 rounded-xl text-sm text-slate-900 dark:text-white focus:outline-none focus:border-cyan-500"
                    />
                  </div>

                  <button
                    type="submit"
                    disabled={loadingSymptoms}
                    className="w-full py-3 px-4 bg-gradient-to-r from-cyan-500 to-blue-600 text-white font-bold rounded-xl shadow-lg shadow-cyan-500/20"
                  >
                    {loadingSymptoms ? 'Tahlil qilinmoqda...' : t.runSymptomTriage}
                  </button>
                </form>
              </div>

              {/* Symptom Result Display */}
              <div className="p-6 rounded-3xl bg-white dark:bg-slate-900/90 border border-slate-200 dark:border-slate-800 shadow-sm dark:shadow-xl">
                <h3 className="text-lg font-bold text-slate-900 dark:text-white mb-4 flex items-center gap-2">
                  <Sparkles className="w-5 h-5 text-cyan-500" /> AI Xulosasi
                </h3>

                {symptomResult ? (
                  <div className="space-y-4 text-xs">
                    <div className="p-4 rounded-2xl bg-slate-50 dark:bg-slate-950 border border-slate-200 dark:border-slate-800">
                      <span className="text-slate-500 dark:text-slate-400 font-bold uppercase block mb-1">Xavf Darajasi</span>
                      <span className="text-base font-extrabold px-3 py-1 rounded-full inline-block bg-amber-500/20 text-amber-700 dark:text-amber-400">
                        {symptomResult.riskLevel} Risk
                      </span>
                    </div>

                    <div className="p-4 rounded-2xl bg-slate-50 dark:bg-slate-950 border border-slate-200 dark:border-slate-800">
                      <span className="text-slate-500 dark:text-slate-400 font-bold uppercase block mb-2">Ehtimoliy Sabablar</span>
                      <ul className="space-y-1 text-slate-800 dark:text-slate-200">
                        {symptomResult.potentialCauses.map((c, i) => (
                          <li key={i} className="flex items-center gap-2">
                            <CheckCircle2 className="w-3.5 h-3.5 text-cyan-500" /> {c}
                          </li>
                        ))}
                      </ul>
                    </div>
                  </div>
                ) : (
                  <p className="text-slate-500 dark:text-slate-400 text-sm italic py-8 text-center">
                    Simptomlarni tahlil qilish uchun chap tarafdagi formani to'ldiring.
                  </p>
                )}
              </div>
            </div>
          )}
        </main>
      </div>
    </div>
  );
}
