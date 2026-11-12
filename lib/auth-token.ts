const NAME_ID_CLAIM = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
const ROLE_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

export interface TokenInfo {
  userId: string | null;
  role: string | null;
}

/** Reads user id and role from the stored JWT payload (no signature check — UI use only). */
export function getTokenInfo(): TokenInfo {
  if (typeof window === "undefined") return { userId: null, role: null };
  const token = localStorage.getItem("accessToken");
  if (!token) return { userId: null, role: null };

  try {
    const base64 = token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/");
    const json = decodeURIComponent(
      atob(base64)
        .split("")
        .map((c) => "%" + c.charCodeAt(0).toString(16).padStart(2, "0"))
        .join("")
    );
    const payload = JSON.parse(json) as Record<string, unknown>;
    const pick = (...keys: string[]) => {
      for (const k of keys) {
        const v = payload[k];
        if (typeof v === "string") return v;
        if (Array.isArray(v) && typeof v[0] === "string") return v[0];
      }
      return null;
    };
    return {
      userId: pick("sub", NAME_ID_CLAIM, "userId", "id"),
      role: pick("role", ROLE_CLAIM, "roles"),
    };
  } catch {
    return { userId: null, role: null };
  }
}
