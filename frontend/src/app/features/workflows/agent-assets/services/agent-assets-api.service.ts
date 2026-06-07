import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { ApiResponse } from '../../../../shared/types/api';
import { unwrapApiResponse } from '../../../../shared/utils/api/unwrap-api-response';
import {
  AgentAssetBrowseCriteria,
  AgentAssetsHome,
  AgentAssetSearchCriteria,
  AgentAssetSearchResult
} from '../models/agent-asset.model';

@Injectable({ providedIn: 'root' })
export class AgentAssetsApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/agent-assets`;

  getHome(): Observable<AgentAssetsHome> {
    return this.http
      .get<ApiResponse<AgentAssetsHome>>(`${this.base}/home`)
      .pipe(map(unwrapApiResponse));
  }

  browse(criteria: AgentAssetBrowseCriteria): Observable<AgentAssetSearchResult> {
    const params: Record<string, string> = {
      page: String(criteria.page),
      pageSize: String(criteria.pageSize)
    };

    if (criteria.types?.length) {
      params['types'] = criteria.types.join(',');
    }

    return this.http
      .get<ApiResponse<AgentAssetSearchResult>>(`${this.base}/browse`, { params })
      .pipe(map(unwrapApiResponse));
  }

  search(criteria: AgentAssetSearchCriteria): Observable<AgentAssetSearchResult> {
    return this.http
      .post<ApiResponse<AgentAssetSearchResult>>(`${this.base}/search`, criteria)
      .pipe(map(unwrapApiResponse));
  }
}
