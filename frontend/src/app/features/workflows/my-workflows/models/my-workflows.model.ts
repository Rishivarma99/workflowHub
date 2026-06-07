export interface MyWorkflowListItem {
  id: string;
  name: string;
  description: string;
  sourceIde: string;
  starCount: number;
  downloadCount: number;
  componentCount: number;
  updatedAtUtc: string;
}

export interface MyWorkflowsSummary {
  items: MyWorkflowListItem[];
  publishedCount: number;
  totalDownloads: number;
  totalComponentsIndexed: number;
}
