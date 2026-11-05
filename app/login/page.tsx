import LoginForm from "@/components/auth/login-form";
import Link from "next/link";
import { Suspense } from "react";

export default function LoginPage() {
  return (
    <main className="min-h-screen flex flex-col items-center justify-center p-4 bg-gray-50">
      <div className="w-full max-w-md bg-white p-8 border border-gray-200 rounded-2xl shadow-sm">
        <h1 className="text-2xl font-bold text-gray-900 text-center mb-1">Zaloguj się</h1>
        <p className="text-sm text-gray-500 text-center mb-6">
          Wpisz swoje dane, aby uzyskać dostęp do konta
        </p>

        <Suspense fallback={<div>Ładowanie formularza...</div>}>
          <LoginForm />
        </Suspense>

        <p className="text-xs text-center text-gray-500 mt-6">
          Nie masz jeszcze konta?{" "}
          <Link href="/register" className="text-lime-700 font-semibold hover:underline">
            Zarejestruj się
          </Link>
        </p>
      </div>
    </main>
  );
}