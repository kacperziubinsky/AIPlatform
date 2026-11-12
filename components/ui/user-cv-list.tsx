"use client";

import { UserCV } from "@/types/cv";

const PARSE_STATUS: Record<string, { label: string; className: string }> = {
  Pending: { label: "Oczekuje na analizę", className: "bg-gray-100 text-gray-600" },
  Processing: { label: "Analizowanie", className: "bg-amber-100 text-amber-800" },
  Completed: { label: "Przeanalizowane", className: "bg-lime-100 text-lime-800" },
  Failed: { label: "Błąd analizy", className: "bg-red-100 text-red-700" },
};

interface UserCvListProps {
  cvList: UserCV[];
  onDelete?: (id: string) => void;
}

export default function UserCvList({ cvList, onDelete }: UserCvListProps) {
  if (!cvList || cvList.length === 0) {
    return (
      <p className="text-sm text-gray-400 italic mt-3">
        Nie masz jeszcze przesłanych dokumentów CV.
      </p>
    );
  }

  return (
    <div className="space-y-3 mt-4">
      <h3 className="text-sm font-semibold text-gray-700">Twoje pliki CV:</h3>
      <ul className="divide-y divide-gray-100 border border-gray-200 rounded-xl bg-gray-50/50">
        {cvList.map((cv) => (
          <li
            key={cv.id}
            className="flex items-center justify-between p-3.5 hover:bg-gray-100/60 transition-colors"
          >
            <div className="flex items-center space-x-3">
              <svg className="w-6 h-6 text-lime-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
              <div>
                <p className="text-sm font-medium text-gray-800">{cv.filename}</p>
                <p className="text-xs text-gray-400">
                  Dodano: {new Date(cv.uploadDate).toLocaleDateString("pl-PL")}
                </p>
              </div>
            </div>

            <div className="flex items-center space-x-2">
              {PARSE_STATUS[cv.parseStatus] && (
                <span className={`text-xs font-medium px-2 py-0.5 rounded-full ${PARSE_STATUS[cv.parseStatus].className}`}>
                  {PARSE_STATUS[cv.parseStatus].label}
                </span>
              )}

              {onDelete && (
                <button
                  onClick={() => onDelete(cv.id)}
                  className="text-xs text-red-500 hover:text-red-700 px-2 py-1 rounded hover:bg-red-50 transition-colors"
                >
                  Usuń
                </button>
              )}
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
}