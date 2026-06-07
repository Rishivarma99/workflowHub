import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, map, of } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { ApiResponse } from '../../../../shared/types/api';
import { unwrapApiResponse } from '../../../../shared/utils/api/unwrap-api-response';
import {
  NameAvailabilityResult,
  PublishWorkflowRequest,
  PublishWorkflowResponse,
  RepoValidationResult
} from '../models/publish-analysis.model';
import { GITHUB_REPO_PATTERN } from '../constants/publish.constants';

@Injectable({ providedIn: 'root' })
export class PublishApiService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiBaseUrl;

  validateRepository(repositoryUrl: string): Observable<RepoValidationResult | null> {
    const trimmed = repositoryUrl.trim();
    if (!GITHUB_REPO_PATTERN.test(trimmed)) {
      return of(null);
    }

    return this.http
      .post<ApiResponse<RepoValidationResult>>(`${this.base}/workflows/validate-repo`, {
        repositoryUrl: trimmed
      })
      .pipe(
        map(unwrapApiResponse),
        catchError(() => of(null))
      );
  }

  checkWorkflowName(name: string): Observable<NameAvailabilityResult> {
    const trimmed = name.trim();
    if (!trimmed) {
      return of({ available: false });
    }

    return this.http
      .get<ApiResponse<NameAvailabilityResult>>(`${this.base}/workflows/check-name`, {
        params: { name: trimmed }
      })
      .pipe(
        map(unwrapApiResponse),
        catchError(() =>
          of({
            available: true,
            suggestion: undefined
          })
        )
      );
  }

  publishWorkflow(body: PublishWorkflowRequest): Observable<PublishWorkflowResponse> {
    return this.http
      .post<ApiResponse<PublishWorkflowResponse>>(`${this.base}/workflows`, body)
      .pipe(map(unwrapApiResponse));
  }
}
