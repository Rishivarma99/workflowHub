import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { ApiResponse } from '../../../../shared/types/api';
import { unwrapApiResponse } from '../../../../shared/utils/api/unwrap-api-response';
import { RecordWorkflowDownloadResult, StarActionResult } from '../models/engagement.model';

@Injectable({ providedIn: 'root' })
export class WorkflowEngagementApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/workflows`;

  star(workflowId: string): Observable<StarActionResult> {
    return this.http
      .post<ApiResponse<StarActionResult>>(`${this.base}/${workflowId}/star`, {})
      .pipe(map(unwrapApiResponse));
  }

  unstar(workflowId: string): Observable<StarActionResult> {
    return this.http
      .delete<ApiResponse<StarActionResult>>(`${this.base}/${workflowId}/star`)
      .pipe(map(unwrapApiResponse));
  }

  recordDownload(workflowId: string): Observable<RecordWorkflowDownloadResult> {
    return this.http
      .post<ApiResponse<RecordWorkflowDownloadResult>>(`${this.base}/${workflowId}/download`, {})
      .pipe(map(unwrapApiResponse));
  }
}
