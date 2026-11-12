import {
  ChangeStatusRequest,
  CreateApplication,
  CreatedApplication,
  CreateEvaluationRequest,
  Evaluation,
  MyApplication,
  RecruiterApplication,
} from "@/types/applications";
import { readApiError } from "@/lib/api-error";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:8080";
const BASE = `${API_URL}/recruitment/api`;

function getToken(): string {
  const token = typeof window !== "undefined" ? localStorage.getItem("accessToken") : null;
  if (!token) {
    throw new Error("Brak tokena uwierzytelniającego");
  }
  return token;
}

async function request(path: string, init: RequestInit = {}): Promise<Response> {
  const headers: Record<string, string> = { Authorization: `Bearer ${getToken()}` };
  if (init.body) headers["Content-Type"] = "application/json";

  const response = await fetch(`${BASE}${path}`, {
    cache: "no-store",
    ...init,
    headers: { ...headers, ...(init.headers as Record<string, string> | undefined) },
  });

  if (response.status === 401) {
    localStorage.removeItem("accessToken");
    window.location.href = "/login";
    throw new Error("Sesja wygasła. Zaloguj się ponownie.");
  }

  if (response.status === 403) {
    throw new Error("Brak uprawnień do tej operacji.");
  }

  return response;
}

export async function createApplication(data: CreateApplication): Promise<CreatedApplication> {
  const response = await request("/applications", {
    method: "POST",
    body: JSON.stringify(data),
  });

  if (response.status === 404) {
    throw await readApiError(response, "CV lub oferta nie jest dostępna. Odśwież i spróbuj ponownie.");
  }

  if (response.status === 409) {
    throw new Error("Już aplikowałeś na tę ofertę.");
  }

  if (!response.ok) {
    throw await readApiError(response, "Wystąpił błąd podczas wysyłania aplikacji.");
  }

  return response.json();
}

export async function fetchApplications(): Promise<MyApplication[]> {
  const response = await request("/applications/me");

  if (!response.ok) {
    throw new Error("Nie udało się pobrać aplikacji.");
  }

  return response.json();
}

export async function withdrawApplication(id: string): Promise<void> {
  const response = await request(`/applications/${id}`, { method: "DELETE" });

  if (response.status === 404) {
    throw new Error("Aplikacja nie została znaleziona.");
  }

  if (!response.ok) {
    throw await readApiError(response, "Nie udało się wycofać aplikacji.");
  }
}

export async function fetchOfferApplications(offerId: string): Promise<RecruiterApplication[]> {
  const response = await request(`/offers/${offerId}/applications`);

  if (!response.ok) {
    throw new Error(`Nie udało się pobrać aplikacji (${response.status})`);
  }

  return response.json();
}

export async function fetchApplicationById(id: string): Promise<RecruiterApplication> {
  const response = await request(`/applications/${id}`);

  if (response.status === 404) {
    throw new Error("Aplikacja nie została znaleziona.");
  }

  if (!response.ok) {
    throw new Error(`Nie udało się pobrać aplikacji (${response.status})`);
  }

  return response.json();
}

export async function changeApplicationStatus(id: string, data: ChangeStatusRequest): Promise<void> {
  const response = await request(`/applications/${id}/status`, {
    method: "PATCH",
    body: JSON.stringify(data),
  });

  if (response.status === 404) {
    throw new Error("Aplikacja nie została znaleziona.");
  }

  if (!response.ok) {
    throw await readApiError(response, "Nie udało się zmienić statusu.");
  }
}

export async function requestAiEvaluation(id: string): Promise<void> {
  const response = await request(`/applications/${id}/ai-evaluation`, { method: "POST" });

  if (!response.ok) {
    throw await readApiError(response, "Nie udało się zlecić oceny AI.");
  }
}

export async function addEvaluation(id: string, data: CreateEvaluationRequest): Promise<Evaluation> {
  const response = await request(`/applications/${id}/evaluations`, {
    method: "POST",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    throw await readApiError(response, "Nie udało się dodać oceny.");
  }

  return response.json();
}
