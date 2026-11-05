"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";

interface SearchBarProps {
  initialQuery?: string;
}

export default function SearchBar({ initialQuery = "" }: SearchBarProps) {
  const [query, setQuery] = useState(initialQuery);
  const router = useRouter();

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    const trimmed = query.trim();

    if (trimmed) {
      router.push(`/offers?searchTerm=${encodeURIComponent(trimmed)}`);
    } else {
      router.push("/offers");
    }
  };

  const handleClear = () => {
    setQuery("");
    router.push("/offers");
  };

  return (
    <div className="w-full max-w-md mt-4">
      <form
        onSubmit={handleSearch}
        className="group relative flex items-center border border-gray-300 rounded-lg p-1.5 bg-white transition-all duration-300 focus-within:border-lime-600 focus-within:ring-2 focus-within:ring-lime-600/30 hover:border-gray-400"
      >
        <div className="pl-2 pr-1 text-gray-400 group-focus-within:text-lime-700">
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth="2"
              d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
            />
          </svg>
        </div>

        <input
          type="text"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder="Szukaj stanowiska, technologii lub firmy..."
          className="flex-grow px-2 py-1 text-gray-700 placeholder-gray-400 focus:outline-none w-full bg-transparent"
        />

        {query && (
          <button
            type="button"
            onClick={handleClear}
            className="mr-2 text-gray-400 hover:text-gray-600 transition-all hover:scale-110 active:scale-95"
            aria-label="Wyszczyść"
          >
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        )}

        <button
          type="submit"
          className="bg-lime-700 text-white px-4 py-1.5 rounded-md font-medium transition-all duration-200 hover:bg-lime-800 hover:shadow-md active:scale-95"
        >
          Szukaj
        </button>
      </form>
    </div>
  );
}