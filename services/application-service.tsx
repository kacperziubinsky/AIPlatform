import { CreateApplication, Application } from "@/types/applications";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:8080";

export async function createApplication(applicationData: Partial<CreateApplication>): Promise<CreateApplication | null> {
  const token = typeof window !== "undefined" ? localStorage.getItem("accessToken") : null;

  if (!token) {
    throw new Error("Brak tokena uwierzytelniającego");
  }

  const response = await fetch(`${API_URL}/recruitment/api/applications`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify(applicationData),
  });

  if (response.status === 401) {
    localStorage.removeItem("accessToken");
    window.location.href = "/login";
    throw new Error("Sesja wygasła. Zaloguj się ponownie.");
  }

  if (response.status === 404) {
    throw new Error("CV lub oferta nie jest dostępna. Odśwież i spróbuj ponownie.");
  }

  if (response.status === 409) {
    throw new Error("Już aplikowałeś na tę ofertę.");
  }

  if (!response.ok) {
    throw new Error("Wystąpił błąd podczas wysyłania aplikacji.");
  }

  const text = await response.text();
  if (!text) {
    return null;
  }

  return JSON.parse(text) as CreateApplication;
}

export async function fetchApplications(): Promise<Application[]> {
  const token = typeof window !== "undefined" ? localStorage.getItem("accessToken") : null;

  if (!token) {
    throw new Error("Brak tokena uwierzytelniającego");
  }

  const response = await fetch(`${API_URL}/recruitment/api/applications/me`, {
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
    throw new Error("Nie udało się pobrać aplikacji.");
  }

  const text = await response.text();
  if (!text) {
    return [];
  }

  return JSON.parse(text);
}