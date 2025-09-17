import { Capacitor } from "@capacitor/core";

const API_BASE_KEY = "kapitelshelf.mobile.api-base-url";

export const IsMobileApp = (): boolean => Capacitor.getPlatform() !== "web";

export const SetMobileApiBaseUrl = (url: string): void => {
  localStorage.setItem(API_BASE_KEY, url);
};

export const ClearMobileApiBaseUrl = (): void => {
  localStorage.removeItem(API_BASE_KEY);
};

export const GetMobileApiBaseUrl = (): string | null =>
  localStorage.getItem(API_BASE_KEY);

export const IsMobileApiBaseUrlConfigured = (): boolean =>
  GetMobileApiBaseUrl() !== null;

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
