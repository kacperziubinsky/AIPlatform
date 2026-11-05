import { UserProfile, parseUserProfile } from "@/types/profile";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:8080";

export async function fetchUserProfile(): Promise<UserProfile> {
  const token = typeof window !== "undefined" ? localStorage.getItem("accessToken") : null;

  if (!token) {
    throw new Error("Brak tokena uwierzytelniającego");
  }

  const response = await fetch(`${API_URL}/user/api/users/me`, {
    headers: {
      "Content-Type": "application/json",
      "Authorization": `Bearer ${token}`,
    },
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
    throw new Error("Failed to fetch user profile");
  }

  const data = await response.json();
  return parseUserProfile(data);
}