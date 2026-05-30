import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

/**
 * Root application shell. Renders the active route into the router outlet.
 * Authenticated business chrome lives in the business layout component, not here.
 */
@Component({
  selector: 'wh-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {}
