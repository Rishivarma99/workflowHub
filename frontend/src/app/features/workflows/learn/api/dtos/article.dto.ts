export interface ArticleAuthor {
  name: string;
  displayName: string | null;
}

export interface ArticleCategory {
  id: string;
  name: string;
  slug: string;
  description: string;
}

export interface ArticleCard {
  id: string;
  slug: string;
  title: string;
  summary: string;
  contentType: 'guide' | 'article';
  categoryName: string;
  categorySlug: string;
  isPinned: boolean;
  publishedAtUtc: string | null;
  author: ArticleAuthor;
}

export interface LearnHome {
  pinnedGuides: ArticleCard[];
  latestArticles: ArticleCard[];
  categories: ArticleCategory[];
}

export interface ArticleDetail {
  id: string;
  slug: string;
  title: string;
  summary: string;
  content: string;
  contentType: 'guide' | 'article';
  category: ArticleCategory;
  author: ArticleAuthor;
  isPinned: boolean;
  publishedAtUtc: string | null;
}
