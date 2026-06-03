import { HttpErrorResponse } from '@angular/common/http';
import { ApiBusinessError } from './unwrap-api-response';

export function toUserFacingError(err: unknown): string {
  if (err instanceof ApiBusinessError) {
    return err.message;
  }
  if (err instanceof HttpErrorResponse && err.status === 0) {
    return 'You appear to be offline. Check your connection.';
  }
  if (err instanceof HttpErrorResponse && err.status >= 500) {
    return 'The server is having trouble. Try again shortly.';
  }
  if (err instanceof HttpErrorResponse) {
    return 'Something went wrong. Please try again.';
  }
  return 'Unexpected error.';
}
