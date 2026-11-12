import { JobOffer, OfferFilterRequest, CreateOfferRequest, UpdateOfferRequest } from "@/types/offer";
import { readApiError } from "@/lib/api-error";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:8080";

export async function fetchJobOffers(filters?: OfferFilterRequest): Promise<JobOffer[]> {
    const queryParams = new URLSearchParams();

    if (filters?.searchTerm) queryParams.append("searchTerm", filters.searchTerm);
    if (filters?.category !== undefined) queryParams.append("category", filters.category.toString());
    if (filters?.experience !== undefined) queryParams.append("experience", filters.experience.toString());
    if (filters?.minSalary !== undefined) queryParams.append("minSalary", filters.minSalary.toString());
    if (filters?.maxSalary !== undefined) queryParams.append("maxSalary", filters.maxSalary.toString());
    if (filters?.limit !== undefined) queryParams.append("limit", filters.limit.toString());

    const url = `${API_URL}/offer/offers${queryParams.toString() ? `?${queryParams.toString()}` : ""}`;

    const response = await fetch(url, {
        cache: "no-store",
    });

    if (!response.ok) {
        throw new Error("Failed to fetch job offers");
    }

    return response.json();
}

export async function fetchJobOfferById(id: string): Promise<JobOffer> {
  const response = await fetch(`${API_URL}/offer/offers/${id}`, {
    cache: "no-store",
  });

  if (!response.ok) {
    if (response.status === 404) {
      throw new Error("Oferta nie została znaleziona.");
    }
    throw new Error(`Błąd pobierania oferty (${response.status})`);
  }

  return response.json();
}

export async function fetchMyOffers(token: string): Promise<JobOffer[]> {
  const response = await fetch(`${API_URL}/offer/offers/mine`, {
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

  if (response.status === 403) {
    throw new Error("Tylko rekruterzy mają dostęp do tej strony.");
  }

  if (!response.ok) {
    throw new Error(`Nie udało się pobrać ofert (${response.status})`);
  }

  return response.json();
}

export async function DeleteOffer(id: string, token: string): Promise<void> {
  const response = await fetch(`${API_URL}/offer/offers/${id}`, {
    method: "DELETE",
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  if (!response.ok) {
    if (response.status === 404) {
      throw new Error("Oferta nie została znaleziona.");
    }
    if (response.status === 403) {
      throw new Error("Brak uprawnień do usunięcia tej oferty.");
    }
    throw new Error(`Nie udało się usunąć oferty (${response.status})`);
  }
}

export async function CreateOffer(data: CreateOfferRequest, token: string): Promise<void> {
  const response = await fetch(`${API_URL}/offer/offers`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify(data),
  });
 
  if (!response.ok) {
    throw await readApiError(response, "Nie udało się utworzyć oferty.");
  }
}

export async function UpdateOffer(id: string, data: UpdateOfferRequest, token: string): Promise<void> {
  const response = await fetch(`${API_URL}/offer/offers/${id}`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    if (response.status === 404) {
      throw new Error("Oferta nie została znaleziona.");
    }
    if (response.status === 403) {
      throw new Error("Brak uprawnień do edycji tej oferty.");
    }
    throw await readApiError(response, `Nie udało się zaktualizować oferty (${response.status})`);
  }
}
 