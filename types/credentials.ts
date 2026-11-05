export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
    token: string;
    userEmail: string;
}

export interface RegisterStep1Request {
    email: string;
    password: string;
    role: "Applicant" | "Recruiter";
}

export interface RegisterStep2Request {
    firstName: string;
    lastName: string;
    phoneNumber: string;
    city: string;
    stack: string[];
    experience: number;
}