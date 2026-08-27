'use client';

import React, { createContext, useContext, useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { authService } from '@/services/allServices';
import { UserMe, AuthResponse } from '@/types';

export type UserRoleType = 'Patient' | 'Doctor' | 'ClinicAdmin' | 'SuperAdmin';

interface AuthContextType {
  user: UserMe | null;
  role: UserRoleType | null;
  token: string | null;
  refreshToken: string | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<{ success: boolean; role?: string; message?: string }>;
  loginAsDemo: (roleType: 'patient' | 'doctor' | 'admin') => void;
  logout: () => Promise<void>;
  setAuthUser: (user: UserMe | null) => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const router = useRouter();
  const [user, setUser] = useState<UserMe | null>(null);
  const [role, setRole] = useState<UserRoleType | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [refreshToken, setRefreshToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);

  // Initialize auth state on mount
  useEffect(() => {
    const initAuth = async () => {
      if (typeof window === 'undefined') return;

      const storedToken = localStorage.getItem('medai_token');
      const storedRole = localStorage.getItem('medai_user_role') as UserRoleType | null;
      const storedRefresh = localStorage.getItem('medai_refresh_token');

      if (storedToken) {
        setToken(storedToken);
        setRefreshToken(storedRefresh);

        // Check if demo token
        if (storedToken === 'demo-patient-token') {
          const demoUser: UserMe = {
            id: 'demo-patient-id',
            email: 'patient@medai.com',
            firstName: 'Aziz',
            lastName: 'Karimov',
            phoneNumber: '+998 90 123 45 67',
            dateOfBirth: '1992-05-14',
            gender: 'Male',
            role: 'Patient',
            preferredLanguage: 'uz',
            patientId: 'demo-patient-id'
          };
          setUser(demoUser);
          setRole('Patient');
          setIsLoading(false);
          return;
        } else if (storedToken === 'demo-doctor-token') {
          const demoUser: UserMe = {
            id: 'demo-doctor-id',
            email: 'doctor@medai.com',
            firstName: 'Dr. Nigora',
            lastName: 'Yusupova',
            phoneNumber: '+998 90 987 65 43',
            dateOfBirth: '1985-11-20',
            gender: 'Female',
            role: 'Doctor',
            preferredLanguage: 'uz',
            doctorId: 'demo-doctor-id'
          };
          setUser(demoUser);
          setRole('Doctor');
          setIsLoading(false);
          return;
        } else if (storedToken === 'demo-admin-token') {
          const demoUser: UserMe = {
            id: 'demo-admin-id',
            email: 'admin@medai.com',
            firstName: 'Administrator',
            lastName: 'MedAI',
            phoneNumber: '+998 71 200 00 00',
            dateOfBirth: '1980-01-01',
            gender: 'NotSpecified',
            role: 'SuperAdmin',
            preferredLanguage: 'uz'
          };
          setUser(demoUser);
          setRole('SuperAdmin');
          setIsLoading(false);
          return;
        }

        // Try to fetch real user profile from API
        try {
          const res = await authService.getMe();
          if (res.success && res.data) {
            setUser(res.data);
            setRole(res.data.role as UserRoleType);
            localStorage.setItem('medai_user_role', res.data.role);
          } else if (storedRole) {
            setRole(storedRole);
          }
        } catch {
          if (storedRole) {
            setRole(storedRole);
          }
        }
      } else {
        // Default guest / unauthenticated
        setRole(null);
        setUser(null);
      }
      setIsLoading(false);
    };

    initAuth();
  }, []);

  const login = async (email: string, password: string): Promise<{ success: boolean; role?: string; message?: string }> => {
    const cleanEmail = email.trim().toLowerCase();

    try {
      const res = await authService.login({ email: cleanEmail, password });
      if (res && res.success && res.data) {
        const authData = res.data;
        const userRole = authData.role as UserRoleType;

        setToken(authData.accessToken);
        setRefreshToken(authData.refreshToken || null);
        setRole(userRole);

        if (typeof window !== 'undefined') {
          localStorage.setItem('medai_token', authData.accessToken);
          if (authData.refreshToken) {
            localStorage.setItem('medai_refresh_token', authData.refreshToken);
          }
          localStorage.setItem('medai_user_role', userRole);
        }

        // Fetch user profile
        try {
          const meRes = await authService.getMe();
          if (meRes.success) setUser(meRes.data);
        } catch {
          setUser({
            id: authData.userId,
            email: authData.email,
            firstName: authData.firstName,
            lastName: authData.lastName,
            phoneNumber: '',
            dateOfBirth: '',
            gender: '',
            role: authData.role,
            preferredLanguage: 'uz',
            patientId: authData.patientId,
            doctorId: authData.doctorId
          });
        }

        return { success: true, role: userRole };
      }
      return { success: false, message: res?.message || 'Login failed' };
    } catch (err: any) {
      // Check demo fallback if API fails
      if (cleanEmail === 'patient@medai.com') {
        loginAsDemo('patient');
        return { success: true, role: 'Patient' };
      } else if (cleanEmail === 'doctor@medai.com') {
        loginAsDemo('doctor');
        return { success: true, role: 'Doctor' };
      } else if (cleanEmail === 'admin@medai.com') {
        loginAsDemo('admin');
        return { success: true, role: 'SuperAdmin' };
      }

      return {
        success: false,
        message: err.response?.data?.message || 'Email yoki parol noto\'g\'ri.'
      };
    }
  };

  const loginAsDemo = (roleType: 'patient' | 'doctor' | 'admin') => {
    if (typeof window === 'undefined') return;

    if (roleType === 'patient') {
      const demoToken = 'demo-patient-token';
      const demoRole: UserRoleType = 'Patient';
      const demoUser: UserMe = {
        id: 'demo-patient-id',
        email: 'patient@medai.com',
        firstName: 'Aziz',
        lastName: 'Karimov',
        phoneNumber: '+998 90 123 45 67',
        dateOfBirth: '1992-05-14',
        gender: 'Male',
        role: 'Patient',
        preferredLanguage: 'uz',
        patientId: 'demo-patient-id'
      };

      localStorage.setItem('medai_token', demoToken);
      localStorage.setItem('medai_user_role', demoRole);
      setToken(demoToken);
      setRole(demoRole);
      setUser(demoUser);
      router.push('/dashboard');
    } else if (roleType === 'doctor') {
      const demoToken = 'demo-doctor-token';
      const demoRole: UserRoleType = 'Doctor';
      const demoUser: UserMe = {
        id: 'demo-doctor-id',
        email: 'doctor@medai.com',
        firstName: 'Dr. Nigora',
        lastName: 'Yusupova',
        phoneNumber: '+998 90 987 65 43',
        dateOfBirth: '1985-11-20',
        gender: 'Female',
        role: 'Doctor',
        preferredLanguage: 'uz',
        doctorId: 'demo-doctor-id'
      };

      localStorage.setItem('medai_token', demoToken);
      localStorage.setItem('medai_user_role', demoRole);
      setToken(demoToken);
      setRole(demoRole);
      setUser(demoUser);
      router.push('/doctors/dashboard');
    } else if (roleType === 'admin') {
      const demoToken = 'demo-admin-token';
      const demoRole: UserRoleType = 'SuperAdmin';
      const demoUser: UserMe = {
        id: 'demo-admin-id',
        email: 'admin@medai.com',
        firstName: 'Administrator',
        lastName: 'MedAI',
        phoneNumber: '+998 71 200 00 00',
        dateOfBirth: '1980-01-01',
        gender: 'NotSpecified',
        role: 'SuperAdmin',
        preferredLanguage: 'uz'
      };

      localStorage.setItem('medai_token', demoToken);
      localStorage.setItem('medai_user_role', demoRole);
      setToken(demoToken);
      setRole(demoRole);
      setUser(demoUser);
      router.push('/admin/dashboard');
    }
  };

  const logout = async () => {
    try {
      await authService.logout();
    } catch {
      // Ignore errors on logout
    } finally {
      if (typeof window !== 'undefined') {
        localStorage.removeItem('medai_token');
        localStorage.removeItem('medai_refresh_token');
        localStorage.removeItem('medai_user_role');
        localStorage.removeItem('medai_user');
      }
      setToken(null);
      setRefreshToken(null);
      setUser(null);
      setRole(null);
      router.push('/login');
    }
  };

  const setAuthUser = (updatedUser: UserMe | null) => {
    setUser(updatedUser);
    if (updatedUser) {
      setRole(updatedUser.role as UserRoleType);
      if (typeof window !== 'undefined') {
        localStorage.setItem('medai_user_role', updatedUser.role);
      }
    }
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        role,
        token,
        refreshToken,
        isLoading,
        isAuthenticated: !!token,
        login,
        loginAsDemo,
        logout,
        setAuthUser
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
