"use client";

import { useState, useEffect, Suspense } from "react";
import { useSearchParams } from "next/navigation";
import SearchBar from "@/components/ui/search-bar";
import FilterBar from "@/components/ui/filter-bar";
import { fetchJobOffers } from "@/services/offer-service";
import { JobOffer, Category, Experience } from "@/types/offer";
import Link from "next/dist/client/link";

function OffersContent() {
  const searchParams = useSearchParams();

  const searchTerm = searchParams.get("searchTerm") || "";
  const categoryParam = searchParams.get("category");
  const experienceParam = searchParams.get("experience");
  const minSalaryParam = searchParams.get("minSalary");
  const maxSalaryParam = searchParams.get("maxSalary");

  const [offers, setOffers] = useState<JobOffer[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadOffers = async () => {
      setIsLoading(true);
      setError(null);

      try {
        const data = await fetchJobOffers({
          searchTerm: searchTerm || undefined,
          category: categoryParam !== null ? (Number(categoryParam) as Category) : undefined,
          experience: experienceParam !== null ? (Number(experienceParam) as Experience) : undefined,
          minSalary: minSalaryParam ? Number(minSalaryParam) : undefined,
          maxSalary: maxSalaryParam ? Number(maxSalaryParam) : undefined,
          limit: 50,
        });
        setOffers(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : "Nie udało się pobrać ofert");
      } finally {
        setIsLoading(false);
      }
    };

    loadOffers();
  }, [searchTerm, categoryParam, experienceParam, minSalaryParam, maxSalaryParam]);

  return (
    <main className="container mx-auto px-4 py-8 max-w-5xl">
      <h1 className="text-3xl font-bold text-gray-900 mb-2">Wyszukiwarka Ofert Pracy</h1>
      <p className="text-gray-600 mb-6">Znajdź zatrudnienie dopasowane do Twojego stosu technologicznego.</p>

      <SearchBar initialQuery={searchTerm} />

      <FilterBar />

      {error && (
        <div className="mt-6 p-4 bg-red-50 border border-red-200 text-red-700 rounded-lg">
          {error}
        </div>
      )}

      <section className="mt-6">
        {isLoading ? (
          <div className="space-y-4">
            {[...Array(4)].map((_, i) => (
              <div key={i} className="h-28 bg-gray-100 animate-pulse rounded-lg border border-gray-200" />
            ))}
          </div>
        ) : offers.length === 0 ? (
          <div className="text-center py-12 border rounded-lg bg-gray-50 text-gray-500">
            Brak ofert spełniających wybrane kryteria.
          </div>
        ) : (
          <div className="grid gap-4">
            {offers.map((offer) => (
              <article
                key={offer.id}
                className="p-5 border rounded-lg bg-white shadow-sm hover:shadow-md transition-shadow duration-200 border-gray-200"
              >
                <div className="flex justify-between items-start">
                  <div>
                    <h2 className="text-xl font-semibold text-gray-900">{offer.title}</h2>
                    <p className="text-gray-600 font-medium mt-0.5">{offer.company}</p>
                  </div>
                  {offer.salary && (
                    <span className="text-sm font-semibold bg-lime-100 text-lime-800 px-3 py-1 rounded-full">
                      {offer.salary} PLN
                    </span>
                  )}
                </div>

                {offer.stack && offer.stack.length > 0 && (
                  <div className="flex flex-wrap gap-2 mt-4">
                    {offer.stack.map((tech, idx) => (
                      <span
                        key={idx}
                        className="text-xs bg-gray-100 text-gray-700 px-2.5 py-1 rounded-md font-mono"
                      >
                        {tech}
                      </span>
                    ))}
                  </div>
                )}
                       <Link
                href={`/offers/${offer.id}`}
                className="inline-flex items-center text-lime-700 hover:text-lime-800 font-medium mt-5"
                >
                Zobacz więcej
                </Link>
              </article>
            ))}
          </div>
        )}
      </section>
    </main>
  );
}

export default function OffersPage() {
  return (
    <Suspense fallback={<div className="container mx-auto p-8">Ładowanie widoku ofert...</div>}>
      <OffersContent />
    </Suspense>
  );
}