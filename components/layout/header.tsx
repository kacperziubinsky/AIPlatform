"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { getTokenInfo } from "@/lib/auth-token";

export default function Header() {
  const router = useRouter();
  const [isAuthenticated, setIsAuthenticated] = useState<boolean>(false);
  const [isRecruiter, setIsRecruiter] = useState<boolean>(false);

  useEffect(() => {
    const checkAuth = () => {
      const token = localStorage.getItem("accessToken");
      setIsAuthenticated(!!token);
      setIsRecruiter(getTokenInfo().role?.toLowerCase() === "recruiter");
    };

    checkAuth();

    window.addEventListener("storage", checkAuth);
    return () => window.removeEventListener("storage", checkAuth);
  }, []);

  const handleLogout = () => {
    localStorage.removeItem("accessToken");
    setIsAuthenticated(false);
    setIsRecruiter(false);
    router.push("/login");
    router.refresh();
  };

  return (
    <header className="bg-lime-700 text-white flex justify-between items-stretch shadow-md">
      <div className="flex items-center p-4">
        <Link href="/offers" className="text-xl font-bold hover:opacity-90 transition-opacity">
          AI Jobs Portal
        </Link>
      </div>

      <div className="flex items-center">
        <ul className="flex space-x-6 p-4 text-sm font-medium">
          <li>
            <Link href="/offers" className="hover:underline transition-all">
              Oferty
            </Link>
          </li>
          <li>
            <Link href="/about" className="hover:underline transition-all">
              O nas
            </Link>
          </li>
          <li>
            <Link href="/contact" className="hover:underline transition-all">
              Kontakt
            </Link>
          </li>
          {isAuthenticated && !isRecruiter && (
            <li>
              <Link href="/profile/applications" className="hover:underline transition-all">
                Aplikacje
              </Link>
            </li>
          )}
          {isAuthenticated && isRecruiter && (
            <li>
              <Link href="/recruitment" className="hover:underline transition-all">
                Rekrutacja
              </Link>
            </li>
          )}
        </ul>

        <div className="flex items-center space-x-3 border-l border-lime-600/60 px-4 h-full text-sm">
          {isAuthenticated ? (
            <>
              <Link
                href="/profile"
                className="hover:bg-lime-800 text-white py-2 px-3 rounded-md font-medium transition-colors border border-lime-600/50"
              >
                Mój profil
              </Link>
              <button
                onClick={handleLogout}
                className="bg-white text-lime-900 hover:bg-red-50 hover:text-red-600 py-2 px-3 rounded-md font-semibold transition-colors shadow-sm"
              >
                Wyloguj
              </button>
            </>
          ) : (
            <>
              <Link
                href="/login"
                className="bg-white text-lime-900 hover:bg-lime-100 py-2 px-4 rounded-md font-semibold transition-colors shadow-sm"
              >
                Zaloguj się
              </Link>
              <Link
                href="/register"
                className="hover:underline py-2 px-2 font-medium transition-all"
              >
                Rejestracja
              </Link>
            </>
          )}
        </div>
      </div>
    </header>
  );
}