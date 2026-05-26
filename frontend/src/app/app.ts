import { Component, OnInit, inject, signal, HostListener } from '@angular/core';
import { RouterOutlet, Router, NavigationEnd, RouterLink } from '@angular/router';
import { NgIf, NgClass } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { AuthService } from './services/auth.service';
import { filter } from 'rxjs';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, NgIf, NgClass],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  protected readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly http = inject(HttpClient);

  isAdminMode = signal(false);
  liveTime = signal('--:--');
  weatherText = signal('Weather');
  currentTheme = signal('light');
  showScrollTop = signal(false);

  constructor() {
    if (typeof window !== 'undefined') {
      const savedTheme = localStorage.getItem('au-theme') || 'light';
      this.currentTheme.set(savedTheme);
      document.documentElement.setAttribute('data-theme', savedTheme);
    }
  }

  ngOnInit(): void {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      const url = event.urlAfterRedirects || event.url;
      this.isAdminMode.set(url.startsWith('/admin') && url !== '/admin/login');
      
      window.scrollTo(0, 0);

      setTimeout(() => this.initScrollReveal(), 300);
    });

    this.updateTime();
    setInterval(() => this.updateTime(), 30000);

    this.fetchWeather();
  }

  toggleTheme(): void {
    const nextTheme = this.currentTheme() === 'light' ? 'dark' : 'light';
    this.currentTheme.set(nextTheme);
    if (typeof window !== 'undefined') {
      document.documentElement.setAttribute('data-theme', nextTheme);
      localStorage.setItem('au-theme', nextTheme);
    }
  }

  private updateTime(): void {
    const now = new Date();
    this.liveTime.set(now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }));
  }

  private fetchWeather(): void {
    if (typeof navigator !== 'undefined' && navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        (pos) => {
          const lat = pos.coords.latitude;
          const lon = pos.coords.longitude;
          this.http.get<any>(`http://localhost:5007/api/public/weather?lat=${lat}&lon=${lon}`).subscribe({
            next: (data) => {
              if (data && data.temperature !== null) {
                this.weatherText.set(`${data.city} ${Math.round(data.temperature)}${data.unit}`);
              } else {
                this.weatherText.set(data.city || 'Weather');
              }
            },
            error: () => this.weatherText.set('Weather unavailable')
          });
        },
        () => this.weatherText.set('Location off'),
        { timeout: 5000 }
      );
    } else {
      this.weatherText.set('Weather unavailable');
    }
  }

  @HostListener('window:scroll', [])
  onWindowScroll(): void {
    if (typeof window !== 'undefined') {
      this.showScrollTop.set(window.scrollY > 360);
    }
  }

  scrollToTop(): void {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  adminLogout(): void {
    this.authService.logout();
    this.router.navigate(['/admin/login']);
  }

  private initScrollReveal(): void {
    if (typeof window === 'undefined') return;
    
    const selector = [
      '.reveal-up',
      '.feedback-hub-card',
      '.feature-panel',
      '.coverage-card',
      '.poll-card',
      '.metric-card',
      '.glass-card',
      '.about-step-card',
      '.about-mini-card',
      '.feedback-form-card',
      '.academic-page-hero',
      '.academic-feedback-hero'
    ].join(',');

    const elements = document.querySelectorAll(selector);
    elements.forEach(el => el.classList.add('scroll-reveal'));

    if (!('IntersectionObserver' in window)) {
      elements.forEach(el => el.classList.add('is-visible'));
      return;
    }

    const observer = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (entry.isIntersecting) {
          entry.target.classList.add('is-visible');
          observer.unobserve(entry.target);
        }
      });
    }, { threshold: 0.12, rootMargin: '0px 0px -8% 0px' });

    elements.forEach(el => observer.observe(el));
  }
}
