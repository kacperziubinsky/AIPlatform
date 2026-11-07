"use client";

import { useEffect, useState } from "react";
import Modal from "@/components/ui/modal";
import { createApplication } from "@/services/application-service";
import { fetchUserCV } from "@/services/cv-service";
import { UserCV } from "@/types/cv";

interface CreateApplicationModalProps {
  isOpen: boolean;
  onClose: () => void;
  offerId: string;
  offerTitle: string;
}

export default function CreateApplicationModal({
  isOpen,
  onClose,
  offerId,
  offerTitle,
}: CreateApplicationModalProps) {
  const [cvList, setCvList] = useState<UserCV[]>([]);
  const [selectedCvId, setSelectedCvId] = useState<string>("");
  const [message, setMessage] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  useEffect(() => {
    if (!isOpen) return;

    setError(null);
    setSuccess(false);
    setMessage("");

    fetchUserCV()
      .then((cvs) => {
        setCvList(cvs);
        setSelectedCvId(cvs[0]?.id ?? "");
      })
      .catch(() => setError("Nie udało się pobrać listy CV."));
  }, [isOpen]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setError(null);

    try {
      await createApplication({ offerId, cvId: selectedCvId, coverLetter: message.trim() || undefined });
      setSuccess(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Wystąpił błąd.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Aplikuj na ofertę">
      {success ? (
        <div className="text-center py-4">
          <p className="text-lg font-semibold text-lime-700">Aplikacja wysłana! ✓</p>
          <p className="text-sm text-gray-500 mt-1">Powodzenia w rekrutacji.</p>
          <button
            onClick={onClose}
            className="mt-4 px-4 py-2 text-sm font-medium text-white bg-lime-700 hover:bg-lime-800 rounded-lg"
          >
            Zamknij
          </button>
        </div>
      ) : (
        <form onSubmit={handleSubmit} className="space-y-4">
          <p className="text-sm text-gray-600">
            Aplikujesz na: <strong>{offerTitle}</strong>
          </p>

          {error && (
            <div className="bg-red-50 text-red-600 text-xs p-3 rounded-lg border border-red-200">
              {error}
            </div>
          )}

          {cvList.length === 0 ? (
            <p className="text-sm text-gray-400 italic">
              Nie masz jeszcze żadnego CV. Dodaj je najpierw w swoim profilu.
            </p>
          ) : (
            <div>
              <label className="block text-xs font-semibold text-gray-700 mb-2">Wybierz CV</label>
              <div className="space-y-2">
                {cvList.map((cv) => (
                  <label
                    key={cv.id}
                    className={`flex items-center gap-3 p-3 border rounded-lg cursor-pointer transition-colors ${
                      selectedCvId === cv.id
                        ? "border-lime-600 bg-lime-50"
                        : "border-gray-200 hover:bg-gray-50"
                    }`}
                  >
                    <input
                      type="radio"
                      name="cv"
                      value={cv.id}
                      checked={selectedCvId === cv.id}
                      onChange={() => setSelectedCvId(cv.id)}
                      className="accent-lime-700"
                    />
                    <span className="text-sm text-gray-800">{cv.filename}</span>
                  </label>
                ))}
              </div>
            </div>
          )}

          <div>
            <label className="block text-xs font-semibold text-gray-700 mb-1">
                Wiadomość do rekrutera <span className="font-normal text-gray-400">(opcjonalnie)</span>
            </label>
            <textarea
                value={message}
                onChange={(e) => setMessage(e.target.value)}
                maxLength={1000}
                rows={4}
                placeholder="Napisz kilka słów o sobie i dlaczego ta oferta Cię interesuje..."
                className="w-full px-3 py-2 border rounded-lg text-sm text-gray-800 resize-none focus:outline-none focus:ring-2 focus:ring-lime-600 border-gray-300"
            />
            <p className="text-xs text-gray-400 text-right mt-1">{message.length}/1000</p>
            </div>

          <div className="border-t pt-4 flex justify-end gap-3">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-sm font-medium text-gray-600 hover:bg-gray-100 rounded-lg"
            >
              Anuluj
            </button>
            <button
              type="submit"
              disabled={isSubmitting || !selectedCvId}
              className="px-4 py-2 text-sm font-medium text-white bg-lime-700 hover:bg-lime-800 rounded-lg disabled:opacity-50"
            >
              {isSubmitting ? "Wysyłanie..." : "Aplikuj"}
            </button>
          </div>
        </form>
      )}
    </Modal>
  );
}