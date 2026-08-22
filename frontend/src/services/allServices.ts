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
  AdminStats
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

  async getAppointments(): Promise<ApiResponse<Appointment[]>> {
    const res = await api.get<ApiResponse<Appointment[]>>('/doctors/my-appointments');
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
  async upload(data: any): Promise<ApiResponse<MedicalDocument>> {
    const res = await api.post<ApiResponse<MedicalDocument>>('/documents', data);
    return res.data;
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
