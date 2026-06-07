import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Component({
  selector: 'wh-markdown',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<div class="markdown-body" [innerHTML]="html()"></div>`,
  styleUrl: './wh-markdown.component.scss'
})
export class WhMarkdownComponent {
  readonly content = input.required<string>();

  private readonly sanitizer = inject(DomSanitizer);

  readonly html = computed((): SafeHtml =>
    this.sanitizer.bypassSecurityTrustHtml(renderMarkdown(this.content()))
  );
}

function renderMarkdown(source: string): string {
  const lines = source.replace(/\r\n/g, '\n').split('\n');
  const html: string[] = [];
  let inList = false;
  let inTable = false;
  let tableRows: string[][] = [];

  const flushList = () => {
    if (inList) {
      html.push('</ul>');
      inList = false;
    }
  };

  const flushTable = () => {
    if (!inTable || tableRows.length === 0) {
      inTable = false;
      tableRows = [];
      return;
    }

    html.push('<table><thead><tr>');
    tableRows[0].forEach((cell) => html.push(`<th>${inlineFormat(cell.trim())}</th>`));
    html.push('</tr></thead><tbody>');
    tableRows.slice(1).forEach((row) => {
      html.push('<tr>');
      row.forEach((cell) => html.push(`<td>${inlineFormat(cell.trim())}</td>`));
      html.push('</tr>');
    });
    html.push('</tbody></table>');
    inTable = false;
    tableRows = [];
  };

  for (const rawLine of lines) {
    const line = rawLine.trimEnd();

    if (line.startsWith('|') && line.endsWith('|')) {
      flushList();
      const cells = line.slice(1, -1).split('|');
      if (cells.every((cell) => /^[\s-:]+$/.test(cell))) {
        continue;
      }
      inTable = true;
      tableRows.push(cells);
      continue;
    }

    flushTable();

    if (line.startsWith('### ')) {
      flushList();
      html.push(`<h3>${inlineFormat(line.slice(4))}</h3>`);
      continue;
    }

    if (line.startsWith('## ')) {
      flushList();
      html.push(`<h2>${inlineFormat(line.slice(3))}</h2>`);
      continue;
    }

    if (line.startsWith('# ')) {
      flushList();
      html.push(`<h1>${inlineFormat(line.slice(2))}</h1>`);
      continue;
    }

    if (line.startsWith('- ')) {
      if (!inList) {
        html.push('<ul>');
        inList = true;
      }
      html.push(`<li>${inlineFormat(line.slice(2))}</li>`);
      continue;
    }

    flushList();

    if (line.length === 0) {
      continue;
    }

    html.push(`<p>${inlineFormat(line)}</p>`);
  }

  flushList();
  flushTable();
  return html.join('');
}

function inlineFormat(text: string): string {
  return escapeHtml(text)
    .replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>')
    .replace(/`([^`]+)`/g, '<code>$1</code>');
}

function escapeHtml(text: string): string {
  return text
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}
