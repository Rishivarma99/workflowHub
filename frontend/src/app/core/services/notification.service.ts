import { Injectable, inject } from '@angular/core';
import { MessageService } from 'primeng/api';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly messageService = inject(MessageService);

  success(message: string): void {
    this.messageService.add({ severity: 'success', summary: message, life: 4000, closable: true });
  }

  error(message: string): void {
    this.messageService.add({ severity: 'error', summary: message, life: 8000, closable: true });
  }

  warning(message: string): void {
    this.messageService.add({ severity: 'warn', summary: message, life: 6000, closable: true });
  }

  info(message: string): void {
    this.messageService.add({ severity: 'info', summary: message, life: 4000, closable: true });
  }
}
