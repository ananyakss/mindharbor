export type MoodType =
  | 'joy'
  | 'sadness'
  | 'anger'
  | 'fear'
  | 'nervousness'
  | 'disgust'
  | 'surprise'
  | 'neutral'
  | 'confusion'
  | 'annoyance'
  | 'remorse';

export interface MoodEntry {
  id: string;
  userId: string;
  recordedAt: string;
  mood: MoodType;
  moodIntensity: number;
  createdAt: string;
  updatedAt: string;
}

export type MoodEntryPayload = Pick<MoodEntry, 'recordedAt' | 'mood' | 'moodIntensity'>;

export interface BehaviorEntry {
  id: string;
  userId: string;
  day: string;
  sleep: number;
  water: number;
  meals: {
    breakfast: boolean;
    lunch: boolean;
    dinner: boolean;
  };
  exerciseHours: number;
  steps: number;
  diet: number;
  social: number;
  hobbies: number;
  stress: number;
  createdAt: string;
  updatedAt: string;
}

export interface BehaviorEntryPayload {
  day: string;
  sleep: number;
  water: number;
  meals: {
    breakfast: boolean;
    lunch: boolean;
    dinner: boolean;
  };
  exerciseHours: number;
  steps: number;
  diet: number;
  social: number;
  hobbies: number;
  stress: number;
}

export interface JournalEntryModel {
  id: string;
  userId: string;
  entryText: string;
  allowAnalysisConsent: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface JournalEntryPayload {
  entryText: string;
  allowAnalysisConsent: boolean;
}

export interface MoodTrendPoint {
  date: string;
  mood: string | null;
  intensity: number | null;
}

export interface BehavioralSummary {
  sleep: number;
  water: number;
  meals: {
    breakfast: boolean;
    lunch: boolean;
    dinner: boolean;
  };
  exerciseHours: number;
  steps: number;
  diet: number;
  social: number;
  hobbies: number;
  stress: number;
  day: string;
}

export interface TextPredictionPayload {
  journalText: string;
  mode?: 'comparison' | 'production';
}

export interface BehaviorPredictionResult {
  riskLevel: string;
  message?: string | null;
}

export interface TextPredictionResult {
  sentiment: string;
}

export interface DashboardSummary {
  loginStreak: number;
  gamificationPoints: number;
  weeklyMoodTrend: MoodTrendPoint[];
  behavioralSummary: BehavioralSummary | null;
  recentJournalEntries: JournalEntryModel[];
}

/** Saved worksheet row (CBT/ERP use their own models; therapy worksheets share this shape for metadata). */
export interface TherapyWorksheetSavedEntry {
  id: string;
  userId: string;
  createdAt: string;
  updatedAt: string;
}
