import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home.component';
import { LoginComponent } from './pages/login.component';
import { RegisterComponent } from './pages/register.component';
import { DashboardComponent } from './pages/dashboard.component';
import { ElectionsComponent } from './pages/elections.component';
import { ElectionDetailComponent } from './pages/election-detail.component';
import { ElectionResultsComponent } from './pages/election-results.component';
import { FeedbackComponent } from './pages/feedback.component';
import { AboutComponent } from './pages/about.component';
import { ContactComponent } from './pages/contact.component';

// Admin Components
import { AdminLoginComponent } from './pages/admin-login.component';
import { AdminDashboardComponent } from './pages/admin-dashboard.component';
import { AdminClassesComponent } from './pages/admin-classes.component';
import { AdminCreateElectionComponent } from './pages/admin-create-election.component';
import { AdminManageElectionsComponent } from './pages/admin-manage-elections.component';
import { AdminElectionResultsComponent } from './pages/admin-election-results.component';
import { AdminFeedbackComponent } from './pages/admin-feedback.component';
import { AdminSentimentComponent } from './pages/admin-sentiment.component';
import { AdminMastersComponent } from './pages/admin-masters.component';
import { AdminAuditLogsComponent } from './pages/admin-audit-logs.component';

// Guards
import { authGuard } from './guards/auth.guard';
import { adminGuard } from './guards/admin.guard';

export const routes: Routes = [
  // Public Routes
  { path: '', component: HomeComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'about', component: AboutComponent },
  { path: 'contact', component: ContactComponent },

  // Guarded Student Routes
  { path: 'dashboard', component: DashboardComponent, canActivate: [authGuard] },
  { path: 'elections', component: ElectionsComponent, canActivate: [authGuard] },
  { path: 'elections/:id', component: ElectionDetailComponent, canActivate: [authGuard] },
  { path: 'elections/:id/results', component: ElectionResultsComponent, canActivate: [authGuard] },
  { path: 'feedback/:category', component: FeedbackComponent, canActivate: [authGuard] },

  // Guarded Admin Routes
  { path: 'admin/login', component: AdminLoginComponent },
  { path: 'admin/dashboard', component: AdminDashboardComponent, canActivate: [adminGuard] },
  { path: 'admin/classes', component: AdminClassesComponent, canActivate: [adminGuard] },
  { path: 'admin/elections/create', component: AdminCreateElectionComponent, canActivate: [adminGuard] },
  { path: 'admin/elections', component: AdminManageElectionsComponent, canActivate: [adminGuard] },
  { path: 'admin/elections/:id/results', component: AdminElectionResultsComponent, canActivate: [adminGuard] },
  { path: 'admin/feedback', component: AdminFeedbackComponent, canActivate: [adminGuard] },
  { path: 'admin/sentiment', component: AdminSentimentComponent, canActivate: [adminGuard] },
  { path: 'admin/masters', component: AdminMastersComponent, canActivate: [adminGuard] },
  { path: 'admin/audit-logs', component: AdminAuditLogsComponent, canActivate: [adminGuard] },

  // Fallback Route
  { path: '**', redirectTo: '' }
];
