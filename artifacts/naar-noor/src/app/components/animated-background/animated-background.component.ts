import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-animated-background',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './animated-background.component.html',
  styleUrls: ['./animated-background.component.css']
})
export class AnimatedBackgroundComponent {
  @Input() zIndex: string = '-z-50';
}
