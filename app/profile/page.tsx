"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { fetchUserProfile } from "@/services/profile-service";
import { UserProfile } from "@/types/profile";
import UploadBox from "@/components/ui/upload-box";
import UserCvList from "@/components/ui/user-cv-list";
import { deleteUserCV, fetchUserCV } from "@/services/cv-service";
import { UserCV } from "@/types/cv";
import EditProfileModal from "@/components/modals/EditProfileModal";
import { dictionaryLabel, EXPERIENCE_OPTIONS } from "@/types/offer";

export default function ProfilePage() {
  const router = useRouter();
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [cvList, setCvList] = useState<UserCV[]>([]);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [cvError, setCvError] = useState<string | null>(null);


    const loadCvList = async () => {
      try {
        const cvs = await fetchUserCV();
        if (cvs) {
          setCvList(cvs);
        }
      } catch (err) {
        console.error("CV load error:", err);
      }
    }

  useEffect(() => {
    async function loadProfile() {
      try {
        setLoading(true);
        const data = await fetchUserProfile();
        setProfile(data);
      } catch (err) {
        console.error("Profile load error:", err);
        setError(err instanceof Error ? err.message : "Wystąpił błąd");
        router.push("/login");
      } finally {
        setLoading(false);
      }
    }

    loadProfile();
    loadCvList();
  }, [router]);

  if (loading) {
    return (
      <div className="container mx-auto py-12 px-4 flex justify-center items-center min-h-[50vh]">
        <div className="flex items-center space-x-3 text-lime-700">
          <svg className="w-6 h-6 animate-spin" fill="none" viewBox="0 0 24 24">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
          </svg>
          <span className="font-medium text-gray-700">Ładowanie profilu...</span>
        </div>
      </div>
    );
  }

  if (error || !profile) {
    return null; 
  }

  return (
    <div className="container mx-auto py-10 px-4 max-w-3xl">
           <EditProfileModal
              isOpen={isModalOpen}
              onClose={() => setIsModalOpen(false)}
              profile={profile}
              onProfileUpdated={(updatedProfile) => setProfile(updatedProfile)}
            />
            
      <Link
        href="/offers"
        className="inline-flex items-center text-sm font-medium text-lime-700 hover:text-lime-800 mb-6 transition-colors"
      >
        ← Powrót do ofert pracy
      </Link>

      

      <div className="bg-white border border-gray-200 rounded-2xl p-8 shadow-sm">
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center border-b border-gray-100 pb-6 mb-6 gap-4">
          <div>
            <span className="text-xs font-semibold text-lime-700 uppercase tracking-wider">
              Konto Kandydata
            </span>
            <h1 className="text-3xl font-bold text-gray-900 mt-1">
              {profile.firstName} {profile.lastName}
            </h1>
            <p className="text-sm text-gray-500 mt-1 flex items-center gap-2">
              <span>📍 {profile.city || "Brak podanego miasta"}</span>
              <span>•</span>
              <span>📞 {profile.phoneNumber || "Brak numeru telefonu"}</span>
            </p>
          </div>

          <button
            onClick={() => setIsModalOpen(true)}
            className="px-4 py-2 text-sm font-medium text-lime-700 border border-lime-700/30 rounded-lg hover:bg-lime-50 transition-colors"
          >
            Edytuj profil
          </button>
        </div>

        <div className="space-y-6">
          <div>
            <h2 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-1">
              Doświadczenie zawodowe
            </h2>
            <p className="text-lg font-bold text-gray-800">
              {dictionaryLabel(EXPERIENCE_OPTIONS, profile.experience)}
            </p>
          </div>

          <div>
            <h2 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-3">
              Stack Technologiczny
            </h2>
            {profile.stack && profile.stack.length > 0 ? (
              <div className="flex flex-wrap gap-2">
                {profile.stack.map((tech) => (
                  <span
                    key={tech}
                    className="bg-lime-100 text-lime-900 text-sm px-3 py-1 rounded-md font-medium border border-lime-200"
                  >
                    {tech}
                  </span>
                ))}
              </div>
            ) : (
              <p className="text-gray-400 text-sm italic">Brak dodanych technologii.</p>
            )}
          </div>

          <div className="max-w-2xl mx-auto p-6 bg-white rounded-2xl border border-gray-200 shadow-sm mt-8">
            <h2 className="text-lg font-bold text-gray-900 mb-4">Dodaj lub zaktualizuj swoje CV</h2>
            
            <UploadBox onSuccess={() => loadCvList()} />

            {cvError && (
              <p className="text-sm text-red-600 mt-3">{cvError}</p>
            )}

            <UserCvList
              cvList={cvList}
              onDelete={async (id) => {
                if (!confirm("Czy na pewno chcesz usunąć to CV?")) return;
                setCvError(null);
                try {
                  await deleteUserCV(id);
                  setCvList((prev) => prev.filter((cv) => cv.id !== id));
                } catch (err) {
                  setCvError(err instanceof Error ? err.message : "Nie udało się usunąć CV.");
                }
              }}
            />
          </div>
        </div>
      </div>
    </div>
  );
}