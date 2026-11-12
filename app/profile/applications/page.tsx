"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { fetchApplications, withdrawApplication } from "@/services/application-service";
import { FINAL_STATUSES, MyApplication, STATUS_LABELS } from "@/types/applications";

function StatusBadge({ status }: { status: MyApplication["status"] }) {
  const style = STATUS_LABELS[status] ?? { label: status, className: "bg-gray-100 text-gray-700" };

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
  const [applications, setApplications] = useState<MyApplication[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [withdrawingId, setWithdrawingId] = useState<string | null>(null);

  useEffect(() => {
    fetchApplications()
      .then(setApplications)
      .catch((err) => setError(err instanceof Error ? err.message : "Nie udało się pobrać aplikacji"))
      .finally(() => setIsLoading(false));
  }, []);

  const handleWithdraw = async (id: string) => {
    if (!confirm("Czy na pewno chcesz wycofać tę aplikację?")) return;

    setWithdrawingId(id);
    setError(null);
    try {
      await withdrawApplication(id);
      setApplications((prev) =>
        prev.map((app) =>
          app.id === id ? { ...app, status: "Withdrawn", updatedAt: new Date().toISOString() } : app
        )
      );
    } catch (err) {
      setError(err instanceof Error ? err.message : "Nie udało się wycofać aplikacji.");
    } finally {
      setWithdrawingId(null);
    }
  };

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
            {applications.map((app) => (
              <article
                key={app.id}
                className="p-5 border rounded-lg bg-white shadow-sm hover:shadow-md transition-shadow duration-200 border-gray-200"
              >
                <div className="flex justify-between items-start gap-4">
                  <div>
                    <h2 className="text-xl font-semibold text-gray-900">
                      {app.offerTitle ?? "Oferta niedostępna"}
                    </h2>
                    {app.company && <p className="text-gray-600 font-medium mt-0.5">{app.company}</p>}
                    <p className="text-sm text-gray-400 mt-1">
                      Wysłano: {formatDate(app.appliedAt)}
                      {app.updatedAt !== app.appliedAt && <> · Aktualizacja: {formatDate(app.updatedAt)}</>}
                    </p>
                  </div>
                  <StatusBadge status={app.status} />
                </div>

                <div className="flex items-center gap-5 mt-5">
                  <Link
                    href={`/offers/${app.offerId}`}
                    className="inline-flex items-center text-lime-700 hover:text-lime-800 font-medium"
                  >
                    Zobacz ofertę
                  </Link>
                  {!FINAL_STATUSES.includes(app.status) && (
                    <button
                      onClick={() => handleWithdraw(app.id)}
                      disabled={withdrawingId === app.id}
                      className="text-sm text-red-500 hover:text-red-700 disabled:opacity-50"
                    >
                      {withdrawingId === app.id ? "Wycofywanie..." : "Wycofaj aplikację"}
                    </button>
                  )}
                </div>
              </article>
            ))}
          </div>
        )}
      </section>
    </main>
  );
}
