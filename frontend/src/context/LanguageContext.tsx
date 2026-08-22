'use client';

import React, { createContext, useContext, useState, useEffect } from 'react';

export type Language = 'uz' | 'ru' | 'en';

export interface Translations {
  // Navigation
  patientPortal: string;
  aiAssistant: string;
  doctorPortal: string;
  adminPortal: string;
  signIn: string;
  getStarted: string;
  signOut: string;
  healthPassport: string;
  healthTimeline: string;
  labResults: string;
  medications: string;
  documents: string;
  appointments: string;
  familyHub: string;
  overview: string;
  patientRoster: string;
  userDirectory: string;
  clinicManagement: string;
  auditLogs: string;

  // Hero & General
  heroTag: string;
  heroTitle1: string;
  heroTitleHighlight: string;
  heroDesc: string;
  launchPatientPortal: string;
  doctorCopilot: string;
  demoPatient: string;
  demoDoctor: string;
  demoAdmin: string;

  // AI & Safety
  aiSafetyNoticeTitle: string;
  aiSafetyNoticeDesc: string;
  safetyDisclaimerTitle: string;
  safetyDisclaimerDesc: string;
  startAiChat: string;
  symptomAnalyzer: string;
  explainLabResult: string;
  doctorBrief: string;
  askAiPlaceholder: string;
  send: string;
  runSymptomTriage: string;

  // Auth
  welcomeBack: string;
  createAccount: string;
  emailLabel: string;
  passwordLabel: string;
  firstNameLabel: string;
  lastNameLabel: string;
  phoneNumberLabel: string;
  accountRoleLabel: string;
  patientRole: string;
  doctorRole: string;

  // Dashboard & Clinical
  patientOverviewTitle: string;
  bloodType: string;
  activeMedications: string;
  recentLabPanels: string;
  upcomingAppointments: string;
  bookAppointment: string;
  viewFullReports: string;
  manageAll: string;
  noAppointments: string;

  // Health Passport
  passportTitle: string;
  passportDesc: string;
  verifiedPassport: string;
  emergencyContact: string;
  activeAllergies: string;
  primaryClinic: string;

  // Documents & Labs
  digitalHealthDocs: string;
  uploadDoc: string;
  ocrProcessed: string;
  aiExecSummary: string;
  plainLangSummary: string;
  questionsForDoctor: string;

  // Doctor & Admin
  doctorHubTitle: string;
  patientSchedule: string;
  recommendedFocus: string;
  platformAdmin: string;
  totalUsers: string;
  verifiedDoctors: string;
  aiSessions: string;
  systemStatus: string;
}

const translations: Record<Language, Translations> = {
  uz: {
    // Navigation
    patientPortal: "Bemor Portali",
    aiAssistant: "AI Yordamchi",
    doctorPortal: "Shifokor Portali",
    adminPortal: "Admin Panel",
    signIn: "Tizimga kirish",
    getStarted: "Boshlash",
    signOut: "Chiqish",
    healthPassport: "Salomatlik Pasporti",
    healthTimeline: "Salomatlik Xronologiyasi",
    labResults: "Tahlil Natijalari",
    medications: "Dori-darmonlar",
    documents: "Hujjatlar",
    appointments: "Qabullar",
    familyHub: "Oila Portali",
    overview: "Umumiy Sharh",
    patientRoster: "Bemorlar Ro'yxati",
    userDirectory: "Foydalanuvchilar Katalogi",
    clinicManagement: "Klinikalar Boshqaruvi",
    auditLogs: "Audit Jurnali",

    // Hero & General
    heroTag: "Yangi Avlod AI Tibbiy Platformasi",
    heroTitle1: "Sizning Salomatligingiz.",
    heroTitleHighlight: "AI Bilan Aqlliroq.",
    heroDesc: "MEDAI — Bemorlar, Shifokorlar, Klinikalar va tibbiy ma'lumotlarni sun'iy intellekt orqali yagona ekotizimga birlashtiruvchi professional HealthTech platforma.",
    launchPatientPortal: "Bemor Portalini Ochish",
    doctorCopilot: "Shifokor Kopiloti",
    demoPatient: "Demo Bemor",
    demoDoctor: "Demo Shifokor",
    demoAdmin: "Demo Admin",

    // AI & Safety
    aiSafetyNoticeTitle: "AI Xavfsizlik Eslatmasi",
    aiSafetyNoticeDesc: "MEDAI intellektual yordamchi hisoblanadi. AI javoblari shifokor ko'rigi o'rnini bosa olmaydi.",
    safetyDisclaimerTitle: "Tibbiy Xavfsizlik Ogohlantirishi",
    safetyDisclaimerDesc: "MedAI ma'lumot beruvchi va tavsiyaviy yordamchi hisoblanadi. AI hech qachon mustaqil yakuniy tashxis qo'ymaydi va dori yozmaydi.",
    startAiChat: "AI Bilan Chat",
    symptomAnalyzer: "Simptom Tahlilchisi",
    explainLabResult: "Tahlilni Tushuntirish",
    doctorBrief: "Shifokor Xulosasi",
    askAiPlaceholder: "Simptomlar, tahlil ko'rsatkichlari yoki tibbiy atamalar haqida so'rang...",
    send: "Yuborish",
    runSymptomTriage: "Simptomlarni Tahlil Qilish",

    // Auth
    welcomeBack: "MEDAI Platformasiga Xush Kelibsiz",
    createAccount: "MEDAI Akkaunt Yaratish",
    emailLabel: "Email Manzil",
    passwordLabel: "Parol",
    firstNameLabel: "Ism",
    lastNameLabel: "Familiya",
    phoneNumberLabel: "Telefon Raqam",
    accountRoleLabel: "Rol",
    patientRole: "Bemor",
    doctorRole: "Shifokor",

    // Dashboard & Clinical
    patientOverviewTitle: "Bemor Salomatlik Sharhi",
    bloodType: "Qon Guruhi",
    activeMedications: "Faol Dorilar",
    recentLabPanels: "So'nggi Tahlillar",
    upcomingAppointments: "Kutilayotgan Qabullar",
    bookAppointment: "Qabulga Yozilish",
    viewFullReports: "Barcha Tahlillarni Ko'rish",
    manageAll: "Barchasini Boshqarish",
    noAppointments: "Rejalashtirilgan qabullar mavjud emas.",

    // Health Passport
    passportTitle: "Bemor Salomatlik Pasporti",
    passportDesc: "Tezkor tibbiy ma'lumot va favqulodda vaziyatlar uchun rasmiy raqamli pasport.",
    verifiedPassport: "Tasdiqlangan MedAI Pasport",
    emergencyContact: "Favqulodda Aloqa",
    activeAllergies: "Mavjud Allergiyalar",
    primaryClinic: "Asosiy Klinika",

    // Documents & Labs
    digitalHealthDocs: "Raqamli Tibbiy Hujjatlar",
    uploadDoc: "Hujjat Yuklash",
    ocrProcessed: "AI Tahlildan O'tgan",
    aiExecSummary: "AI Qisqacha Xulosasi",
    plainLangSummary: "Ommabop Tildagi Sharh",
    questionsForDoctor: "Shifokorga Beriladigan Savollar",

    // Doctor & Admin
    doctorHubTitle: "Shifokor Klinik Hubi",
    patientSchedule: "Qabul Jadvali",
    recommendedFocus: "Tavsiya Etilgan Diqqat Markazi",
    platformAdmin: "Platforma Admin Paneli",
    totalUsers: "Jami Foydalanuvchilar",
    verifiedDoctors: "Tasdiqlangan Shifokorlar",
    aiSessions: "AI Seanslari",
    systemStatus: "Tizim Xavfsizlik Holati"
  },
  ru: {
    // Navigation
    patientPortal: "Портал Пациента",
    aiAssistant: "AI Помощник",
    doctorPortal: "Портал Врача",
    adminPortal: "Панель Админа",
    signIn: "Войти",
    getStarted: "Начать",
    signOut: "Выйти",
    healthPassport: "Паспорт Здоровья",
    healthTimeline: "Хронология Здоровья",
    labResults: "Результаты Анализов",
    medications: "Лекарства",
    documents: "Документы",
    appointments: "Записи к Врачу",
    familyHub: "Семейный Портал",
    overview: "Обзор",
    patientRoster: "Список Пациентов",
    userDirectory: "Каталог Пользователей",
    clinicManagement: "Управление Клиниками",
    auditLogs: "Журнал Аудита",

    // Hero & General
    heroTag: "Платформа Здравоохранения Нового Поколения",
    heroTitle1: "Ваше Здоровье.",
    heroTitleHighlight: "Умнее с ИИ.",
    heroDesc: "MEDAI — профессиональная платформа HealthTech, объединяющая Пациентов, Врачей, Клиники и медицинские данные через ИИ.",
    launchPatientPortal: "Открыть Портал Пациента",
    doctorCopilot: "Копилот Врача",
    demoPatient: "Демо Пациент",
    demoDoctor: "Демо Врач",
    demoAdmin: "Демо Админ",

    // AI & Safety
    aiSafetyNoticeTitle: "Уведомление Безопасности ИИ",
    aiSafetyNoticeDesc: "MEDAI является клиническим помощником. Ответы ИИ не заменяют консультацию врача.",
    safetyDisclaimerTitle: "Предупреждение о Безопасности",
    safetyDisclaimerDesc: "MedAI предоставляет справочную информацию. ИИ никогда не ставит окончательный диагноз и не назначает лекарства.",
    startAiChat: "Чат с ИИ",
    symptomAnalyzer: "Анализатор Симптомов",
    explainLabResult: "Объяснить Анализ",
    doctorBrief: "Сводка Врача",
    askAiPlaceholder: "Спросите о симптомах, анализах или терминах...",
    send: "Отправить",
    runSymptomTriage: "Анализировать Симптомы",

    // Auth
    welcomeBack: "Добро пожаловать в MEDAI",
    createAccount: "Создать Аккаунт MEDAI",
    emailLabel: "Электронная Почта",
    passwordLabel: "Пароль",
    firstNameLabel: "Имя",
    lastNameLabel: "Фамилия",
    phoneNumberLabel: "Номер Телефона",
    accountRoleLabel: "Роль",
    patientRole: "Пациент",
    doctorRole: "Врач",

    // Dashboard & Clinical
    patientOverviewTitle: "Обзор Здоровья Пациента",
    bloodType: "Группа Крови",
    activeMedications: "Активные Лекарства",
    recentLabPanels: "Последние Анализы",
    upcomingAppointments: "Предстоящие Приемы",
    bookAppointment: "Записаться к Врачу",
    viewFullReports: "Смотреть Все Анализы",
    manageAll: "Управлять Всеми",
    noAppointments: "Запланированные приемы отсутствуют.",

    // Health Passport
    passportTitle: "Паспорт Здоровья Пациента",
    passportDesc: "Официальный цифровой медицинский ID для экстренных ситуаций.",
    verifiedPassport: "Подтвержденный Паспорт MedAI",
    emergencyContact: "Экстренный Контакт",
    activeAllergies: "Аллергии",
    primaryClinic: "Основная Клиника",

    // Documents & Labs
    digitalHealthDocs: "Цифровые Документы",
    uploadDoc: "Загрузить Документ",
    ocrProcessed: "Обработано ИИ",
    aiExecSummary: "Краткое Резюме ИИ",
    plainLangSummary: "Понятное Объяснение",
    questionsForDoctor: "Вопросы к Врачу",

    // Doctor & Admin
    doctorHubTitle: "Клинический Хаб Врача",
    patientSchedule: "Расписание Приемов",
    recommendedFocus: "Рекомендуемый Фокус",
    platformAdmin: "Администрирование Платформы",
    totalUsers: "Всего Пользователей",
    verifiedDoctors: "Врачи",
    aiSessions: "ИИ Сессии",
    systemStatus: "Безопасность Системы"
  },
  en: {
    // Navigation
    patientPortal: "Patient Portal",
    aiAssistant: "AI Assistant",
    doctorPortal: "Doctor Portal",
    adminPortal: "Admin Portal",
    signIn: "Sign In",
    getStarted: "Get Started",
    signOut: "Sign Out",
    healthPassport: "Health Passport",
    healthTimeline: "Health Timeline",
    labResults: "Lab Results",
    medications: "Medications",
    documents: "Documents",
    appointments: "Appointments",
    familyHub: "Family Hub",
    overview: "Overview",
    patientRoster: "Patient Roster",
    userDirectory: "User Directory",
    clinicManagement: "Clinic Management",
    auditLogs: "Audit Logs",

    // Hero & General
    heroTag: "Next-Gen AI Healthcare Platform",
    heroTitle1: "Your Health.",
    heroTitleHighlight: "Smarter with AI.",
    heroDesc: "MEDAI is an intelligent healthcare ecosystem unifying Patients, Doctors, Clinics, and medical data through clinical AI.",
    launchPatientPortal: "Launch Patient Portal",
    doctorCopilot: "Doctor Copilot",
    demoPatient: "Demo Patient",
    demoDoctor: "Demo Doctor",
    demoAdmin: "Demo Admin",

    // AI & Safety
    aiSafetyNoticeTitle: "AI Safety Notice",
    aiSafetyNoticeDesc: "MEDAI is an intelligent healthcare assistant. AI responses do not replace professional clinical advice.",
    safetyDisclaimerTitle: "Clinical Safety Disclaimer",
    safetyDisclaimerDesc: "MedAI provides educational clarification. AI NEVER issues formal medical diagnoses or prescribes medication independently.",
    startAiChat: "Start AI Chat",
    symptomAnalyzer: "Symptom Analyzer",
    explainLabResult: "Explain Lab Result",
    doctorBrief: "Doctor Brief",
    askAiPlaceholder: "Ask about symptoms, lab ranges, or general medical terms...",
    send: "Send",
    runSymptomTriage: "Run Symptom Triage",

    // Auth
    welcomeBack: "Welcome to MEDAI",
    createAccount: "Create your MEDAI Account",
    emailLabel: "Email Address",
    passwordLabel: "Password",
    firstNameLabel: "First Name",
    lastNameLabel: "Last Name",
    phoneNumberLabel: "Phone Number",
    accountRoleLabel: "Account Role",
    patientRole: "Patient",
    doctorRole: "Doctor",

    // Dashboard & Clinical
    patientOverviewTitle: "Patient Overview",
    bloodType: "Blood Type",
    activeMedications: "Active Medications",
    recentLabPanels: "Recent Lab Panels",
    upcomingAppointments: "Upcoming Appointments",
    bookAppointment: "Book Appointment",
    viewFullReports: "View Full Reports",
    manageAll: "Manage All",
    noAppointments: "No upcoming appointments scheduled.",

    // Health Passport
    passportTitle: "Patient Health Passport",
    passportDesc: "Your official digital medical ID card for quick clinical reference and emergency identification.",
    verifiedPassport: "Verified MedAI Passport",
    emergencyContact: "Emergency Contact",
    activeAllergies: "Active Allergies",
    primaryClinic: "Primary Care Clinic",

    // Documents & Labs
    digitalHealthDocs: "Digital Health Documents",
    uploadDoc: "Upload Document",
    ocrProcessed: "AI OCR Processed",
    aiExecSummary: "AI Executive Summary",
    plainLangSummary: "Plain-Language Summary",
    questionsForDoctor: "Questions to Discuss with Doctor",

    // Doctor & Admin
    doctorHubTitle: "Doctor Clinical Hub",
    patientSchedule: "Patient Schedule",
    recommendedFocus: "Recommended Clinical Focus",
    platformAdmin: "Platform Administration",
    totalUsers: "Total Users",
    verifiedDoctors: "Verified Doctors",
    aiSessions: "AI Sessions",
    systemStatus: "System Security Status"
  }
};

interface LanguageContextType {
  language: Language;
  setLanguage: (lang: Language) => void;
  t: Translations;
}

const LanguageContext = createContext<LanguageContextType | undefined>(undefined);

export const LanguageProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [language, setLanguageState] = useState<Language>('uz');

  useEffect(() => {
    if (typeof window !== 'undefined') {
      const saved = localStorage.getItem('medai_lang') as Language;
      if (saved && (saved === 'uz' || saved === 'ru' || saved === 'en')) {
        setLanguageState(saved);
      }
    }
  }, []);

  const setLanguage = (lang: Language) => {
    setLanguageState(lang);
    if (typeof window !== 'undefined') {
      localStorage.setItem('medai_lang', lang);
    }
  };

  return (
    <LanguageContext.Provider value={{ language, setLanguage, t: translations[language] }}>
      {children}
    </LanguageContext.Provider>
  );
};

export const useLanguage = (): LanguageContextType => {
  const context = useContext(LanguageContext);
  if (!context) {
    throw new Error('useLanguage must be used within a LanguageProvider');
  }
  return context;
};
