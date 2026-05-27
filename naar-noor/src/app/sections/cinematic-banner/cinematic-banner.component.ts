import { Component, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RevealDirective } from '../../directives/scroll-reveal.directive';

@Component({
  selector: 'app-cinematic-banner',
  standalone: true,
  imports: [CommonModule, RevealDirective],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './cinematic-banner.component.html',
  styleUrls: ['./cinematic-banner.component.css']
})
export class CinematicBannerComponent {}
