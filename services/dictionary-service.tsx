import { OfferDictionaries } from "@/types/offer";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:8080";

let cached: Promise<OfferDictionaries> | null = null;

export function fetchOfferDictionaries(): Promise<OfferDictionaries> {
  if (!cached) {
    cached = fetch(`${API_URL}/offer/offers/dictionaries`)
      .then((response) => {
        if (!response.ok) throw new Error("Nie udało się pobrać słowników.");
        return response.json() as Promise<OfferDictionaries>;
      })
      .catch((err) => {
        cached = null;
        throw err;
      });
  }
  return cached;
}
