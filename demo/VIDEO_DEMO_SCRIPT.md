# PollPulse Video Demo Script

Target duration: 5 to 10 minutes.

## 1. Team Introduction

Each member should speak briefly:

- Name
- Roll number
- Main contribution

## 2. Project Introduction

Say:

"PollPulse is a student voice portal built with Angular, ASP.NET Core Web API and MongoDB. It provides student login, class-wise elections, voting, feedback submission, sentiment analysis, admin dashboard and audit logs."

## 3. Show Running Application

Open:

```text
http://localhost:4200
```

Show the landing page and dynamic metrics.

## 4. Student Flow

1. Open Register page and explain validation.
2. Login with:

```text
2502077@students.au.edu.pk / Student@123
```

3. Show dashboard.
4. Open elections list.
5. Open election detail using dynamic route.
6. Cast vote if election is active.
7. Open result page.
8. Open feedback page.
9. Submit category-wise feedback.

## 5. Admin Flow

Login with:

```text
admin@aucs.local / Admin@12345
```

Show:

1. Admin dashboard
2. Create election
3. Manage elections
4. Masters page
5. Feedback records
6. Sentiment dashboard
7. Audit logs

## 6. Database Proof

Open MongoDB Compass and show collections:

- users
- admins
- elections
- candidates
- votes
- feedbacks
- teachers
- courses
- events
- auditlogs

## 7. Backend/API Proof

Show backend terminal running on:

```text
https://localhost:7235
```

Optionally test one API in browser/Postman:

```text
GET https://localhost:7235/api/public/metrics
```

## 8. Closing

Say:

"This project meets the required stack: Angular with TypeScript frontend, ASP.NET Core backend, MongoDB database, JWT authentication, reactive forms, HTTP Client integration, dynamic pages, CRUD controllers, seed data, report and video demo."
