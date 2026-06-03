import { ApiResponse } from '../../types/api';

export class ApiBusinessError extends Error {
  constructor(
    readonly code: string,
    message: string
  ) {
    super(message);
    this.name = 'ApiBusinessError';
  }
}

export function unwrapApiResponse<T>(response: ApiResponse<T>): T {
  if (!response.success || response.data === undefined || response.data === null) {
    throw new ApiBusinessError(
      response.error?.code ?? 'UNKNOWN_ERROR',
      response.error?.message ?? 'Request failed.'
    );
  }

  return response.data;
}
