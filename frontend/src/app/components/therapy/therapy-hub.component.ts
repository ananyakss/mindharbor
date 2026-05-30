import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-therapy-hub',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './therapy-hub.component.html',
  styleUrls: ['./therapy-hub.component.css']
})
export class TherapyHubComponent {
  protected readonly modules = [
    { path: '/therapy/cbt', title: 'CBT practice', emoji: '🧩', blurb: 'Situation → thoughts → gentle reframes.' },
    { path: '/therapy/erp', title: 'ERP log', emoji: '🌊', blurb: 'Exposure with compassion and honesty.' },
    { path: '/therapy/self-validation', title: 'Self-validation', emoji: '🤗', blurb: 'Validate, separate, reframe with care.' },
    { path: '/therapy/self-confidence', title: 'Self-confidence', emoji: '💪', blurb: 'Evidence, doubt, and your next step.' },
    { path: '/therapy/self-worth', title: 'Self-worth', emoji: '💎', blurb: 'What defines you — and what never could.' },
    { path: '/therapy/schema-abandonment', title: 'Abandonment / deprivation', emoji: '🫂', blurb: 'Needs, expectations, reality.' },
    { path: '/therapy/schema-failure', title: 'Low self-worth / failure', emoji: '🌱', blurb: 'Beliefs vs evidence.' },
    { path: '/therapy/schema-people', title: 'People-pleasing', emoji: '🕊️', blurb: 'Needs, fears, boundaries.' },
    { path: '/therapy/schema-suppression', title: 'Emotional suppression', emoji: '🫧', blurb: 'What you avoid — and why.' }
  ];
}
