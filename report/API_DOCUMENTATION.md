# API Documentation - PollPulse

Base URL for local development:

```text
https://localhost:7235
```

Authentication is handled through JWT Bearer tokens. Protected requests must include:

```text
Authorization: Bearer <token>
```

## Auth APIs

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/api/auth/register` | Register student account | Public |
| POST | `/api/auth/login` | Student login | Public |
| POST | `/api/auth/admin/login` | Admin login | Public |

### Student Register Request

```json
{
  "name": "Demo Student",
  "email": "2502077@students.au.edu.pk",
  "password": "Student@123",
  "degree": "BSCS",
  "year": 2,
  "section": "A"
}
```

### Login Request

```json
{
  "email": "2502077@students.au.edu.pk",
  "password": "Student@123"
}
```

## Public APIs

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/public/metrics` | Public landing page metrics | Public |
| GET | `/api/public/weather?lat=33.6&lon=73.0` | Weather widget data | Public |

## Student Election APIs

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/elections/dashboard` | Student dashboard metrics | Student |
| GET | `/api/elections` | List elections for student's class | Student |
| GET | `/api/elections/{id}` | Election detail with candidates | Student/Admin |
| POST | `/api/elections/{id}/vote` | Cast vote | Student |
| GET | `/api/elections/{id}/results` | Election results | Student/Admin |

### Vote Request

```json
{
  "candidateId": "candidateObjectId"
}
```

## Feedback APIs

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/feedbacks/masters` | Get teachers, courses and events | Student |
| GET | `/api/feedbacks/recent` | Get student's recent feedback | Student |
| POST | `/api/feedbacks` | Submit feedback | Student |

### Feedback Request

```json
{
  "category": "teacher",
  "teacher": "teacherObjectId",
  "course": null,
  "event": null,
  "targetName": "Lab lecture",
  "rating1": 5,
  "rating2": 5,
  "rating3": 4,
  "rating4": 5,
  "ratingOverall": 5,
  "comment": "The teacher explains concepts clearly.",
  "suggestion": "More lab examples would help."
}
```

## Admin Dashboard and Reporting APIs

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/admin/dashboard` | Admin dashboard metrics | Admin |
| GET | `/api/admin/classes` | Class section list | Admin |
| GET | `/api/admin/elections` | Admin election list | Admin |
| POST | `/api/admin/elections` | Create election | Admin |
| POST | `/api/admin/elections/{id}/status/{status}` | Change election status | Admin |
| DELETE | `/api/admin/elections/{id}` | Delete election | Admin |
| GET | `/api/admin/masters` | Teacher/course/event master data | Admin |
| POST | `/api/admin/masters/teachers` | Add teacher | Admin |
| POST | `/api/admin/masters/courses` | Add course | Admin |
| POST | `/api/admin/masters/events` | Add event | Admin |
| GET | `/api/admin/feedback` | Feedback records with filters | Admin |
| GET | `/api/admin/sentiment` | Sentiment dashboard metrics | Admin |
| GET | `/api/admin/audit-logs` | Audit trail | Admin |

## Full CRUD Controllers Added for Requirement

These controllers clearly demonstrate full CRUD with GET, POST, PUT and DELETE.

### Teacher CRUD

| Method | Endpoint |
|---|---|
| GET | `/api/admin/crud/teachers` |
| GET | `/api/admin/crud/teachers/{id}` |
| POST | `/api/admin/crud/teachers` |
| PUT | `/api/admin/crud/teachers/{id}` |
| DELETE | `/api/admin/crud/teachers/{id}` |

### Course CRUD

| Method | Endpoint |
|---|---|
| GET | `/api/admin/crud/courses` |
| GET | `/api/admin/crud/courses/{id}` |
| POST | `/api/admin/crud/courses` |
| PUT | `/api/admin/crud/courses/{id}` |
| DELETE | `/api/admin/crud/courses/{id}` |

### Event CRUD

| Method | Endpoint |
|---|---|
| GET | `/api/admin/crud/events` |
| GET | `/api/admin/crud/events/{id}` |
| POST | `/api/admin/crud/events` |
| PUT | `/api/admin/crud/events/{id}` |
| DELETE | `/api/admin/crud/events/{id}` |

### Class Section CRUD

| Method | Endpoint |
|---|---|
| GET | `/api/admin/crud/classes` |
| GET | `/api/admin/crud/classes/{id}` |
| POST | `/api/admin/crud/classes` |
| PUT | `/api/admin/crud/classes/{id}` |
| DELETE | `/api/admin/crud/classes/{id}` |
