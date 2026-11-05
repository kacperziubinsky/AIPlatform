import { LoginRequest, AuthResponse, RegisterStep1Request, RegisterStep2Request } from "@/types/credentials";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:8080";

export async function LoginUser(credentials: LoginRequest): Promise<AuthResponse> {
    const response = await fetch(`${API_URL}/auth/auth/login`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(credentials),
    });

    if (!response.ok) {
        throw new Error("Failed to login");
    }

    return response.json();
}

export async function RegisterUserStep1(credentials: RegisterStep1Request): Promise<void> {
    const response = await fetch(`${API_URL}/auth/auth/register`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(credentials),
    });

    if (!response.ok) {
        throw new Error("Failed to register");
    }
}

export async function RegisterUserStep2(data: RegisterStep2Request, token: string): Promise<void> {
    const response = await fetch(`${API_URL}/user/api/users/me`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`
        },
        body: JSON.stringify({
            ...data,
            firstname: data.firstName,
            lastname: data.lastName,
            phone: data.phoneNumber,
            city: data.city,
            stack: data.stack,
            experience: data.experience
        }),
    });

    if (!response.ok) {
        throw new Error("Failed to update user information");
    }
}