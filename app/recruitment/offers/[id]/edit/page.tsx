"use client";

import { useEffect, useState, use } from "react";
import Link from "next/link";
import CreateOfferForm from "@/components/CreateOfferForm";
import { fetchJobOfferById } from "@/services/offer-service";
import { JobOffer } from "@/types/offer";

interface EditOfferPageProps {
  params: Promise<{ id: string }>;
}

export default function EditOfferPage({ params }: EditOfferPageProps) {
  const { id } = use(params);
  const [offer, setOffer] = useState<JobOffer | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchJobOfferById(id)
      .then(setOffer)
      .catch((err) => setError(err instanceof Error ? err.message : "Błąd podczas ładowania oferty."));
  }, [id]);

  return (
    <main className="min-h-screen flex flex-col items-center justify-center bg-gray-50 p-6">
      <div className="w-full max-w-2xl mb-4">
        <Link href="/recruitment" className="text-sm text-gray-500 hover:text-gray-700">
          ← Powrót do rekrutacji
        </Link>
      </div>
      {error ? (
        <div className="w-full max-w-2xl p-6 bg-red-50 border border-red-200 rounded-xl text-red-700">{error}</div>
      ) : !offer ? (
        <div className="w-full max-w-2xl h-96 bg-gray-100 animate-pulse rounded-2xl border border-gray-200" />
      ) : (
        <CreateOfferForm initialOffer={offer} />
      )}
    </main>
  );
}
