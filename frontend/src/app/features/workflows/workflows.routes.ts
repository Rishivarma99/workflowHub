import { Routes } from '@angular/router';

export const WORKFLOWS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./layout/workflows-layout.component').then((m) => m.WorkflowsLayoutComponent),
    children: [
      { path: '', redirectTo: 'discover', pathMatch: 'full' },
      {
        path: 'discover',
        loadComponent: () =>
          import('./discover/pages/discover-page/discover-page.component').then((m) => m.DiscoverPageComponent)
      },
      {
        path: 'agent-assets',
        loadComponent: () =>
          import('./agent-assets/pages/agent-assets-page/agent-assets-page.component').then(
            (m) => m.AgentAssetsPageComponent
          )
      },
      {
        path: 'learn',
        loadComponent: () =>
          import('./learn/pages/learn-page/learn-page.component').then((m) => m.LearnPageComponent)
      },
      {
        path: 'learn/:slug',
        loadComponent: () =>
          import('./learn/pages/article-detail-page/article-detail-page.component').then(
            (m) => m.ArticleDetailPageComponent
          )
      },
      {
        path: 'browse',
        loadComponent: () =>
          import('./discover/pages/browse-all-page/browse-all-page.component').then((m) => m.BrowseAllPageComponent)
      },
      {
        path: 'mine',
        loadComponent: () =>
          import('./my-workflows/pages/my-workflows-page/my-workflows-page.component').then(
            (m) => m.MyWorkflowsPageComponent
          )
      },
      {
        path: 'create',
        loadComponent: () =>
          import('./publish/pages/publish-wizard-page/publish-wizard-page.component').then((m) => m.PublishWizardPageComponent)
      },
      {
        path: 'settings',
        loadChildren: () =>
          import('./settings/settings.routes').then((m) => m.SETTINGS_ROUTES)
      },
      {
        path: ':workflowId',
        loadComponent: () =>
          import('./discover/pages/workflow-detail-page/workflow-detail-page.component').then(
            (m) => m.WorkflowDetailPageComponent
          )
      }
    ]
  }
];
