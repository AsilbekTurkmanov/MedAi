'use client';

import React, { useEffect, useState } from 'react';
import Navbar from '@/components/Navbar';
import Sidebar from '@/components/Sidebar';
import { patientService, documentService } from '@/services/allServices';
import { MedicalDocument } from '@/types';
import { useLanguage } from '@/context/LanguageContext';
import { FolderOpen, Upload, FileText, Sparkles, Plus } from 'lucide-react';

export default function DocumentsPage() {
  const { t } = useLanguage();
  const [documents, setDocuments] = useState<MedicalDocument[]>([]);
  const [uploading, setUploading] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [docName, setDocName] = useState('');
  const [docType, setDocType] = useState('LabReport');

  useEffect(() => {
    patientService.getDocuments().then(res => {
      if (res.success) setDocuments(res.data);
    });
  }, []);

  const handleUploadSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!docName.trim()) return;
    setUploading(true);
    try {
      await documentService.upload({
        fileName: docName,
        documentType: docType,
        fileUrl: '/uploads/sample-scan.pdf'
      });
      setShowModal(false);
      setDocName('');
      const updated = await patientService.getDocuments();
      if (updated.success) setDocuments(updated.data);
    } catch (err) {
      console.error(err);
    } finally {
      setUploading(false);
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
                <FolderOpen className="w-8 h-8 text-purple-500" /> {t.digitalHealthDocs}
              </h1>
              <p className="text-sm text-slate-600 dark:text-slate-400 mt-1">
                Laboratoriya skanerlari va shifokor xulosalarini AI tahlilidan o'tkazish uchun yuklang.
              </p>
            </div>

            <button
              onClick={() => setShowModal(true)}
              className="px-5 py-3 bg-purple-600 hover:bg-purple-500 text-white font-bold text-sm rounded-xl shadow-lg shadow-purple-500/20 flex items-center gap-2"
            >
              <Plus className="w-4 h-4" /> {t.uploadDoc}
            </button>
          </div>

          <div className="space-y-6">
            {documents.length === 0 ? (
              <div className="py-16 text-center">
                <FolderOpen className="w-12 h-12 text-slate-300 dark:text-slate-600 mx-auto mb-4" />
                <p className="text-slate-500 dark:text-slate-400 text-sm">Hozircha yuklangan hujjatlar mavjud emas.</p>
              </div>
            ) : (
            documents.map((doc) => (
              <div key={doc.id} className="p-6 rounded-3xl bg-white dark:bg-slate-900/90 border border-slate-200 dark:border-slate-800 shadow-sm dark:shadow-xl space-y-4">
                <div className="flex flex-wrap justify-between items-start gap-4">
                  <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-xl bg-purple-500/10 text-purple-500 flex items-center justify-center font-bold">
                      <FileText className="w-5 h-5" />
                    </div>
                    <div>
                      <h3 className="text-base font-bold text-slate-900 dark:text-white">{doc.fileName}</h3>
                      <span className="text-xs text-slate-500 dark:text-slate-400">
                        Turi: {doc.documentType} • Yuklangan: {new Date(doc.uploadedAt).toLocaleDateString()}
                      </span>
                    </div>
                  </div>
                  <span className="px-3 py-1 rounded-full bg-cyan-500/10 text-cyan-600 dark:text-cyan-400 text-xs font-bold border border-cyan-500/30">
                    {t.ocrProcessed}
                  </span>
                </div>

                <div className="p-4 rounded-2xl bg-slate-50 dark:bg-slate-950 border border-slate-200 dark:border-slate-800 text-xs space-y-2">
                  <div className="flex items-center gap-2 text-cyan-600 dark:text-cyan-400 font-bold">
                    <Sparkles className="w-3.5 h-3.5" /> {t.aiExecSummary}
                  </div>
                  <p className="text-slate-800 dark:text-slate-200 leading-relaxed">{doc.aiSummary}</p>
                </div>

                <div className="p-3 rounded-xl bg-slate-100 dark:bg-slate-950/60 border border-slate-200 dark:border-slate-800/60 text-[11px] text-slate-500 dark:text-slate-400 italic">
                  Matnli Ko'rinish: {doc.extractedText}
                </div>
              </div>
            ))
            )}
          </div>

          {/* Upload Modal */}
          {showModal && (
            <div className="fixed inset-0 bg-slate-950/70 backdrop-blur-sm z-50 flex items-center justify-center p-4">
              <div className="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-6 rounded-3xl w-full max-w-md shadow-2xl">
                <h3 className="text-xl font-bold text-slate-900 dark:text-white mb-4">Hujjat Yuklash & AI Tahlil</h3>

                <form onSubmit={handleUploadSubmit} className="space-y-4 text-xs">
                  <div>
                    <label className="block text-slate-700 dark:text-slate-300 font-semibold mb-1">Hujjat Nomi</label>
                    <input
                      type="text"
                      required
                      value={docName}
                      onChange={(e) => setDocName(e.target.value)}
                      placeholder="Qon tahlili skaneri 2026..."
                      className="w-full p-3 bg-slate-50 dark:bg-slate-950 border border-slate-300 dark:border-slate-800 rounded-xl text-slate-900 dark:text-white text-sm"
                    />
                  </div>

                  <div>
                    <label className="block text-slate-700 dark:text-slate-300 font-semibold mb-1">Hujjat Turi</label>
                    <select
                      value={docType}
                      onChange={(e) => setDocType(e.target.value)}
                      className="w-full p-3 bg-slate-50 dark:bg-slate-950 border border-slate-300 dark:border-slate-800 rounded-xl text-slate-900 dark:text-white text-sm"
                    >
                      <option value="LabReport">Tahlil Xulosasi (Lab Report)</option>
                      <option value="DischargeSummary">Kasalxona Xulosasi (Discharge Summary)</option>
                      <option value="Prescription">Retsept (Prescription)</option>
                      <option value="ImagingReport">MRT / Rentgen (Imaging Report)</option>
                    </select>
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
                      disabled={uploading}
                      className="px-5 py-2.5 bg-purple-600 hover:bg-purple-500 text-white rounded-xl font-bold shadow-lg shadow-purple-500/20"
                    >
                      {uploading ? 'Tahlil qilinmoqda...' : 'Yuklash & AI OCR'}
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
