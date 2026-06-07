import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { ApiResponse, PagedResult } from '../../../../shared/types/api';
import { unwrapApiResponse } from '../../../../shared/utils/api/unwrap-api-response';
import { ArticleDetail, ArticleCard, LearnHome } from './dtos/article.dto';
import { ArticleSearchCriteria } from './requests/article-search-criteria';

@Injectable({ providedIn: 'root' })
export class LearnApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/articles`;

  getHome(): Observable<LearnHome> {
    return this.http
      .get<ApiResponse<LearnHome>>(`${this.base}/home`)
      .pipe(map(unwrapApiResponse));
  }

  search(criteria: ArticleSearchCriteria): Observable<PagedResult<ArticleCard>> {
    return this.http
      .post<ApiResponse<PagedResult<ArticleCard>>>(`${this.base}/search`, criteria)
      .pipe(map(unwrapApiResponse));
  }

  getBySlug(slug: string): Observable<ArticleDetail> {
    return this.http
      .get<ApiResponse<ArticleDetail>>(`${this.base}/${slug}`)
      .pipe(map(unwrapApiResponse));
  }
}
