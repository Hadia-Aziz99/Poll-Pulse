# PollPulse - Student Voice, Feedback and Election Portal

PollPulse is a full-stack Web Technologies semester project built with Angular, TypeScript, ASP.NET Core Web API, and MongoDB. The application allows university students to register/login, view class-specific elections, cast one secure vote per election, submit category-wise feedback, and view dashboards. Admin users can manage elections, academic master data, feedback records, sentiment reports, and audit logs.

## Team Details

| Member | Roll No. | Responsibility | GitHub Username |
|---|---|---|---|
| Member 1 | Add roll no. | Frontend pages and routing | Add username |
| Member 2 | Add roll no. | Backend APIs and database | Add username |
| Member 3 | Add roll no. | Report, testing and demo | Add username |

Update this table before final GitHub submission so every member can defend their own contribution.

## Technology Stack

- Frontend: Angular + TypeScript + HTML + CSS
- Backend: ASP.NET Core Web API using C#
- Database: MongoDB
- Authentication: JWT Bearer Token Authentication
- Styling: Responsive CSS using Bootstrap utility classes, custom CSS, Flexbox/Grid

## Folder Structure

```text
Pollpulse-final-submission/
├── frontend/     Angular frontend application
├── backend/      ASP.NET Core Web API project
├── database/     MongoDB schema, seed data notes and setup guide
├── report/       Project report PDF/Markdown and screenshot checklist
├── demo/         Video demo script and final video-link placeholder
└── README.md     Main project setup and defense guide
```

## Main Features

- Attractive landing page with dynamic metrics and animations
- Student registration and login with validation
- Admin login with protected admin panel
- Student dashboard with dynamic database data
- Dynamic elections list, election detail route, voting and result page
- Feedback module with category-wise forms and sentiment classification
- Admin dashboard with metrics, audit logs, feedback, sentiment and master-data pages
- Four full CRUD API controllers for teachers, courses, events and class sections
- MongoDB seed data for demo users, admins, classes, elections, candidates, votes and feedback
- Vanilla JavaScript DOM feature through document query selection, IntersectionObserver reveal animation, theme switching and scroll handling in Angular root component

## Demo Credentials

### Student Login

```text
Email: 2502077@students.au.edu.pk
Password: Student@123
```

Other seeded students:

```text
2502078@students.au.edu.pk / Student@123
2502079@students.au.edu.pk / Student@123
2502080@students.au.edu.pk / Student@123
```

### Admin Login

```text
Email: admin@aucs.local
Password: Admin@12345
```

Second admin:

```text
Email: coordinator@aucs.local
Password: Admin@12345
```

## Setup Requirements

Install these before running the project:

1. Visual Studio Code
2. Node.js LTS
3. Angular CLI
4. .NET 8 SDK
5. MongoDB Community Server or MongoDB Atlas

## How to Run Locally

### 1. Open the Project in VS Code

Open VS Code, then open the extracted project folder.

Or use terminal:

```bash
cd path/to/Pollpulse-final-submission
code .
```

### 2. Start MongoDB

If MongoDB is installed locally, make sure the MongoDB service is running.

Default connection string used by backend:

```text
mongodb://127.0.0.1:27017/au_cs_voice_portal
```

The backend automatically creates seed data on first run.

### 3. Run Backend

Open a VS Code terminal:

```bash
cd backend
dotnet restore
dotnet run --launch-profile https
```

Backend URL:

```text
https://localhost:7235
```

If HTTPS certificate warning appears, run:

```bash
dotnet dev-certs https --trust
```

Then run backend again.

### 4. Run Frontend

Open a second VS Code terminal:

```bash
cd frontend
npm install
npm start
```

Frontend URL:

```text
http://localhost:4200
```

Open the frontend URL in your browser.

## API Summary

Detailed API documentation is available in:

```text
report/API_DOCUMENTATION.md
```

Main API groups:

- `/api/auth` - student/admin login and student registration
- `/api/elections` - student election dashboard, details, voting and results
- `/api/feedbacks` - feedback masters, recent feedback and feedback submission
- `/api/admin` - admin dashboard, elections, feedback, sentiment, audit logs and master data
- `/api/admin/crud/teachers` - full CRUD controller
- `/api/admin/crud/courses` - full CRUD controller
- `/api/admin/crud/events` - full CRUD controller
- `/api/admin/crud/classes` - full CRUD controller

## Database

Database schema and seed-data explanation are available in:

```text
database/MONGODB_SCHEMA.md
database/mongodb-schema-diagram.mmd
database/MONGODB_SETUP.md
```

## Project Report

Project report is available in:

```text
report/Pollpulse_Project_Report.pdf
report/Pollpulse_Project_Report.md
```

Before final submission, add real screenshots captured from your own running browser into:

```text
report/screenshots/
```

## Video Demo Link

Replace this placeholder with your final Google Drive, YouTube, or LMS video link:

```text
Add final video demo link here
```

Video script is available in:

```text
demo/VIDEO_DEMO_SCRIPT.md
```

## Live Link

Deployment is optional in the requirement. If deployed, add the deployed frontend/backend links here:

```text
Frontend live link: Add here
Backend live link: Add here
```

## Requirement Coverage Checklist

| Requirement | Status |
|---|---|
| Angular + TypeScript frontend | Done |
| ASP.NET Core Web API backend | Done |
| MongoDB database | Done |
| Minimum 5-6 dynamic pages | Done |
| Authentication login/signup | Done |
| Dashboard with database data | Done |
| Main feature page | Done |
| Detail/profile dynamic route | Done |
| Admin panel CRUD | Done |
| Angular components/services/routing/directives | Done |
| Reactive forms with validation | Done |
| HTTP Client backend integration | Done |
| Responsive CSS | Done |
| 4 API controllers with full CRUD | Done |
| JWT authentication | Done |
| RESTful status codes | Done |
| 5+ related collections | Done |
| Seed data | Done |
| Database schema diagram | Done |
| Report PDF | Added |
| Video demo | Record and add final link |
| GitHub 10 commits/member commits | Must be done in GitHub |
| Screenshots of all pages | Capture locally and add before final upload |

## Image Assets

The home feedback cards and feedback page hero sections use high-quality Unsplash images via `images.unsplash.com` URLs. These replace the earlier generated placeholder images and make the UI more professional for demo and viva. An internet connection is required for those remote images to load.
