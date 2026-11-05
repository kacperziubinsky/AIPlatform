import RegisterWizard from "@/components/auth/register-wizard";
import Link from "next/link";

export default function RegisterPage() {
  return (
    <main className="min-h-screen flex flex-col items-center justify-center p-4 bg-gray-50">
      <h1 className="text-2xl font-bold text-gray-900 text-center mb-1">Dołącz jako Kandydat</h1>
      <p className="text-sm text-gray-500 text-center mb-6">
        Utwórz konto i uzupełnij profil, aby aplikować na oferty
      </p>

      <RegisterWizard />

      <p className="text-xs text-center text-gray-500 mt-6">
        Masz już konto?{" "}
        <Link href="/login" className="text-lime-700 font-semibold hover:underline">
          Zaloguj się
        </Link>
      </p>
    </main>
  );
}