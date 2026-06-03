/**
 * HttpClient often exposes absolute URLs in interceptors (e.g. http://localhost:4200/api/me).
 * Match both relative /api/... and absolute same-origin API paths.
 */
export function isApiRequest(url: string, apiBaseUrl: string): boolean {
  if (url.startsWith(apiBaseUrl)) {
    return true;
  }

  try {
    if (url.startsWith('http://') || url.startsWith('https://')) {
      const pathname = new URL(url).pathname;
      return pathname === apiBaseUrl || pathname.startsWith(`${apiBaseUrl}/`);
    }
  } catch {
    return false;
  }

  return false;
}
