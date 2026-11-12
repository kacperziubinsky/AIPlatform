"use client";

import { use, useCallback, useEffect, useState } from "react";
import Link from "next/link";
import {
  addEvaluation,
  changeApplicationStatus,
  fetchApplicationById,
  requestAiEvaluation,
} from "@/services/application-service";
import { fetchJobOfferById } from "@/services/offer-service";
import {
  allowedNextStatuses,
  ApplicationStatus,
  MatchLevel,
  RecruiterApplication,
  STATUS_LABELS,
} from "@/types/applications";
import { JobOffer } from "@/types/offer";

interface ApplicationPageProps {
  params: Promise<{ id: string }>;
}

const MATCH_LABELS: Record<MatchLevel, { label: string; className: string }> = {
  Full: { label: "Pełne", className: "bg-lime-100 text-lime-800" },
  Partial: { label: "Częściowe", className: "bg-amber-100 text-amber-800" },
  Weak: { label: "Słabe", className: "bg-orange-100 text-orange-800" },
  None: { label: "Brak", className: "bg-red-100 text-red-700" },
};

function formatDate(date?: string) {
  if (!date || isNaN(Date.parse(date))) return "—";
  return new Date(date).toLocaleString("pl-PL");
}

export default function RecruiterApplicationPage({ params }: ApplicationPageProps) {
  const { id } = use(params);
  const [application, setApplication] = useState<RecruiterApplication | null>(null);
  const [offer, setOffer] = useState<JobOffer | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [isBusy, setIsBusy] = useState(false);

  const [nextStatus, setNextStatus] = useState<ApplicationStatus | "">("");
  const [note, setNote] = useState("");
  const [score, setScore] = useState("");
  const [comment, setComment] = useState("");

  const reload = useCallback(async () => {
    setApplication(await fetchApplicationById(id));
  }, [id]);

  useEffect(() => {
    fetchApplicationById(id)
      .then((app) => {
        setApplication(app);
        return fetchJobOfferById(app.offerId).then(setOffer).catch(() => setOffer(null));
      })
      .catch((err) => setError(err instanceof Error ? err.message : "Nie udało się pobrać aplikacji."))
      .finally(() => setIsLoading(false));
  }, [id]);

  const runAction = async (action: () => Promise<unknown>) => {
    setIsBusy(true);
    setActionError(null);
    try {
      await action();
      await reload();
    } catch (err) {
      setActionError(err instanceof Error ? err.message : "Wystąpił błąd.");
    } finally {
      setIsBusy(false);
    }
  };

  const handleStatusChange = (e: React.FormEvent) => {
    e.preventDefault();
    if (!nextStatus) return;
    runAction(async () => {
      await changeApplicationStatus(id, { status: nextStatus, note: note.trim() || undefined });
      setNextStatus("");
      setNote("");
    });
  };

  const handleAddEvaluation = (e: React.FormEvent) => {
    e.preventDefault();
    const value = Number(score);
    if (score === "" || isNaN(value) || value < 0 || value > 100) {
      setActionError("Ocena musi być liczbą od 0 do 100.");
      return;
    }
    runAction(async () => {
      await addEvaluation(id, { score: value, comment: comment.trim() || undefined });
      setScore("");
      setComment("");
    });
  };

  const backHref = application ? `/recruitment/offers/${application.offerId}/applications` : "/recruitment";
  const status = application ? STATUS_LABELS[application.status] : null;
  const nextOptions = application ? allowedNextStatuses(application.status) : [];
  const aiInProgress = application?.aiStatus === "Pending" || application?.aiStatus === "Processing";

  return (
    <main className="container mx-auto px-4 py-8 max-w-3xl">
      <Link href={backHref} className="inline-flex text-sm text-gray-500 hover:text-gray-700 mb-6">
        ← Powrót do listy aplikacji
      </Link>

      {isLoading ? (
        <div className="h-48 bg-gray-100 animate-pulse rounded-xl border border-gray-200" />
      ) : error || !application ? (
        <div className="p-6 bg-red-50 border border-red-200 rounded-xl text-red-700">
          {error || "Aplikacja nie istnieje."}
        </div>
      ) : (
        <div className="space-y-6">
          <article className="bg-white border border-gray-200 rounded-xl p-6 shadow-sm space-y-4">
            <div className="flex flex-wrap justify-between items-start gap-3">
              <div>
                <h1 className="text-2xl font-bold text-gray-900">Aplikacja</h1>
                <p className="text-xs text-gray-400 font-mono mt-1 break-all">{application.id}</p>
              </div>
              <span className={`text-sm font-semibold px-3 py-1 rounded-full ${status?.className ?? ""}`}>
                {status?.label ?? application.status}
              </span>
            </div>

            <dl className="grid grid-cols-1 sm:grid-cols-2 gap-4 text-sm">
              <div>
                <dt className="text-gray-500">Oferta</dt>
                <dd className="font-medium text-gray-900">
                  {offer ? `${offer.title}${offer.company ? ` — ${offer.company}` : ""}` : application.offerId}
                </dd>
              </div>
              <div>
                <dt className="text-gray-500">Wysłano</dt>
                <dd className="font-medium text-gray-900">{formatDate(application.createdAt)}</dd>
              </div>
              <div>
                <dt className="text-gray-500">Wynik AI</dt>
                <dd className="font-medium text-gray-900">
                  {application.aiScore !== null ? `${application.aiScore} / 100` : application.aiStatus}
                </dd>
              </div>
            </dl>

            {application.aiError && (
              <p className="text-sm text-red-600">Błąd oceny AI: {application.aiError}</p>
            )}

            <div>
              <h2 className="text-sm text-gray-500 mb-1">List motywacyjny</h2>
              <p className="text-gray-800 whitespace-pre-line">
                {application.coverLetter || <span className="italic text-gray-400">Brak</span>}
              </p>
            </div>
          </article>

          {actionError && (
            <div className="p-4 bg-red-50 border border-red-200 text-red-700 rounded-lg">{actionError}</div>
          )}

          <section className="bg-white border border-gray-200 rounded-xl p-6 shadow-sm space-y-4">
            <h2 className="text-lg font-semibold text-gray-900">Działania</h2>

            {nextOptions.length > 0 ? (
              <form onSubmit={handleStatusChange} className="space-y-3">
                <div className="flex flex-wrap gap-2">
                  <select
                    value={nextStatus}
                    onChange={(e) => setNextStatus(e.target.value as ApplicationStatus)}
                    className="flex-1 min-w-0 p-2.5 border border-gray-300 rounded-lg text-sm"
                  >
                    <option value="">Zmień status...</option>
                    {nextOptions.map((s) => (
                      <option key={s} value={s}>
                        {STATUS_LABELS[s].label}
                      </option>
                    ))}
                  </select>
                  <button
                    type="submit"
                    disabled={!nextStatus || isBusy}
                    className="bg-lime-700 hover:bg-lime-800 text-white font-medium py-2.5 px-4 rounded-lg disabled:opacity-50"
                  >
                    Zapisz
                  </button>
                </div>
                <input
                  type="text"
                  value={note}
                  maxLength={1000}
                  onChange={(e) => setNote(e.target.value)}
                  placeholder="Notatka (opcjonalnie)"
                  className="w-full p-2.5 border border-gray-300 rounded-lg text-sm"
                />
              </form>
            ) : (
              <p className="text-sm text-gray-500">Status jest końcowy i nie może zostać zmieniony.</p>
            )}

            <button
              onClick={() => runAction(() => requestAiEvaluation(id))}
              disabled={isBusy || aiInProgress}
              className="bg-white border border-lime-700 text-lime-800 hover:bg-lime-50 font-medium py-2.5 px-4 rounded-lg disabled:opacity-50"
            >
              {aiInProgress ? "Ocena AI w toku..." : "Oceń ponownie przez AI"}
            </button>

            <form onSubmit={handleAddEvaluation} className="space-y-2 pt-4 border-t border-gray-100">
              <h3 className="text-sm font-semibold text-gray-700">Twoja ocena</h3>
              <div className="flex flex-wrap gap-2">
                <input
                  type="number"
                  min={0}
                  max={100}
                  value={score}
                  onChange={(e) => setScore(e.target.value)}
                  placeholder="0–100"
                  className="w-28 p-2.5 border border-gray-300 rounded-lg text-sm"
                />
                <input
                  type="text"
                  value={comment}
                  maxLength={2000}
                  onChange={(e) => setComment(e.target.value)}
                  placeholder="Komentarz (opcjonalnie)"
                  className="flex-1 min-w-0 p-2.5 border border-gray-300 rounded-lg text-sm"
                />
                <button
                  type="submit"
                  disabled={isBusy}
                  className="bg-lime-700 hover:bg-lime-800 text-white font-medium py-2.5 px-4 rounded-lg disabled:opacity-50"
                >
                  Dodaj
                </button>
              </div>
            </form>
          </section>

          <section className="space-y-4">
            <h2 className="text-lg font-semibold text-gray-900">Oceny</h2>
            {application.evaluations.length === 0 ? (
              <p className="text-sm text-gray-500">Brak ocen.</p>
            ) : (
              application.evaluations.map((ev) => (
                <article key={ev.id} className="bg-white border border-gray-200 rounded-xl p-5 shadow-sm space-y-3">
                  <div className="flex justify-between items-center">
                    <span className="text-sm font-medium text-gray-600">
                      {ev.source === "AI" ? "AI" : "Rekruter"} · {formatDate(ev.createdAt)}
                    </span>
                    <span className="text-xl font-bold text-gray-900">{ev.score}</span>
                  </div>
                  {ev.comment && <p className="text-sm text-gray-700 whitespace-pre-line">{ev.comment}</p>}
                  {ev.breakdown.length > 0 && (
                    <ul className="divide-y divide-gray-100 border border-gray-100 rounded-lg">
                      {ev.breakdown.map((b) => (
                        <li key={b.requirementId} className="p-3 text-sm">
                          <div className="flex justify-between items-center gap-2">
                            <span className="font-medium text-gray-800">
                              {b.name}
                              {b.isRequired && <span className="text-red-500"> *</span>}
                              <span className="text-gray-400 font-normal"> · waga {b.weight}</span>
                            </span>
                            <span className={`text-xs font-medium px-2 py-0.5 rounded-full ${MATCH_LABELS[b.level]?.className ?? ""}`}>
                              {MATCH_LABELS[b.level]?.label ?? b.level}
                            </span>
                          </div>
                          {b.evidence && <p className="text-gray-500 mt-1">{b.evidence}</p>}
                        </li>
                      ))}
                    </ul>
                  )}
                </article>
              ))
            )}
          </section>
        </div>
      )}
    </main>
  );
}
