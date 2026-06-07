import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { ApiResponse } from '../../../../shared/types/api';
import { unwrapApiResponse } from '../../../../shared/utils/api/unwrap-api-response';
import { StarActionResult } from '../../shared/models/engagement.model';

@Injectable({ providedIn: 'root' })
export class AgentAssetEngagementApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/agent-assets`;

  star(agentAssetId: string): Observable<StarActionResult> {
    return this.http
      .post<ApiResponse<StarActionResult>>(`${this.base}/${agentAssetId}/star`, {})
      .pipe(map(unwrapApiResponse));
  }

  unstar(agentAssetId: string): Observable<StarActionResult> {
    return this.http
      .delete<ApiResponse<StarActionResult>>(`${this.base}/${agentAssetId}/star`)
      .pipe(map(unwrapApiResponse));
  }
}
