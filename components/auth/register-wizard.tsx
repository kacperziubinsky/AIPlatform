"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { RegisterUserStep2, RegisterUserStep1 } from "@/services/auth-service";
import { LoginUser } from "@/services/auth-service";
import { Experience, EXPERIENCE_OPTIONS } from "@/types/offer";

type Role = "Applicant" | "Recruiter";

export default function RegisterWizard() {
  const router = useRouter();
  const [step, setStep] = useState<1 | 2>(1);
  const [role, setRole] = useState<Role>("Applicant");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const [firstname, setFirstname] = useState("");
  const [lastname, setLastname] = useState("");
  const [phone, setPhone] = useState("+48");
  const [city, setCity] = useState("");
  const [stackInput, setStackInput] = useState("C#, .NET, PostgreSQL, Docker");
  const [experience, setExperience] = useState<Experience>(Experience.Junior);

  const isRecruiter = role === "Recruiter";

  const handleStep1Submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError(null);

    try {
      await RegisterUserStep1({
        email,
        password,
        role,
      });

      const authData = await LoginUser({ email, password });
      localStorage.setItem("accessToken", authData.token);

      setStep(2);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Błąd rejestracji.");
    } finally {
      setIsLoading(false);
    }
  };

  const handleStep2Submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError(null);

    const token = localStorage.getItem("accessToken");
    if (!token) {
      setError("Brak sesji. Zaloguj się ponownie.");
      setIsLoading(false);
      router.push("/login");
      return;
    }

    const stackArray = isRecruiter
      ? []
      : stackInput
          .split(",")
          .map((item) => item.trim())
          .filter((item) => item.length > 0);

    try {
      await RegisterUserStep2(
        {
          firstName: firstname,
          lastName: lastname,
          phoneNumber: phone,
          city,
          stack: stackArray,
          experience: Number(experience) as Experience,
        },
        token
      );

      router.push("/offers");
      router.refresh();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Nie udało się zapisać profilu.");
    } finally {
      setIsLoading(false);
    }
  };

  const inputClass =
    "w-full p-2.5 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-lime-600 focus:outline-none";

  return (
    <div className="w-full max-w-md bg-white p-8 border border-gray-200 rounded-2xl shadow-sm">
      <div className="flex items-center justify-between mb-6">
        <div className={`flex-1 text-center font-semibold text-xs py-1 border-b-2 ${step === 1 ? "border-lime-600 text-lime-700" : "border-gray-200 text-gray-400"}`}>
          1. Konto
        </div>
        <div className={`flex-1 text-center font-semibold text-xs py-1 border-b-2 ${step === 2 ? "border-lime-600 text-lime-700" : "border-gray-200 text-gray-400"}`}>
          {isRecruiter ? "2. Profil Rekrutera" : "2. Profil Kandydata"}
        </div>
      </div>

      {error && (
        <div className="mb-4 p-3 bg-red-50 border border-red-200 text-red-700 text-sm rounded-lg">
          {error}
        </div>
      )}

      {step === 1 && (
        <form onSubmit={handleStep1Submit} className="space-y-4">
          <div className="grid grid-cols-2 gap-1 p-1 bg-gray-100 rounded-lg">
            <button
              type="button"
              onClick={() => setRole("Applicant")}
              className={`py-2 text-sm font-medium rounded-md transition-colors ${
                !isRecruiter ? "bg-white text-lime-700 shadow-sm" : "text-gray-500 hover:text-gray-700"
              }`}
            >
              Szukam pracy
            </button>
            <button
              type="button"
              onClick={() => setRole("Recruiter")}
              className={`py-2 text-sm font-medium rounded-md transition-colors ${
                isRecruiter ? "bg-white text-lime-700 shadow-sm" : "text-gray-500 hover:text-gray-700"
              }`}
            >
              Jestem rekruterem
            </button>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">E-mail</label>
            <input
              type="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className={inputClass}
              placeholder={isRecruiter ? "rekruter@firma.com" : "kandydat@example.com"}
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Hasło</label>
            <input
              type="password"
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className={inputClass}
              placeholder="••••••••"
            />
          </div>

          <button
            type="submit"
            disabled={isLoading}
            className="w-full bg-lime-700 hover:bg-lime-800 text-white font-medium py-2.5 rounded-lg transition-colors disabled:opacity-50 mt-2"
          >
            {isLoading ? "Tworzenie konta..." : "Przejdź do profilu →"}
          </button>
        </form>
      )}

      {step === 2 && (
        <form onSubmit={handleStep2Submit} className="space-y-4">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Imię</label>
              <input
                type="text"
                required
                value={firstname}
                onChange={(e) => setFirstname(e.target.value)}
                className={inputClass}
                placeholder="Jan"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Nazwisko</label>
              <input
                type="text"
                required
                value={lastname}
                onChange={(e) => setLastname(e.target.value)}
                className={inputClass}
                placeholder="Testowy"
              />
            </div>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Telefon</label>
              <input
                type="tel"
                required
                value={phone}
                onChange={(e) => setPhone(e.target.value)}
                className={inputClass}
                placeholder="+48123456789"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Miasto</label>
              <input
                type="text"
                required
                value={city}
                onChange={(e) => setCity(e.target.value)}
                className={inputClass}
                placeholder="Warszawa"
              />
            </div>
          </div>

          {!isRecruiter && (
            <>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Doświadczenie
                </label>
                <select
                  value={experience}
                  onChange={(e) => setExperience(Number(e.target.value) as Experience)}
                  className="w-full p-2.5 border border-gray-300 rounded-lg text-sm bg-white focus:ring-2 focus:ring-lime-600 focus:outline-none"
                >
                  {EXPERIENCE_OPTIONS.map((x) => (
                    <option key={x.value} value={x.value}>
                      {x.label}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Stos technologiczny (rozdziel po przecinku)
                </label>
                <input
                  type="text"
                  required
                  value={stackInput}
                  onChange={(e) => setStackInput(e.target.value)}
                  className="w-full p-2.5 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-lime-600 focus:outline-none font-mono text-xs"
                  placeholder="C#, .NET, PostgreSQL, Docker"
                />
              </div>
            </>
          )}

          <button
            type="submit"
            disabled={isLoading}
            className="w-full bg-lime-700 hover:bg-lime-800 text-white font-medium py-2.5 rounded-lg transition-colors disabled:opacity-50 mt-2"
          >
            {isLoading
              ? "Zapisywanie..."
              : isRecruiter
              ? "Zakończ i przejdź do panelu"
              : "Zakończ i przejdź do ofert"}
          </button>
        </form>
      )}
    </div>
  );
}