import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ErpEntry, ErpEntryPayload } from '../../models/journal.models';
import { JournalService } from '../../services/journal.service';

@Component({
  selector: 'app-erp',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './erp.component.html',
  styleUrls: ['./erp.component.css']
})
export class ErpComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly journalService = inject(JournalService);

  protected readonly entries = signal<ErpEntry[]>([]);
  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly editingId = signal<string | null>(null);

  protected form = this.fb.nonNullable.group({
    date: [this.todayInput(), [Validators.required]],
    trigger: ['', [Validators.required]],
    anxietyLevel: [5, [Validators.required, Validators.min(1), Validators.max(10)]],
    exposureActivity: ['', [Validators.required]],
    resistedCompulsion: [true],
    reflection: ['', [Validators.required]]
  });

  ngOnInit(): void {
    this.loadEntries();
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();
    const payload: ErpEntryPayload = {
      date: new Date(v.date).toISOString(),
      trigger: v.trigger,
      anxietyLevel: Number(v.anxietyLevel),
      exposureActivity: v.exposureActivity,
      resistedCompulsion: v.resistedCompulsion,
      reflection: v.reflection
    };

    this.saving.set(true);
    this.error.set(null);
    const id = this.editingId();
    const req$ = id ? this.journalService.updateErpEntry(id, payload) : this.journalService.createErpEntry(payload);

    req$
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.resetForm();
          this.loadEntries();
        },
        error: () => this.error.set('Unable to save ERP entry.')
      });
  }

  protected editEntry(entry: ErpEntry): void {
    this.editingId.set(entry.id);
    this.form.reset({
      date: this.toInputDate(entry.date),
      trigger: entry.trigger,
      anxietyLevel: entry.anxietyLevel,
      exposureActivity: entry.exposureActivity,
      resistedCompulsion: entry.resistedCompulsion,
      reflection: entry.reflection
    });
  }

  protected deleteEntry(id: string): void {
    if (!confirm('Delete this ERP entry?')) return;
    this.journalService.deleteErpEntry(id).subscribe({
      next: () => this.loadEntries(),
      error: () => this.error.set('Unable to delete.')
    });
  }

  protected cancelEdit(): void {
    this.resetForm();
  }

  private loadEntries(): void {
    this.loading.set(true);
    this.error.set(null);
    this.journalService
      .getErpEntries()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (rows) => this.entries.set(rows),
        error: () => this.error.set('Unable to load ERP entries.')
      });
  }

  private resetForm(): void {
    this.editingId.set(null);
    this.form.reset({
      date: this.todayInput(),
      trigger: '',
      anxietyLevel: 5,
      exposureActivity: '',
      resistedCompulsion: true,
      reflection: ''
    });
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
