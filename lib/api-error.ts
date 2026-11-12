export async function readApiError(response: Response, fallback: string): Promise<Error> {
  const text = await response.text().catch(() => "");
  if (!text) return new Error(fallback);

  try {
    const parsed = JSON.parse(text);
    if (typeof parsed === "string") return new Error(parsed);

    if (parsed?.errors && typeof parsed.errors === "object") {
      const messages = Object.values(parsed.errors as Record<string, string[]>).flat();
      if (messages.length > 0) return new Error(messages.join(" "));
    }

    if (typeof parsed?.detail === "string") return new Error(parsed.detail);
    if (typeof parsed?.title === "string") return new Error(parsed.title);
  } catch {
    return new Error(text);
  }

  return new Error(fallback);
}
