# PollPulse Defense Guide

## One-line Project Explanation

PollPulse is a student voice portal that combines class elections, student feedback, sentiment analysis, admin reporting, and audit logging in one full-stack web application.

## Why This Project Meets the Web Technologies Requirement

- Frontend is built with Angular standalone components, TypeScript, HTML and CSS.
- Backend is built with ASP.NET Core Web API in C#.
- MongoDB is used as the real database.
- JWT is used for authentication and route protection.
- Frontend communicates with backend through Angular HttpClient services.
- The project has dynamic pages, database-driven dashboards, CRUD operations, routing, guards, and reactive forms.

## Important Pages to Show in Viva

1. Home page - dynamic public metrics and animations
2. Register page - reactive form validation
3. Login page - JWT login
4. Student dashboard - data from database
5. Elections page - class-wise dynamic elections
6. Election detail page - dynamic route and voting
7. Feedback page - category-wise feedback and sentiment
8. Admin dashboard - aggregate metrics
9. Admin elections - create, close and delete elections
10. Admin masters - teacher/course/event management
11. Admin feedback and sentiment pages - reporting
12. Audit logs - backend activity tracking

## Key Backend Concepts to Explain

- Controllers expose REST endpoints.
- Services encapsulate MongoDB access, JWT token generation, and sentiment calculation.
- MongoDB collections store users, admins, elections, candidates, votes, feedback, audit logs, class sections, teachers, courses and events.
- JWT token contains role and classKey claims.
- Admin routes require the admin role.
- Voting prevents duplicate votes through a unique MongoDB index on election + user.

## Key Frontend Concepts to Explain

- Angular Router handles navigation and dynamic route parameters.
- Auth guards protect student/admin pages.
- Reactive Forms handle validation in login, signup, feedback, contact, vote, admin login and admin forms.
- Angular services call backend API endpoints.
- Signals are used for reactive UI state.
- Vanilla DOM/JavaScript concepts are demonstrated through document selection, IntersectionObserver scroll reveal, theme switching, and window scroll handling.

## Common Viva Questions

### Why did you use MongoDB?
MongoDB is suitable because the project has document-style data such as feedback, ratings, sentiment objects, users and elections. It also makes seed data and demo setup simple.

### How does authentication work?
The backend validates email/password, generates a JWT token, and the frontend stores the token in localStorage. The JWT interceptor attaches the token to API requests.

### How do you prevent duplicate voting?
Before inserting a vote, the backend checks whether the current user already voted in the same election. The database also has a unique index on election + user.

### How is feedback sentiment calculated?
The backend SentimentService checks positive and negative terms in the student's comment and suggestion, then classifies the result as Positive, Neutral or Negative.

### What is the main CRUD part?
The admin side has full CRUD controllers for teachers, courses, events and class sections. The admin panel also creates/updates/deletes elections and views feedback reports.

### What is the main dynamic route?
`/elections/:id` loads a specific election from the database and shows its candidates. `/elections/:id/results` shows dynamic result data.

## Demo Flow for Defense

1. Start MongoDB.
2. Start backend with `dotnet run --launch-profile https`.
3. Start frontend with `npm start`.
4. Open `http://localhost:4200`.
5. Register or login as seeded student.
6. Show dashboard and elections.
7. Cast a vote if an active election is open.
8. Submit feedback.
9. Login as admin.
10. Create an election and show it in admin list.
11. Open feedback/sentiment reports and audit logs.
