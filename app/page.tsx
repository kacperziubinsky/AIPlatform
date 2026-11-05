import SearchBar from "@/components/ui/search-bar";
import Image from "next/image";

export default function Home() {
  return (
    <main className="flex min-h-screen flex-col items-center p-24">
      <h1 className="text-4xl font-bold">AI Jobs Portal</h1>
      <p className="text-lg">Find your next AI job with our curated opportunities.</p>
      <SearchBar />
    </main>
  );
}
