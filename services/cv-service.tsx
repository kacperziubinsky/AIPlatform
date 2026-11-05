import { parseUserCV, UserCV } from "@/types/cv";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000";

export async function uploadUserCV(file: File): Promise<UserCV | null> {
  const token = typeof window !== "undefined" ? localStorage.getItem("accessToken") : null;

  if (!token) {
    throw new Error("Brak tokena uwierzytelniającego");
  }

  const formData = new FormData();
  formData.append("file", file);

  const response = await fetch(`${API_URL}/user/api/users/me/cvs`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`,
    },
    body: formData,
    cache: "no-store",
  });

  if (response.status === 401) {
    localStorage.removeItem("accessToken");
    if (typeof window !== "undefined") {
      window.location.href = "/login";
    }
    throw new Error("Sesja wygasła. Zaloguj się ponownie.");
  }

  if (!response.ok) {
    throw new Error("Failed to upload user CV");
  }

  if (response.status === 204) {
    return null;
  }

  const text = await response.text();
  if (!text) {
    return null;
  }

  return JSON.parse(text) as UserCV;
}

export async function fetchUserCV(): Promise<UserCV[]> {
  const token = typeof window !== "undefined" ? localStorage.getItem("accessToken") : null;

  if (!token) {
    throw new Error("Brak tokena uwierzytelniającego");
  }

  const response = await fetch(`${API_URL}/user/api/users/me/cvs`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${token}`,
    },
    cache: "no-store",
  });

  if (response.status === 401) {
    localStorage.removeItem("accessToken");
    window.location.href = "/login";
    throw new Error("Sesja wygasła. Zaloguj się ponownie.");
  }

  if (!response.ok) {
    throw new Error("Failed to fetch user CVs");
  }

  const text = await response.text();
  if (!text) {
    return [];
  }

  const data = JSON.parse(text);
  return data.map(parseUserCV);
}