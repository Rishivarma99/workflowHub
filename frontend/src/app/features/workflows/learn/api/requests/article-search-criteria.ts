export interface ArticleSearchCriteria {
  query?: string;
  page: number;
  pageSize: number;
  filters: Record<string, string | number | boolean | undefined>;
}
