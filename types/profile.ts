export interface UserProfile {
    id: string;
    firstName: string;
    lastName: string;
    phoneNumber: string;
    city: string;
    stack: string[];
    experience: number;
}

export function parseUserProfile(data: Record<string, unknown>): UserProfile {
  return {
    id: String(data.id ?? ""),
    firstName: String(data.firstname ?? ""),
    lastName: String(data.lastname ?? ""),
    phoneNumber: String(data.phone ?? ""),
    city: String(data.city ?? ""),
    stack: Array.isArray(data.stack) ? (data.stack as string[]) : [],
    experience: typeof data.experience === "number" ? data.experience : 0,
  };
}