export interface Emotion {
  name: string;
  intensity: number;
}

export interface CbtEntry {
  id: string;
  userId: string;
  date: string;
  situation: string;
  automaticThought: string;
  emotions: Emotion[];
  cognitiveDistortions: string[];
  challengeThoughts?: string;
  reframeThoughts?: string;
  actionPlan?: string;
  evidenceFor: string;
  evidenceAgainst: string;
  rationalResponse: string;
  createdAt: string;
  updatedAt: string;
}

export type CbtEntryPayload = Omit<CbtEntry, 'id' | 'userId' | 'createdAt' | 'updatedAt'>;

export interface ErpEntry {
  id: string;
  userId: string;
  date: string;
  trigger: string;
  anxietyLevel: number;
  exposureActivity: string;
  resistedCompulsion: boolean;
  reflection: string;
  createdAt: string;
  updatedAt: string;
}

export type ErpEntryPayload = Omit<ErpEntry, 'id' | 'userId' | 'createdAt' | 'updatedAt'>;

export interface DailyJournalEntry {
  id: string;
  userId: string;
  date: string;
  entryText: string;
  sleepHours: number;
  waterGlasses: number;
  foodNotes: string;
  mood: string;
  moodEmoji: string | null;
  gratitude: string[];
  createdAt: string;
  updatedAt: string;
}

export type DailyJournalPayload = Omit<DailyJournalEntry, 'id' | 'userId' | 'createdAt' | 'updatedAt'>;

