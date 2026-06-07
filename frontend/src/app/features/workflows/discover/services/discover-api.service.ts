import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { ApiResponse } from '../../../../shared/types/api';
import { unwrapApiResponse } from '../../../../shared/utils/api/unwrap-api-response';
import {
  DiscoverHome,
  GenerateInstallPromptRequest,
  GenerateInstallPromptResponse,
  WorkflowBrowseCriteria,
  WorkflowDetail,
  WorkflowSearchCriteria,
  WorkflowSearchResult
} from '../models/discover.model';

@Injectable({ providedIn: 'root' })
export class DiscoverApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/workflows`;

  getHome(): Observable<DiscoverHome> {
    return this.http
      .get<ApiResponse<DiscoverHome>>(`${this.base}/home`)
      .pipe(map(unwrapApiResponse));
  }

  browse(criteria: WorkflowBrowseCriteria): Observable<WorkflowSearchResult> {
    const params: Record<string, string> = {
      page: String(criteria.page),
      pageSize: String(criteria.pageSize)
    };

    if (criteria.types?.length) {
      params['types'] = criteria.types.join(',');
    }

    return this.http
      .get<ApiResponse<WorkflowSearchResult>>(`${this.base}/browse`, { params })
      .pipe(map(unwrapApiResponse));
  }

  search(criteria: WorkflowSearchCriteria): Observable<WorkflowSearchResult> {
    return this.http
      .post<ApiResponse<WorkflowSearchResult>>(`${this.base}/search`, criteria)
      .pipe(map(unwrapApiResponse));
  }

  getDetail(workflowId: string): Observable<WorkflowDetail> {
    return this.http
      .get<ApiResponse<WorkflowDetail>>(`${this.base}/${workflowId}`)
      .pipe(map(unwrapApiResponse));
  }

  generateInstallPrompt(
    workflowId: string,
    request: GenerateInstallPromptRequest
  ): Observable<GenerateInstallPromptResponse> {
    return this.http
      .post<ApiResponse<GenerateInstallPromptResponse>>(
        `${this.base}/${workflowId}/install-prompt`,
        request
      )
      .pipe(map(unwrapApiResponse));
  }
}
