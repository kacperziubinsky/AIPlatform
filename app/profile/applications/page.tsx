"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { fetchApplications } from "@/services/application-service";
import { Application } from "@/types/applications";
import { fetchJobOfferById } from "@/services/offer-service";
import { JobOffer } from "@/types/offer";

const STATUS_STYLES: Record<string, { label: string; className: string }> = {
  SENT: { label: "Wysłana", className: "bg-blue-100 text-blue-800" },
  IN_REVIEW: { label: "W trakcie", className: "bg-amber-100 text-amber-800" },
  ACCEPTED: { label: "Zaakceptowana", className: "bg-lime-100 text-lime-800" },
  REJECTED: { label: "Odrzucona", className: "bg-red-100 text-red-800" },
};

function StatusBadge({ status }: { status: string }) {
  const style = STATUS_STYLES[status] ?? { label: status, className: "bg-gray-100 text-gray-700" };

  return (
    <span className={`text-sm font-semibold px-3 py-1 rounded-full ${style.className}`}>
      {style.label}
    </span>
  );
}

function formatDate(date?: string) {
  if (!date || isNaN(Date.parse(date))) return "—";
  return new Date(date).toLocaleDateString("pl-PL");
}

export default function ApplicationsPage() {
    const [applications, setApplications] = useState<Application[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [offers, setOffers] = useState<Record<string, JobOffer>>({});
    
    useEffect(() => {
    async function load() {
        try {
        const apps = await fetchApplications();
        setApplications(apps);

        const offerIds = [...new Set(apps.map((app) => app.offerId))];

        const results = await Promise.allSettled(offerIds.map((id) => fetchJobOfferById(id)));

        const offerMap: Record<string, JobOffer> = {};
        results.forEach((result, i) => {
            if (result.status === "fulfilled") {
            offerMap[offerIds[i]] = result.value;
            }
        });
        setOffers(offerMap);
        } catch (err) {
        setError(err instanceof Error ? err.message : "Nie udało się pobrać aplikacji");
        } finally {
        setIsLoading(false);
        }
    }

    load();
    }, []);

  return (
    <main className="container mx-auto px-4 py-8 max-w-5xl">
      <h1 className="text-3xl font-bold text-gray-900 mb-2">Moje aplikacje</h1>
      <p className="text-gray-600 mb-6">Śledź status swoich aplikacji na oferty pracy.</p>

      {error && (
        <div className="mt-6 p-4 bg-red-50 border border-red-200 text-red-700 rounded-lg">
          {error}
        </div>
      )}

      <section className="mt-6">
        {isLoading ? (
          <div className="space-y-4">
            {[...Array(3)].map((_, i) => (
              <div key={i} className="h-28 bg-gray-100 animate-pulse rounded-lg border border-gray-200" />
            ))}
          </div>
        ) : applications.length === 0 ? (
          <div className="text-center py-12 border rounded-lg bg-gray-50 text-gray-500">
            <p>Nie masz jeszcze żadnych aplikacji.</p>
            <Link
              href="/offers"
              className="inline-flex items-center text-lime-700 hover:text-lime-800 font-medium mt-3"
            >
              Przeglądaj oferty →
            </Link>
          </div>
        ) : (
          <div className="grid gap-4">
            {applications.map((app) => {
            const offer = offers[app.offerId];

            return (
                <article key={app.id} className="p-5 border rounded-lg bg-white shadow-sm hover:shadow-md transition-shadow duration-200 border-gray-200">
                <div className="flex justify-between items-start">
                    <div>
                    <h2 className="text-xl font-semibold text-gray-900">
                        {offer?.title ?? "Oferta niedostępna"}
                    </h2>
                    {offer?.company && (
                        <p className="text-gray-600 font-medium mt-0.5">{offer.company}</p>
                    )}
                    <p className="text-sm text-gray-400 mt-1">Wysłano: {formatDate(app.createdAt)}</p>
                    </div>
                    <StatusBadge status={app.status} />
                </div>



                <Link
                  href={`/offers/${app.offerId}`}
                  className="inline-flex items-center text-lime-700 hover:text-lime-800 font-medium mt-5"
                >
                  Zobacz ofertę
                </Link>
                </article>
            );
            })}
          </div>
        )}
      </section>
    </main>
  );
}