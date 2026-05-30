import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { MoodEntry, MoodType } from '../../models/wellness.models';
import { MoodService } from '../../services/mood.service';

@Component({
  selector: 'app-mood-tracker',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './mood-tracker.component.html',
  styleUrls: ['./mood-tracker.component.css']
})
export class MoodTrackerComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly moodService = inject(MoodService);

  protected readonly options: { emoji: string; label: string; value: MoodType }[] = [
    { emoji: '😊', label: 'Joy', value: 'joy' },
    { emoji: '😔', label: 'Sadness', value: 'sadness' },
    { emoji: '😡', label: 'Anger', value: 'anger' },
    { emoji: '😨', label: 'Fear', value: 'fear' },
    { emoji: '😰', label: 'Anxiety', value: 'nervousness' },
    { emoji: '🤢', label: 'Disgust', value: 'disgust' },
    { emoji: '😲', label: 'Surprise', value: 'surprise' },
    { emoji: '😐', label: 'Neutral', value: 'neutral' },
    { emoji: '😕', label: 'Confusion', value: 'confusion' },
    { emoji: '😤', label: 'Frustration', value: 'annoyance' },
    { emoji: '😞', label: 'Shame', value: 'remorse' }
  ];

  protected readonly entries = signal<MoodEntry[]>([]);
  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly editingId = signal<string | null>(null);

  protected form = this.fb.nonNullable.group({
    recordedAt: [this.todayInput(), [Validators.required]],
    mood: ['neutral' as MoodType, [Validators.required]],
    moodIntensity: [5, [Validators.required, Validators.min(1), Validators.max(10)]]
  });

  ngOnInit(): void {
    this.load();
  }

  protected selectMood(value: MoodType): void {
    this.form.controls.mood.setValue(value);
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();
    const payload = {
      recordedAt: new Date(v.recordedAt).toISOString(),
      mood: v.mood,
      moodIntensity: Number(v.moodIntensity)
    };

    this.saving.set(true);
    this.error.set(null);

    const id = this.editingId();
    const req$ = id ? this.moodService.update(id, payload) : this.moodService.create(payload);

    req$
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.reset();
          this.load();
        },
        error: () => this.error.set('Could not save your mood entry. Please try again.')
      });
  }

  protected edit(entry: MoodEntry): void {
    this.editingId.set(entry.id);
    this.form.reset({
      recordedAt: this.toInput(entry.recordedAt),
      mood: entry.mood,
      moodIntensity: entry.moodIntensity
    });
  }

  protected delete(id: string): void {
    if (!confirm('Delete this mood log?')) return;
    this.moodService.delete(id).subscribe({
      next: () => this.load(),
      error: () => this.error.set('Could not delete entry.')
    });
  }

  protected cancelEdit(): void {
    this.reset();
  }

  private load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.moodService
      .getAll()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (rows) => this.entries.set(rows),
        error: () => this.error.set('Unable to load moods.')
      });
  }

  private reset(): void {
    this.editingId.set(null);
    this.form.reset({
      recordedAt: this.todayInput(),
      mood: 'neutral',
      moodIntensity: 5
    });
  }

  private todayInput(): string {
    const d = new Date();
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }

  private toInput(iso: string): string {
    const d = new Date(iso);
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }
}
