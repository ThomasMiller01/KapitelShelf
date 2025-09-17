import { Capacitor } from "@capacitor/core";

const API_BASE_KEY = "kapitelshelf.mobile.api-base-url";

export const IsMobileApp = (): boolean => Capacitor.getPlatform() !== "web";

export const SetMobileApiBaseUrl = (url: string): void => {
  localStorage.setItem(API_BASE_KEY, url);
};

/**
 * Read the API base URL with sensible fallbacks.
 * Order: localStorage → Vite env (baked) → empty string.
 * @returns {string} The resolved API base URL or empty string.
 */
export const GetMobileApiBaseUrl = (): string | null =>
  localStorage.getItem(API_BASE_KEY);

export const IsMobileApiBaseUrlConfigured = (): boolean =>
  GetMobileApiBaseUrl() !== null;

/**
 * Validate a URL string (must include protocol).
 * @param input The candidate URL string.
 * @returns {boolean} True if input is a valid absolute URL.
 */
export function IsValidUrl(input: string | null): boolean {
  if (input === null) {
    return false;
  }

  try {
    // URL() throws when invalid and also enforces absolute URLs (with protocol)
    const u = new URL(input);
    return Boolean(u.protocol === "http:" || u.protocol === "https:");
  } catch {
    return false;
  }
}
