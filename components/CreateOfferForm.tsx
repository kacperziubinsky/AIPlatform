"use client";

import { useEffect, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import { CreateOffer, UpdateOffer } from "@/services/offer-service";
import { Category, Experience, JobOffer, OFFER_LIMITS, OfferRequirementRequest } from "@/types/offer";
import { useOfferDictionaries } from "@/lib/use-offer-dictionaries";

const emptyRequirement = (): OfferRequirementRequest => ({
  name: "",
  description: "",
  weight: 3,
  isRequired: false,
});

const inputClass =
  "w-full p-2.5 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-lime-600 focus:outline-none";
const labelClass = "block text-sm font-medium text-gray-700 mb-1";

const toDateInput = (iso?: string) => (iso ? iso.slice(0, 10) : "");

interface OfferFormProps {
  initialOffer?: JobOffer;
  redirectTo?: string;
}

export default function CreateOfferForm({ initialOffer, redirectTo }: OfferFormProps = {}) {
  const router = useRouter();
  const isEdit = !!initialOffer;
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const errorRef = useRef<HTMLDivElement>(null);
  const { categories, experiences } = useOfferDictionaries();

  useEffect(() => {
    if (error) errorRef.current?.scrollIntoView({ behavior: "smooth", block: "center" });
  }, [error]);

  const [title, setTitle] = useState(initialOffer?.title ?? "");
  const [description, setDescription] = useState(initialOffer?.description ?? "");
  const [company, setCompany] = useState(initialOffer?.company ?? "");
  const [salary, setSalary] = useState(initialOffer ? String(initialOffer.salary) : "");
  const [category, setCategory] = useState<Category>((initialOffer?.category as Category) ?? Category.Backend);
  const [experience, setExperience] = useState<Experience>((initialOffer?.experience as Experience) ?? Experience.Junior);
  const [stackInput, setStackInput] = useState(initialOffer?.stack.join(", ") ?? "");
  const [startDate, setStartDate] = useState(toDateInput(initialOffer?.startDate));
  const [endDate, setEndDate] = useState(toDateInput(initialOffer?.endDate));
  const [requirements, setRequirements] = useState<OfferRequirementRequest[]>(
    initialOffer?.requirements.length
      ? initialOffer.requirements.map(({ name, description, weight, isRequired }) => ({
          name,
          description,
          weight,
          isRequired,
        }))
      : [emptyRequirement()]
  );

  const updateRequirement = (
    index: number,
    patch: Partial<OfferRequirementRequest>
  ) => {
    setRequirements((prev) =>
      prev.map((r, i) => (i === index ? { ...r, ...patch } : r))
    );
  };

  const removeRequirement = (index: number) => {
    setRequirements((prev) => prev.filter((_, i) => i !== index));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    const token = localStorage.getItem("accessToken");
    if (!token) {
      setError("Brak sesji. Zaloguj się ponownie.");
      router.push("/login");
      return;
    }

    if (new Date(endDate) < new Date(startDate)) {
      setError("Data zakończenia nie może być wcześniejsza niż data rozpoczęcia.");
      return;
    }

    const stack = stackInput
      .split(",")
      .map((s) => s.trim())
      .filter((s) => s.length > 0);

    const cleanRequirements = requirements.filter((r) => r.name.trim().length > 0);

    setIsLoading(true);
    try {
      const payload = {
        title,
        description,
        salary: Number(salary),
        category,
        experience,
        stack,
        requirements: cleanRequirements,
        startDate: new Date(startDate).toISOString(),
        endDate: new Date(endDate).toISOString(),
        company,
      };

      if (isEdit) {
        await UpdateOffer(initialOffer.id, payload, token);
      } else {
        await CreateOffer(payload, token);
      }

      router.push(redirectTo ?? (isEdit ? "/recruitment" : "/offers"));
      router.refresh();
    } catch (err) {
      setError(err instanceof Error ? err.message : isEdit ? "Nie udało się zaktualizować oferty." : "Nie udało się utworzyć oferty.");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <form
      onSubmit={handleSubmit}
      className="w-full max-w-2xl bg-white p-8 border border-gray-200 rounded-2xl shadow-sm space-y-4"
    >
      <h1 className="text-xl font-semibold text-gray-900">
        {isEdit ? "Edycja oferty" : "Nowa oferta pracy"}
      </h1>

      {error && (
        <div
          ref={errorRef}
          role="alert"
          className="flex items-start justify-between gap-3 p-3 bg-red-50 border border-red-200 text-red-700 text-sm rounded-lg"
        >
          <span>{error}</span>
          <button type="button" onClick={() => setError(null)} className="text-red-400 hover:text-red-600" aria-label="Zamknij">
            ✕
          </button>
        </div>
      )}

      <div>
        <label className={labelClass}>Tytuł</label>
        <input
          type="text"
          required
          minLength={3}
          maxLength={OFFER_LIMITS.title}
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          className={inputClass}
          placeholder="Senior .NET Developer"
        />
        <p className="text-xs text-gray-400 mt-1 text-right">
          {title.length}/{OFFER_LIMITS.title}
        </p>
      </div>

      <div>
        <label className={labelClass}>Opis</label>
        <textarea
          required
          rows={4}
          maxLength={OFFER_LIMITS.description}
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          className={inputClass}
          placeholder="Rozwój mikroserwisów w .NET z bazą PostgreSQL."
        />
      </div>

      <div className="grid grid-cols-2 gap-3">
        <div>
          <label className={labelClass}>Firma</label>
          <input
            type="text"
            required
            maxLength={OFFER_LIMITS.company}
            value={company}
            onChange={(e) => setCompany(e.target.value)}
            className={inputClass}
            placeholder="Test Corp Sp. z o.o."
          />
        </div>
        <div>
          <label className={labelClass}>Wynagrodzenie (PLN)</label>
          <input
            type="number"
            required
            min={0}
            value={salary}
            onChange={(e) => setSalary(e.target.value)}
            className={inputClass}
            placeholder="20000"
          />
        </div>
      </div>

      <div className="grid grid-cols-2 gap-3">
        <div>
          <label className={labelClass}>Kategoria</label>
          <select
            value={category}
            onChange={(e) => setCategory(Number(e.target.value) as Category)}
            className={`${inputClass} bg-white`}
          >
            {categories.map((c) => (
              <option key={c.value} value={c.value}>
                {c.label}
              </option>
            ))}
          </select>
        </div>
        <div>
          <label className={labelClass}>Doświadczenie</label>
          <select
            value={experience}
            onChange={(e) => setExperience(Number(e.target.value) as Experience)}
            className={`${inputClass} bg-white`}
          >
            {experiences.map((x) => (
              <option key={x.value} value={x.value}>
                {x.label}
              </option>
            ))}
          </select>
        </div>
      </div>

      <div>
        <label className={labelClass}>Stos technologiczny (rozdziel po przecinku)</label>
        <input
          type="text"
          required
          value={stackInput}
          onChange={(e) => setStackInput(e.target.value)}
          className={`${inputClass} font-mono text-xs`}
          placeholder="C#, .NET, PostgreSQL, Kubernetes"
        />
      </div>

      <div className="grid grid-cols-2 gap-3">
        <div>
          <label className={labelClass}>Data rozpoczęcia</label>
          <input
            type="date"
            required
            value={startDate}
            onChange={(e) => setStartDate(e.target.value)}
            className={inputClass}
          />
        </div>
        <div>
          <label className={labelClass}>Data zakończenia</label>
          <input
            type="date"
            required
            value={endDate}
            onChange={(e) => setEndDate(e.target.value)}
            className={inputClass}
          />
        </div>
      </div>

      <div>
        <div className="flex items-center justify-between mb-2">
          <label className="text-sm font-medium text-gray-700">Wymagania</label>
          <button
            type="button"
            onClick={() => setRequirements((prev) => [...prev, emptyRequirement()])}
            className="text-sm text-lime-700 hover:text-lime-800 font-medium"
          >
            + Dodaj wymaganie
          </button>
        </div>

        <div className="space-y-3">
          {requirements.map((req, i) => (
            <div key={i} className="p-3 border border-gray-200 rounded-lg space-y-2">
              <div className="grid grid-cols-2 gap-2">
                <input
                  type="text"
                  value={req.name}
                  onChange={(e) => updateRequirement(i, { name: e.target.value })}
                  className={inputClass}
                  placeholder="Nazwa (np. PostgreSQL)"
                />
                <select
                  value={req.weight}
                  onChange={(e) => updateRequirement(i, { weight: Number(e.target.value) })}
                  className={`${inputClass} bg-white`}
                >
                  {[1, 2, 3, 4, 5].map((w) => (
                    <option key={w} value={w}>
                      Waga: {w}
                    </option>
                  ))}
                </select>
              </div>
              <input
                type="text"
                value={req.description}
                onChange={(e) => updateRequirement(i, { description: e.target.value })}
                className={inputClass}
                placeholder="Opis wymagania"
              />
              <div className="flex items-center justify-between">
                <label className="flex items-center gap-2 text-sm text-gray-700">
                  <input
                    type="checkbox"
                    checked={req.isRequired}
                    onChange={(e) => updateRequirement(i, { isRequired: e.target.checked })}
                    className="accent-lime-700"
                  />
                  Wymagane
                </label>
                {requirements.length > 1 && (
                  <button
                    type="button"
                    onClick={() => removeRequirement(i)}
                    className="text-sm text-red-600 hover:text-red-700"
                  >
                    Usuń
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      </div>

      <button
        type="submit"
        disabled={isLoading}
        className="w-full bg-lime-700 hover:bg-lime-800 text-white font-medium py-2.5 rounded-lg transition-colors disabled:opacity-50"
      >
        {isLoading ? "Zapisywanie..." : isEdit ? "Zapisz zmiany" : "Opublikuj ofertę"}
      </button>
    </form>
  );
}