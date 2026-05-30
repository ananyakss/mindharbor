import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { BehaviorEntry, BehaviorEntryPayload } from '../../models/wellness.models';
import { BehaviorService } from '../../services/behavior.service';

@Component({
  selector: 'app-behavior-tracker',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './behavior-tracker.component.html',
  styleUrls: ['./behavior-tracker.component.css']
})
export class BehaviorTrackerComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly behaviorService = inject(BehaviorService);

  protected readonly entries = signal<BehaviorEntry[]>([]);
  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly editingId = signal<string | null>(null);

  protected form = this.fb.nonNullable.group({
    day: [this.todayInput(), [Validators.required]],
    sleep: [7, [Validators.required, Validators.min(1), Validators.max(10)]],
    water: [8, [Validators.required, Validators.min(0), Validators.max(8)]],
    breakfast: [true],
    lunch: [true],
    dinner: [true],
    diet: [2, [Validators.required, Validators.min(0), Validators.max(5)]],
    exerciseHours: [1, [Validators.required, Validators.min(0)]],
    steps: [5, [Validators.required, Validators.min(0), Validators.max(10)]],
    social: [3, [Validators.required, Validators.min(0), Validators.max(10)]],
    hobbies: [2, [Validators.required, Validators.min(0), Validators.max(10)]],
    stress: [3, [Validators.required, Validators.min(1), Validators.max(5)]]
  });

  ngOnInit(): void {
    this.load();
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();
    const payload: BehaviorEntryPayload = {
      day: new Date(v.day).toISOString(),
      sleep: Number(v.sleep),
      water: Number(v.water),
      meals: {
        breakfast: Boolean(v.breakfast),
        lunch: Boolean(v.lunch),
        dinner: Boolean(v.dinner)
      },
      exerciseHours: Number(v.exerciseHours),
      steps: Number(v.steps),
      diet: Number(v.diet),
      social: Number(v.social),
      hobbies: Number(v.hobbies),
      stress: Number(v.stress)
    };
    console.log('Submitting behavioral data:', payload);

    this.saving.set(true);
    this.error.set(null);
    const id = this.editingId();
    const req$ = id ? this.behaviorService.update(id, payload) : this.behaviorService.create(payload);

    req$
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.reset();
          this.load();
        },
        error: (err) => {
          console.error('Save failed:', err);
          this.error.set(err?.error?.message ?? 'Could not save behavioral log. Please try again.');
          alert('Could not save behavioral log. Please try again.');
        }
      });
  }

  protected edit(entry: BehaviorEntry): void {
    this.editingId.set(entry.id);
    this.form.reset({
      day: this.toInput(entry.day),
      sleep: entry.sleep,
      water: entry.water,
      breakfast: entry.meals?.breakfast ?? false,
      lunch: entry.meals?.lunch ?? false,
      dinner: entry.meals?.dinner ?? false,
      exerciseHours: entry.exerciseHours,
      steps: entry.steps,
      diet: entry.diet,
      social: entry.social,
      hobbies: entry.hobbies,
      stress: entry.stress
    });
  }

  protected delete(id: string): void {
    if (!confirm('Delete this behavior log?')) return;
    this.behaviorService.delete(id).subscribe({
      next: () => this.load(),
      error: () => this.error.set('Could not delete.')
    });
  }

  protected cancelEdit(): void {
    this.reset();
  }

  private load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.behaviorService
      .getAll()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (rows) => this.entries.set(rows),
        error: () => this.error.set('Unable to load behavior data.')
      });
  }

  private reset(): void {
    this.editingId.set(null);
    this.form.reset({
      day: this.todayInput(),
      sleep: 7,
      water: 8,
      breakfast: true,
      lunch: true,
      dinner: true,
      exerciseHours: 1,
      steps: 5,
      diet: 2,
      social: 3,
      hobbies: 2,
      stress: 3
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
