"use client";

import { useState } from "react";
import { updateUserProfile } from "@/services/profile-service";
import { UserProfile } from "@/types/profile";
import Modal from "@/components/ui/modal";

interface EditProfileModalProps {
  isOpen: boolean;
  onClose: () => void;
  profile: UserProfile;
  onProfileUpdated: (updatedProfile: UserProfile) => void;
}

export default function EditProfileModal({
  isOpen,
  onClose,
  profile,
  onProfileUpdated,
}: EditProfileModalProps) {
  const [firstName, setFirstName] = useState(profile.firstName || "");
  const [lastName, setLastName] = useState(profile.lastName || "");
  const [phoneNumber, setPhoneNumber] = useState(profile.phoneNumber || "");
  const [city, setCity] = useState(profile.city || "");
  const [experience, setExperience] = useState<number>(profile.experience || 0);
  const [stack, setStack] = useState<string[]>(profile.stack || []);
  const [newTech, setNewTech] = useState("");

  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (!isOpen) {
    return null;
  }

  const handleAddTech = (e: React.FormEvent) => {
    e.preventDefault();
    const tech = newTech.trim();
    if (tech && !stack.includes(tech)) {
      setStack((prev) => [...prev, tech]);
      setNewTech("");
    }
  };

  const handleRemoveTech = (techToRemove: string) => {
    setStack((prev) => prev.filter((tech) => tech !== techToRemove));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setError(null);

    try {
      const updated = await updateUserProfile({
        firstName,
        lastName,
        phoneNumber,
        city,
        experience,
        stack,
      });
      onProfileUpdated(updated);
      onClose(); 
    } catch (err) {
      setError("Nie udało się zaktualizować profilu.");
    } finally {
      setIsSubmitting(false);
    }
  };

    return (
    <Modal isOpen={isOpen} onClose={onClose} title="Edycja profilu">
        <form onSubmit={handleSubmit} className="space-y-4">
          {error && (
            <div className="bg-red-50 text-red-600 text-xs p-3 rounded-lg border border-red-200">
              {error}
            </div>
          )}

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-xs font-semibold text-gray-700 mb-1">Imię</label>
              <input
                type="text"
                value={firstName}
                onChange={(e) => setFirstName(e.target.value)}
                required
                className="w-full px-3 py-2 border rounded-lg text-sm text-gray-800 focus:outline-none focus:ring-2 focus:ring-lime-600 border-gray-300"
              />
            </div>
            <div>
              <label className="block text-xs font-semibold text-gray-700 mb-1">Nazwisko</label>
              <input
                type="text"
                value={lastName}
                onChange={(e) => setLastName(e.target.value)}
                required
                className="w-full px-3 py-2 border rounded-lg text-sm text-gray-800 focus:outline-none focus:ring-2 focus:ring-lime-600 border-gray-300"
              />
            </div>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-xs font-semibold text-gray-700 mb-1">Telefon</label>
              <input
                type="text"
                value={phoneNumber}
                onChange={(e) => setPhoneNumber(e.target.value)}
                className="w-full px-3 py-2 border rounded-lg text-sm text-gray-800 focus:outline-none focus:ring-2 focus:ring-lime-600 border-gray-300"
              />
            </div>
            <div>
              <label className="block text-xs font-semibold text-gray-700 mb-1">Miasto</label>
              <input
                type="text"
                value={city}
                onChange={(e) => setCity(e.target.value)}
                className="w-full px-3 py-2 border rounded-lg text-sm text-gray-800 focus:outline-none focus:ring-2 focus:ring-lime-600 border-gray-300"
              />
            </div>
          </div>

          <div>
            <label className="block text-xs font-semibold text-gray-700 mb-1">
              Doświadczenie (lata)
            </label>
            <input
              type="number"
              min="0"
              max="50"
              value={experience}
              onChange={(e) => setExperience(Number(e.target.value))}
              className="w-full px-3 py-2 border rounded-lg text-sm text-gray-800 focus:outline-none focus:ring-2 focus:ring-lime-600 border-gray-300"
            />
          </div>

          <div>
            <label className="block text-xs font-semibold text-gray-700 mb-1">
              Stack Technologiczny
            </label>

            <div className="flex gap-2 mb-2">
              <input
                type="text"
                value={newTech}
                onChange={(e) => setNewTech(e.target.value)}
                placeholder="np. Docker"
                className="flex-grow px-3 py-1.5 border rounded-lg text-sm border-gray-300 focus:outline-none focus:ring-2 focus:ring-lime-600"
              />
              <button
                type="button"
                onClick={handleAddTech}
                className="bg-lime-700 text-white text-xs px-3 py-1.5 rounded-lg hover:bg-lime-800 font-medium"
              >
                Dodaj
              </button>
            </div>

            <div className="flex flex-wrap gap-1.5 border p-2.5 rounded-lg min-h-[50px] bg-gray-50 border-gray-200">
              {stack.map((tech) => (
                <span
                  key={tech}
                  className="inline-flex items-center gap-1 bg-lime-100 text-lime-900 text-xs px-2.5 py-1 rounded-md font-medium border border-lime-200"
                >
                  {tech}
                  <button
                    type="button"
                    onClick={() => handleRemoveTech(tech)}
                    className="text-lime-700 hover:text-red-600 font-bold ml-1"
                  >
                    ×
                  </button>
                </span>
              ))}
            </div>
          </div>

          <div className="border-t pt-4 flex justify-end gap-3">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-sm font-medium text-gray-600 hover:bg-gray-100 rounded-lg transition-colors"
            >
              Anuluj
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="px-4 py-2 text-sm font-medium text-white bg-lime-700 hover:bg-lime-800 rounded-lg transition-colors shadow disabled:opacity-50"
            >
              {isSubmitting ? "Zapisywanie..." : "Zapisz zmiany"}
            </button>
          </div>
    </form>
  </Modal>
)
}