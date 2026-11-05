"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { useState, useEffect } from "react";
import { Category, Experience } from "@/types/offer";

export default function FilterBar() {
  const router = useRouter();
  const searchParams = useSearchParams();

  const [category, setCategory] = useState<string>(searchParams.get("category") || "");
  const [experience, setExperience] = useState<string>(searchParams.get("experience") || "");
  const [minSalary, setMinSalary] = useState<string>(searchParams.get("minSalary") || "");
  const [maxSalary, setMaxSalary] = useState<string>(searchParams.get("maxSalary") || "");

  useEffect(() => {
    setCategory(searchParams.get("category") || "");
    setExperience(searchParams.get("experience") || "");
    setMinSalary(searchParams.get("minSalary") || "");
    setMaxSalary(searchParams.get("maxSalary") || "");
  }, [searchParams]);

  const applyFilters = (updates: Record<string, string | null>) => {
    const params = new URLSearchParams(searchParams.toString());

    Object.entries(updates).forEach(([key, value]) => {
      if (value !== null && value !== "") {
        params.set(key, value);
      } else {
        params.delete(key);
      }
    });

    router.push(`/offers?${params.toString()}`);
  };

  const handleReset = () => {
    setCategory("");
    setExperience("");
    setMinSalary("");
    setMaxSalary("");

    const searchTerm = searchParams.get("searchTerm");
    router.push(searchTerm ? `/offers?searchTerm=${encodeURIComponent(searchTerm)}` : "/offers");
  };

  return (
    <div className="bg-white p-4 border border-gray-200 rounded-lg shadow-sm mt-4 grid grid-cols-1 md:grid-cols-4 gap-4 items-end">
      <div>
        <label className="block text-xs font-semibold text-gray-600 mb-1 uppercase tracking-wider">
          Kategoria
        </label>
        <select
          value={category}
          onChange={(e) => applyFilters({ category: e.target.value })}
          className="w-full border border-gray-300 rounded-md p-2 text-sm text-gray-700 bg-white focus:outline-none focus:ring-2 focus:ring-lime-600"
        >
          <option value="">Wszystkie kategorie</option>
          <option value={Category.Frontend}>Frontend</option>
          <option value={Category.Backend}>Backend</option>
          <option value={Category.FullStack}>FullStack</option>
          <option value={Category.Ai}>AI</option>
          <option value={Category.Software}>Software</option>
          <option value={Category.Pm}>PM (Project Manager)</option>
        </select>
      </div>

      <div>
        <label className="block text-xs font-semibold text-gray-600 mb-1 uppercase tracking-wider">
          Poziom
        </label>
        <select
          value={experience}
          onChange={(e) => applyFilters({ experience: e.target.value })}
          className="w-full border border-gray-300 rounded-md p-2 text-sm text-gray-700 bg-white focus:outline-none focus:ring-2 focus:ring-lime-600"
        >
          <option value="">Wszystkie poziomy</option>
          <option value={Experience.Trainee}>Stażysta / Trainee</option>
          <option value={Experience.Junior}>Junior</option>
          <option value={Experience.Middle}>Mid / Middle</option>
          <option value={Experience.Senior}>Senior</option>
        </select>
      </div>

      <div>
        <label className="block text-xs font-semibold text-gray-600 mb-1 uppercase tracking-wider">
          Wynagrodzenie (PLN)
        </label>
        <div className="flex gap-2">
          <input
            type="number"
            placeholder="Od"
            value={minSalary}
            onChange={(e) => setMinSalary(e.target.value)}
            onBlur={() => applyFilters({ minSalary })}
            className="w-1/2 border border-gray-300 rounded-md p-2 text-sm text-gray-700 focus:outline-none focus:ring-2 focus:ring-lime-600"
          />
          <input
            type="number"
            placeholder="Do"
            value={maxSalary}
            onChange={(e) => setMaxSalary(e.target.value)}
            onBlur={() => applyFilters({ maxSalary })}
            className="w-1/2 border border-gray-300 rounded-md p-2 text-sm text-gray-700 focus:outline-none focus:ring-2 focus:ring-lime-600"
          />
        </div>
      </div>

      <div>
        <button
          onClick={handleReset}
          className="w-full border border-gray-300 text-gray-600 px-3 py-2 rounded-md text-sm font-medium hover:bg-gray-50 transition-colors"
        >
          Wyczyść filtry
        </button>
      </div>
    </div>
  );
}