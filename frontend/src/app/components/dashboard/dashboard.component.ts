import { CommonModule } from '@angular/common';
import {
  AfterViewInit,
  Component,
  ElementRef,
  OnDestroy,
  OnInit,
  ViewChild,
  inject,
  signal
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { Chart, ChartConfiguration, registerables } from 'chart.js';
import { finalize } from 'rxjs';
import { DashboardSummary, WeeklyRiskBreakdown } from '../../models/wellness.models';
import { DashboardService } from '../../services/dashboard.service';
import { AuthService } from '../../services/auth.service';

Chart.register(...registerables);

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit, AfterViewInit, OnDestroy {
  private readonly dashboardService = inject(DashboardService);
  protected readonly authService = inject(AuthService);

  @ViewChild('moodChart') moodChartRef?: ElementRef<HTMLCanvasElement>;

  protected readonly summary = signal<DashboardSummary | null>(null);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  private chart?: Chart;

  ngOnInit(): void {
    this.load();
  }

  ngAfterViewInit(): void {
    this.tryRenderChart();
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
  }

  protected refresh(): void {
    this.load();
  }

  protected behavioralRiskSummary(breakdown: WeeklyRiskBreakdown | null | undefined): string {
    if (!breakdown?.totalEntries) {
      return 'No behavioral entries recorded this week.';
    }
    if (breakdown.highRiskEntries > breakdown.lowRiskEntries) {
      return `${breakdown.highRiskEntries} of ${breakdown.totalEntries} logged days showed HIGH RISK.`;
    }
    return 'Your behavioral patterns have generally been LOW RISK this week.';
  }

  protected textualRiskSummary(breakdown: WeeklyRiskBreakdown | null | undefined): string {
    if (!breakdown?.totalEntries) {
      return 'No analyzed journal entries this week.';
    }
    if (breakdown.highRiskEntries > breakdown.lowRiskEntries) {
      return `${breakdown.highRiskEntries} of ${breakdown.totalEntries} analyzed journal entries showed HIGH RISK.`;
    }
    return 'Your journal entries have generally been LOW RISK this week.';
  }

  private load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.dashboardService
      .getSummary()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (data) => {
          this.summary.set(data);
          setTimeout(() => this.tryRenderChart());
        },
        error: () => this.error.set('We could not load your dashboard just now.')
      });
  }

  private tryRenderChart(): void {
    const canvas = this.moodChartRef?.nativeElement;
    const data = this.summary();
    if (!canvas || !data) return;

    const labels = data.weeklyMoodTrend.map((p) => p.date.slice(5));
    const values = data.weeklyMoodTrend.map((p) => (p.intensity ?? 0) || 0);

    this.chart?.destroy();

    const config: ChartConfiguration = {
      type: 'line',
      data: {
        labels,
        datasets: [
          {
            label: 'Mood intensity (1–10)',
            data: values,
            borderColor: '#87cefa',
            backgroundColor: 'rgba(135, 206, 250, 0.25)',
            tension: 0.35,
            fill: true,
            spanGaps: true
          }
        ]
      },
      options: {
        responsive: true,
        plugins: {
          legend: { display: true }
        },
        scales: {
          y: { min: 0, max: 10, ticks: { stepSize: 1 } }
        }
      }
    };

    this.chart = new Chart(canvas, config);
  }
}
