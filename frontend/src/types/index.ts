export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
}

export interface UserMe {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  dateOfBirth: string;
  gender: string;
  role: 'Patient' | 'Doctor' | 'ClinicAdmin' | 'SuperAdmin';
  preferredLanguage: string;
  patientId?: string;
  doctorId?: string;
}

export interface AuthResponse {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  role: 'Patient' | 'Doctor' | 'ClinicAdmin' | 'SuperAdmin';
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  patientId?: string;
  doctorId?: string;
}

export interface PatientProfile {
  id: string;
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  dateOfBirth: string;
  gender: string;
  bloodType: string;
  emergencyContactName: string;
  emergencyContactPhone: string;
  address: string;
  createdAt: string;
}

export interface HealthPassport {
  patientId: string;
  fullName: string;
  dateOfBirth: string;
  age: number;
  bloodType: string;
  gender: string;
  emergencyContactName: string;
  emergencyContactPhone: string;
  activeMedications: { name: string; dosage: string; frequency: string }[];
  recentLabResults: { testName: string; value: string; unit: string; status: string; testDate: string }[];
  activeConditions: { id: string; type: string; title: string; description: string; eventDate: string }[];
}

export interface TimelineItem {
  id: string;
  category: string;
  title: string;
  description: string;
  date: string;
  badgeColor: string;
}

export interface DoctorProfile {
  id: string;
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  specialization: string;
  licenseNumber: string;
  experienceYears: number;
  bio: string;
  clinicId: string;
  clinicName: string;
  isVerified: boolean;
}

export interface Appointment {
  id: string;
  patientId: string;
  patientName: string;
  doctorId: string;
  doctorName: string;
  doctorSpecialization: string;
  clinicId: string;
  clinicName: string;
  appointmentDate: string;
  startTime: string;
  endTime: string;
  status: 'Pending' | 'Confirmed' | 'Completed' | 'Cancelled' | 'NoShow';
  reason: string;
  notes: string;
  createdAt: string;
}

export interface MedicalRecord {
  id: string;
  patientId: string;
  patientName: string;
  doctorId: string;
  doctorName: string;
  appointmentId?: string;
  title: string;
  description: string;
  diagnosisNotes: string;
  createdAt: string;
}

export interface MedicalDocument {
  id: string;
  patientId: string;
  fileName: string;
  fileType: string;
  fileUrl: string;
  documentType: string;
  extractedText: string;
  aiSummary: string;
  uploadedAt: string;
}

export interface LabResult {
  id: string;
  patientId: string;
  patientName: string;
  doctorId: string;
  doctorName: string;
  testName: string;
  value: string;
  unit: string;
  referenceRange: string;
  status: 'Normal' | 'Abnormal' | 'Critical' | 'Pending';
  testDate: string;
  notes: string;
}

export interface Medication {
  id: string;
  patientId: string;
  name: string;
  dosage: string;
  frequency: string;
  startDate: string;
  endDate?: string;
  notes: string;
  createdAt: string;
}

export interface AIChatResponse {
  sessionId: string;
  response: string;
  safetyLevel: 'Safe' | 'Precaution' | 'EmergencyWarning';
  safetyMessage: string;
  createdAt: string;
}

export interface SymptomAnalysisResponse {
  summary: string;
  followUpQuestions: string[];
  riskLevel: 'Low' | 'Moderate' | 'High' | 'Emergency';
  recommendedNextStep: string;
  safetyMessage: string;
  potentialCauses: string[];
}

export interface DoctorBriefResponse {
  patientId: string;
  patientName: string;
  bloodType: string;
  age: number;
  gender: string;
  overview: string;
  activeMedications: string[];
  criticalLabAlerts: string[];
  recentAppointments: string[];
  recommendedClinicalFocus: string[];
}

export interface AdminStats {
  totalUsers: number;
  totalPatients: number;
  totalDoctors: number;
  totalClinics: number;
  totalAppointments: number;
  totalAiSessions: number;
  pendingAppointments: number;
  completedAppointments: number;
}
