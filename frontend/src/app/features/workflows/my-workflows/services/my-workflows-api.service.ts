import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { ApiResponse } from '../../../../shared/types/api';
import { unwrapApiResponse } from '../../../../shared/utils/api/unwrap-api-response';
import { MyWorkflowsSummary } from '../models/my-workflows.model';

@Injectable({ providedIn: 'root' })
export class MyWorkflowsApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/workflows`;

  getMine(): Observable<MyWorkflowsSummary> {
    return this.http
      .get<ApiResponse<MyWorkflowsSummary>>(`${this.base}/mine`)
      .pipe(map(unwrapApiResponse));
  }

  delete(workflowId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${workflowId}`);
  }
}
