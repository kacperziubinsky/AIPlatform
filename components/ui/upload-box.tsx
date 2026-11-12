"use client";

import { useState } from "react";
import { uploadUserCV } from "@/services/cv-service";
import { UserCV } from "@/types/cv";

interface UploadBoxProps {
  onSuccess?: (cv: UserCV | null) => void;
}

export default function UploadBox({ onSuccess }: UploadBoxProps) {
  const [file, setFile] = useState<File | null>(null);
  const [isUploading, setIsUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isDragging, setIsDragging] = useState(false);

  const handleUpload = async (selectedFile: File) => {
    setFile(selectedFile);
    setIsUploading(true);
    setError(null);

    try {
      const uploadedCv = await uploadUserCV(selectedFile);
      if (onSuccess) {
        onSuccess(uploadedCv);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Wystąpił błąd podczas wysyłania pliku.");
    } finally {
      setIsUploading(false);
    }
  };

  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFile = event.target.files?.[0];
    if (selectedFile) {
      handleUpload(selectedFile);
    }
  };

  const handleDragOver = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    setIsDragging(true);
  };

  const handleDragLeave = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    setIsDragging(false);
  };

  const handleDrop = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    setIsDragging(false);
    const droppedFile = e.dataTransfer.files?.[0];
    if (droppedFile) {
      handleUpload(droppedFile);
    }
  };

  return (
    <div className="w-full">
      <div
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
        onDrop={handleDrop}
        className={`relative flex flex-col items-center justify-center w-full h-36 border-2 border-dashed rounded-xl p-4 text-center transition-all duration-200 ${
          isDragging
            ? "border-lime-600 bg-lime-50"
            : error
            ? "border-red-400 bg-red-50/50"
            : "border-gray-300 hover:border-lime-600 bg-white hover:bg-lime-50/30"
        }`}
      >
        <input
          type="file"
          accept=".pdf,.docx"
          onChange={handleFileChange}
          disabled={isUploading}
          className="absolute inset-0 w-full h-full opacity-0 cursor-pointer disabled:cursor-not-allowed"
        />

        {isUploading ? (
          <div className="flex flex-col items-center space-y-2 text-lime-700">
            <svg className="w-8 h-8 animate-spin" fill="none" viewBox="0 0 24 24">
              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
            </svg>
            <span className="text-sm font-medium">Wysyłanie CV na serwer...</span>
          </div>
        ) : (
          <div className="flex flex-col items-center justify-center">
            <svg
              className={`w-8 h-8 mb-2 transition-colors ${
                isDragging ? "text-lime-700" : "text-gray-400"
              }`}
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth="2"
                d="M4 16v1a2 2 0 002 2h12a2 2 0 002-2v-1M4 12V8a2 2 0 012-2h3.586a1 1 0 01.707.293l1.414 1.414A1 1 0 0013.414 8H20a2 2 0 012 2v4M16 12l-4-4m0 0L8 12m4-4v12"
              />
            </svg>
            <p className="text-gray-700 text-sm font-medium">
              Przeciągnij i upuść plik CV lub <span className="text-lime-700 underline">kliknij, aby wybrać</span>
            </p>
            <p className="text-gray-400 text-xs mt-1">Obsługiwane formaty: PDF, DOC, DOCX</p>
          </div>
        )}
      </div>

      {file && !error && !isUploading && (
        <p className="text-xs text-lime-700 mt-2 font-medium">
          ✓ Przesłano plik: {file.name}
        </p>
      )}

      {error && (
        <p className="text-xs text-red-500 mt-2 font-medium">
          ✕ {error}
        </p>
      )}
    </div>
  );
}