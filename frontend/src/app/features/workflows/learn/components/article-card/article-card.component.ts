import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { WhIconComponent } from '../../../../../shared/ui/icon/wh-icon.component';
import { ArticleCard } from '../../api/dtos/article.dto';

@Component({
  selector: 'wh-article-card',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WhIconComponent],
  templateUrl: './article-card.component.html',
  styleUrl: './article-card.component.scss'
})
export class ArticleCardComponent {
  readonly article = input.required<ArticleCard>();
  readonly compact = input(false);
  readonly open = output<string>();

  typeLabel(type: ArticleCard['contentType']): string {
    return type === 'guide' ? 'Guide' : 'Article';
  }
}
