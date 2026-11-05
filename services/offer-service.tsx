import { JobOffer, OfferFilterRequest } from "@/types/offer";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:8080";

export async function fetchJobOffers(filters?: OfferFilterRequest): Promise<JobOffer[]> {
    const queryParams = new URLSearchParams();

    if (filters?.searchTerm) queryParams.append("searchTerm", filters.searchTerm);
    if (filters?.category) queryParams.append("category", filters.category.toString());
    if (filters?.experience) queryParams.append("experience", filters.experience.toString());
    if (filters?.minSalary) queryParams.append("minSalary", filters.minSalary.toString());
    if (filters?.maxSalary) queryParams.append("maxSalary", filters.maxSalary.toString());
    if (filters?.limit) queryParams.append("limit", filters.limit.toString());

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