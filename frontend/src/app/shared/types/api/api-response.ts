/**
 * Centralized transport contract types shared across all features.
 * Mirrors the backend api-response-contract. Feature folders consume these
 * primitives instead of redefining their own response/error/paging shapes.
 */

export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
}

export interface ApiError {
  code: string;
  message: string;
  details?: unknown;
}

export interface PagedResult<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
}
