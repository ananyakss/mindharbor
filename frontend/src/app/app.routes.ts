import { Routes } from '@angular/router';
import { AppShellComponent } from './layout/app-shell/app-shell.component';
import { TherapyLayoutComponent } from './layout/therapy-layout/therapy-layout.component';
import { AuthComponent } from './components/auth/auth.component';
import { BehaviorTrackerComponent } from './components/behavior/behavior-tracker.component';
import { CbtComponent } from './components/cbt/cbt.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { ErpComponent } from './components/erp/erp.component';
import { HomeComponent } from './components/home/home.component';
import { JournalPageComponent } from './components/journal/journal-page.component';
import { MoodTrackerComponent } from './components/mood/mood-tracker.component';
import { SchemaAbandonmentComponent } from './components/therapy/schema-abandonment.component';
import { SchemaFailureComponent } from './components/therapy/schema-failure.component';
import { SchemaPeopleComponent } from './components/therapy/schema-people.component';
import { SchemaSuppressionComponent } from './components/therapy/schema-suppression.component';
import { SelfConfidenceComponent } from './components/therapy/self-confidence.component';
import { SelfValidationComponent } from './components/therapy/self-validation.component';
import { SelfWorthComponent } from './components/therapy/self-worth.component';
import { TherapyHubComponent } from './components/therapy/therapy-hub.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'auth', component: AuthComponent },
  {
    path: '',
    component: AppShellComponent,
    canActivate: [authGuard],
    children: [
      { path: 'dashboard', component: DashboardComponent },
      { path: 'mood', component: MoodTrackerComponent },
      { path: 'behavior', component: BehaviorTrackerComponent },
      { path: 'journal', component: JournalPageComponent },
      {
        path: 'therapy',
        component: TherapyLayoutComponent,
        children: [
          { path: '', component: TherapyHubComponent },
          { path: 'cbt', component: CbtComponent },
          { path: 'erp', component: ErpComponent },
          { path: 'self-validation', component: SelfValidationComponent },
          { path: 'self-confidence', component: SelfConfidenceComponent },
          { path: 'self-worth', component: SelfWorthComponent },
          { path: 'schema-abandonment', component: SchemaAbandonmentComponent },
          { path: 'schema-failure', component: SchemaFailureComponent },
          { path: 'schema-people', component: SchemaPeopleComponent },
          { path: 'schema-suppression', component: SchemaSuppressionComponent }
        ]
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
