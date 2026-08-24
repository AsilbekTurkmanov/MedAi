import { api } from './api';
import {
  ApiResponse,
  LoginRequest,
  RegisterRequest,
  AuthResponse,
  UserMe,
  HealthPassport,
  Appointment,
  LabResult,
  Medication,
  MedicalDocument,
  TimelineItem,
  DoctorProfile,
  PatientProfile,
  AIChatResponse,
  SymptomAnalysisResponse,
  DoctorBriefResponse,
  AdminStats,
  Allergy,
  ChronicCondition,
  Vaccination,
  DataConsent,
  DataAccessLog,
  DoctorAvailability,
  AIHandoffSummary,
  SearchResult
} from '@/types';

export const authService = {
  async login(data: LoginRequest): Promise<ApiResponse<AuthResponse>> {
    const res = await api.post<ApiResponse<AuthResponse>>('/auth/login', data);
    if (res.data.success && res.data.data.accessToken) {
      localStorage.setItem('medai_token', res.data.data.accessToken);
    }
    return res.data;
  },

  async register(data: RegisterRequest): Promise<ApiResponse<AuthResponse>> {
    const res = await api.post<ApiResponse<AuthResponse>>('/auth/register', data);
    if (res.data.success && res.data.data.accessToken) {
      localStorage.setItem('medai_token', res.data.data.accessToken);
    }
    return res.data;
  },

  async getMe(): Promise<ApiResponse<UserMe>> {
    const res = await api.get<ApiResponse<UserMe>>('/auth/me');
    return res.data;
  },

  async logout(): Promise<void> {
    try {
      await api.post('/auth/logout');
    } finally {
      localStorage.removeItem('medai_token');
    }
  }
};

export const patientService = {
  async getHealthPassport(): Promise<ApiResponse<HealthPassport>> {
    const res = await api.get<ApiResponse<HealthPassport>>('/patients/me/health-passport');
    return res.data;
  },

  async getAppointments(): Promise<ApiResponse<Appointment[]>> {
    const res = await api.get<ApiResponse<Appointment[]>>('/patients/me/appointments');
    return res.data;
  },

  async getLabResults(): Promise<ApiResponse<LabResult[]>> {
    const res = await api.get<ApiResponse<LabResult[]>>('/patients/me/lab-results');
    return res.data;
  },

  async getMedications(): Promise<ApiResponse<Medication[]>> {
    const res = await api.get<ApiResponse<Medication[]>>('/patients/me/medications');
    return res.data;
  },

  async getDocuments(): Promise<ApiResponse<MedicalDocument[]>> {
    const res = await api.get<ApiResponse<MedicalDocument[]>>('/patients/me/documents');
    return res.data;
  },

  async getTimeline(): Promise<ApiResponse<TimelineItem[]>> {
    const res = await api.get<ApiResponse<TimelineItem[]>>('/patients/me/timeline');
    return res.data;
  },

  // Allergies
  async getAllergies(): Promise<ApiResponse<Allergy[]>> {
    const res = await api.get<ApiResponse<Allergy[]>>('/patients/me/allergies');
    return res.data;
  },

  async addAllergy(data: { name: string; severity: string; reaction: string; diagnosedDate?: string }): Promise<ApiResponse<Allergy>> {
    const res = await api.post<ApiResponse<Allergy>>('/patients/me/allergies', data);
    return res.data;
  },

  async deleteAllergy(id: string): Promise<ApiResponse<boolean>> {
    const res = await api.delete<ApiResponse<boolean>>(`/patients/me/allergies/${id}`);
    return res.data;
  },

  // Chronic Conditions
  async getChronicConditions(): Promise<ApiResponse<ChronicCondition[]>> {
    const res = await api.get<ApiResponse<ChronicCondition[]>>('/patients/me/chronic-conditions');
    return res.data;
  },

  async addChronicCondition(data: { name: string; status: string; notes?: string; diagnosedDate?: string }): Promise<ApiResponse<ChronicCondition>> {
    const res = await api.post<ApiResponse<ChronicCondition>>('/patients/me/chronic-conditions', data);
    return res.data;
  },

  async deleteChronicCondition(id: string): Promise<ApiResponse<boolean>> {
    const res = await api.delete<ApiResponse<boolean>>(`/patients/me/chronic-conditions/${id}`);
    return res.data;
  },

  // Vaccinations
  async getVaccinations(): Promise<ApiResponse<Vaccination[]>> {
    const res = await api.get<ApiResponse<Vaccination[]>>('/patients/me/vaccinations');
    return res.data;
  },

  async addVaccination(data: { name: string; dateAdministered: string; provider: string; doseNumber?: number; notes?: string }): Promise<ApiResponse<Vaccination>> {
    const res = await api.post<ApiResponse<Vaccination>>('/patients/me/vaccinations', data);
    return res.data;
  },

  async deleteVaccination(id: string): Promise<ApiResponse<boolean>> {
    const res = await api.delete<ApiResponse<boolean>>(`/patients/me/vaccinations/${id}`);
    return res.data;
  }
};

export const consentService = {
  async getMyConsents(): Promise<ApiResponse<DataConsent[]>> {
    const res = await api.get<ApiResponse<DataConsent[]>>('/consent/my-consents');
    return res.data;
  },

  async grantConsent(data: { grantToUserId: string; scope: string; expiresAt?: string }): Promise<ApiResponse<DataConsent>> {
    const res = await api.post<ApiResponse<DataConsent>>('/consent/grant', data);
    return res.data;
  },

  async revokeConsent(consentId: string): Promise<ApiResponse<boolean>> {
    const res = await api.post<ApiResponse<boolean>>(`/consent/revoke/${consentId}`);
    return res.data;
  },

  async getAccessLogs(): Promise<ApiResponse<DataAccessLog[]>> {
    const res = await api.get<ApiResponse<DataAccessLog[]>>('/consent/access-log');
    return res.data;
  }
};

export const doctorService = {
  async getAll(): Promise<ApiResponse<DoctorProfile[]>> {
    const res = await api.get<ApiResponse<DoctorProfile[]>>('/doctors');
    return res.data;
  },

  async getMyPatients(): Promise<ApiResponse<PatientProfile[]>> {
    const res = await api.get<ApiResponse<PatientProfile[]>>('/doctors/my-patients');
    return res.data;
  },

  async getMyAppointments(): Promise<ApiResponse<Appointment[]>> {
    const res = await api.get<ApiResponse<Appointment[]>>('/doctors/my-appointments');
    return res.data;
  },

  async getAvailability(doctorId: string, date?: string): Promise<ApiResponse<DoctorAvailability>> {
    const res = await api.get<ApiResponse<DoctorAvailability>>(`/doctors/${doctorId}/availability`, { params: { date } });
    return res.data;
  },

  async updateAppointmentStatus(id: string, status: string): Promise<ApiResponse<Appointment>> {
    const res = await api.post<ApiResponse<Appointment>>(`/appointments/${id}/status`, { status });
    return res.data;
  }
};

export const appointmentService = {
  async create(data: any): Promise<ApiResponse<Appointment>> {
    const res = await api.post<ApiResponse<Appointment>>('/appointments', data);
    return res.data;
  },

  async getAll(): Promise<ApiResponse<Appointment[]>> {
    const res = await api.get<ApiResponse<Appointment[]>>('/appointments');
    return res.data;
  },

  async confirm(id: string): Promise<ApiResponse<Appointment>> {
    const res = await api.post<ApiResponse<Appointment>>(`/appointments/${id}/confirm`);
    return res.data;
  },

  async cancel(id: string): Promise<ApiResponse<Appointment>> {
    const res = await api.post<ApiResponse<Appointment>>(`/appointments/${id}/cancel`);
    return res.data;
  },

  async complete(id: string): Promise<ApiResponse<Appointment>> {
    const res = await api.post<ApiResponse<Appointment>>(`/appointments/${id}/complete`);
    return res.data;
  }
};

export const medicationService = {
  async create(data: any): Promise<ApiResponse<Medication>> {
    const res = await api.post<ApiResponse<Medication>>('/medications', data);
    return res.data;
  },

  async getMedications(patientId?: string): Promise<ApiResponse<Medication[]>> {
    const res = await api.get<ApiResponse<Medication[]>>('/medications', { params: { patientId } });
    return res.data;
  }
};

export const documentService = {
  async upload(formData: FormData): Promise<ApiResponse<MedicalDocument>> {
    const res = await api.post<ApiResponse<MedicalDocument>>('/documents/upload', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
    return res.data;
  },

  getDownloadUrl(id: string): string {
    return `${process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000/api'}/documents/${id}/download`;
  }
};

export const aiService = {
  async chat(message: string, sessionId?: string): Promise<ApiResponse<AIChatResponse>> {
    const res = await api.post<ApiResponse<AIChatResponse>>('/ai/chat', { message, sessionId });
    return res.data;
  },

  async analyzeSymptoms(data: { symptoms: string; duration: string; age: number }): Promise<ApiResponse<SymptomAnalysisResponse>> {
    const res = await api.post<ApiResponse<SymptomAnalysisResponse>>('/ai/analyze-symptoms', data);
    return res.data;
  },

  async explainLabResult(labResultId: string): Promise<ApiResponse<any>> {
    const res = await api.post<ApiResponse<any>>(`/ai/explain-lab-result/${labResultId}`);
    return res.data;
  },

  async getDoctorBrief(patientId: string): Promise<ApiResponse<DoctorBriefResponse>> {
    const res = await api.get<ApiResponse<DoctorBriefResponse>>(`/ai/doctor-brief/${patientId}`);
    return res.data;
  },

  async getHandoffSummary(sessionId: string, patientId: string): Promise<ApiResponse<AIHandoffSummary>> {
    const res = await api.post<ApiResponse<AIHandoffSummary>>('/ai/handoff-summary', null, {
      params: { sessionId, patientId }
    });
    return res.data;
  }
};

export const searchService = {
  async search(query: string): Promise<ApiResponse<SearchResult[]>> {
    const res = await api.get<ApiResponse<SearchResult[]>>('/search', { params: { q: query } });
    return res.data;
  }
};

export const adminService = {
  async getStats(): Promise<ApiResponse<AdminStats>> {
    const res = await api.get<ApiResponse<AdminStats>>('/admin/analytics');
    return res.data;
  },

  async getUsers(): Promise<ApiResponse<any[]>> {
    const res = await api.get<ApiResponse<any[]>>('/admin/users');
    return res.data;
  },

  async toggleUserStatus(id: string): Promise<ApiResponse<any>> {
    const res = await api.post<ApiResponse<any>>(`/admin/users/${id}/toggle-status`);
    return res.data;
  },

  async getAuditLogs(): Promise<ApiResponse<any[]>> {
    const res = await api.get<ApiResponse<any[]>>('/admin/audit-logs');
    return res.data;
  }
};
