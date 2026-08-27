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
  RefreshCw,
  Trash2,
  HelpCircle,
  Stethoscope,
  Heart,
  Thermometer,
  Pill,
  Apple
} from 'lucide-react';

export default function AIAssistantPage() {
  const { t } = useLanguage();
  const [activeTab, setActiveTab] = useState<'chat' | 'symptoms'>('chat');
  
  // Chat state
  const [messages, setMessages] = useState<Array<{ role: string; content: string; safetyLevel?: string }>>([
    {
      role: 'assistant',
      content: "👋 **Assalomu alaykum! Men MedAI intellektual tibbiy maslahatchisiman.**\n\nBugun salomatligingiz bo'yicha har qanday savolingizga javob berishga tayyorman:\n• Simptomlar va ularning sabablari\n• Qon bosimi, isitma, shamollash, og'riqlar\n• Dori-darmonlar va vitaminlar qabul qilish qoidalari\n• Laboratoriya tahlillari ko'rsatkichlari\n• Sog'lom ovqatlanish va birinchi yordam.",
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

  // Quick suggestion prompts
  const quickPrompts = [
    { label: "Qon bosimini tushirish", icon: Heart, prompt: "Qon bosimi 140/90 bo'lsa nima qilish kerak va qanday tushirish mumkin?" },
    { label: "Paratsetamol qoidalari", icon: Pill, prompt: "Paratsetamol qanday ichiladi va kunlik dozasi qancha?" },
    { label: "Isitma va shamollash", icon: Thermometer, prompt: "38.5 daraja isitma va shamollashda nima qilish kerak?" },
    { label: "Bosh og'rig'i yordami", icon: Stethoscope, prompt: "Bosh qattiq og'riyapti, qanday qilib tez yengillashtirish mumkin?" },
    { label: "Sog'lom ovqatlanish", icon: Apple, prompt: "Sog'lom turmush tarzi va parhez uchun qanday ovqatlanish qoidalariga rioya qilish kerak?" }
  ];

  const generateLocalAnswer = (text: string): { response: string; safetyLevel: string } => {
    const lower = text.toLowerCase();
    
    // Emergency check
    if (lower.includes('ko\'krak') && (lower.includes('og\'riq') || lower.includes('siqil')) || lower.includes('nafas qis') || lower.includes('hushdan ket')) {
      return {
        safetyLevel: 'EmergencyWarning',
        response: "🚨 **OGOHLANTIRISH: ZUDLIK BILAN TEZ YORDAM (103) CHAQIRING!**\n\nSiz bildirgan belgilar o'tkir kardiologik yoki nafas yetishmovchiligi holatiga to'g'ri kelishi mumkin.\n\n1. Darhol **103** yoki **112** ga qo'ng'iroq qiling.\n2. Bemorga yarim o'tirgan holat bering va xonani shamollating.\n3. Bo'yin va belni siqib turgan kiyimlarni bo'shating."
      };
    }

    if (lower.includes('bosim') || lower.includes('davleniya') || lower.includes('140/')) {
      return {
        safetyLevel: 'Safe',
        response: "🩺 **Arterial Qon Bosimi Bo'yicha Tavsiyalar:**\n\n• **Me'yoriy bosim:** 120/80 mmHg. 140/90 dan yuqori bo'lsa — yuqori bosim (gipertoniya) hisoblanadi.\n\n**Birlamchi choralar:**\n1. Tinchlaning, xonani shamollating va yarim o'tirgan holatda dam oling.\n2. Oyoqlarni iliq suvga solib o'tirish miyadagi qon bosimini kamaytirishga yordam beradi.\n3. Sho'r, qovurilgan ovqatlar, kofe va qattiq choydan saqlaning.\n4. Shifokor tayinlagan bosim dorilarini qabul qiling.\n\n⚠️ *Bosim 160/100 dan oshsa, zudlik bilan shifokorga murojaat qiling.*"
      };
    }

    if (lower.includes('dori') || lower.includes('paratsetamol') || lower.includes('ibuprofen')) {
      return {
        safetyLevel: 'Safe',
        response: "💊 **Dori Qabul Qilish Qoidalari:**\n\n• **Paratsetamol:** Tana harorati 38.5°C dan oshganda yoki yengil og'riqlarda ichiladi. Kattalar uchun 500 mg (kuniga ko'pi bilan 3-4 marta, orasi 4-6 soat).\n• **Ibuprofen (400 mg):** Yallig'lanishga qarshi va og'riq qoldiruvchi. Oshqozonni asrash uchun ovqatdan so'ng ichiladi.\n• **Muhim:** Spirtli ichimliklar bilan dorilarni aralashtirmang va bir nechta og'riq qoldiruvchini bir vaqtda ichmang."
      };
    }

    if (lower.includes('isitma') || lower.includes('shamollash') || lower.includes('harorat')) {
      return {
        safetyLevel: 'Safe',
        response: "🌡️ **Isitma va Shamollashda Amaliy Tavsiyalar:**\n\n1. **Ko'p iliq suyuqlik:** Na'matak damlamasi, limonli choy, iliq suv (kuniga 2-2.5 litr).\n2. **Harorat 38.5°C dan yuqori bo'lsa:** Paratsetamol yoki Ibuprofen qabul qiling.\n3. **Xona harorati:** 20-22°C va namligi 50-60% bo'lishi lozim.\n4. **Antibiotiklar:** O'zboshimchalik bilan ichilmaydi (virusli shamollashga ta'sir qilmaydi!)."
      };
    }

    if (lower.includes('rahmat') || lower.includes('tashakkur')) {
      return {
        safetyLevel: 'Safe',
        response: "🌟 **Arzimaydi! Salomatligingiz har doim birinchi o'rinda.**\n\nO'zingizni ehtiyot qiling. Yana qandaydir savollaringiz bo'lsa, bemalol so'rang!"
      };
    }

    return {
      safetyLevel: 'Safe',
      response: `💡 **MedAI Tibbiy Tahlili:**\n\nSizning so'rovingiz: **"${text}"**\n\n**1. Umumiy baholash:**\nUshbu holat fiziologik o'zgarishlar, charchoq, ovqatlanish tartibi yoki organizmdagi immunitet jarayonlari bilan bog'liq bo'lishi mumkin.\n\n**2. Amaliy tavsiyalar:**\n• Kun davomida yetarli miqdorda iliq suv iching va dam oling.\n• Ortiqcha jismoniy va psixologik zo'riqishdan saqlaning.\n• Agar holat 48 soat ichida yaxshilanmasa, tegishli mutaxassis (terapevt/kardiolog) ko'rigidan o'tish tavsiya etiladi.\n\n*(Eslatma: AI maslahati ma'lumot xarakteriga ega bo'lib, shifokor ko'rigi o'rnini bosa olmaydi.)*`
    };
  };

  const handleSendChat = async (e: React.FormEvent, customText?: string) => {
    if (e) e.preventDefault();
    const query = customText || inputMsg;
    if (!query.trim() || loadingChat) return;

    setInputMsg('');
    setMessages(prev => [...prev, { role: 'user', content: query }]);
    setLoadingChat(true);

    try {
      const res = await aiService.chat(query, sessionId);
      if (res && res.success && res.data) {
        setSessionId(res.data.sessionId);
        setMessages(prev => [
          ...prev,
          { role: 'assistant', content: res.data.response, safetyLevel: res.data.safetyLevel }
        ]);
      } else {
        const local = generateLocalAnswer(query);
        setMessages(prev => [
          ...prev,
          { role: 'assistant', content: local.response, safetyLevel: local.safetyLevel }
        ]);
      }
    } catch {
      const local = generateLocalAnswer(query);
      setMessages(prev => [
        ...prev,
        { role: 'assistant', content: local.response, safetyLevel: local.safetyLevel }
      ]);
    } finally {
      setLoadingChat(false);
    }
  };

  const handleClearChat = () => {
    setMessages([
      {
        role: 'assistant',
        content: "👋 Yangi suhbat boshlandi. Salomatligingiz bo'yicha qanday savolingiz bor?",
        safetyLevel: 'Safe'
      }
    ]);
    setSessionId(undefined);
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
    } catch {
      // Local fallback symptom analysis
      setSymptomResult({
        summary: `${age} yoshli bemor: '${symptoms}' (${duration} davomida).`,
        followUpQuestions: [
          "Qon bosimingizni o'lchab ko'rdingizmi?",
          "Tana haroratingiz 38°C dan yuqorimi?",
          "Nafas olishda qiyinchilik bormi?"
        ],
        riskLevel: "O'rtacha (Moderate)",
        recommendedNextStep: "Ko'proq iliq suyuqlik iching, dam oling va umumiy amaliyot shifokori (terapevt) ko'rigiga yoziling.",
        safetyMessage: "AI simptomlarni dastlabki toifalashga yordam beradi. Aniq tashxisni faqat shifokor qo'yadi.",
        potentialCauses: [
          "Virusli yoki mavsumiy nafas yo'llari infektsiyasi",
          "Funksional toliqish yoki qon bosimi o'zgarishi",
          "Mavsumiy allergik yallig'lanish"
        ]
      });
    } finally {
      setLoadingSymptoms(false);
    }
  };

  return (
    <div className="min-h-screen bg-slate-50 dark:bg-slate-950 text-slate-900 dark:text-slate-100 flex flex-col transition-colors duration-300">
      <Navbar />

      <div className="flex flex-1">
        <Sidebar />

        <main className="flex-1 p-4 md:p-8 overflow-y-auto max-w-7xl">
          {/* Header */}
          <div className="flex flex-wrap items-center justify-between gap-4 mb-6">
            <div>
              <h1 className="text-2xl md:text-3xl font-extrabold tracking-tight text-slate-900 dark:text-white flex items-center gap-3">
                <Bot className="w-8 h-8 text-cyan-500" /> {t.aiAssistant || 'AI Yordamchi'}
              </h1>
              <p className="text-xs md:text-sm text-slate-600 dark:text-slate-400 mt-1">
                Klinik intellektual maslahatchi: har qanday tibbiy savol, simptom va tahlillar tahlili.
              </p>
            </div>

            <div className="flex bg-slate-200 dark:bg-slate-900 p-1 rounded-2xl border border-slate-300 dark:border-slate-800 text-xs font-semibold">
              <button
                onClick={() => setActiveTab('chat')}
                className={`px-4 py-2 rounded-xl transition-colors ${
                  activeTab === 'chat' ? 'bg-cyan-500 text-white font-bold shadow-md shadow-cyan-500/20' : 'text-slate-600 dark:text-slate-400 hover:text-slate-900 dark:hover:text-white'
                }`}
              >
                {t.startAiChat || 'AI Bilan Chat'}
              </button>
              <button
                onClick={() => setActiveTab('symptoms')}
                className={`px-4 py-2 rounded-xl transition-colors ${
                  activeTab === 'symptoms' ? 'bg-cyan-500 text-white font-bold shadow-md shadow-cyan-500/20' : 'text-slate-600 dark:text-slate-400 hover:text-slate-900 dark:hover:text-white'
                }`}
              >
                {t.symptomAnalyzer || 'Simptom Tahlilchisi'}
              </button>
            </div>
          </div>

          {/* AI Safety Banner */}
          <div className="mb-6 p-4 rounded-2xl bg-cyan-50 dark:bg-cyan-950/40 border border-cyan-200 dark:border-cyan-800/60 text-xs text-cyan-900 dark:text-cyan-200 flex items-center gap-3 shadow-sm">
            <ShieldCheck className="w-5 h-5 text-cyan-500 shrink-0" />
            <div>
              <span className="font-bold block">{t.safetyDisclaimerTitle || 'Tibbiy Xavfsizlik Ogohlantirishi'}</span>
              {t.safetyDisclaimerDesc || "MedAI ma'lumot beruvchi intellektual yordamchi hisoblanadi. AI javoblari shifokor ko'rigi o'rnini bosa olmaydi."}
            </div>
          </div>

          {activeTab === 'chat' && (
            <div className="space-y-4">
              {/* Quick Suggestion Prompts */}
              <div className="flex items-center gap-2 overflow-x-auto pb-1 text-xs">
                <span className="text-slate-400 font-bold uppercase tracking-wider text-[10px] shrink-0">
                  Tezkor Savollar:
                </span>
                {quickPrompts.map((p, idx) => {
                  const Icon = p.icon;
                  return (
                    <button
                      key={idx}
                      onClick={() => handleSendChat(null as any, p.prompt)}
                      className="px-3 py-1.5 rounded-xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 text-slate-700 dark:text-slate-300 hover:border-cyan-500 hover:text-cyan-500 shrink-0 flex items-center gap-1.5 transition-colors"
                    >
                      <Icon className="w-3.5 h-3.5 text-cyan-500" />
                      {p.label}
                    </button>
                  );
                })}
              </div>

              {/* Chat Container */}
              <div className="bg-white dark:bg-slate-900/90 border border-slate-200 dark:border-slate-800 rounded-3xl h-[560px] flex flex-col overflow-hidden shadow-xl">
                {/* Chat Top Toolbar */}
                <div className="px-5 py-3 border-b border-slate-200 dark:border-slate-800 flex items-center justify-between bg-slate-50/50 dark:bg-slate-950/50 text-xs">
                  <div className="flex items-center gap-2 font-bold text-slate-700 dark:text-slate-300">
                    <Sparkles className="w-4 h-4 text-cyan-500" /> MedAI Intelligent Copilot
                  </div>
                  <button
                    onClick={handleClearChat}
                    className="flex items-center gap-1 text-slate-500 hover:text-red-500 transition-colors"
                    title="Suhbatni tozalash"
                  >
                    <Trash2 className="w-3.5 h-3.5" /> Suhbatni tozalash
                  </button>
                </div>

                {/* Messages Feed */}
                <div className="flex-1 p-6 overflow-y-auto space-y-4">
                  {messages.map((msg, index) => (
                    <div
                      key={index}
                      className={`flex ${msg.role === 'user' ? 'justify-end' : 'justify-start'}`}
                    >
                      <div
                        className={`max-w-2xl p-4 rounded-2xl text-sm leading-relaxed whitespace-pre-line ${
                          msg.role === 'user'
                            ? 'bg-blue-600 text-white rounded-br-none shadow-md'
                            : msg.safetyLevel === 'EmergencyWarning'
                            ? 'bg-red-50 dark:bg-red-950/80 border border-red-200 dark:border-red-800 text-red-900 dark:text-red-200 rounded-bl-none shadow-sm'
                            : 'bg-slate-100 dark:bg-slate-950 border border-slate-200 dark:border-slate-800 text-slate-800 dark:text-slate-200 rounded-bl-none shadow-sm'
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
                        <RefreshCw className="w-4 h-4 animate-spin" /> AI javob tayyorlamoqda...
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
                    placeholder="Har qanday tibbiy savol, dori yoki simptom haqida so'rang..."
                    className="flex-1 px-4 py-3 bg-white dark:bg-slate-900 border border-slate-300 dark:border-slate-800 rounded-xl text-sm text-slate-900 dark:text-white focus:outline-none focus:border-cyan-500 shadow-inner"
                  />
                  <button
                    type="submit"
                    disabled={loadingChat || !inputMsg.trim()}
                    className="px-5 py-3 bg-gradient-to-r from-blue-600 to-cyan-500 hover:from-blue-500 hover:to-cyan-400 disabled:opacity-50 text-white font-bold rounded-xl flex items-center gap-2 shadow-lg shadow-cyan-500/20 transition-all hover:scale-[1.02]"
                  >
                    <Send className="w-4 h-4" /> {t.send || 'Yuborish'}
                  </button>
                </form>
              </div>
            </div>
          )}

          {activeTab === 'symptoms' && (
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
              {/* Symptom Input Form */}
              <div className="p-6 rounded-3xl bg-white dark:bg-slate-900/90 border border-slate-200 dark:border-slate-800 shadow-xl">
                <h3 className="text-lg font-bold text-slate-900 dark:text-white mb-4 flex items-center gap-2">
                  <Activity className="w-5 h-5 text-cyan-500" /> {t.symptomAnalyzer || 'Simptom Tahlilchisi'}
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
                      placeholder="Masalan: 3 kundan beri quruq yo'tal, isitma 38.2 va bosh og'rig'i..."
                      className="w-full p-3.5 bg-slate-50 dark:bg-slate-950 border border-slate-300 dark:border-slate-800 rounded-xl text-sm text-slate-900 dark:text-white focus:outline-none focus:border-cyan-500"
                    />
                  </div>

                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-xs font-semibold text-slate-700 dark:text-slate-300 uppercase tracking-wider mb-2">
                        Davomiyligi
                      </label>
                      <input
                        type="text"
                        value={duration}
                        onChange={(e) => setDuration(e.target.value)}
                        className="w-full p-3 bg-slate-50 dark:bg-slate-950 border border-slate-300 dark:border-slate-800 rounded-xl text-sm text-slate-900 dark:text-white"
                        placeholder="2 kun"
                      />
                    </div>
                    <div>
                      <label className="block text-xs font-semibold text-slate-700 dark:text-slate-300 uppercase tracking-wider mb-2">
                        Bemor Yoshi
                      </label>
                      <input
                        type="number"
                        value={age}
                        onChange={(e) => setAge(Number(e.target.value))}
                        className="w-full p-3 bg-slate-50 dark:bg-slate-950 border border-slate-300 dark:border-slate-800 rounded-xl text-sm text-slate-900 dark:text-white"
                        placeholder="30"
                      />
                    </div>
                  </div>

                  <button
                    type="submit"
                    disabled={loadingSymptoms}
                    className="w-full py-3.5 px-4 bg-gradient-to-r from-cyan-500 to-blue-600 hover:from-cyan-400 hover:to-blue-500 text-white font-bold rounded-xl shadow-lg shadow-cyan-500/20 transition-all hover:scale-[1.01]"
                  >
                    {loadingSymptoms ? 'Tahlil qilinmoqda...' : (t.runSymptomTriage || 'Simptomlarni Tahlil Qilish')}
                  </button>
                </form>
              </div>

              {/* Symptom Result Display */}
              <div className="p-6 rounded-3xl bg-white dark:bg-slate-900/90 border border-slate-200 dark:border-slate-800 shadow-xl">
                <h3 className="text-lg font-bold text-slate-900 dark:text-white mb-4 flex items-center gap-2">
                  <Sparkles className="w-5 h-5 text-cyan-500" /> AI Klinik Xulosasi
                </h3>

                {symptomResult ? (
                  <div className="space-y-4 text-xs">
                    <div className="p-4 rounded-2xl bg-slate-50 dark:bg-slate-950 border border-slate-200 dark:border-slate-800">
                      <span className="text-slate-500 dark:text-slate-400 font-bold uppercase block mb-1">Xavf Darajasi</span>
                      <span className="text-base font-extrabold px-3 py-1 rounded-full inline-block bg-amber-500/20 text-amber-700 dark:text-amber-400">
                        {symptomResult.riskLevel}
                      </span>
                    </div>

                    <div className="p-4 rounded-2xl bg-slate-50 dark:bg-slate-950 border border-slate-200 dark:border-slate-800">
                      <span className="text-slate-500 dark:text-slate-400 font-bold uppercase block mb-2">Ehtimoliy Sabablar</span>
                      <ul className="space-y-1 text-slate-800 dark:text-slate-200">
                        {symptomResult.potentialCauses.map((c, i) => (
                          <li key={i} className="flex items-center gap-2">
                            <CheckCircle2 className="w-3.5 h-3.5 text-cyan-500 shrink-0" /> {c}
                          </li>
                        ))}
                      </ul>
                    </div>

                    <div className="p-4 rounded-2xl bg-slate-50 dark:bg-slate-950 border border-slate-200 dark:border-slate-800">
                      <span className="text-slate-500 dark:text-slate-400 font-bold uppercase block mb-2">Tavsiya Etiladigan Keyingi Qadam</span>
                      <p className="text-slate-800 dark:text-slate-200 leading-relaxed font-medium">
                        {symptomResult.recommendedNextStep}
                      </p>
                    </div>
                  </div>
                ) : (
                  <div className="py-16 text-center text-slate-500 dark:text-slate-400 text-sm italic">
                    Simptomlarni tahlil qilish uchun chap tarafdagi formani to'ldiring va tugmani bosing.
                  </div>
                )}
              </div>
            </div>
          )}
        </main>
      </div>
    </div>
  );
}
