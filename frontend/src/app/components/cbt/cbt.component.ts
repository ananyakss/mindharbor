import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { CbtEntry, CbtEntryPayload } from '../../models/journal.models';
import { JournalService } from '../../services/journal.service';

@Component({
  selector: 'app-cbt',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './cbt.component.html',
  styleUrls: ['./cbt.component.css']
})
export class CbtComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly journalService = inject(JournalService);

  protected readonly distortions = [
    'All-or-nothing thinking',
    'Overgeneralization',
    'Catastrophizing',
    'Mind reading',
    'Emotional reasoning'
  ];

  protected readonly step = signal(0);
  protected readonly entries = signal<CbtEntry[]>([]);
  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly editingId = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    date: [this.todayInput(), [Validators.required]],
    situation: ['', [Validators.required]],
    emotions: this.fb.array([this.emotionGroup()]),
    automaticThought: ['', [Validators.required]],
    cognitiveDistortions: this.fb.nonNullable.control<string[]>([]),
    challengeThoughts: ['', [Validators.required]],
    reframeThoughts: ['', [Validators.required]],
    actionPlan: ['', [Validators.required]]
  });

  ngOnInit(): void {
    this.loadEntries();
  }

  protected emotions(): FormArray {
    return this.form.controls.emotions;
  }

  protected toggleDistortion(value: string): void {
    const ctrl = this.form.controls.cognitiveDistortions;
    const set = new Set(ctrl.value);
    if (set.has(value)) {
      set.delete(value);
    } else {
      set.add(value);
    }
    ctrl.setValue([...set]);
  }

  protected isDistortionSelected(value: string): boolean {
    return this.form.controls.cognitiveDistortions.value.includes(value);
  }

  protected addEmotion(): void {
    this.emotions().push(this.emotionGroup());
  }

  protected removeEmotion(index: number): void {
    if (this.emotions().length > 1) {
      this.emotions().removeAt(index);
    }
  }

  protected next(): void {
    this.step.update((s) => Math.min(6, s + 1));
  }

  protected back(): void {
    this.step.update((s) => Math.max(0, s - 1));
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = this.buildPayload();
    const entryId = this.editingId();

    this.saving.set(true);
    this.error.set(null);

    const request$ = entryId
      ? this.journalService.updateCbtEntry(entryId, payload)
      : this.journalService.createCbtEntry(payload);

    request$
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.resetForm();
          this.loadEntries();
        },
        error: () => this.error.set('Unable to save CBT entry. Please try again.')
      });
  }

  protected editEntry(entry: CbtEntry): void {
    this.editingId.set(entry.id);
    this.step.set(0);
    this.form.reset({
      date: this.toInputDate(entry.date),
      situation: entry.situation,
      automaticThought: entry.automaticThought,
      cognitiveDistortions: entry.cognitiveDistortions ?? [],
      challengeThoughts: entry.challengeThoughts || entry.evidenceFor || '',
      reframeThoughts: entry.reframeThoughts || entry.rationalResponse || '',
      actionPlan: entry.actionPlan || ''
    });
    this.emotions().clear();
    const list = entry.emotions?.length ? entry.emotions : [{ name: '', intensity: 5 }];
    for (const emotion of list) {
      this.emotions().push(this.emotionGroup(emotion.name, emotion.intensity));
    }
  }

  protected deleteEntry(id: string): void {
    if (!confirm('Delete this CBT entry?')) return;
    this.journalService.deleteCbtEntry(id).subscribe({
      next: () => this.loadEntries(),
      error: () => this.error.set('Unable to delete CBT entry.')
    });
  }

  protected cancelEdit(): void {
    this.resetForm();
  }

  private emotionGroup(name = '', intensity = 5) {
    return this.fb.nonNullable.group({
      name: [name, [Validators.required]],
      intensity: [intensity, [Validators.required, Validators.min(1), Validators.max(10)]]
    });
  }

  private resetForm(): void {
    this.editingId.set(null);
    this.step.set(0);
    this.form.reset({
      date: this.todayInput(),
      situation: '',
      automaticThought: '',
      cognitiveDistortions: [],
      challengeThoughts: '',
      reframeThoughts: '',
      actionPlan: ''
    });
    this.emotions().clear();
    this.emotions().push(this.emotionGroup());
  }

  private loadEntries(): void {
    this.loading.set(true);
    this.error.set(null);
    this.journalService
      .getCbtEntries()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (rows) => this.entries.set(rows),
        error: () => this.error.set('Unable to load CBT entries.')
      });
  }

  private buildPayload(): CbtEntryPayload {
    const value = this.form.getRawValue();
    return {
      date: new Date(value.date).toISOString(),
      situation: value.situation,
      automaticThought: value.automaticThought,
      emotions: value.emotions
        .map((e) => ({ name: e.name.trim(), intensity: Number(e.intensity) }))
        .filter((e) => e.name.length > 0),
      cognitiveDistortions: value.cognitiveDistortions ?? [],
      challengeThoughts: value.challengeThoughts,
      reframeThoughts: value.reframeThoughts,
      actionPlan: value.actionPlan,
      evidenceFor: value.challengeThoughts,
      evidenceAgainst: '',
      rationalResponse: value.reframeThoughts
    };
  }

  private todayInput(): string {
    const d = new Date();
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }

  private toInputDate(value: string): string {
    const date = new Date(value);
    return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
  }
}
