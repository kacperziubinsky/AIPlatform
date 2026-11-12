"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { DeleteOffer, fetchMyOffers } from "@/services/offer-service";
import { JobOffer } from "@/types/offer";

function formatDate(date?: string) {
  if (!date || isNaN(Date.parse(date))) return "—";
  return new Date(date).toLocaleDateString("pl-PL");
}

function isExpired(offer: JobOffer) {
  return new Date(offer.endDate).getTime() <= Date.now();
}

export default function RecruitmentPage() {
  const router = useRouter();
  const [offers, setOffers] = useState<JobOffer[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  useEffect(() => {
    const token = localStorage.getItem("accessToken");
    if (!token) {
      router.push("/login");
      return;
    }

    fetchMyOffers(token)
      .then(setOffers)
      .catch((err) => setError(err instanceof Error ? err.message : "Nie udało się pobrać ofert."))
      .finally(() => setIsLoading(false));
  }, [router]);

  const handleDelete = async (offer: JobOffer) => {
    if (!confirm(`Czy na pewno chcesz usunąć ofertę „${offer.title}”?`)) return;

    const token = localStorage.getItem("accessToken");
    if (!token) {
      router.push("/login");
      return;
    }

    setDeletingId(offer.id);
    setError(null);
    try {
      await DeleteOffer(offer.id, token);
      setOffers((prev) => prev.filter((o) => o.id !== offer.id));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Nie udało się usunąć oferty.");
    } finally {
      setDeletingId(null);
    }
  };

  return (
    <main className="container mx-auto px-4 py-8 max-w-5xl">
      <div className="flex flex-wrap items-start justify-between gap-4 mb-8">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 mb-2">Rekrutacja</h1>
          <p className="text-gray-600">Zarządzaj swoimi ofertami i aplikacjami kandydatów.</p>
        </div>
        <Link
          href="/create-offer"
          className="bg-lime-700 hover:bg-lime-800 text-white font-medium py-2.5 px-4 rounded-lg transition-colors"
        >
          + Nowa oferta
        </Link>
      </div>

      <h2 className="text-xl font-semibold text-gray-900 mb-4">Moje oferty</h2>

      {error && (
        <div className="p-4 mb-4 bg-red-50 border border-red-200 text-red-700 rounded-lg">{error}</div>
      )}

      {isLoading ? (
        <div className="space-y-4">
          {[...Array(3)].map((_, i) => (
            <div key={i} className="h-24 bg-gray-100 animate-pulse rounded-lg border border-gray-200" />
          ))}
        </div>
      ) : offers.length === 0 ? (
        <div className="text-center py-12 border rounded-lg bg-gray-50 text-gray-500">
          <p>Nie masz jeszcze żadnych ofert.</p>
          <Link href="/create-offer" className="inline-flex text-lime-700 hover:text-lime-800 font-medium mt-3">
            Utwórz pierwszą ofertę →
          </Link>
        </div>
      ) : (
        <div className="grid gap-4">
          {offers.map((offer) => (
            <article
              key={offer.id}
              className="p-5 border border-gray-200 rounded-lg bg-white shadow-sm flex flex-wrap justify-between items-start gap-4"
            >
              <div>
                <div className="flex items-center gap-2">
                  <h3 className="text-lg font-semibold text-gray-900">{offer.title}</h3>
                  {isExpired(offer) && (
                    <span className="text-xs font-medium px-2 py-0.5 rounded-full bg-gray-100 text-gray-500">
                      Zakończona
                    </span>
                  )}
                </div>
                {offer.company && <p className="text-gray-600 font-medium mt-0.5">{offer.company}</p>}
                <p className="text-sm text-gray-400 mt-1">
                  {formatDate(offer.startDate)} – {formatDate(offer.endDate)} ·{" "}
                  {offer.salary.toLocaleString("pl-PL")} PLN
                </p>
              </div>
              <div className="flex flex-wrap gap-4 text-sm font-medium">
                <Link
                  href={`/recruitment/offers/${offer.id}/applications`}
                  className="text-lime-700 hover:text-lime-800"
                >
                  Aplikacje
                </Link>
                <Link href={`/offers/${offer.id}`} className="text-gray-600 hover:text-gray-800">
                  Podgląd
                </Link>
                <Link href={`/recruitment/offers/${offer.id}/edit`} className="text-gray-600 hover:text-gray-800">
                  Edytuj
                </Link>
                <button
                  onClick={() => handleDelete(offer)}
                  disabled={deletingId === offer.id}
                  className="text-red-500 hover:text-red-700 disabled:opacity-50"
                >
                  {deletingId === offer.id ? "Usuwanie..." : "Usuń"}
                </button>
              </div>
            </article>
          ))}
        </div>
      )}
    </main>
  );
}
