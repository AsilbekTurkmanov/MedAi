'use client';

import React, { useEffect, useState } from 'react';
import Navbar from '@/components/Navbar';
import Sidebar from '@/components/Sidebar';
import { patientService, doctorService, appointmentService } from '@/services/allServices';
import { Appointment, DoctorProfile } from '@/types';
import { useLanguage } from '@/context/LanguageContext';
import { Calendar, Stethoscope, Clock, Plus } from 'lucide-react';

export default function AppointmentsPage() {
  const { t } = useLanguage();
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [doctors, setDoctors] = useState<DoctorProfile[]>([]);
  const [showModal, setShowModal] = useState(false);
  const [selectedDoctor, setSelectedDoctor] = useState('');
  const [date, setDate] = useState('2026-08-25');
  const [time, setTime] = useState('10:00:00');
  const [reason, setReason] = useState('Yillik Profilaktik Ko\'rik (Routine Checkup)');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    patientService.getAppointments().then(res => {
      if (res.success) setAppointments(res.data);
    });
    doctorService.getAll().then(res => {
      if (res.success) {
        setDoctors(res.data);
        if (res.data.length > 0) setSelectedDoctor(res.data[0].id);
      }
    });
  }, []);

  const handleBook = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedDoctor) return;
    setLoading(true);

    const docObj = doctors.find(d => d.id === selectedDoctor);
    const newAppt: Appointment = {
      id: Date.now().toString(),
      patientId: 'patient-1',
      patientName: 'Jane Doe',
      doctorId: selectedDoctor,
      doctorName: docObj ? `Dr. ${docObj.firstName} ${docObj.lastName}` : 'Dr. Jamshid Alimov',
      doctorSpecialization: docObj?.specialization || 'Cardiology',
      clinicId: docObj?.clinicId || '',
      clinicName: docObj?.clinicName || 'Central Clinic',
      appointmentDate: date,
      startTime: time,
      endTime: '10:30:00',
      status: 'Pending',
      reason,
      notes: '',
      createdAt: new Date().toISOString()
    };

    try {
      const res = await appointmentService.create({
        doctorId: selectedDoctor,
        clinicId: docObj?.clinicId,
        appointmentDate: date,
        startTime: time,
        endTime: '10:30:00',
        reason
      });

      if (res && res.success && res.data) {
        setAppointments(prev => [res.data, ...prev]);
      } else {
        setAppointments(prev => [newAppt, ...prev]);
      }
    } catch (err) {
      console.error('Failed to book appointment via API, adding locally:', err);
      setAppointments(prev => [newAppt, ...prev]);
    } finally {
      setShowModal(false);
      setLoading(false);
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
                <Calendar className="w-8 h-8 text-blue-500" /> {t.appointments}
              </h1>
              <p className="text-sm text-slate-600 dark:text-slate-400 mt-1">
                Tibbiy mutaxassislar va klinikalar bilan onlayn qabul va uchrashuvlarni band qiling.
              </p>
            </div>

            <button
              onClick={() => setShowModal(true)}
              className="px-5 py-3 bg-gradient-to-r from-blue-600 to-cyan-500 hover:from-blue-500 hover:to-cyan-400 text-white font-bold text-sm rounded-xl shadow-lg shadow-blue-500/20 flex items-center gap-2"
            >
              <Plus className="w-4 h-4" /> {t.bookAppointment}
            </button>
          </div>

          {/* Appointments Grid */}
          <div className="space-y-4">
            {appointments.length === 0 ? (
              <div className="py-16 text-center">
                <Calendar className="w-12 h-12 text-slate-300 dark:text-slate-600 mx-auto mb-4" />
                <p className="text-slate-500 dark:text-slate-400 text-sm">Hozircha rejalashtirilgan qabullar mavjud emas.</p>
              </div>
            ) : (
            appointments.map((appt) => (
              <div key={appt.id} className="p-6 rounded-3xl bg-white dark:bg-slate-900/90 border border-slate-200 dark:border-slate-800 shadow-sm flex flex-wrap items-center justify-between gap-4">
                <div className="flex items-center gap-4">
                  <div className="w-12 h-12 rounded-2xl bg-blue-500/10 text-blue-500 flex items-center justify-center font-bold">
                    <Stethoscope className="w-6 h-6" />
                  </div>
                  <div>
                    <span className="text-xs text-blue-600 dark:text-blue-400 font-bold uppercase tracking-wider block">
                      {appt.doctorSpecialization}
                    </span>
                    <h3 className="text-lg font-bold text-slate-900 dark:text-white">{appt.doctorName}</h3>
                    <span className="text-xs text-slate-600 dark:text-slate-400 block mt-0.5">{appt.clinicName} • Sabab: {appt.reason}</span>
                  </div>
                </div>

                <div className="text-right">
                  <span className="text-sm font-extrabold text-cyan-600 dark:text-cyan-300 block">
                    {new Date(appt.appointmentDate).toLocaleDateString()} — {appt.startTime}
                  </span>
                  <span className={`mt-1 inline-block text-[10px] font-bold px-2.5 py-0.5 rounded-full uppercase ${
                    appt.status === 'Confirmed' ? 'bg-emerald-500/20 text-emerald-600 dark:text-emerald-400' : 'bg-amber-500/20 text-amber-600 dark:text-amber-400'
                  }`}>
                    {appt.status}
                  </span>
                </div>
              </div>
            ))
            )}
          </div>

          {/* Booking Modal */}
          {showModal && (
            <div className="fixed inset-0 bg-slate-950/70 backdrop-blur-sm z-50 flex items-center justify-center p-4">
              <div className="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-6 rounded-3xl w-full max-w-md shadow-2xl">
                <h3 className="text-xl font-bold text-slate-900 dark:text-white mb-4">Shifokor Qabuliga Yozilish</h3>

                <form onSubmit={handleBook} className="space-y-4 text-xs">
                  <div>
                    <label className="block text-slate-700 dark:text-slate-300 font-semibold mb-1">Shifokorni Tanlang</label>
                    <select
                      value={selectedDoctor}
                      onChange={(e) => setSelectedDoctor(e.target.value)}
                      className="w-full p-3 bg-slate-50 dark:bg-slate-950 border border-slate-300 dark:border-slate-800 rounded-xl text-slate-900 dark:text-white text-sm"
                    >
                      {doctors.map(d => (
                        <option key={d.id} value={d.id}>
                          Dr. {d.firstName} {d.lastName} ({d.specialization})
                        </option>
                      ))}
                    </select>
                  </div>

                  <div>
                    <label className="block text-slate-700 dark:text-slate-300 font-semibold mb-1">Sana</label>
                    <input
                      type="date"
                      value={date}
                      onChange={(e) => setDate(e.target.value)}
                      className="w-full p-3 bg-slate-50 dark:bg-slate-950 border border-slate-300 dark:border-slate-800 rounded-xl text-slate-900 dark:text-white text-sm"
                    />
                  </div>

                  <div>
                    <label className="block text-slate-700 dark:text-slate-300 font-semibold mb-1">Qabul Sababi</label>
                    <input
                      type="text"
                      required
                      value={reason}
                      onChange={(e) => setReason(e.target.value)}
                      className="w-full p-3 bg-slate-50 dark:bg-slate-950 border border-slate-300 dark:border-slate-800 rounded-xl text-slate-900 dark:text-white text-sm"
                      placeholder="Profilaktik ko'rik..."
                    />
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
                      disabled={loading}
                      className="px-5 py-2.5 bg-blue-600 hover:bg-blue-500 text-white rounded-xl font-bold shadow-lg shadow-blue-500/20"
                    >
                      {loading ? 'Yuborilmoqda...' : 'Qabulni Tasdiqlash'}
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
