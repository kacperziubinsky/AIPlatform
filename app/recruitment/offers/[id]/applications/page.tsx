"use client";

import { use, useEffect, useState } from "react";
import Link from "next/link";
import { fetchOfferApplications } from "@/services/application-service";
import { fetchJobOfferById } from "@/services/offer-service";
import { RecruiterApplication, STATUS_LABELS } from "@/types/applications";
import { JobOffer } from "@/types/offer";

interface OfferApplicationsPageProps {
  params: Promise<{ id: string }>;
}

function formatDate(date?: string) {
  if (!date || isNaN(Date.parse(date))) return "—";
  return new Date(date).toLocaleDateString("pl-PL");
}

function scoreClass(score: number) {
  if (score >= 75) return "text-lime-700";
  if (score >= 50) return "text-amber-600";
  return "text-red-600";
}

export default function OfferApplicationsPage({ params }: OfferApplicationsPageProps) {
  const { id } = use(params);
  const [offer, setOffer] = useState<JobOffer | null>(null);
  const [applications, setApplications] = useState<RecruiterApplication[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    Promise.all([fetchJobOfferById(id).catch(() => null), fetchOfferApplications(id)])
      .then(([offerData, apps]) => {
        setOffer(offerData);
        setApplications(apps);
      })
      .catch((err) => setError(err instanceof Error ? err.message : "Nie udało się pobrać aplikacji."))
      .finally(() => setIsLoading(false));
  }, [id]);

  return (
    <main className="container mx-auto px-4 py-8 max-w-5xl">
      <Link href="/recruitment" className="inline-flex text-sm text-gray-500 hover:text-gray-700 mb-6">
        ← Powrót do rekrutacji
      </Link>

      <h1 className="text-3xl font-bold text-gray-900 mb-1">{offer?.title ?? "Aplikacje"}</h1>
      {offer?.company && <p className="text-gray-600 font-medium mb-6">{offer.company}</p>}

      {error && (
        <div className="p-4 mb-4 bg-red-50 border border-red-200 text-red-700 rounded-lg">{error}</div>
      )}

      {isLoading ? (
        <div className="space-y-4">
          {[...Array(3)].map((_, i) => (
            <div key={i} className="h-20 bg-gray-100 animate-pulse rounded-lg border border-gray-200" />
          ))}
        </div>
      ) : applications.length === 0 ? (
        !error && (
          <div className="text-center py-12 border rounded-lg bg-gray-50 text-gray-500">
            Brak aplikacji na tę ofertę.
          </div>
        )
      ) : (
        <div className="grid gap-3">
          {applications.map((app) => {
            const status = STATUS_LABELS[app.status];
            return (
              <Link
                key={app.id}
                href={`/recruitment/applications/${app.id}`}
                className="p-4 border border-gray-200 rounded-lg bg-white shadow-sm hover:shadow-md transition-shadow flex flex-wrap items-center justify-between gap-4"
              >
                <div>
                  <p className="text-sm text-gray-400">Wysłano: {formatDate(app.createdAt)}</p>
                  <p className="text-xs text-gray-400 font-mono mt-1 break-all">{app.id}</p>
                </div>
                <div className="flex items-center gap-4">
                  <div className="text-right">
                    <p className="text-xs text-gray-500">Wynik AI</p>
                    {app.aiScore !== null ? (
                      <p className={`text-xl font-bold ${scoreClass(app.aiScore)}`}>{app.aiScore}</p>
                    ) : (
                      <p className="text-sm text-gray-400">{app.aiStatus}</p>
                    )}
                  </div>
                  <span className={`text-sm font-semibold px-3 py-1 rounded-full ${status?.className ?? ""}`}>
                    {status?.label ?? app.status}
                  </span>
                </div>
              </Link>
            );
          })}
        </div>
      )}
    </main>
  );
}
