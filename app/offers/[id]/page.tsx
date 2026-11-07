"use client";

import { useEffect, useState, use } from "react";
import Link from "next/link";
import { JobOffer } from "@/types/offer";
import { fetchJobOfferById } from "@/services/offer-service";
import CreateApplicationModal from "@/components/modals/CreateApplicationModal";

interface SingleOfferPageProps {
  params: Promise<{ id: string }>;
}

export default function SingleOfferPage({ params }: SingleOfferPageProps) {
  const { id } = use(params);

  const [offer, setOffer] = useState<JobOffer | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isApplyOpen, setIsApplyOpen] = useState(false);

  useEffect(() => {
    const loadOffer = async () => {
      setIsLoading(true);
      setError(null);

      try {
        const data = await fetchJobOfferById(id);
        setOffer(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : "Błąd podczas ładowania oferty.");
      } finally {
        setIsLoading(false);
      }
    };

    loadOffer();
  }, [id]);

  if (isLoading) {
    return (
      <main className="container mx-auto px-4 py-8 max-w-4xl">
        <div className="h-6 w-32 bg-gray-200 animate-pulse rounded mb-6" />
        <div className="bg-white border border-gray-200 rounded-xl p-6 shadow-sm space-y-4">
          <div className="h-8 w-2/3 bg-gray-200 animate-pulse rounded" />
          <div className="h-5 w-1/3 bg-gray-200 animate-pulse rounded" />
          <div className="h-24 bg-gray-100 animate-pulse rounded mt-6" />
        </div>
      </main>
    );
  }

  if (error || !offer) {
    return (
      
      <main className="container mx-auto px-4 py-12 max-w-4xl text-center">
        <div className="p-6 bg-red-50 border border-red-200 rounded-xl text-red-700 mb-6">
          {error || "Oferta nie istnieje."}
        </div>
        <Link
          href="/offers"
          className="inline-flex items-center text-lime-700 hover:text-lime-800 font-medium"
        >
          ← Wróć do listy ofert
        </Link>
      </main>
    );
  }

  return (
    <main className="container mx-auto px-4 py-8 max-w-4xl">
      <Link
        href="/offers"
        className="inline-flex items-center text-sm text-gray-500 hover:text-gray-700 mb-6 transition-colors"
      >
        <svg className="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M15 19l-7-7 7-7" />
        </svg>
        Powrót do listy ofert
      </Link>

    <CreateApplicationModal
      isOpen={isApplyOpen}
      onClose={() => setIsApplyOpen(false)}
      offerId={offer.id}
      offerTitle={offer.title}
    />

      <article className="bg-white border border-gray-200 rounded-xl shadow-sm overflow-hidden">
        <div className="p-6 md:p-8 border-b border-gray-100 bg-gray-50/50">
          <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
            <div>
              <h1 className="text-2xl md:text-3xl font-bold text-gray-900">{offer.title}</h1>
              <p className="text-lg text-gray-600 font-medium mt-1">{offer.company}</p>
            </div>

            {offer.salary && (
              <div className="self-start md:self-auto bg-lime-100 text-lime-900 px-4 py-2 rounded-xl text-lg font-bold border border-lime-200">
                {offer.salary} PLN
              </div>
            )}
          </div>

          {offer.stack && offer.stack.length > 0 && (
            <div className="flex flex-wrap gap-2 mt-6">
              {offer.stack.map((tech, idx) => (
                <span
                  key={idx}
                  className="bg-white border border-gray-200 text-gray-800 text-xs px-3 py-1 rounded-md font-mono shadow-sm"
                >
                  {tech}
                </span>
              ))}
            </div>
          )}
        </div>

        <div className="p-6 md:p-8 space-y-8">
          {offer.description && (
            <section>
              <h2 className="text-lg font-semibold text-gray-900 mb-3">Opis stanowiska</h2>
              <p className="text-gray-700 leading-relaxed whitespace-pre-line">{offer.description}</p>
            </section>
          )}

          {offer.requirements && offer.requirements.length > 0 && (
            <section>
              <h2 className="text-lg font-semibold text-gray-900 mb-3">Wymagania</h2>
              <ul className="space-y-2">
                {offer.requirements.map((req) => (
                  <li key={req.id} className="flex items-start text-gray-700 text-sm">
                    <span className="text-lime-600 mr-2 font-bold">•</span>
                    <span>
                      {req.name}
                      {req.isRequired && (
                        <span className="ml-2 text-xs font-semibold text-red-500 bg-red-50 px-2 py-0.5 rounded">
                          Wymagane
                        </span>
                      )}
                    </span>
                  </li>
                ))}
              </ul>
            </section>
          )}

          <div className="pt-6 border-t border-gray-100 flex justify-end">
            <button onClick={() => setIsApplyOpen(true)} className="bg-lime-700 hover:bg-lime-800 text-white font-medium px-6 py-2.5 rounded-lg shadow transition-all hover:shadow-md active:scale-95">
              Aplikuj teraz
            </button>
          </div>
        </div>
      </article>
    </main>
  );
}